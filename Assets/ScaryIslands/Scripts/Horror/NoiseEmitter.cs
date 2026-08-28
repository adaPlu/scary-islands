using System;
using UnityEngine;

namespace ScaryIslands.Horror
{
    public readonly struct NoiseEvent
    {
        public readonly Vector3 Position; public readonly float Loudness;
        public NoiseEvent(Vector3 position, float loudness) { Position = position; Loudness = loudness; }
    }
    public static class NoiseBus { public static event Action<NoiseEvent> Emitted; public static void Emit(Vector3 p, float l) => Emitted?.Invoke(new NoiseEvent(p, l)); }
    public sealed class NoiseEmitter : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float loudness = .35f;
        [SerializeField, Min(.1f)] private float minimumInterval = .5f;
        private float nextAllowed;
        public void Emit()
        {
            if (Time.time < nextAllowed) return;
            nextAllowed = Time.time + minimumInterval;
            NoiseBus.Emit(transform.position, loudness);
        }
    }
}

