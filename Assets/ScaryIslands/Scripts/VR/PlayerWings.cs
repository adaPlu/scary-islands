using UnityEngine;

namespace ScaryIslands.VR
{
    /// <summary>
    /// Gives every player a permanent wing attached to each tracked arm.
    /// The wings follow controller motion directly, so the player's real arm flap is the visual flap.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerWings : MonoBehaviour
    {
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField, Range(0.5f, 1.8f)] private float wingScale = 1f;
        [SerializeField] private Color wingColor = new Color(0.12f, 0.14f, 0.16f, 1f);

        private Transform leftWing;
        private Transform rightWing;
        private Material generatedMaterial;

        public bool IsConfigured => leftWing != null && rightWing != null;

        public void Configure(Transform trackedLeftArm, Transform trackedRightArm)
        {
            leftArm = trackedLeftArm;
            rightArm = trackedRightArm;
            EnsureWings();
        }

        private void Awake()
        {
            EnsureWings();
        }

        private void EnsureWings()
        {
            if (leftArm == null || rightArm == null) return;

            if (generatedMaterial == null)
                generatedMaterial = CreateWingMaterial();

            leftWing = EnsureArmWing(leftArm, "Left Arm Wing", -1f);
            rightWing = EnsureArmWing(rightArm, "Right Arm Wing", 1f);
        }

        private Transform EnsureArmWing(Transform arm, string wingName, float side)
        {
            Transform existing = arm.Find(wingName);
            if (existing != null) return existing;

            GameObject wingObject = new GameObject(wingName);
            Transform wing = wingObject.transform;
            wing.SetParent(arm, false);
            wing.localPosition = new Vector3(0f, 0f, -0.04f);
            wing.localRotation = Quaternion.identity;
            wing.localScale = Vector3.one * wingScale;

            MeshFilter filter = wingObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = wingObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh { name = wingName + " Mesh" };
            mesh.vertices = new[]
            {
                new Vector3(0.00f,  0.05f,  0.00f),
                new Vector3(0.22f * side,  0.16f, -0.12f),
                new Vector3(0.60f * side,  0.12f, -0.36f),
                new Vector3(1.00f * side,  0.00f, -0.68f),
                new Vector3(0.66f * side, -0.18f, -0.58f),
                new Vector3(0.22f * side, -0.14f, -0.26f)
            };
            mesh.triangles = side < 0f
                ? new[] { 0, 2, 1, 0, 3, 2, 0, 4, 3, 0, 5, 4 }
                : new[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            filter.sharedMesh = mesh;
            renderer.sharedMaterial = generatedMaterial;
            return wing;
        }

        private Material CreateWingMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material material = new Material(shader) { name = "Scary Islands Player Wing Material" };
            material.color = wingColor;
            return material;
        }

        private void OnDestroy()
        {
            if (generatedMaterial != null)
                Destroy(generatedMaterial);
        }
    }
}
