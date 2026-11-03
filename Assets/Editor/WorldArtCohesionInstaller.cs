using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IL6.EditorBuild
{
    public static class WorldArtCohesionInstaller
    {
        public const string ManifestPath="Assets/art/generated/environment/art_cohesion20261103/generation_manifest.json";
        [Serializable] public class Manifest { public Entry[] entries; public Atlas[] modular_atlases; public Piece[] modular_entries; }
        [Serializable] public class Atlas { public string path; }
        [Serializable] public class Piece { public string key,path; }
        [Serializable] public class Entry { public string key,path; public float ppu,painted_width,foot_y; public Vector2 pivot; }
        public static Manifest ReadManifest()=>JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach(var e in ReadManifest().entries)
            {
                var imp=AssetImporter.GetAtPath(e.path) as TextureImporter;
                if(imp==null)throw new InvalidOperationException("Missing cohesive world art: "+e.path);
                imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;
                imp.spritePixelsPerUnit=e.ppu;
                var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);
                settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spriteMeshType=SpriteMeshType.Tight;
                imp.SetTextureSettings(settings);imp.spritePivot=e.pivot;
                imp.filterMode=FilterMode.Bilinear;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;
                imp.wrapMode=TextureWrapMode.Clamp;imp.npotScale=TextureImporterNPOTScale.None;
                imp.maxTextureSize=4096;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.isReadable=true;
                foreach(var platform in new[]{"Standalone","Android","iPhone","WebGL"})
                {var p=imp.GetPlatformTextureSettings(platform);p.overridden=false;imp.SetPlatformTextureSettings(p);}
                imp.SaveAndReimport();
            }
            if(ReadManifest().modular_atlases!=null)foreach(var e in ReadManifest().modular_atlases)
            {
                var imp=AssetImporter.GetAtPath(e.path) as TextureImporter;
                if(imp==null)throw new InvalidOperationException("Missing modular world art: "+e.path);
                imp.filterMode=FilterMode.Bilinear;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;
                imp.wrapMode=TextureWrapMode.Clamp;imp.npotScale=TextureImporterNPOTScale.None;
                imp.maxTextureSize=4096;imp.textureCompression=TextureImporterCompression.Uncompressed;
                foreach(var platform in new[]{"Standalone","Android","iPhone","WebGL"})
                {var p=imp.GetPlatformTextureSettings(platform);p.overridden=false;imp.SetPlatformTextureSettings(p);}
                imp.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
