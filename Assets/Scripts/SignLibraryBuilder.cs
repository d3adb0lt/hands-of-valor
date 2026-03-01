#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

public static class SignLibraryBuilder
{
    [MenuItem("Tools/Rebuild Sign Library")]
    public static void Rebuild()
    {
        SignLibrary library = FindLibraryAsset();
        if (library == null)
        {
            Debug.LogError("No SignLibrary asset found.");
            return;
        }

        string folder = "Assets/SLR-GTk/Package/Reference Videos";

        string[] guids = AssetDatabase.FindAssets("t:VideoClip", new[] { folder });

        library.signs.Clear();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>(path);
            if (clip == null) continue;

            library.signs.Add(new SignEntry
            {
                word = clip.name,
                clip = clip
            });
        }

        library.signs = library.signs.OrderBy(s => s.word).ToList();

        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();

        Debug.Log("Rebuilt SignLibrary with " + library.signs.Count + " entries.");
    }

    private static SignLibrary FindLibraryAsset()
    {
        string[] guids = AssetDatabase.FindAssets("t:SignLibrary");
        if (guids.Length == 0) return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<SignLibrary>(path);
    }
}
#endif