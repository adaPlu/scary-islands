using UnityEngine;
using ScaryIslands.VR;

namespace ScaryIslands.Multiplayer
{
    /// <summary>Lightweight visual representation of another VR player.</summary>
    public sealed class RemotePlayerAvatar : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float smoothing = 18f;

        private Transform head;
        private Transform leftHand;
        private Transform rightHand;
        private Transform torso;
        private GameObject pet;
        private Material bodyMaterial;
        private Material petMaterial;

        private Vector3 targetRootPosition;
        private Quaternion targetRootRotation = Quaternion.identity;
        private Vector3 targetHeadPosition;
        private Quaternion targetHeadRotation = Quaternion.identity;
        private Vector3 targetLeftPosition;
        private Quaternion targetLeftRotation = Quaternion.identity;
        private Vector3 targetRightPosition;
        private Quaternion targetRightRotation = Quaternion.identity;
        private byte petIndex;
        private bool initialized;

        public ulong ClientId { get; private set; }

        public void Initialize(ulong clientId)
        {
            ClientId = clientId;
            name = "Remote Player " + clientId;
            BuildAvatar();
        }

        public void ApplySnapshot(
            Vector3 rootPosition,
            Quaternion rootRotation,
            Vector3 headPosition,
            Quaternion headRotation,
            Vector3 leftPosition,
            Quaternion leftRotation,
            Vector3 rightPosition,
            Quaternion rightRotation,
            byte equippedPetIndex)
        {
            targetRootPosition = rootPosition;
            targetRootRotation = rootRotation;
            targetHeadPosition = headPosition;
            targetHeadRotation = headRotation;
            targetLeftPosition = leftPosition;
            targetLeftRotation = leftRotation;
            targetRightPosition = rightPosition;
            targetRightRotation = rightRotation;

            if (petIndex != equippedPetIndex)
            {
                petIndex = equippedPetIndex;
                RefreshPet();
            }

            if (!initialized)
            {
                transform.SetPositionAndRotation(targetRootPosition, targetRootRotation);
                head.SetPositionAndRotation(targetHeadPosition, targetHeadRotation);
                leftHand.SetPositionAndRotation(targetLeftPosition, targetLeftRotation);
                rightHand.SetPositionAndRotation(targetRightPosition, targetRightRotation);
                initialized = true;
            }
        }

        private void Update()
        {
            if (!initialized) return;

            float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetRootPosition, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRootRotation, t);
            head.position = Vector3.Lerp(head.position, targetHeadPosition, t);
            head.rotation = Quaternion.Slerp(head.rotation, targetHeadRotation, t);
            leftHand.position = Vector3.Lerp(leftHand.position, targetLeftPosition, t);
            leftHand.rotation = Quaternion.Slerp(leftHand.rotation, targetLeftRotation, t);
            rightHand.position = Vector3.Lerp(rightHand.position, targetRightPosition, t);
            rightHand.rotation = Quaternion.Slerp(rightHand.rotation, targetRightRotation, t);

            Vector3 torsoTarget = Vector3.Lerp(transform.position, head.position, 0.58f);
            torso.position = Vector3.Lerp(torso.position, torsoTarget, t);
            torso.rotation = Quaternion.Slerp(torso.rotation, transform.rotation, t);

            if (pet != null)
            {
                Vector3 desired = head.position + transform.right * 0.55f - transform.forward * 0.5f - Vector3.up * 0.15f;
                pet.transform.position = Vector3.Lerp(pet.transform.position, desired, t);
                pet.transform.Rotate(Vector3.up, 45f * Time.deltaTime, Space.World);
            }
        }

        private void BuildAvatar()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            bodyMaterial = new Material(shader) { name = "Remote Player Material " + ClientId };
            float hue = (ClientId * 0.173f) % 1f;
            bodyMaterial.color = Color.HSVToRGB(hue, 0.45f, 0.8f);

            torso = MakePrimitive("Torso", PrimitiveType.Capsule, transform, new Vector3(0.34f, 0.45f, 0.22f)).transform;
            head = MakePrimitive("Head", PrimitiveType.Sphere, transform, Vector3.one * 0.24f).transform;
            leftHand = MakePrimitive("Left Hand", PrimitiveType.Sphere, transform, Vector3.one * 0.13f).transform;
            rightHand = MakePrimitive("Right Hand", PrimitiveType.Sphere, transform, Vector3.one * 0.13f).transform;

            PlayerWings wings = gameObject.AddComponent<PlayerWings>();
            wings.Configure(leftHand, rightHand);

            GameObject gunBody = MakePrimitive("Remote Starter Gun", PrimitiveType.Cube, rightHand, new Vector3(0.065f, 0.08f, 0.28f));
            gunBody.transform.localPosition = new Vector3(0.02f, -0.03f, 0.16f);
        }

        private GameObject MakePrimitive(string objectName, PrimitiveType type, Transform parent, Vector3 scale)
        {
            GameObject item = GameObject.CreatePrimitive(type);
            item.name = objectName;
            item.transform.SetParent(parent, false);
            item.transform.localScale = scale;

            Collider collider = item.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = bodyMaterial;
            return item;
        }

        private void RefreshPet()
        {
            if (pet != null) Destroy(pet);
            if (petMaterial != null) Destroy(petMaterial);
            pet = null;
            petMaterial = null;

            if (petIndex == 0) return;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            pet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pet.name = "Remote Pet";
            pet.transform.localScale = Vector3.one * 0.2f;
            Collider collider = pet.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            petMaterial = new Material(shader) { name = "Remote Pet Material " + ClientId };
            petMaterial.color = PetColor(petIndex);
            pet.GetComponent<Renderer>().sharedMaterial = petMaterial;
        }

        private static Color PetColor(byte index)
        {
            switch (index)
            {
                case 1: return new Color(0.75f, 0.84f, 0.80f);
                case 2: return new Color(0.25f, 0.75f, 0.95f);
                case 3: return new Color(0.12f, 0.12f, 0.16f);
                case 4: return new Color(0.38f, 0.85f, 0.48f);
                case 5: return new Color(0.48f, 0.42f, 0.78f);
                case 6: return new Color(0.18f, 0.42f, 0.58f);
                default: return Color.gray;
            }
        }

        private void OnDestroy()
        {
            if (bodyMaterial != null) Destroy(bodyMaterial);
            if (petMaterial != null) Destroy(petMaterial);
            if (pet != null) Destroy(pet);
        }
    }
}
