using System;
using UnityEngine;

namespace ScaryIslands.World
{
    [Serializable] public sealed class BiomeRecord
    {
        public string id, name, relic, threat, hazard, traversal;
        public int targetMinutes, relicsRequired;
    }
    [Serializable] public sealed class BiomeRecordList { public BiomeRecord[] biomes; }

    public sealed class BiomeCatalog : MonoBehaviour
    {
        public BiomeRecord[] Biomes { get; private set; } = Array.Empty<BiomeRecord>();
        private void Awake()
        {
            var asset = Resources.Load<TextAsset>("Biomes");
            if (asset == null) { Debug.LogError("Missing Resources/Biomes.json"); return; }
            Biomes = JsonUtility.FromJson<BiomeRecordList>(asset.text)?.biomes ?? Array.Empty<BiomeRecord>();
        }
        public bool IsUnlocked(BiomeRecord biome, int collectedRelics) => biome != null && collectedRelics >= biome.relicsRequired;
    }
}

