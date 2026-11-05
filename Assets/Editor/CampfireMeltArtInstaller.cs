using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class CampfireMeltArtInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            var imp=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/"+CampfireSnowMelt.SpritePath+".png");
            imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.spritePixelsPerUnit=256;
            var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Center;settings.spriteMeshType=SpriteMeshType.FullRect;imp.SetTextureSettings(settings);
            imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.wrapMode=TextureWrapMode.Clamp;imp.filterMode=FilterMode.Bilinear;imp.npotScale=TextureImporterNPOTScale.None;
            imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=2048;imp.SaveAndReimport();AssetDatabase.SaveAssets();
        }
    }
}
