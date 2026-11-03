using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public sealed class RecruitConversationSkinImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/Resources/UI/RecruitConversation/")
                && !assetPath.StartsWith("Assets/Resources/UI/DialoguePortraits/")) return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=4096;
        }
    }
}
