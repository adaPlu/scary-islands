using UnityEngine;
using ScaryIslands.Economy;

namespace ScaryIslands.Pets
{
    /// <summary>
    /// World-space prototype pet shop. Public actions can be bound to XR interactables.
    /// </summary>
    public sealed class PetShopTerminal : MonoBehaviour
    {
        private TextMesh display;
        private int selectedIndex;

        private void Start()
        {
            BuildTerminal();
            if (PetShop.Instance != null)
                PetShop.Instance.Changed += Refresh;
            if (DotWallet.Instance != null)
                DotWallet.Instance.Changed += OnDotsChanged;
            Refresh();
        }

        private void OnDestroy()
        {
            if (PetShop.Instance != null)
                PetShop.Instance.Changed -= Refresh;
            if (DotWallet.Instance != null)
                DotWallet.Instance.Changed -= OnDotsChanged;
        }

        public void NextPet()
        {
            if (PetShop.Instance == null || PetShop.Instance.Pets.Length == 0) return;
            selectedIndex = (selectedIndex + 1) % PetShop.Instance.Pets.Length;
            Refresh();
        }

        public void PreviousPet()
        {
            if (PetShop.Instance == null || PetShop.Instance.Pets.Length == 0) return;
            selectedIndex = (selectedIndex - 1 + PetShop.Instance.Pets.Length) % PetShop.Instance.Pets.Length;
            Refresh();
        }

        public bool BuySelected()
        {
            PetDefinition pet = SelectedPet();
            bool bought = pet != null && PetShop.Instance.BuyPet(pet.id);
            Refresh();
            return bought;
        }

        public bool EquipSelected()
        {
            PetDefinition pet = SelectedPet();
            bool equipped = pet != null && PetShop.Instance.EquipPet(pet.id);
            Refresh();
            return equipped;
        }

        private PetDefinition SelectedPet()
        {
            if (PetShop.Instance == null || PetShop.Instance.Pets.Length == 0) return null;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, PetShop.Instance.Pets.Length - 1);
            return PetShop.Instance.Pets[selectedIndex];
        }

        private void BuildTerminal()
        {
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.name = "Pet Shop Pedestal";
            pedestal.transform.SetParent(transform, false);
            pedestal.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            pedestal.transform.localScale = new Vector3(1.8f, 1.3f, 0.35f);

            GameObject label = new GameObject("Pet Shop Display");
            label.transform.SetParent(transform, false);
            label.transform.localPosition = new Vector3(-0.78f, 1.25f, -0.19f);
            label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            display = label.AddComponent<TextMesh>();
            display.fontSize = 42;
            display.characterSize = 0.055f;
            display.anchor = TextAnchor.UpperLeft;
            display.alignment = TextAlignment.Left;
            display.color = new Color(0.85f, 0.92f, 0.88f);
        }

        private void OnDotsChanged(int _) => Refresh();

        private void Refresh()
        {
            if (display == null) return;

            PetDefinition pet = SelectedPet();
            int dots = DotWallet.Instance != null ? DotWallet.Instance.Balance : 0;
            if (pet == null)
            {
                display.text = "PET SHOP\nDOTS: " + dots + "\nNo pets available";
                return;
            }

            bool owned = PetShop.Instance.IsOwned(pet.id);
            bool equipped = PetShop.Instance.EquippedPetId == pet.id;
            string status = equipped ? "EQUIPPED" : owned ? "OWNED — EQUIP" : "BUY — " + pet.costDots + " DOTS";
            display.text =
                "PET SHOP\n" +
                "DOTS: " + dots + "\n\n" +
                pet.name.ToUpperInvariant() + "\n" +
                pet.species + "\n" +
                status + "\n\n" +
                "Use shop controls to browse";
        }
    }
}
