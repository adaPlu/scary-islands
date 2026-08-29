using System;
using UnityEngine;

namespace ScaryIslands.Combat
{
    /// <summary>
    /// Shared monster hit component. Monsters are invincible by default:
    /// gunfire registers valid hits for Dot streaks but never removes health.
    /// </summary>
    public sealed class MonsterHealth : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximumHealth = 100f;
        [SerializeField] private bool invincible = true;
        [SerializeField] private bool destroyOnDeath = false;

        public float CurrentHealth { get; private set; }
        public bool IsAlive => true;
        public bool IsInvincible => invincible;

        public event Action<MonsterHealth, float> Damaged;
        public event Action<MonsterHealth> Died;

        private void Awake()
        {
            CurrentHealth = maximumHealth;
        }

        public bool TakeDamage(float damage)
        {
            if (damage <= 0f) return false;

            if (invincible)
            {
                // Count this as a valid monster hit so the gun's sustained-hit
                // Dot streak still works, while health remains unchanged.
                CurrentHealth = maximumHealth;
                Damaged?.Invoke(this, 0f);
                return true;
            }

            float applied = Mathf.Min(CurrentHealth, damage);
            CurrentHealth -= applied;
            Damaged?.Invoke(this, applied);

            if (CurrentHealth <= 0f)
            {
                CurrentHealth = 0f;
                Died?.Invoke(this);
                if (destroyOnDeath)
                    Destroy(gameObject);
            }

            return true;
        }

        public void RestoreFullHealth()
        {
            CurrentHealth = maximumHealth;
        }
    }
}
