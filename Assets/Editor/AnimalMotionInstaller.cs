using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class AnimalMotionInstaller
    {
        [Serializable] private sealed class Layout {public int canvasWidth,canvasHeight;public Slice[] slices;}
        [Serializable] private sealed class Slice {public int x,y,width,height;public float pivotX,pivotY;}
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach(string key in new[]{"deer","rabbit","boar","wolf","bear","mammoth","fox","snow_hare","dog","hawk"})
            {
                string source="Assets/art/generated/animals/motion20261026/"+key+".png";
                var atlas=new Texture2D(2,2,TextureFormat.RGBA32,false);
                if(!atlas.LoadImage(File.ReadAllBytes(source)))throw new InvalidDataException(source);
                // Alpha-gutter inspection supplies complete slices even for long tails and wings.
                var layout=JsonUtility.FromJson<Layout>(File.ReadAllText(Path.ChangeExtension(source,"json")));
                if(layout.slices==null||layout.slices.Length!=9)throw new InvalidDataException(key+" atlas layout");
                // Normalize painted pixels, not source canvas padding or unrelated legacy sprite sizes.
                var profile=AnimalSizeProfile.For(key);
                int referencePixels=0;
                foreach(var slice in key=="hawk"?layout.slices:new[]{layout.slices[2]})
                {
                    var sample=new Texture2D(slice.width,slice.height,TextureFormat.RGBA32,false);
                    sample.SetPixels(atlas.GetPixels(slice.x,slice.y,slice.width,slice.height));sample.Apply();
                    var opaque=OpaqueBounds(sample);
                    referencePixels=Math.Max(referencePixels,key=="hawk"?opaque.width:opaque.height);
                    UnityEngine.Object.DestroyImmediate(sample);
                }
                float ppu=referencePixels*profile.ActorScale/profile.Size;
                string target="Assets/Resources/Sprites/Animals/Motion20261026/"+key;
                Directory.CreateDirectory(target);
                for(int row=0;row<3;row++)for(int col=0;col<3;col++)
                {
                    var slice=layout.slices[row*3+col];
                    var frame=new Texture2D(layout.canvasWidth,layout.canvasHeight,TextureFormat.RGBA32,false);
                    frame.SetPixels32(new Color32[layout.canvasWidth*layout.canvasHeight]);
                    frame.SetPixels((layout.canvasWidth-slice.width)/2,0,slice.width,slice.height,atlas.GetPixels(slice.x,slice.y,slice.width,slice.height));frame.Apply();
                    var bounds=OpaqueBounds(frame);
                    string direction=col==0?"front":col==1?"back":"side";
                    string path=target+"/"+(row==0?"idle_"+direction:"move_"+direction+"_"+(row-1))+".png";
                    File.WriteAllBytes(path,frame.EncodeToPNG());AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                    var imp=(TextureImporter)AssetImporter.GetAtPath(path);
                    imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;
                    imp.spritePixelsPerUnit=ppu;imp.spritePivot=key=="hawk"?new Vector2(slice.pivotX,slice.pivotY):new Vector2(.5f,(float)bounds.yMin/frame.height);
                    var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;imp.SetTextureSettings(settings);
                    imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.filterMode=FilterMode.Bilinear;
                    imp.npotScale=TextureImporterNPOTScale.None;
                    imp.textureCompression=TextureImporterCompression.Uncompressed;imp.wrapMode=TextureWrapMode.Clamp;
                    imp.maxTextureSize=2048;imp.SaveAndReimport();UnityEngine.Object.DestroyImmediate(frame);
                }
                UnityEngine.Object.DestroyImmediate(atlas);
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("Animal motion installed: 10 species, 90 directional idle/movement frames.");
        }
        private static RectInt OpaqueBounds(Texture2D image)
        {
            var pixels=image.GetPixels32();int x0=image.width,y0=image.height,x1=-1,y1=-1;
            for(int y=0;y<image.height;y++)for(int x=0;x<image.width;x++)if(pixels[y*image.width+x].a>127)
            {x0=Math.Min(x0,x);y0=Math.Min(y0,y);x1=Math.Max(x1,x);y1=Math.Max(y1,y);}
            if(x1<0)throw new InvalidDataException("Empty animal frame");return new RectInt(x0,y0,x1-x0+1,y1-y0+1);
        }
    }
}
