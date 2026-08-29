using System;
using UnityEngine;
using ScaryIslands.Economy;

namespace ScaryIslands.Game
{
    public sealed class RunState : MonoBehaviour
    {
        public static RunState Instance { get; private set; }

        [SerializeField, Min(30)] private float runDurationSeconds = 900f;
        [SerializeField, Min(0)] private int escapeDotsReward = 25;

        public float TimeRemaining { get; private set; }
        public bool HasChapelKey { get; private set; }
        public bool HasSaltBell { get; private set; }
        public bool BellRung { get; private set; }
        public bool IsFinished { get; private set; }

        public event Action Changed;
        public event Action<bool> Finished;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            TimeRemaining = runDurationSeconds;
        }

        private void Update()
        {
            if (IsFinished) return;
            TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);
            if (TimeRemaining <= 0f) Finish(false);
            Changed?.Invoke();
        }

        public void TakeKey() { HasChapelKey = true; Changed?.Invoke(); }
        public void TakeBell() { HasSaltBell = true; Changed?.Invoke(); }

        public bool RingBell()
        {
            if (!HasSaltBell || BellRung) return false;
            BellRung = true;
            Changed?.Invoke();
            return true;
        }

        public void Escape()
        {
            if (BellRung) Finish(true);
        }

        private void Finish(bool escaped)
        {
            if (IsFinished) return;
            IsFinished = true;

            if (escaped && DotWallet.Instance != null)
                DotWallet.Instance.AddDots(escapeDotsReward);

            Finished?.Invoke(escaped);
        }
    }
}
