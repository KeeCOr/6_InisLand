using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class CharacterActionInstaller
    {
        [Serializable] private class Manifest {public Group[] groups;}
        [Serializable] private class Group {public string key,path;public float ppu;public Frame[] frames;}
        [Serializable] private class Frame {public string name;public Rect rect;public Vector2 pivot;}
        public static void Install()
        {
            AssetDatabase.Refresh();
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/art/generated/characters/actions20261103/generation_manifest.json"));
            foreach(var group in manifest.groups)
            {
                var imp=(TextureImporter)AssetImporter.GetAtPath(group.path);imp.textureType=TextureImporterType.Sprite;
                imp.spriteImportMode=SpriteImportMode.Multiple;imp.spritePixelsPerUnit=group.ppu;
                imp.filterMode=FilterMode.Bilinear;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;
                imp.wrapMode=TextureWrapMode.Clamp;imp.npotScale=TextureImporterNPOTScale.None;
                imp.maxTextureSize=4096;imp.textureCompression=TextureImporterCompression.Uncompressed;
                var frames=new SpriteMetaData[group.frames.Length];
                for(int i=0;i<frames.Length;i++)frames[i]=new SpriteMetaData{name=group.frames[i].name,rect=group.frames[i].rect,pivot=group.frames[i].pivot,alignment=(int)SpriteAlignment.Custom};
                imp.spritesheet=frames;imp.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
