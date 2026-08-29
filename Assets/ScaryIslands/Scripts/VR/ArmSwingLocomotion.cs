using UnityEngine;
using ScaryIslands.Combat;

namespace ScaryIslands.VR
{
    /// <summary>
    /// Arm-powered VR locomotion. Pull the hands backward to move on the ground.
    /// Flap both winged arms downward to take off and gain altitude; spread the arms to glide.
    /// Every player also receives a free starter gun on the tracked right hand.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class ArmSwingLocomotion : MonoBehaviour
    {
        [Header("XR Tracking")]
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;

        [Header("Ground Movement")]
        [SerializeField, Min(0.1f)] private float speedMultiplier = 2.2f;
        [SerializeField, Min(0.1f)] private float maximumSpeed = 3.5f;
        [SerializeField, Min(0f)] private float minimumStrokeSpeed = 0.12f;
        [SerializeField, Min(0f)] private float acceleration = 10f;
        [SerializeField, Min(0f)] private float braking = 14f;
        [SerializeField] private float groundGravity = -18f;

        [Header("Wing Flight")]
        [SerializeField, Min(0.1f)] private float minimumFlapSpeed = 0.55f;
        [SerializeField, Min(0.1f)] private float takeoffFlapSpeed = 0.9f;
        [SerializeField, Min(0f)] private float takeoffVelocity = 2.2f;
        [SerializeField, Min(0f)] private float flapLiftAcceleration = 7.5f;
        [SerializeField, Min(0f)] private float flapForwardAcceleration = 3.2f;
        [SerializeField, Min(0.1f)] private float maximumFlightSpeed = 7f;
        [SerializeField, Min(0.1f)] private float maximumRiseSpeed = 5.5f;
        [SerializeField, Min(0.1f)] private float maximumFallSpeed = 7f;
        [SerializeField] private float flightGravity = -7f;
        [SerializeField] private float glideGravity = -2.4f;
        [SerializeField, Min(0.2f)] private float glideArmSpan = 0.85f;
        [SerializeField, Min(0f)] private float airDrag = 0.8f;

        [Header("Starter Gun")]
        [SerializeField] private bool giveFreeStarterGun = true;

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
        private bool wingsConfigured;
        private bool gunConfigured;
        private bool isFlying;

        public bool IsFlying => isFlying;
        public Transform Head => head;
        public Transform LeftHand => leftHand;
        public Transform RightHand => rightHand;

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
            verticalSpeed = 0f;
            isFlying = false;
            wingsConfigured = false;
            gunConfigured = false;
        }

        private void Update()
        {
            if (head == null || leftHand == null || rightHand == null) return;

            if (!wingsConfigured)
                ConfigureArmWings();
            if (!gunConfigured)
                ConfigureStarterGun();

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
            if (forward.sqrMagnitude < 0.01f)
                forward = transform.forward;

            float leftDown = Mathf.Max(0f, Vector3.Dot(leftVelocity, Vector3.down));
            float rightDown = Mathf.Max(0f, Vector3.Dot(rightVelocity, Vector3.down));
            bool bothArmsFlapping = leftDown >= minimumFlapSpeed && rightDown >= minimumFlapSpeed;
            float flapStrength = bothArmsFlapping ? (leftDown + rightDown) * 0.5f : 0f;

            bool grounded = controller.isGrounded;
            if (grounded && verticalSpeed < 0f)
                verticalSpeed = -1f;

            if (grounded && flapStrength >= takeoffFlapSpeed)
            {
                isFlying = true;
                verticalSpeed = Mathf.Max(verticalSpeed, takeoffVelocity);
            }
            else if (!grounded && bothArmsFlapping)
            {
                isFlying = true;
            }
            else if (grounded && verticalSpeed <= 0f)
            {
                isFlying = false;
            }

            if (isFlying)
                UpdateFlight(forward, flapStrength, dt);
            else
                UpdateGroundMovement(forward, leftVelocity, rightVelocity, dt);

            controller.Move((forward * currentSpeed + Vector3.up * verticalSpeed) * dt);
        }

        private void UpdateGroundMovement(Vector3 forward, Vector3 leftVelocity, Vector3 rightVelocity, float dt)
        {
            float leftPull = Mathf.Max(0f, -Vector3.Dot(leftVelocity, forward));
            float rightPull = Mathf.Max(0f, -Vector3.Dot(rightVelocity, forward));
            float stroke = (leftPull + rightPull) * 0.5f;
            float targetSpeed = stroke >= minimumStrokeSpeed
                ? Mathf.Min(stroke * speedMultiplier, maximumSpeed)
                : 0f;

            float rate = targetSpeed > currentSpeed ? acceleration : braking;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * dt);
            verticalSpeed += groundGravity * dt;
        }

        private void UpdateFlight(Vector3 forward, float flapStrength, float dt)
        {
            bool gliding = Vector3.Distance(leftHand.position, rightHand.position) >= glideArmSpan;

            if (flapStrength > 0f)
            {
                verticalSpeed += flapStrength * flapLiftAcceleration * dt;
                currentSpeed += flapStrength * flapForwardAcceleration * dt;
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, airDrag * dt);
            }

            float activeGravity = gliding ? glideGravity : flightGravity;
            verticalSpeed += activeGravity * dt;

            currentSpeed = Mathf.Clamp(currentSpeed, 0f, maximumFlightSpeed);
            verticalSpeed = Mathf.Clamp(verticalSpeed, -maximumFallSpeed, maximumRiseSpeed);
        }

        private void ConfigureArmWings()
        {
            PlayerWings wings = GetComponent<PlayerWings>();
            if (wings == null)
                wings = gameObject.AddComponent<PlayerWings>();

            wings.Configure(leftHand, rightHand);
            wingsConfigured = wings.IsConfigured;
        }

        private void ConfigureStarterGun()
        {
            if (!giveFreeStarterGun)
            {
                gunConfigured = true;
                return;
            }

            Transform existing = rightHand.Find("Free Starter Gun");
            if (existing != null)
            {
                gunConfigured = true;
                return;
            }

            GameObject gunObject = new GameObject("Free Starter Gun");
            gunObject.transform.SetParent(rightHand, false);
            gunObject.transform.localPosition = Vector3.zero;
            gunObject.transform.localRotation = Quaternion.identity;
            gunObject.AddComponent<StarterGun>();
            gunConfigured = true;
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
