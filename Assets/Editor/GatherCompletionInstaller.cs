using UnityEditor;
using UnityEngine;

namespace IL6.EditorBuild
{
    public static class GatherCompletionInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach(string material in new[]{"wood","stone","plant"})
            foreach(string phase in new[]{"start","loop","end"})
            {
                string path="Assets/Resources/Sprites/FX/Gather20261103/"+material+"_"+phase+".png";
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=256f/1.35f;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
                settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(.5f,.3f);
                settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
                importer.filterMode=FilterMode.Bilinear;importer.mipmapEnabled=false;
                importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;
                importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
