using UnityEngine;

namespace ScaryIslands.VR
{
    /// <summary>
    /// Comfortable arm-swing locomotion for a CharacterController XR Origin.
    /// Pull both controllers backward to move in the flattened head direction.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class ArmSwingLocomotion : MonoBehaviour
    {
        [Header("XR Tracking")]
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float speedMultiplier = 2.2f;
        [SerializeField, Min(0.1f)] private float maximumSpeed = 3.5f;
        [SerializeField, Min(0f)] private float minimumStrokeSpeed = 0.12f;
        [SerializeField, Min(0f)] private float acceleration = 10f;
        [SerializeField, Min(0f)] private float braking = 14f;
        [SerializeField] private float gravity = -18f;

        [Header("Body")]
        [SerializeField, Range(0.8f, 2.2f)] private float standingHeight = 1.7f;
        [SerializeField, Range(0.1f, 0.8f)] private float bodyRadius = 0.25f;
        [SerializeField] private Renderer[] legAndFootRenderers;

        private CharacterController controller;
        private Vector3 previousLeft;
        private Vector3 previousRight;
        private float currentSpeed;
        private float verticalSpeed;
        private bool trackingReady;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.radius = bodyRadius;
            HideLegs();
        }

        private void OnEnable()
        {
            trackingReady = false;
            currentSpeed = 0f;
        }

        private void Update()
        {
            if (head == null || leftHand == null || rightHand == null) return;
            UpdateCapsule();

            if (!trackingReady)
            {
                previousLeft = leftHand.position;
                previousRight = rightHand.position;
                trackingReady = true;
                return;
            }

            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 leftVelocity = (leftHand.position - previousLeft) / dt;
            Vector3 rightVelocity = (rightHand.position - previousRight) / dt;
            previousLeft = leftHand.position;
            previousRight = rightHand.position;

            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            float leftPull = Mathf.Max(0f, -Vector3.Dot(leftVelocity, forward));
            float rightPull = Mathf.Max(0f, -Vector3.Dot(rightVelocity, forward));
            float stroke = (leftPull + rightPull) * 0.5f;
            float targetSpeed = stroke >= minimumStrokeSpeed
                ? Mathf.Min(stroke * speedMultiplier, maximumSpeed)
                : 0f;

            float rate = targetSpeed > currentSpeed ? acceleration : braking;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * dt);
            verticalSpeed = controller.isGrounded && verticalSpeed < 0f ? -1f : verticalSpeed + gravity * dt;
            controller.Move((forward * currentSpeed + Vector3.up * verticalSpeed) * dt);
        }

        private void UpdateCapsule()
        {
            float height = Mathf.Clamp(head.localPosition.y, 0.8f, standingHeight);
            controller.height = height;
            controller.center = new Vector3(head.localPosition.x, height * 0.5f, head.localPosition.z);
        }

        private void HideLegs()
        {
            foreach (Renderer rendererToHide in legAndFootRenderers)
                if (rendererToHide != null) rendererToHide.enabled = false;

            foreach (Renderer candidate in GetComponentsInChildren<Renderer>(true))
            {
                string lowerName = candidate.name.ToLowerInvariant();
                if (lowerName.Contains("leg") || lowerName.Contains("foot") || lowerName.Contains("feet"))
                    candidate.enabled = false;
            }
        }
    }
}

