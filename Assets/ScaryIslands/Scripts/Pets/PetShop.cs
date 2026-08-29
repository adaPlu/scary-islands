using System;
using UnityEngine;
using ScaryIslands.Economy;

namespace ScaryIslands.Pets
{
    [Serializable]
    public sealed class PetDefinition
    {
        public string id;
        public string name;
        public string species;
        public string description;
        public int costDots;
    }

    [Serializable]
    internal sealed class PetDefinitionList
    {
        public PetDefinition[] pets;
    }

    /// <summary>Persistent pet catalog, purchasing, ownership, and equip state.</summary>
    [DefaultExecutionOrder(-100)]
    public sealed class PetShop : MonoBehaviour
    {
        private const string OwnedPrefix = "scary_islands_pet_owned_";
        private const string EquippedKey = "scary_islands_pet_equipped";

        public static PetShop Instance { get; private set; }
        public PetDefinition[] Pets { get; private set; } = Array.Empty<PetDefinition>();
        public string EquippedPetId { get; private set; } = string.Empty;

        public event Action Changed;
        public event Action<string> EquippedPetChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            GameObject service = new GameObject("Pet Shop Service");
            service.AddComponent<PetShop>();
            service.AddComponent<PetCompanionSpawner>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCatalog();
            EquippedPetId = PlayerPrefs.GetString(EquippedKey, string.Empty);
            if (!string.IsNullOrEmpty(EquippedPetId) && !IsOwned(EquippedPetId))
                EquippedPetId = string.Empty;
        }

        public PetDefinition GetPet(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            foreach (PetDefinition pet in Pets)
                if (pet != null && pet.id == id) return pet;
            return null;
        }

        public bool IsOwned(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return PlayerPrefs.GetInt(OwnedPrefix + id, 0) == 1;
        }

        public bool BuyPet(string id)
        {
            PetDefinition pet = GetPet(id);
            if (pet == null || IsOwned(id) || DotWallet.Instance == null) return false;
            if (!DotWallet.Instance.TrySpendDots(Mathf.Max(0, pet.costDots))) return false;

            PlayerPrefs.SetInt(OwnedPrefix + id, 1);
            PlayerPrefs.Save();
            Changed?.Invoke();
            return true;
        }

        public bool EquipPet(string id)
        {
            if (!IsOwned(id)) return false;
            EquippedPetId = id;
            PlayerPrefs.SetString(EquippedKey, id);
            PlayerPrefs.Save();
            Changed?.Invoke();
            EquippedPetChanged?.Invoke(id);
            return true;
        }

        public void UnequipPet()
        {
            EquippedPetId = string.Empty;
            PlayerPrefs.DeleteKey(EquippedKey);
            PlayerPrefs.Save();
            Changed?.Invoke();
            EquippedPetChanged?.Invoke(string.Empty);
        }

        private void LoadCatalog()
        {
            TextAsset asset = Resources.Load<TextAsset>("Pets");
            if (asset == null)
            {
                Debug.LogError("Missing Resources/Pets.json");
                Pets = Array.Empty<PetDefinition>();
                return;
            }

            PetDefinitionList parsed = JsonUtility.FromJson<PetDefinitionList>(asset.text);
            Pets = parsed?.pets ?? Array.Empty<PetDefinition>();
        }
    }
}
