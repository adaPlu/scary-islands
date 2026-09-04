using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScaryIslands.Economy
{
    [Serializable]
    public sealed class CosmeticDefinition
    {
        public string id;
        public string name;
        public string slot;
        public string rarity;
        public int minDots;
        public int maxDots;
    }

    [Serializable]
    internal sealed class CosmeticCatalogData
    {
        public CosmeticDefinition[] cosmetics;
    }

    public sealed class CosmeticShop : MonoBehaviour
    {
        private const string Prefix = "scaryislands.cosmetics.";

        [SerializeField] private DotWallet wallet;
        [SerializeField] private string resourceName = "Cosmetics";
        [SerializeField, Min(1f)] private float priceRefreshHours = 24f;

        private readonly List<CosmeticDefinition> cosmetics = new();
        private readonly Dictionary<string, int> prices = new();

        public IReadOnlyList<CosmeticDefinition> Cosmetics => cosmetics;

        private void Awake()
        {
            LoadCatalog();
            RefreshPricesIfNeeded();
        }

        public int GetPrice(string cosmeticId)
        {
            if (string.IsNullOrWhiteSpace(cosmeticId)) return 0;
            return prices.TryGetValue(cosmeticId, out int price) ? price : 0;
        }

        public bool IsOwned(string cosmeticId)
        {
            return PlayerPrefs.GetInt(Prefix + "owned." + cosmeticId, 0) == 1;
        }

        public string GetEquipped(string slot)
        {
            return PlayerPrefs.GetString(Prefix + "equipped." + slot, string.Empty);
        }

        public bool TryPurchase(string cosmeticId)
        {
            CosmeticDefinition cosmetic = cosmetics.Find(c => c.id == cosmeticId);
            if (cosmetic == null || IsOwned(cosmeticId) || wallet == null) return false;

            int price = GetPrice(cosmeticId);
            if (price <= 0 || !wallet.TrySpend(price)) return false;

            PlayerPrefs.SetInt(Prefix + "owned." + cosmeticId, 1);
            PlayerPrefs.Save();
            return true;
        }

        public bool TryEquip(string cosmeticId)
        {
            CosmeticDefinition cosmetic = cosmetics.Find(c => c.id == cosmeticId);
            if (cosmetic == null || !IsOwned(cosmeticId)) return false;

            PlayerPrefs.SetString(Prefix + "equipped." + cosmetic.slot, cosmetic.id);
            PlayerPrefs.Save();
            return true;
        }

        public void RefreshPricesNow()
        {
            GeneratePrices();
            PlayerPrefs.SetString(Prefix + "priceEpoch", DateTime.UtcNow.ToString("O"));
            PlayerPrefs.Save();
        }

        private void RefreshPricesIfNeeded()
        {
            string raw = PlayerPrefs.GetString(Prefix + "priceEpoch", string.Empty);
            bool stale = !DateTime.TryParse(raw, out DateTime epoch) ||
                         (DateTime.UtcNow - epoch.ToUniversalTime()).TotalHours >= priceRefreshHours;

            if (stale)
            {
                RefreshPricesNow();
                return;
            }

            bool missingAny = false;
            foreach (CosmeticDefinition cosmetic in cosmetics)
            {
                string key = Prefix + "price." + cosmetic.id;
                if (!PlayerPrefs.HasKey(key))
                {
                    missingAny = true;
                    break;
                }
                prices[cosmetic.id] = PlayerPrefs.GetInt(key);
            }

            if (missingAny) RefreshPricesNow();
        }

        private void GeneratePrices()
        {
            prices.Clear();
            foreach (CosmeticDefinition cosmetic in cosmetics)
            {
                int min = Mathf.Max(1, cosmetic.minDots);
                int max = Mathf.Max(min, cosmetic.maxDots);
                int price = UnityEngine.Random.Range(min, max + 1);
                prices[cosmetic.id] = price;
                PlayerPrefs.SetInt(Prefix + "price." + cosmetic.id, price);
            }
        }

        private void LoadCatalog()
        {
            TextAsset asset = Resources.Load<TextAsset>(resourceName);
            if (asset == null) return;

            CosmeticCatalogData data = JsonUtility.FromJson<CosmeticCatalogData>(asset.text);
            cosmetics.Clear();
            if (data?.cosmetics != null)
                cosmetics.AddRange(data.cosmetics);
        }
    }
}
