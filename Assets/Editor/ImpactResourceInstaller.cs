using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class ImpactResourceInstaller
    {
        [Serializable] private class Manifest {public Entry[] entries;}
        [Serializable] private class Entry {public string key,path;public float ppu;}
        public static void Install()
        {
            AssetDatabase.Refresh();
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/art/generated/combat/impact_wreckage20261103/generation_manifest.json"));
            foreach(var e in manifest.entries)
            {
                var imp=(TextureImporter)AssetImporter.GetAtPath(e.path);imp.textureType=TextureImporterType.Sprite;
                imp.spriteImportMode=e.key=="impact"?SpriteImportMode.Multiple:SpriteImportMode.Single;
                imp.spritePixelsPerUnit=e.ppu;imp.spritePivot=new Vector2(.5f,.5f);
                imp.filterMode=FilterMode.Bilinear;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;
                imp.wrapMode=TextureWrapMode.Clamp;imp.npotScale=TextureImporterNPOTScale.None;
                imp.maxTextureSize=4096;imp.textureCompression=TextureImporterCompression.Uncompressed;
                if(e.key=="impact")
                {
                    var tex=new Texture2D(2,2);tex.LoadImage(File.ReadAllBytes(e.path));
                    var frames=new SpriteMetaData[3];float width=tex.width/3f;
                    for(int i=0;i<3;i++)frames[i]=new SpriteMetaData{name="impact_"+i,rect=new Rect(i*width,0,width,tex.height),alignment=(int)SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)};
                    imp.spritesheet=frames;UnityEngine.Object.DestroyImmediate(tex);
                }
                imp.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
