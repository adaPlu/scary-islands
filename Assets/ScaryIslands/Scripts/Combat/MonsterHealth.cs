using System;
using UnityEngine;
using ScaryIslands.Multiplayer;

namespace ScaryIslands.Combat
{
    /// <summary>Shared monster health with host-authoritative multiplayer damage.</summary>
    public sealed class MonsterHealth : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximumHealth = 100f;
        [SerializeField] private bool destroyOnDeath = true;

        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;
        public int NetworkId => Animator.StringToHash(gameObject.scene.name + "/" + gameObject.name);

        public event Action<MonsterHealth, float> Damaged;
        public event Action<MonsterHealth> Died;

        private void Awake()
        {
            CurrentHealth = maximumHealth;
        }

        public bool TakeDamage(float damage)
        {
            if (!IsAlive || damage <= 0f) return false;

            MultiplayerSession session = MultiplayerSession.Instance;
            if (session != null && session.IsActive)
                return session.RequestMonsterDamage(this, damage);

            return ApplyAuthoritativeDamage(damage);
        }

        public bool ApplyAuthoritativeDamage(float damage)
        {
            if (!IsAlive || damage <= 0f) return false;

            float applied = Mathf.Min(CurrentHealth, damage);
            CurrentHealth -= applied;
            Damaged?.Invoke(this, applied);

            if (CurrentHealth <= 0f)
            {
                CurrentHealth = 0f;
                Died?.Invoke(this);

                MultiplayerSession session = MultiplayerSession.Instance;
                if (session != null && session.IsActive)
                    SetDefeatedState(true);
                else if (destroyOnDeath)
                    Destroy(gameObject);
            }

            return true;
        }

        public void ApplyNetworkState(float health)
        {
            bool wasAlive = IsAlive;
            CurrentHealth = Mathf.Clamp(health, 0f, maximumHealth);

            if (CurrentHealth <= 0f)
            {
                if (wasAlive)
                    Died?.Invoke(this);
                SetDefeatedState(true);
            }
            else
            {
                SetDefeatedState(false);
            }
        }

        private void SetDefeatedState(bool defeated)
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
                renderer.enabled = !defeated;

            foreach (Collider collider in GetComponentsInChildren<Collider>(true))
                collider.enabled = !defeated;
        }
    }
}
