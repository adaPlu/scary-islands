using UnityEngine;

namespace ScaryIslands.World
{
    [CreateAssetMenu(menuName = "Scary Islands/Biome Definition")]
    public sealed class BiomeDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string premise;
        public string relic;
        public string threat;
        public string hazard;
        public string traversal;
        public Color fogColor = Color.gray;
        public Color accentColor = Color.red;
        [Min(5)] public int targetMinutes = 15;
        [Min(0)] public int relicsRequired;
    }
}

