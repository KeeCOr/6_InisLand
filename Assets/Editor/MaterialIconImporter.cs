using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild {
public static class MaterialIconImporter {
public static void Apply() {
AssetDatabase.Refresh();
var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/UI/Generated20261017/ore.png");
importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.Uncompressed;
importer.SaveAndReimport();AssetDatabase.SaveAssets();Debug.Log("[MaterialIconImporter] ore PNG configured");
}
}}
