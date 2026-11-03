using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace IL6.EditorBuild
{
    public static class NordicTreeInstaller
    {
        public static void Install()
        {
            AssetDatabase.Refresh();
            foreach (string name in new[] { "spruce", "pine", "birch", "fir", "sparse_spruce", "birch_cluster", "oak", "cedar", "larch" })
            {
                string path = "Assets/Resources/Sprites/Props/NordicTrees20261102/" + name + ".png";
                var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                source.LoadImage(File.ReadAllBytes(path));
                var pixels = source.GetPixels32();
                int minY = source.height, maxY = 0;
                for (int y = 0; y < source.height; y++)
                    for (int x = 0; x < source.width; x++)
                        if (pixels[y * source.width + x].a > 16)
                        { minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); }
                float height = name == "pine" ? 1.35f : name == "birch" ? 1.6f : 1.5f;
                float ppu = (maxY - minY + 1) / height;
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = ppu;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                importer.SetTextureSettings(settings);
                // Preserve the root and collider: the existing trunk collider is at local Y -.24.
                importer.spritePivot = new Vector2(.5f, (minY + .24f * ppu) / source.height);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.isReadable = false;
                importer.SaveAndReimport();
                Object.DestroyImmediate(source);
            }
            KeepResizableTreesOutsideAtlas();
            AssetDatabase.SaveAssets();
            Debug.Log("Nordic tree styles imported; opaque heights 1.35/1.5/1.6, trunk base -.24, root physics unchanged.");
        }

        private static void KeepResizableTreesOutsideAtlas()
        {
            // Sprite.texture becomes the packed atlas in a player build. Density-adjusted
            // sprites need their original texture/rect, so exclude only this tree folder.
            const string trees = "Assets/Resources/Sprites/Props/NordicTrees20261102";
            foreach(var guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
            {
                var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(guid));
                foreach(var item in atlas.GetPackables())
                {
                    string path=AssetDatabase.GetAssetPath(item).Replace('\\','/');
                    if(path==trees || path.StartsWith(trees+"/")) {atlas.Remove(new[]{item});continue;}
                    if(!AssetDatabase.IsValidFolder(path) || !trees.StartsWith(path+"/")) continue;
                    atlas.Remove(new[]{item});
                    AddExceptTreeFolder(atlas,path,trees);
                }
                EditorUtility.SetDirty(atlas);
            }
        }

        private static void AddExceptTreeFolder(SpriteAtlas atlas,string folder,string excluded)
        {
            foreach(string entry in Directory.GetFileSystemEntries(folder))
            {
                string path=entry.Replace('\\','/');
                if(path==excluded || path.EndsWith(".meta")) continue;
                if(AssetDatabase.IsValidFolder(path) && excluded.StartsWith(path+"/"))
                {AddExceptTreeFolder(atlas,path,excluded);continue;}
                var item=AssetDatabase.LoadMainAssetAtPath(path);
                if(item is Texture2D || item is DefaultAsset && AssetDatabase.IsValidFolder(path)) atlas.Add(new[]{item});
            }
        }
    }
}
