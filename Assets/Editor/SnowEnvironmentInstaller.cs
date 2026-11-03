using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class SnowEnvironmentInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            Import("Assets/Resources/Sprites/Environment/Snowfield/snow_ground.png",104.5f,true);
            Import("Assets/Resources/Sprites/Environment/Snowfield/snow_contact.png",256f,false);
            Import("Assets/Resources/Sprites/Props/snowRock.png",924.6897f,false);
            AssetDatabase.SaveAssets();
        }
        private static void Import(string path,float ppu,bool tiled)
        {
            var imp=(TextureImporter)AssetImporter.GetAtPath(path);
            if(imp==null) throw new System.InvalidOperationException("Missing snow style asset: "+path);
            imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;
            imp.spritePixelsPerUnit=ppu;imp.spritePivot=new Vector2(.5f,.5f);
            var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;imp.SetTextureSettings(settings);
            imp.filterMode=FilterMode.Bilinear;imp.mipmapEnabled=false;imp.alphaIsTransparency=!tiled;
            imp.wrapMode=tiled?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
            imp.npotScale=TextureImporterNPOTScale.None;imp.maxTextureSize=4096;
            imp.textureCompression=TextureImporterCompression.Uncompressed;imp.isReadable=true;
            imp.SaveAndReimport();
        }
    }
}
