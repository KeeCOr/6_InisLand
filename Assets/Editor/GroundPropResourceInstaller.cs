using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class GroundPropResourceInstaller
    {
        [Serializable] private sealed class Entry {public string path;public float ppu,pivot_y;}
        [Serializable] private sealed class Manifest {public Entry[] assets;}
        public static void Install()
        {
            AssetDatabase.Refresh();
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/art/generated/environment/ground_props20261103/generation_manifest.json"));
            foreach(var entry in manifest.assets)
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(entry.path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.npotScale=TextureImporterNPOTScale.None;importer.spritePixelsPerUnit=entry.ppu;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
                importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=256;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
                settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(.5f,entry.pivot_y);
                settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();Debug.Log("Ground resource props installed: 6 discovery sites and 6 pickups.");
        }
    }
}
