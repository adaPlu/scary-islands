using UnityEngine;
using ScaryIslands.Economy;

namespace ScaryIslands.Combat
{
    /// <summary>
    /// Free automatic starter gun. Bind BeginFire/EndFire to the XR trigger.
    /// Sustained hits award 1 Dot for the first full second, 2 for the next,
    /// 3 for the next, and so on until the hit streak breaks.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StarterGun : MonoBehaviour
    {
        [Header("Gun")]
        [SerializeField, Min(1f)] private float range = 40f;
        [SerializeField, Min(0.1f)] private float damagePerShot = 6f;
        [SerializeField, Min(1f)] private float shotsPerSecond = 8f;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Dot Hit Streak")]
        [SerializeField, Min(0.05f)] private float hitGraceSeconds = 0.35f;

        private bool triggerHeld;
        private float nextShotTime;
        private float lastMonsterHitTime = float.NegativeInfinity;
        private float sustainedHitTimer;
        private int nextDotReward = 1;

        public bool TriggerHeld => triggerHeld;
        public int NextDotReward => nextDotReward;

        private void Awake()
        {
            BuildPrototypeModel();
        }

        private void Update()
        {
            if (triggerHeld && Time.time >= nextShotTime)
            {
                nextShotTime = Time.time + 1f / shotsPerSecond;
                FireShot();
            }

            bool sustainingHit = triggerHeld && Time.time - lastMonsterHitTime <= hitGraceSeconds;
            if (sustainingHit)
            {
                sustainedHitTimer += Time.deltaTime;
                while (sustainedHitTimer >= 1f)
                {
                    sustainedHitTimer -= 1f;
                    if (DotWallet.Instance != null)
                        DotWallet.Instance.AddDots(nextDotReward);
                    nextDotReward++;
                }
            }
            else if (Time.time - lastMonsterHitTime > hitGraceSeconds)
            {
                ResetHitStreak();
            }
        }

        public void BeginFire()
        {
            triggerHeld = true;
            nextShotTime = 0f;
        }

        public void EndFire()
        {
            triggerHeld = false;
        }

        public void FireOnce()
        {
            FireShot();
        }

        private void FireShot()
        {
            Vector3 origin = transform.position + transform.forward * 0.08f;
            if (!Physics.Raycast(origin, transform.forward, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
                return;

            MonsterHealth monster = hit.collider.GetComponentInParent<MonsterHealth>();
            if (monster == null || !monster.TakeDamage(damagePerShot))
                return;

            lastMonsterHitTime = Time.time;
        }

        private void ResetHitStreak()
        {
            sustainedHitTimer = 0f;
            nextDotReward = 1;
        }

        private void BuildPrototypeModel()
        {
            if (transform.Find("Starter Gun Model") != null) return;

            GameObject root = new GameObject("Starter Gun Model");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0.02f, -0.03f, 0.10f);
            root.transform.localRotation = Quaternion.identity;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Gun Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0f, 0.13f);
            body.transform.localScale = new Vector3(0.07f, 0.09f, 0.28f);
            RemoveCollider(body);

            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Gun Barrel";
            barrel.transform.SetParent(root.transform, false);
            barrel.transform.localPosition = new Vector3(0f, 0.01f, 0.31f);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            barrel.transform.localScale = new Vector3(0.03f, 0.13f, 0.03f);
            RemoveCollider(barrel);

            GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grip.name = "Gun Grip";
            grip.transform.SetParent(root.transform, false);
            grip.transform.localPosition = new Vector3(0f, -0.10f, 0.08f);
            grip.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            grip.transform.localScale = new Vector3(0.055f, 0.16f, 0.07f);
            RemoveCollider(grip);
        }

        private static void RemoveCollider(GameObject item)
        {
            Collider collider = item.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
        }
    }
}
