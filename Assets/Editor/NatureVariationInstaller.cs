using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class NatureVariationInstaller
    {
        [Serializable] private sealed class Entry {public string group,path;public float ppu,pivot_y;}
        [Serializable] private sealed class Manifest {public Entry[] assets;}
        public static void Install()
        {
            AssetDatabase.Refresh();
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/art/generated/environment/nature_variants20261103/generation_manifest.json"));
            foreach(var entry in manifest.assets)
            {
                if(entry.group!="rocks")continue;
                var importer=(TextureImporter)AssetImporter.GetAtPath(entry.path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=entry.ppu;importer.npotScale=TextureImporterNPOTScale.None;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
                importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=1024;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
                settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(.5f,entry.pivot_y);
                settings.spriteMeshType=SpriteMeshType.Tight;importer.SetTextureSettings(settings);importer.SaveAndReimport();
            }
            NordicTreeInstaller.Install();
            AssetDatabase.SaveAssets();Debug.Log("Nature variants imported: six trees, six non-slab rocks; physics roots retained.");
        }
    }
}
