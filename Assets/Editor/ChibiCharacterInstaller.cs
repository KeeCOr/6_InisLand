using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    // Asset preparation only: slice generated artwork; keep runtime paths and sprite names stable.
    public static class ChibiCharacterInstaller
    {
        private const string Source="Assets/art/generated/characters/chibi20261018";
        private const string Hero="Assets/Resources/Sprites/Player/RedHairGirl";
        public static void Install()
        {
            foreach(string weapon in new[]{"axe","bow"})
            {
                File.Copy(Source+"/"+weapon+".png",Hero+"/"+weapon+".png",true);
                ImportHero(weapon,4,weapon=="axe"?.86f:1f);
            }
            File.Copy(Source+"/logging.png",Hero+"/logging.png",true);
            // The action generator retains all existing Start/Loop/End clips and controllers.
            PlayerActionAnimationGenerator.Generate();
            ImportHero("logging",3,1f);
            var atlas=Read(Source+"/companions.png");
            string[] roles={"hunter","warrior","farmer","cook","healer","aunt","uncle","child"};
            for(int i=0;i<roles.Length;i++)
            {
                RectInt cell=Cell(atlas,4,2,i);
                // Discard the atlas gutter at the top; it can contain the previous row's boot fringe.
                cell.height-=12;
                var cut=new Texture2D(cell.width,cell.height,TextureFormat.RGBA32,false);
                cut.SetPixels(atlas.GetPixels(cell.x,cell.y,cell.width,cell.height));cut.Apply();
                string path=i<5 ? "Assets/Resources/Sprites/GeneratedStatic20261014/Companions/"+roles[i]+".png"
                    : "Assets/Resources/Sprites/Companions/"+roles[i]+".png";
                File.WriteAllBytes(path,cut.EncodeToPNG());
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=Configure(path);
                var bounds=OpaqueBounds(cut,new RectInt(0,0,cut.width,cut.height));
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=bounds.height/1.02f;
                importer.spritePivot=new Vector2(bounds.center.x/cut.width,(float)bounds.yMin/cut.height);
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
                settings.spriteAlignment=(int)SpriteAlignment.Custom;importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                UnityEngine.Object.DestroyImmediate(cut);
            }
            UnityEngine.Object.DestroyImmediate(atlas);
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Debug.Log("[ChibiCharacterInstaller] Hero 44 frames + all 8 companion roles installed; original GUIDs retained.");
        }
        private static Texture2D Read(string path)
        {
            var image=new Texture2D(2,2,TextureFormat.RGBA32,false);
            if(!image.LoadImage(File.ReadAllBytes(path))) throw new InvalidDataException(path);
            return image;
        }
        private static RectInt Cell(Texture2D image,int columns,int rows,int index)
        {
            int column=index%columns,row=index/columns;
            int x=Mathf.RoundToInt(column*image.width/(float)columns),right=Mathf.RoundToInt((column+1)*image.width/(float)columns);
            int y=Mathf.RoundToInt((rows-1-row)*image.height/(float)rows),top=Mathf.RoundToInt((rows-row)*image.height/(float)rows);
            return new RectInt(x,y,right-x,top-y);
        }
        private static RectInt OpaqueBounds(Texture2D image,RectInt cell)
        {
            var colors=image.GetPixels32();int minX=cell.width,minY=cell.height,maxX=-1,maxY=-1;
            for(int y=0;y<cell.height;y++) for(int x=0;x<cell.width;x++)
                if(colors[(cell.y+y)*image.width+cell.x+x].a>127)
                {minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);}
            if(maxX<0) throw new InvalidDataException("Empty generated sprite cell");
            return new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);
        }
        private static TextureImporter Configure(string path)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.filterMode=FilterMode.Point;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
            importer.wrapMode=TextureWrapMode.Clamp;
            return importer;
        }
        private static void ImportHero(string key,int rows,float bodyHeightFactor)
        {
            string path=Hero+"/"+key+".png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var image=Read(path);var first=OpaqueBounds(image,Cell(image,4,rows,0));
            var slices=new SpriteMetaData[rows*4];
            for(int i=0;i<slices.Length;i++)
            {
                var cell=Cell(image,4,rows,i);var bounds=OpaqueBounds(image,cell);
                slices[i]=new SpriteMetaData {name="redhair_"+key+"_"+i,rect=new Rect(cell.x,cell.y,cell.width,cell.height),
                    alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(.58f,(float)bounds.yMin/cell.height)};
            }
            var importer=Configure(path);importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit=first.height*bodyHeightFactor/.95f;
            importer.spritesheet=slices;importer.SaveAndReimport();
            UnityEngine.Object.DestroyImmediate(image);
        }
    }
}
