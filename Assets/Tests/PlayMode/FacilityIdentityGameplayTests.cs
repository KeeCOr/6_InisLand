using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class FacilityIdentityGameplayTests
    {
        [UnityTest] public IEnumerator AllNineFactoriesKeepTheirDedicatedArtAfterRuntimeInitialization()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var kinds=new[]{BuildingKind.SeedStorage,BuildingKind.Carpenter,BuildingKind.Blacksmith,
                BuildingKind.TrainingCamp,BuildingKind.FoodStorage,BuildingKind.LookoutPost,
                BuildingKind.Sawmill,BuildingKind.Church,BuildingKind.Brazier};
            var buildings=new List<GameObject>();
            for(int i=0;i<kinds.Length;i++)
            {
                var method=typeof(BuildingFactory).GetMethod("Spawn"+kinds[i],BindingFlags.Static|BindingFlags.Public);
                buildings.Add((GameObject)method.Invoke(null,new object[]{new Vector3(4000+i*10,4000,0)}));
            }
            yield return null;yield return null;
            for(int i=0;i<kinds.Length;i++)
            {
                var sr=buildings[i].GetComponent<SpriteRenderer>();
                Assert.That(sr.sprite,Is.SameAs(ConstructionPlacement.Sprite(kinds[i])),kinds[i]+" must match preview/catalog and avoid the old cabin/log fallback");
                Assert.That(buildings[i].transform.localScale,Is.EqualTo(ConstructionPlacement.Scale(kinds[i])));
                if(buildings[i].GetComponent<CampfireAura>()!=null)
                {
                    // A heat building's existing fire flicker changes brightness, not hue.
                    Assert.That(sr.color.r,Is.EqualTo(sr.color.g).Within(.001f));
                    Assert.That(sr.color.g,Is.EqualTo(sr.color.b).Within(.001f));
                    Assert.That(sr.color.a,Is.EqualTo(1f));
                }
                else Assert.That(sr.color,Is.EqualTo(Color.white),"Keep the generated material colors");
                Object.Destroy(buildings[i]);
            }
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
