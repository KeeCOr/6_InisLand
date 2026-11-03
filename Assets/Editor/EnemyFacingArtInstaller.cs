using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class EnemyFacingArtInstaller
    {
        [Serializable] private class Frame { public string path;public float ppu,pivotX,pivotY; }
        [Serializable] private class Manifest { public Frame[] frames; }
        public static void Install()
        {
            AssetDatabase.Refresh();
            var data=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/art/generated/enemies/directions20261103/generation_manifest.json"));
            foreach(var frame in data.frames)
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(frame.path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=frame.ppu;importer.spritePivot=new Vector2(frame.pivotX,frame.pivotY);
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=1024;
                importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
