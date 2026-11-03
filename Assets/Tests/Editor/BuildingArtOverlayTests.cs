using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class BuildingArtOverlayTests
    {
        [Test] public void SavedBuildingPrefabsAlsoExcludeRoleOverlays()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Prefabs/Buildings"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab.transform.Find("RoleMarker"),Is.Null,path);
            }
        }
        [Test] public void BuildingsDoNotOverlayHudRoleIconsOnTheirArtwork()
        {
            int checkedCount=0;
            foreach(var method in typeof(BuildingFactory).GetMethods(BindingFlags.Public|BindingFlags.Static))
            {
                if(method.ReturnType!=typeof(GameObject))continue;
                var args=method.GetParameters();if(args.Length!=1 || args[0].ParameterType!=typeof(Vector3))continue;
                var building=(GameObject)method.Invoke(null,new object[]{Vector3.zero});
                try {Assert.That(building.transform.Find("RoleMarker"),Is.Null,method.Name+" must show only its own building artwork");checkedCount++;}
                finally {Object.DestroyImmediate(building);}
            }
            Assert.That(checkedCount,Is.GreaterThanOrEqualTo(15));
            Assert.That(SpriteBank.HomeIcon(),Is.Not.Null,"The house icon remains available to menus and HUD");
        }
    }
}
