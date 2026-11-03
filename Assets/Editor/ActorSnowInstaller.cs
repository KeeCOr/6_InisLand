using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class ActorSnowInstaller
    {
        [System.Serializable] private class ShadowRecord{public string runtime;public float ppu;}
        public static void Install()
        {
            AssetDatabase.Refresh();
            var shader=Shader.Find("InisLand/SnowActor");if(shader==null)throw new System.InvalidOperationException("Missing SnowActor shader");
            const string path="Assets/Resources/Materials/SnowActor.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}else material.shader=shader;
            material.SetFloat("_Saturation",.84f);material.SetFloat("_Contrast",.94f);material.SetFloat("_Matte",.1f);
            material.SetColor("_SnowTint",new Color(.96f,.98f,1f,1f));EditorUtility.SetDirty(material);
            foreach(string root in new[]{"Characters","Player/RedHairGirl","Animals","GeneratedStatic20261014/Animals","GeneratedStatic20261014/Companions","Companions"})
            {
                string folder="Assets/Resources/Sprites/"+root;if(!Directory.Exists(folder))continue;
                foreach(string file in Directory.GetFiles(folder,"*.png",SearchOption.AllDirectories))
                {
                    string asset=file.Replace('\\','/');var imp=(TextureImporter)AssetImporter.GetAtPath(asset);
                    if(imp!=null && imp.filterMode!=FilterMode.Bilinear){imp.filterMode=FilterMode.Bilinear;imp.SaveAndReimport();}
                }
            }
            var shadow=JsonUtility.FromJson<ShadowRecord>(File.ReadAllText("Assets/art/generated/environment/actor_snow20261103/generation_manifest.json"));
            var importer=(TextureImporter)AssetImporter.GetAtPath(shadow.runtime);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=shadow.ppu;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;
            importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=4096;importer.SaveAndReimport();
            AssetDatabase.SaveAssets();
        }
    }
}
