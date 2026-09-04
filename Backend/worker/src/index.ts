export interface Env { DB: D1Database }

const json=(data:unknown,status=200)=>Response.json(data,{status,headers:{"Cache-Control":"no-store","Access-Control-Allow-Origin":"*"}});

const normalizeCode=(code:string)=>code.trim().toUpperCase().replace(/\s+/g,"");

async function sha256Hex(value:string):Promise<string>{
 const bytes=new TextEncoder().encode(value);
 const digest=await crypto.subtle.digest("SHA-256",bytes);
 return Array.from(new Uint8Array(digest)).map(b=>b.toString(16).padStart(2,"0")).join("");
}

export default {
 async fetch(req:Request,env:Env):Promise<Response>{
  const url=new URL(req.url);
  if(req.method==="GET"&&url.pathname==="/health") return json({ok:true,game:"scary-islands"});

  if(req.method==="GET"&&url.pathname==="/leaderboard"){
   const island=(url.searchParams.get("island")||"widows-shore").slice(0,40);
   const rows=await env.DB.prepare("SELECT player_id,island,duration_ms,created_at FROM runs WHERE island=? AND escaped=1 ORDER BY duration_ms ASC LIMIT 25").bind(island).all();
   return json(rows.results);
  }

  if(req.method==="POST"&&url.pathname==="/runs"){
   const body=await req.json<{playerId?:string,island?:string,durationMs?:number,escaped?:boolean}>();
   if(!body.playerId||!body.island||!Number.isInteger(body.durationMs)||body.durationMs===undefined||body.durationMs<=0) return json({error:"invalid_run"},400);
   const id=crypto.randomUUID();
   await env.DB.prepare("INSERT INTO runs(id,player_id,island,duration_ms,escaped) VALUES(?,?,?,?,?)").bind(id,body.playerId.slice(0,80),body.island.slice(0,40),body.durationMs,body.escaped?1:0).run();
   return json({id},201);
  }

  if(req.method==="POST"&&url.pathname==="/codes/redeem"){
   const body=await req.json<{playerId?:string,code?:string}>();
   if(!body.playerId||!body.code) return json({error:"invalid_request"},400);

   const normalized=normalizeCode(body.code);
   if(normalized.length<3||normalized.length>64) return json({error:"invalid_code"},400);

   const codeHash=await sha256Hex(normalized);
   const promo=await env.DB.prepare(
    "SELECT id,reward_type,reward_value,max_redemptions,redemption_count,active,expires_at FROM promo_codes WHERE code_hash=? LIMIT 1"
   ).bind(codeHash).first<{
    id:string; reward_type:string; reward_value:number; max_redemptions:number|null;
    redemption_count:number; active:number; expires_at:string|null;
   }>();

   // Do not reveal whether a code exists, is disabled, expired, or exhausted.
   if(!promo||promo.active!==1) return json({error:"invalid_code"},404);
   if(promo.expires_at&&Date.parse(promo.expires_at)<=Date.now()) return json({error:"invalid_code"},404);
   if(promo.max_redemptions!==null&&promo.redemption_count>=promo.max_redemptions) return json({error:"invalid_code"},404);

   const playerId=body.playerId.slice(0,80);
   const existing=await env.DB.prepare(
    "SELECT id FROM promo_redemptions WHERE promo_code_id=? AND player_id=? LIMIT 1"
   ).bind(promo.id,playerId).first<{id:string}>();
   if(existing) return json({error:"already_redeemed"},409);

   const redemptionId=crypto.randomUUID();
   await env.DB.batch([
    env.DB.prepare("INSERT INTO promo_redemptions(id,promo_code_id,player_id) VALUES(?,?,?)").bind(redemptionId,promo.id,playerId),
    env.DB.prepare("UPDATE promo_codes SET redemption_count=redemption_count+1 WHERE id=?").bind(promo.id)
   ]);

   return json({ok:true,reward:{type:promo.reward_type,value:promo.reward_value}},200);
  }

  return json({error:"not_found"},404);
 }
};
