using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class CampfireMeltVisualVerification
    {
        public static void Verify()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Snow melt QA camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=3.5f;
            camera.transform.position=new Vector3(0,.3f,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.9f,.94f,.98f);
            var ground=new GameObject("Snow ground").AddComponent<SpriteRenderer>();ground.sprite=Resources.Load<Sprite>("Sprites/Environment/Snowfield/snow_ground");
            ground.drawMode=SpriteDrawMode.Tiled;ground.size=new Vector2(20,12);ground.sortingOrder=-10000;
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            for(int i=0;i<3;i++)
            {
                var root=new GameObject("Heat state "+i);root.transform.position=new Vector3((i-1)*5,0,0);
                var heat=root.AddComponent<CampfireAura>();root.AddComponent<Building>().Initialize(BuildingKind.Campfire);
                typeof(CampfireAura).GetMethod("Start",flags).Invoke(heat,null);
                var melt=root.GetComponent<CampfireSnowMelt>();melt.UpdateVisual(0);
                if(i>0){heat.Fuel=0;melt.UpdateVisual(i==1?4:8);}
                typeof(CampfireAura).GetMethod("Update",flags).Invoke(heat,null);
                if(i==0)
                {
                    var fire=new GameObject("Burning fire art").AddComponent<SpriteRenderer>();fire.sprite=SpriteBank.Campfire();fire.sortingOrder=10;
                    fire.transform.position=root.transform.position;float scale=1.05f/fire.sprite.bounds.size.y;fire.transform.localScale=Vector3.one*scale;
                }
                var actor=new GameObject("Hero on terrain").AddComponent<SpriteRenderer>();actor.sprite=DirectionalCharacterVisual.DirectionSprite("hero_axe",CharacterFacing.Front);
                actor.transform.position=root.transform.position+new Vector3(-1.1f,-.8f,0);actor.transform.localScale=new Vector3(.9f,1,1);actor.sortingOrder=20;
                var snow=ActorSnowPresentation.Attach(actor);typeof(ActorSnowPresentation).GetMethod("LateUpdate",flags).Invoke(snow,null);
            }
            var target=new RenderTexture(1500,700,24);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var image=new Texture2D(1500,700,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,1500,700),0,0);image.Apply();
            File.WriteAllBytes("D:/codex_temp/InisLand-20261103/campfire-melt-gpu-temp.png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(image);Object.DestroyImmediate(target);
            Debug.Log("SNOW MELT GPU: actual active / half-cooled / refrozen ground with heroine and real terrain/shadow assets");
        }
    }
}
