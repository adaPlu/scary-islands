using UnityEngine;

namespace ScaryIslands.VR
{
    /// <summary>Keeps a floating torso beneath the headset without creating legs.</summary>
    public sealed class PlayerUpperBodyAvatar : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private Transform torso;
        [SerializeField, Range(0.2f, 0.8f)] private float torsoDrop = 0.48f;
        [SerializeField, Range(1f, 30f)] private float rotationSharpness = 12f;

        private void LateUpdate()
        {
            if (head == null || torso == null) return;
            Vector3 flatForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            torso.position = head.position - Vector3.up * torsoDrop;
            if (flatForward.sqrMagnitude > 0.01f)
                torso.rotation = Quaternion.Slerp(torso.rotation, Quaternion.LookRotation(flatForward),
                    1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
        }
    }
}

