using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace IL6.EditorBuild
{
    public static class CompanionSizeInstaller
    {
        [Serializable] private class Manifest {public Entry[] files;}
        [Serializable] private class Entry {public string path;public float ppu;}
        public static void Install()
        {
            AssetDatabase.Refresh();
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/art/generated/characters/size20261104/manifest.json"));
            foreach(var entry in manifest.files)
            {
                if(entry.ppu<=0)throw new InvalidDataException(entry.path);
                var importer=(TextureImporter)AssetImporter.GetAtPath(entry.path);
                importer.spritePixelsPerUnit=entry.ppu;importer.SaveAndReimport();
            }
            CharacterActionInstaller.Install();
            AssetDatabase.SaveAssets();
            Debug.Log("Companion world sizes normalized across 8 roles and 144 locomotion/action frames; actor roots unchanged.");
        }
    }
}
