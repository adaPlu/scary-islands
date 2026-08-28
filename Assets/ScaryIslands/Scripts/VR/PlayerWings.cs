using UnityEngine;

namespace ScaryIslands.VR
{
    /// <summary>
    /// Gives every player avatar a permanent pair of wings.
    /// The fallback geometry is generated at runtime so wings are visible even before final art is added.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerWings : MonoBehaviour
    {
        [SerializeField] private Transform torso;
        [SerializeField, Range(0.5f, 1.8f)] private float wingScale = 1f;
        [SerializeField, Range(0f, 25f)] private float idleFlapDegrees = 6f;
        [SerializeField, Range(0.1f, 4f)] private float idleFlapSpeed = 1.4f;
        [SerializeField] private Color wingColor = new Color(0.12f, 0.14f, 0.16f, 1f);

        private Transform leftWing;
        private Transform rightWing;
        private Material generatedMaterial;

        public void Configure(Transform bodyTorso)
        {
            torso = bodyTorso;
            EnsureWings();
        }

        private void Awake()
        {
            EnsureWings();
        }

        private void Update()
        {
            if (leftWing == null || rightWing == null) return;

            float flap = Mathf.Sin(Time.time * idleFlapSpeed * Mathf.PI * 2f) * idleFlapDegrees;
            leftWing.localRotation = Quaternion.Euler(0f, -12f, flap);
            rightWing.localRotation = Quaternion.Euler(0f, 12f, -flap);
        }

        private void EnsureWings()
        {
            if (torso == null) return;

            Transform existing = torso.Find("Player Wings");
            if (existing != null)
            {
                leftWing = existing.Find("Left Wing");
                rightWing = existing.Find("Right Wing");
                return;
            }

            GameObject rootObject = new GameObject("Player Wings");
            Transform root = rootObject.transform;
            root.SetParent(torso, false);
            root.localPosition = new Vector3(0f, 0.06f, 0.14f);
            root.localRotation = Quaternion.identity;
            root.localScale = Vector3.one * wingScale;

            generatedMaterial = CreateWingMaterial();
            leftWing = CreateWing(root, "Left Wing", -1f, generatedMaterial);
            rightWing = CreateWing(root, "Right Wing", 1f, generatedMaterial);
        }

        private Transform CreateWing(Transform parent, string wingName, float side, Material material)
        {
            GameObject wingObject = new GameObject(wingName);
            Transform wing = wingObject.transform;
            wing.SetParent(parent, false);

            MeshFilter filter = wingObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = wingObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh { name = wingName + " Mesh" };
            mesh.vertices = new[]
            {
                new Vector3(0.00f,  0.18f, 0.00f),
                new Vector3(0.45f * side,  0.34f, 0.05f),
                new Vector3(0.95f * side,  0.20f, 0.12f),
                new Vector3(1.20f * side, -0.05f, 0.16f),
                new Vector3(0.70f * side, -0.30f, 0.10f),
                new Vector3(0.18f * side, -0.18f, 0.02f)
            };
            mesh.triangles = side < 0f
                ? new[] { 0, 2, 1, 0, 3, 2, 0, 4, 3, 0, 5, 4 }
                : new[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            filter.sharedMesh = mesh;
            renderer.sharedMaterial = material;
            return wing;
        }

        private Material CreateWingMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

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
