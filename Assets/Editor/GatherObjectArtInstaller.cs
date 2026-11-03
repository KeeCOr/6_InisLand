using UnityEditor;
using UnityEngine;

namespace IL6.EditorBuild
{
    public static class GatherObjectArtInstaller
    {
        public static void Install()
        {
            // Existing file GUIDs, canvas sizes, pivots and PPU are retained by the art replacement.
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            GroundPropResourceInstaller.Install();
            NatureVariationInstaller.Install();
            GatherCompletionInstaller.Install();
            AssetDatabase.SaveAssets();
            Debug.Log("Gather object art installed:6 trees,6 rocks,6 sites,6 pickups; completion FX retained.");
        }
    }
}
