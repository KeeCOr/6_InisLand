using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class DirectionalCharacterInstaller
    {
        private const string Root="Assets/Resources/Sprites/Characters/Directions20261026";
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach(string key in new[]{"hero_axe","hero_bow","hero_hammer","hunter","warrior","farmer","cook","healer","aunt","uncle","child"})
            {
                string path=Root+"/"+key+".png";
                var image=new Texture2D(2,2,TextureFormat.RGBA32,false);
                if(!image.LoadImage(File.ReadAllBytes(path))) throw new InvalidDataException(path);
                var pixels=image.GetPixels32();var slices=new SpriteMetaData[2];
                Sprite reference=key.StartsWith("hero")?SpriteBank.PlayerIdle():SpriteBank.CompanionSpriteForRole(key);
                float height=key.StartsWith("hero")?1.08f:reference.bounds.size.y;
                for(int i=0;i<2;i++)
                {
                    int left=i*image.width/2,right=(i+1)*image.width/2;
                    int minX=right,minY=image.height,maxX=-1,maxY=-1;
                    for(int y=0;y<image.height;y++) for(int x=left;x<right;x++)
                        if(pixels[y*image.width+x].a>127)
                        {minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);}
                    if(maxX<0) throw new InvalidDataException("Empty direction: "+key);
                    // Full equal-height cells retain consistent PPU and feet level in both directions.
                    slices[i]=new SpriteMetaData {name=i==0?"front":"back",rect=new Rect(left,0,right-left,image.height),
                        alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(.5f,(float)minY/image.height)};
                }
                // Use each pair's tallest opaque silhouette for equal scale across views.
                int top=0,bottom=image.height;
                for(int y=0;y<image.height;y++) for(int x=0;x<image.width;x++)
                    if(pixels[y*image.width+x].a>127){top=Math.Max(top,y);bottom=Math.Min(bottom,y);}
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
                importer.spritePixelsPerUnit=(top-bottom+1)/height;
                importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;
                importer.wrapMode=TextureWrapMode.Clamp;importer.spritesheet=slices;importer.SaveAndReimport();
                UnityEngine.Object.DestroyImmediate(image);
            }
            AssetDatabase.SaveAssets();Debug.Log("Directional character sheets installed: 11 pairs / 22 views.");
        }
    }
}
