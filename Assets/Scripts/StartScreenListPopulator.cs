using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartScreenListPopulator : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ScriptableObject signLibrary;

    [Header("List UI")]
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private SignToggleRow rowPrefab;
    [SerializeField] private TMP_Text selectedCountLabel;
    [SerializeField] private TMP_InputField filterInput;
    [SerializeField] private Button resetButton;

    [Header("Preview UI")]
    [SerializeField] private GameObject previewOverlay;
    [SerializeField] private RawImage previewRawImage;
    [SerializeField] private RenderTexture previewRT;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Button closePreviewButton;

    private readonly HashSet<string> selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private string filterText = "";

    private List<EntryData> allEntries = new List<EntryData>();
    private readonly List<SignToggleRow> spawnedRows = new List<SignToggleRow>();

    private struct EntryData
    {
        public string word;
        public VideoClip clip;
    }

    private void Awake()
    {
        if (previewOverlay != null) previewOverlay.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            if (previewRT != null) videoPlayer.targetTexture = previewRT;

            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }

        if (closePreviewButton != null)
        {
            closePreviewButton.onClick.RemoveAllListeners();
            closePreviewButton.onClick.AddListener(ClosePreview);
        }

        if (previewRawImage != null && previewRT != null)
        {
            previewRawImage.texture = previewRT;
        }

        if (filterInput != null)
        {
            filterInput.onValueChanged.RemoveAllListeners();
            filterInput.onValueChanged.AddListener(OnFilterChanged);
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetSelections);
        }
    }

    private void Start()
    {
        if (signLibrary == null)
        {
            Debug.LogError("SignLibrary is not assigned.");
            return;
        }

        allEntries = ExtractEntries(signLibrary)
            .Where(e => !string.IsNullOrWhiteSpace(e.word))
            .OrderBy(e => e.word, StringComparer.OrdinalIgnoreCase)
            .ToList();

        PullSelectionsFromLevelManager();
        RebuildVisibleList();
        UpdateSelectedCount();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }

    private void PullSelectionsFromLevelManager()
    {
        selected.Clear();

        if (LevelManager.Instance == null) return;

        var saved = LevelManager.Instance.SelectedWords;
        if (saved == null) return;

        for (int i = 0; i < saved.Count; i++)
        {
            var w = saved[i];
            if (!string.IsNullOrWhiteSpace(w)) selected.Add(w);
        }
    }

    private void PushSelectionsToLevelManager()
    {
        if (LevelManager.Instance == null) return;
        LevelManager.Instance.SetSelectedWords(GetSelectedWords());
    }

    private void OnFilterChanged(string text)
    {
        filterText = (text ?? "").Trim().ToLowerInvariant();
        RebuildVisibleList();
    }

    private IEnumerable<EntryData> GetVisible()
    {
        if (string.IsNullOrEmpty(filterText)) return allEntries;

        return allEntries.Where(e =>
            e.word != null && e.word.ToLowerInvariant().StartsWith(filterText)
        );
    }

    private void RebuildVisibleList()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] != null) Destroy(spawnedRows[i].gameObject);
        }
        spawnedRows.Clear();

        if (rowPrefab == null || contentRoot == null) return;

        foreach (var entry in GetVisible())
        {
            var row = Instantiate(rowPrefab, contentRoot);
            spawnedRows.Add(row);

            bool isOn = selected.Contains(entry.word);

            row.Init(
                entry.word,
                isOn,
                (word, on) => OnToggleChanged(word, on),
                word => OnWordClicked(word)
            );
        }
    }

    private void OnToggleChanged(string word, bool on)
    {
        if (string.IsNullOrWhiteSpace(word)) return;

        if (on) selected.Add(word);
        else selected.Remove(word);

        UpdateSelectedCount();
        PushSelectionsToLevelManager();
    }

    private void UpdateSelectedCount()
    {
        if (selectedCountLabel != null)
        {
            selectedCountLabel.text = $"{selected.Count}";
        }
    }

    private void ResetSelections()
    {
        selected.Clear();
        UpdateSelectedCount();
        RebuildVisibleList();
        PushSelectionsToLevelManager();
    }

    private void OnWordClicked(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;

        var entry = allEntries.FirstOrDefault(e => string.Equals(e.word, word, StringComparison.OrdinalIgnoreCase));
        if (entry.clip == null) return;

        if (previewOverlay != null) previewOverlay.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = entry.clip;
            videoPlayer.Prepare();
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        if (vp == null) return;
        vp.Play();
    }

    private void ClosePreview()
    {
        if (videoPlayer != null) videoPlayer.Stop();
        if (previewOverlay != null) previewOverlay.SetActive(false);
    }

    public List<string> GetSelectedWords()
    {
        return selected.ToList();
    }

    private static List<EntryData> ExtractEntries(object lib)
    {
        var results = new List<EntryData>();
        if (lib == null) return results;

        var libType = lib.GetType();
        var candidates = new List<object>();

        foreach (var f in libType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var v = SafeGet(() => f.GetValue(lib));
            AddEnumerableIfAny(candidates, v);
        }

        foreach (var p in libType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!p.CanRead) continue;
            if (p.GetIndexParameters().Length > 0) continue;
            var v = SafeGet(() => p.GetValue(lib, null));
            AddEnumerableIfAny(candidates, v);
        }

        foreach (var c in candidates)
        {
            if (c == null) continue;

            var itemType = c.GetType();
            var stringGetter = FindStringGetter(itemType);
            var clipGetter = FindVideoClipGetter(itemType);

            if (stringGetter == null || clipGetter == null) continue;

            var word = SafeGet(() => stringGetter(c)) as string;
            var clip = SafeGet(() => clipGetter(c)) as VideoClip;

            if (!string.IsNullOrWhiteSpace(word))
            {
                results.Add(new EntryData { word = word.Trim(), clip = clip });
            }
        }

        return results;
    }

    private static void AddEnumerableIfAny(List<object> outItems, object maybeEnumerable)
    {
        if (maybeEnumerable == null) return;
        if (maybeEnumerable is string) return;
        if (maybeEnumerable is not IEnumerable enumerable) return;

        foreach (var item in enumerable)
        {
            if (item != null) outItems.Add(item);
        }
    }

    private static Func<object, object> FindStringGetter(Type t)
    {
        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!p.CanRead) continue;
            if (p.GetIndexParameters().Length > 0) continue;
            if (p.PropertyType != typeof(string)) continue;
            return obj => p.GetValue(obj, null);
        }

        foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (f.FieldType != typeof(string)) continue;
            return obj => f.GetValue(obj);
        }

        return null;
    }

    private static Func<object, object> FindVideoClipGetter(Type t)
    {
        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!p.CanRead) continue;
            if (p.GetIndexParameters().Length > 0) continue;
            if (p.PropertyType != typeof(VideoClip)) continue;
            return obj => p.GetValue(obj, null);
        }

        foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (f.FieldType != typeof(VideoClip)) continue;
            return obj => f.GetValue(obj);
        }

        return null;
    }

    private static object SafeGet(Func<object> fn)
    {
        try { return fn(); }
        catch { return null; }
    }
}