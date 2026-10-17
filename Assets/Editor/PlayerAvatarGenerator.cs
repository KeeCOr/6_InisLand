using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IL6.EditorBuild
{
    public static class PlayerAvatarGenerator
    {
        private const string Folder="Assets/Resources/Sprites/Player/RedHairGirl";
        public static void Generate()
        {
            Directory.CreateDirectory(Folder);
            foreach(var weapon in new[]{"axe","bow"})
            {
                string path=$"{Folder}/{weapon}.png";
                File.Copy($"Assets/art/Sprites/Player/player_survivor_{weapon}.png",path,true);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Multiple;
                importer.filterMode=FilterMode.Point;
                importer.mipmapEnabled=false;
                importer.alphaIsTransparency=true;
                importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.spritePixelsPerUnit=72;
                importer.wrapMode=TextureWrapMode.Clamp;
                var slices=new SpriteMetaData[16];
                for(int i=0;i<16;i++)
                    slices[i]=new SpriteMetaData {
                        name=$"redhair_{weapon}_{i}",rect=new Rect((i%4)*96,(3-i/4)*96,96,96),
                        alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(0.5f,0.1f)
                    };
                importer.spritesheet=slices;
                importer.SaveAndReimport();
                var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                var idle=MakeClip(weapon,"idle",0,6,sprites);
                var walk=MakeClip(weapon,"walk",4,8,sprites);
                var original=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Resources/Sprites/Player/Player.controller");
                string title=weapon=="axe"?"Axe":"Bow";
                string controllerPath=$"{Folder}/RedHairGirl{title}.overrideController";
                var controller=AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(controllerPath);
                if(controller==null)
                {
                    controller=new AnimatorOverrideController(original);
                    AssetDatabase.CreateAsset(controller,controllerPath);
                }
                controller.runtimeAnimatorController=original;
                controller["playerIdle"]=idle;
                controller["playerWalk"]=walk;
                EditorUtility.SetDirty(controller);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[PlayerAvatarGenerator] Red-haired girl axe/bow idle and walk connected.");
        }

        private static AnimationClip MakeClip(string weapon,string state,int first,float fps,Sprite[] sprites)
        {
            string path=$"{Folder}/{weapon}_{state}.anim";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if(clip==null) { clip=new AnimationClip(); AssetDatabase.CreateAsset(clip,path); }
            clip.name=$"redhair_{weapon}_{state}";
            clip.frameRate=fps;
            var frames=new ObjectReferenceKeyframe[5];
            for(int i=0;i<5;i++)
                frames[i]=new ObjectReferenceKeyframe {
                    time=i/fps,value=sprites.Single(s=>s.name==$"redhair_{weapon}_{first+i%4}")
                };
            var binding=new EditorCurveBinding {path="",type=typeof(SpriteRenderer),propertyName="m_Sprite"};
            AnimationUtility.SetObjectReferenceCurve(clip,binding,frames);
            var settings=AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime=true;
            AnimationUtility.SetAnimationClipSettings(clip,settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }
    }
}
