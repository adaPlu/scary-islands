using UnityEngine;

namespace ScaryIslands.Pets
{
    /// <summary>Spawns and follows the currently equipped pet using lightweight prototype geometry.</summary>
    public sealed class PetCompanionSpawner : MonoBehaviour
    {
        [SerializeField] private Vector3 followOffset = new Vector3(0.55f, -0.15f, -0.65f);
        [SerializeField, Min(1f)] private float followSharpness = 7f;
        [SerializeField, Range(0.08f, 0.5f)] private float petScale = 0.22f;

        private Transform target;
        private GameObject currentPet;
        private Material currentMaterial;
        private string currentPetId = string.Empty;

        private void Start()
        {
            if (PetShop.Instance != null)
            {
                PetShop.Instance.EquippedPetChanged += OnEquippedPetChanged;
                RefreshPet(PetShop.Instance.EquippedPetId);
            }
        }

        private void Update()
        {
            if (target == null && Camera.main != null)
                target = Camera.main.transform;

            if (currentPet == null || target == null) return;

            Vector3 desired = target.TransformPoint(followOffset);
            currentPet.transform.position = Vector3.Lerp(
                currentPet.transform.position,
                desired,
                1f - Mathf.Exp(-followSharpness * Time.deltaTime));

            currentPet.transform.Rotate(Vector3.up, 35f * Time.deltaTime, Space.World);
        }

        private void OnDestroy()
        {
            if (PetShop.Instance != null)
                PetShop.Instance.EquippedPetChanged -= OnEquippedPetChanged;
            DestroyCurrent();
        }

        private void OnEquippedPetChanged(string petId)
        {
            RefreshPet(petId);
        }

        private void RefreshPet(string petId)
        {
            if (currentPetId == petId) return;
            DestroyCurrent();
            currentPetId = petId;

            if (string.IsNullOrEmpty(petId) || PetShop.Instance == null) return;
            PetDefinition pet = PetShop.Instance.GetPet(petId);
            if (pet == null) return;

            currentPet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            currentPet.name = pet.name + " Companion";
            currentPet.transform.localScale = Vector3.one * petScale;

            Collider collider = currentPet.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            currentMaterial = new Material(shader) { name = pet.name + " Prototype Material" };
            currentMaterial.color = PetColor(pet.id);
            currentPet.GetComponent<Renderer>().sharedMaterial = currentMaterial;
        }

        private static Color PetColor(string id)
        {
            switch (id)
            {
                case "fog-moth": return new Color(0.75f, 0.84f, 0.80f);
                case "lantern-crab": return new Color(0.25f, 0.75f, 0.95f);
                case "grave-crow": return new Color(0.12f, 0.12f, 0.16f);
                case "mire-slime": return new Color(0.38f, 0.85f, 0.48f);
                case "storm-bat": return new Color(0.48f, 0.42f, 0.78f);
                case "little-leviathan": return new Color(0.18f, 0.42f, 0.58f);
                default: return Color.gray;
            }
        }

        private void DestroyCurrent()
        {
            if (currentPet != null) Destroy(currentPet);
            if (currentMaterial != null) Destroy(currentMaterial);
            currentPet = null;
            currentMaterial = null;
            currentPetId = string.Empty;
        }
    }
}
