using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class WalkCharacterInstaller
    {
        private const string Source="Assets/art/generated/characters/walk20261026";
        private const string Target="Assets/Resources/Sprites/Characters/Walk20261026";
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach(string key in new[]{"hero_axe","hero_bow","hero_hammer","hunter","warrior","farmer","cook","healer","aunt","uncle","child"})
            {
                var image=new Texture2D(2,2,TextureFormat.RGBA32,false);
                if(!image.LoadImage(File.ReadAllBytes(Source+"/"+key+".png"))) throw new InvalidDataException(key);
                Texture2D extra=null;
                if(File.Exists(Source+"/"+key+"_step_b.png"))
                {extra=new Texture2D(2,2,TextureFormat.RGBA32,false);extra.LoadImage(File.ReadAllBytes(Source+"/"+key+"_step_b.png"));}
                float targetHeight=key.StartsWith("hero")?1.08f:SpriteBank.CompanionSpriteForRole(key).bounds.size.y;
                Directory.CreateDirectory(Target+"/"+key);
                for(int column=0;column<3;column++)
                {
                    var frames=new Texture2D[2];var bounds=new RectInt[2];
                    for(int row=0;row<2;row++)
                    {
                        var source=row==1 && column<2 && extra!=null?extra:image;
                        bool replacement=source==extra;
                        int columns=replacement?2:3;
                        int x=column*source.width/columns,right=(column+1)*source.width/columns;
                        int y=replacement?0:(1-row)*source.height/2,top=replacement?source.height:(2-row)*source.height/2;
                        var frame=new Texture2D(right-x,top-y,TextureFormat.RGBA32,false);
                        frame.SetPixels(source.GetPixels(x,y,right-x,top-y));frame.Apply();frames[row]=frame;
                        bounds[row]=OpaqueBounds(frame);
                    }
                    string direction=column==0?"front":column==1?"back":"side";
                    for(int row=0;row<2;row++)
                    {
                        string path=Target+"/"+key+"/walk_"+direction+"_"+row+".png";
                        File.WriteAllBytes(path,frames[row].EncodeToPNG());AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                        importer.spritePixelsPerUnit=bounds[row].height/targetHeight;importer.spritePivot=new Vector2(.5f,(float)bounds[row].yMin/frames[row].height);
                        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
                        settings.spriteAlignment=(int)SpriteAlignment.Custom;importer.SetTextureSettings(settings);
                        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
                        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;
                        importer.maxTextureSize=2048;importer.SaveAndReimport();UnityEngine.Object.DestroyImmediate(frames[row]);
                    }
                }
                UnityEngine.Object.DestroyImmediate(image);
                if(extra!=null) UnityEngine.Object.DestroyImmediate(extra);
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("Walk sprites installed: 66 frames, paired scale and feet pivots.");
        }
        private static RectInt OpaqueBounds(Texture2D frame)
        {
            var pixels=frame.GetPixels32();int x0=frame.width,y0=frame.height,x1=-1,y1=-1;
            for(int y=0;y<frame.height;y++) for(int x=0;x<frame.width;x++) if(pixels[y*frame.width+x].a>127)
            {x0=Math.Min(x0,x);y0=Math.Min(y0,y);x1=Math.Max(x1,x);y1=Math.Max(y1,y);}
            if(x1<0) throw new InvalidDataException("Empty walking frame");return new RectInt(x0,y0,x1-x0+1,y1-y0+1);
        }
    }
}
