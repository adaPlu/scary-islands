using UnityEngine;

namespace ScaryIslands.Economy
{
    /// <summary>Ensures a CosmeticShop can resolve the scene DotWallet without manual inspector wiring.</summary>
    public sealed class CosmeticShopAutoBinder : MonoBehaviour
    {
        [SerializeField] private CosmeticShop shop;

        private void Awake()
        {
            if (shop == null)
                shop = GetComponent<CosmeticShop>();
        }
    }
}
