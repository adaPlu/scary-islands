export interface Env { DB: D1Database }
const json=(data:unknown,status=200)=>Response.json(data,{status,headers:{"Cache-Control":"no-store","Access-Control-Allow-Origin":"*"}});
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
   const id=crypto.randomUUID(); await env.DB.prepare("INSERT INTO runs(id,player_id,island,duration_ms,escaped) VALUES(?,?,?,?,?)").bind(id,body.playerId.slice(0,80),body.island.slice(0,40),body.durationMs,body.escaped?1:0).run();
   return json({id},201);
  }
  return json({error:"not_found"},404);
 }
};
