#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ScaryIslands.Combat;
using ScaryIslands.Economy;
using ScaryIslands.Game;
using ScaryIslands.Multiplayer;
using ScaryIslands.Pets;
using ScaryIslands.UI;
using ScaryIslands.World;

namespace ScaryIslands.Editor
{
    public static class PrototypeSceneBuilder
    {
        [MenuItem("Scary Islands/Build Prototype Scene")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScaryIslands/Scenes"))
                AssetDatabase.CreateFolder("Assets/ScaryIslands", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Run State").AddComponent<RunState>();
            new GameObject("Settings System").AddComponent<SettingsMenu>();

            var walletObject = new GameObject("Dot Wallet");
            walletObject.AddComponent<DotWallet>();

            var cosmeticShop = new GameObject("Cosmetic Shop");
            cosmeticShop.transform.position = new Vector3(8, 0, -8);
            cosmeticShop.AddComponent<CosmeticShop>();

            var light = new GameObject("Moonlight").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = .35f;
            light.color = new Color(.55f, .7f, .66f);

            for (int x = -4; x <= 4; x++)
            for (int z = -4; z <= 4; z++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = "Island Ground";
                tile.transform.position = new Vector3(x * 4, -.6f, z * 4);
                tile.transform.localScale = new Vector3(4, .8f, 4);
            }

            var tide = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tide.name = "Black Tide";
            tide.transform.localScale = new Vector3(80, .25f, 80);
            tide.AddComponent<BlackTide>();

            var spawn = new GameObject("XR Origin Spawn");
            spawn.transform.position = new Vector3(0, 1, -12);

            var multiplayer = new GameObject("Multiplayer Terminal");
            multiplayer.transform.position = new Vector3(-4, 0, -9);
            multiplayer.transform.rotation = Quaternion.Euler(0, 160, 0);
            multiplayer.AddComponent<MultiplayerTerminal>();

            var key = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            key.name = "Chapel Key";
            key.transform.position = new Vector3(-9, 1, 3);
            key.AddComponent<ObjectivePickup>();

            var bell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bell.name = "Salt Bell";
            bell.transform.position = new Vector3(10, 1, 8);
            bell.AddComponent<ObjectivePickup>();

            var petShop = new GameObject("Pet Shop");
            petShop.transform.position = new Vector3(5, 0, -8);
            petShop.transform.rotation = Quaternion.Euler(0, 205, 0);
            petShop.AddComponent<PetShopTerminal>();

            Vector3[] monsterPositions =
            {
                new Vector3(-6f, 1f, -2f),
                new Vector3(4f, 1f, 5f),
                new Vector3(11f, 1f, -1f)
            };

            for (int i = 0; i < monsterPositions.Length; i++)
            {
                GameObject monster = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                monster.name = "Shootable Monster " + (i + 1);
                monster.transform.position = monsterPositions[i];
                monster.transform.localScale = new Vector3(1.1f, 1.6f, 1.1f);
                monster.AddComponent<MonsterHealth>();
            }

            EditorSceneManager.SaveScene(scene, "Assets/ScaryIslands/Scenes/WidowsShore.unity");
            Debug.Log("Scary Islands prototype created with multiplayer, wings, guns, Dots, pets, settings, and randomized cosmetic shop pricing.");
        }
    }
}
#endif
