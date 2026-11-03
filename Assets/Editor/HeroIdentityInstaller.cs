using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class HeroIdentityInstaller
    {
        private const string Archive="Assets/art/generated/characters/wardrobe20261027/hero";
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach(string key in new[]{"hero_axe","hero_bow","hero_hammer"})
            {
                var atlas=new Texture2D(2,2,TextureFormat.RGBA32,false);
                if(!atlas.LoadImage(File.ReadAllBytes(Archive+"/"+key+".png"))) throw new InvalidDataException(key);
                var frames=WardrobeAtlasFrames.Load(atlas,Archive+"/"+key+".png");
                var bounds=new RectInt[9];int tallest=0;
                for(int i=0;i<frames.Length;i++) { bounds[i]=OpaqueBounds(frames[i]);tallest=Math.Max(tallest,bounds[i].height); }
                float ppu=tallest/1.08f;
                string idleRoot="Assets/Resources/Sprites/Characters/HeroIdentity20261026/"+key;
                Directory.CreateDirectory(idleRoot);
                string basePath=idleRoot+"/idle_side.png";
                SaveSingle(basePath,frames[2],bounds[2],ppu);
                // Preserve the existing front/back resource path and sprite names used at runtime.
                var pair=new Texture2D(frames[0].width*2,frames[0].height,TextureFormat.RGBA32,false);
                pair.SetPixels(0,0,frames[0].width,frames[0].height,frames[0].GetPixels());
                // Integer division may make the middle atlas cell one pixel wider.
                for(int y=0;y<pair.height;y++) for(int x=0;x<frames[0].width;x++) pair.SetPixel(x+frames[0].width,y,frames[1].GetPixel(x,y));
                pair.Apply();string pairPath="Assets/Resources/Sprites/Characters/Directions20261026/"+key+".png";
                File.WriteAllBytes(pairPath,pair.EncodeToPNG());AssetDatabase.ImportAsset(pairPath,ImportAssetOptions.ForceSynchronousImport);
                var imp=(TextureImporter)AssetImporter.GetAtPath(pairPath);Configure(imp,ppu);imp.spriteImportMode=SpriteImportMode.Multiple;
                imp.spritesheet=new[]{new SpriteMetaData{name="front",rect=new Rect(0,0,frames[0].width,pair.height),alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(.5f,(float)bounds[0].yMin/pair.height)},new SpriteMetaData{name="back",rect=new Rect(frames[0].width,0,frames[0].width,pair.height),alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(.5f,(float)bounds[1].yMin/pair.height)}};
                imp.SaveAndReimport();
                for(int row=1;row<3;row++) for(int col=0;col<3;col++)
                {
                    string direction=col==0?"front":col==1?"back":"side";int i=row*3+col;
                    SaveSingle("Assets/Resources/Sprites/Characters/Walk20261026/"+key+"/walk_"+direction+"_"+(row-1)+".png",frames[i],bounds[i],ppu);
                }
                foreach(var frame in frames) UnityEngine.Object.DestroyImmediate(frame);
                UnityEngine.Object.DestroyImmediate(pair);UnityEngine.Object.DestroyImmediate(atlas);
            }
            InstallActionSheet("axe");InstallActionSheet("logging");InstallActionSheet("bow");
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("Unified hero installed: 3 weapons, 27 locomotion views and small hatchet action sheets.");
        }
        private static void InstallActionSheet(string key)
        {
            string path="Assets/Resources/Sprites/Player/RedHairGirl/"+key+".png";
            var imp=(TextureImporter)AssetImporter.GetAtPath(path);var slices=imp.spritesheet;
            var source=new Texture2D(2,2,TextureFormat.RGBA32,false);source.LoadImage(File.ReadAllBytes(Archive+"/"+key+"_actions.png"));
            var rects=WardrobeAtlasFrames.Rects(source,Archive+"/"+key+"_actions.png");
            float ppu=WardrobeAtlasFrames.Density(Archive+"/"+key+"_actions.png");
            if(rects.Length!=slices.Length || ppu<=0f) throw new InvalidDataException(key+" action layout changed");
            var frames=new Texture2D[slices.Length];var bounds=new RectInt[slices.Length];int width=256,height=256;
            for(int i=0;i<slices.Length;i++)
            {
                var r=rects[i];var frame=new Texture2D((int)r.width,(int)r.height,TextureFormat.RGBA32,false);
                frame.SetPixels(source.GetPixels((int)r.x,(int)r.y,frame.width,frame.height));frame.Apply();
                frames[i]=frame;bounds[i]=OpaqueBounds(frame);
                width=Math.Max(width,bounds[i].width+16);height=Math.Max(height,bounds[i].height+16);
            }
            int rows=slices.Length/4;
            var packed=new Texture2D(width*4,height*rows,TextureFormat.RGBA32,false);
            packed.SetPixels32(new Color32[packed.width*packed.height]);
            for(int i=0;i<slices.Length;i++)
            {
                int x=(i%4)*width,y=(rows-1-i/4)*height;var b=bounds[i];
                packed.SetPixels(x+(width-b.width)/2,y+8,b.width,b.height,frames[i].GetPixels(b.x,b.y,b.width,b.height));
                slices[i].rect=new Rect(x,y,width,height);
                slices[i].pivot=new Vector2(.5f,8f/height);
            }
            packed.Apply();File.WriteAllBytes(path,packed.EncodeToPNG());AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            imp=(TextureImporter)AssetImporter.GetAtPath(path);Configure(imp,ppu);imp.spriteImportMode=SpriteImportMode.Multiple;
            imp.spritesheet=slices;imp.SaveAndReimport();
            foreach(var frame in frames)UnityEngine.Object.DestroyImmediate(frame);
            UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(packed);
        }

        private static void Configure(TextureImporter imp,float ppu)
        {
            imp.textureType=TextureImporterType.Sprite;imp.spritePixelsPerUnit=ppu;imp.alphaIsTransparency=true;
            imp.mipmapEnabled=false;imp.filterMode=FilterMode.Bilinear;imp.wrapMode=TextureWrapMode.Clamp;
            imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=4096;
        }
        private static void SaveSingle(string path,Texture2D frame,RectInt bounds,float ppu)
        {
            File.WriteAllBytes(path,frame.EncodeToPNG());AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var imp=(TextureImporter)AssetImporter.GetAtPath(path);Configure(imp,ppu);imp.spriteImportMode=SpriteImportMode.Single;
            imp.spritePivot=new Vector2(.5f,(float)bounds.yMin/frame.height);
            var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;imp.SetTextureSettings(settings);imp.SaveAndReimport();
        }
        private static RectInt OpaqueBounds(Texture2D frame)
        {
            var pixels=frame.GetPixels32();int x0=frame.width,y0=frame.height,x1=-1,y1=-1;
            for(int y=0;y<frame.height;y++) for(int x=0;x<frame.width;x++) if(pixels[y*frame.width+x].a>127)
            {x0=Math.Min(x0,x);y0=Math.Min(y0,y);x1=Math.Max(x1,x);y1=Math.Max(y1,y);}
            if(x1<0)throw new InvalidDataException("Empty companion frame");return new RectInt(x0,y0,x1-x0+1,y1-y0+1);
        }
    }
}
