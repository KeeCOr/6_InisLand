using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class CharacterActionInstaller
    {
        [Serializable] private class Manifest {public Group[] groups;}
        [Serializable] private class Group {public string key,path;public float ppu,referenceHeight;public Frame[] frames;}
        // JsonUtility does not populate Unity Rect's x/y/width/height properties from JSON.
        [Serializable] private class FrameRect {public float x,y,width,height;public Rect Value=>new Rect(x,y,width,height);}
        [Serializable] private class Frame {public string name;public FrameRect rect;public Vector2 pivot;}
        public static void Install()
        {
            AssetDatabase.Refresh();
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/art/generated/characters/actions20261103/generation_manifest.json"));
            foreach(var group in manifest.groups)
            {
                var imp=(TextureImporter)AssetImporter.GetAtPath(group.path);imp.textureType=TextureImporterType.Sprite;
                imp.spriteImportMode=SpriteImportMode.Multiple;
                float sizeCorrection=group.key.StartsWith("hero")?1f:(group.referenceHeight>0?group.referenceHeight:1.2f)/SpriteBank.CompanionVisualHeightForRole(group.key);
                imp.spritePixelsPerUnit=group.ppu*sizeCorrection;
                imp.filterMode=FilterMode.Bilinear;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;
                imp.wrapMode=TextureWrapMode.Clamp;imp.npotScale=TextureImporterNPOTScale.None;
                imp.maxTextureSize=4096;imp.textureCompression=TextureImporterCompression.Uncompressed;
                var frames=new SpriteMetaData[group.frames.Length];
                for(int i=0;i<frames.Length;i++)
                {
                    var frame=group.frames[i];
                    if(frame.rect==null || frame.rect.width<=1 || frame.rect.height<=1)
                        throw new InvalidOperationException("Empty action frame: "+group.key+"/"+frame.name);
                    frames[i]=new SpriteMetaData{name=frame.name,rect=frame.rect.Value,pivot=frame.pivot,alignment=(int)SpriteAlignment.Custom};
                }
                imp.spritesheet=frames;imp.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
