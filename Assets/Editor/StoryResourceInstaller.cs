using UnityEditor;
using UnityEditor.U2D.Sprites;
using System.Linq;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class StoryResourceInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            const string path="Assets/Resources/Sprites/Story20261026/village_ruins.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.GetSourceTextureWidthAndHeight(out int width,out int height);
            importer.spritePixelsPerUnit=width/2f/3f;
            importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;
            var factories=new SpriteDataProviderFactories();factories.Init();
            var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var previous=provider.GetSpriteRects();var slices=new SpriteRect[4];
            for(int i=0;i<4;i++)
            {
                string name="ruin_"+i;var old=previous.FirstOrDefault(s=>s.name==name);
                slices[i]=new SpriteRect {name=name,rect=new Rect(i%2*width/2f,(1-i/2)*height/2f,width/2f,height/2f),
                    alignment=SpriteAlignment.Custom,pivot=new Vector2(.5f,.15f),
                    spriteID=old!=null && !old.spriteID.Empty()?old.spriteID:GUID.Generate()};
            }
            provider.SetSpriteRects(slices);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(slices.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
            provider.Apply();importer.SaveAndReimport();AssetDatabase.SaveAssets();
            Debug.Log("Story resources installed: father and four village ruins.");
        }
    }
}
