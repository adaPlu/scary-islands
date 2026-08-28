using UnityEngine;
using ScaryIslands.Game;

namespace ScaryIslands.World
{
    public sealed class BlackTide : MonoBehaviour
    {
        [SerializeField] private float startHeight = -2f;
        [SerializeField] private float endHeight = 5f;
        [SerializeField] private float riseExponent = 1.8f;
        private float initialDuration;
        private void Start() { if (RunState.Instance != null) initialDuration = RunState.Instance.TimeRemaining; }
        private void Update()
        {
            if (RunState.Instance == null || initialDuration <= 0f) return;
            float p = 1f - RunState.Instance.TimeRemaining / initialDuration;
            var pos = transform.position; pos.y = Mathf.Lerp(startHeight, endHeight, Mathf.Pow(p, riseExponent)); transform.position = pos;
        }
    }
}

