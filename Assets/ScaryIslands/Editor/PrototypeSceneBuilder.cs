#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ScaryIslands.Game;
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
            var light = new GameObject("Moonlight").AddComponent<Light>(); light.type = LightType.Directional; light.intensity = .35f; light.color = new Color(.55f,.7f,.66f);
            for (int x=-4;x<=4;x++) for(int z=-4;z<=4;z++)
            {
                var tile=GameObject.CreatePrimitive(PrimitiveType.Cube); tile.name="Island Ground"; tile.transform.position=new Vector3(x*4,-.6f,z*4); tile.transform.localScale=new Vector3(4,.8f,4);
            }
            var tide=GameObject.CreatePrimitive(PrimitiveType.Cube); tide.name="Black Tide"; tide.transform.localScale=new Vector3(80,.25f,80); tide.AddComponent<BlackTide>();
            var spawn=new GameObject("XR Origin Spawn"); spawn.transform.position=new Vector3(0,1, -12);
            var key=GameObject.CreatePrimitive(PrimitiveType.Cylinder); key.name="Chapel Key"; key.transform.position=new Vector3(-9,1,3); key.AddComponent<ObjectivePickup>();
            var bell=GameObject.CreatePrimitive(PrimitiveType.Sphere); bell.name="Salt Bell"; bell.transform.position=new Vector3(10,1,8); bell.AddComponent<ObjectivePickup>();
            EditorSceneManager.SaveScene(scene,"Assets/ScaryIslands/Scenes/WidowsShore.unity");
            Debug.Log("Scary Islands prototype scene created. Add an XR Origin prefab from XR Interaction Toolkit samples at XR Origin Spawn.");
        }
    }
}
#endif
