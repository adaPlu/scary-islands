using System;
using UnityEngine;

namespace ScaryIslands.Combat
{
    /// <summary>Simple shared health component for shootable monsters.</summary>
    public sealed class MonsterHealth : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximumHealth = 100f;
        [SerializeField] private bool destroyOnDeath = true;

        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

        public event Action<MonsterHealth, float> Damaged;
        public event Action<MonsterHealth> Died;

        private void Awake()
        {
            CurrentHealth = maximumHealth;
        }

        public bool TakeDamage(float damage)
        {
            if (!IsAlive || damage <= 0f) return false;

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
    }
}
