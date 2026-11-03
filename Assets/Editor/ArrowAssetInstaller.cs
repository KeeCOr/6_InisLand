using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class ArrowAssetInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            const string path="Assets/Resources/Sprites/FX/nordic_arrow.png";
            var imp=(TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;
            imp.spritePixelsPerUnit=1024;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;
            imp.npotScale=TextureImporterNPOTScale.None;imp.filterMode=FilterMode.Point;
            imp.wrapMode=TextureWrapMode.Clamp;imp.textureCompression=TextureImporterCompression.Uncompressed;
            imp.maxTextureSize=4096;imp.SaveAndReimport();AssetDatabase.SaveAssets();
            const string stone="Assets/Resources/Sprites/FX/throwing_stone.png";
            var pebble=(TextureImporter)AssetImporter.GetAtPath(stone);
            if(pebble!=null)
            {
                pebble.textureType=TextureImporterType.Sprite;pebble.spriteImportMode=SpriteImportMode.Single;
                pebble.spritePixelsPerUnit=256;pebble.alphaIsTransparency=true;pebble.mipmapEnabled=false;
                pebble.filterMode=FilterMode.Point;pebble.textureCompression=TextureImporterCompression.Uncompressed;
                pebble.npotScale=TextureImporterNPOTScale.None;pebble.SaveAndReimport();
            }
            Debug.Log("Nordic arrow sprite imported: +X tip, neutral materials, transparent alpha.");
        }
    }
}
