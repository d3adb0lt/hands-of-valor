using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartScreenListPopulator : MonoBehaviour
{
    [Header("Library + List")]
    [SerializeField] private SignLibrary library;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private SignToggleRow rowPrefab;

    [Header("Filter")]
    [SerializeField] private TMP_InputField filterInput;

    [Header("Preview UI")]
    [SerializeField] private GameObject previewOverlay;
    [SerializeField] private Button overlayCloseButton;
    [SerializeField] private RawImage previewRawImage;

    [Header("Preview Playback")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RenderTexture previewRT;

    private readonly HashSet<string> selected = new HashSet<string>();
    private readonly Dictionary<string, VideoClip> clipByWord = new Dictionary<string, VideoClip>();
    private readonly List<SignToggleRow> rows = new List<SignToggleRow>();

    private void Start()
    {
        if (library == null || contentRoot == null || rowPrefab == null)
        {
            Debug.LogError("StartScreenListPopulator missing list references.");
            return;
        }

        if (previewOverlay == null || overlayCloseButton == null || previewRawImage == null)
        {
            Debug.LogError("StartScreenListPopulator missing preview UI references.");
            return;
        }

        if (videoPlayer == null || previewRT == null)
        {
            Debug.LogError("StartScreenListPopulator missing preview playback references.");
            return;
        }

        BuildClipMap();

        previewOverlay.SetActive(false);

        overlayCloseButton.onClick.RemoveAllListeners();
        overlayCloseButton.onClick.AddListener(HidePreview);

        previewRawImage.texture = previewRT;

        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = previewRT;

        videoPlayer.errorReceived += (vp, msg) => Debug.LogError("VideoPlayer error: " + msg);
        videoPlayer.prepareCompleted += vp => vp.Play();

        SpawnRows();

        if (filterInput != null)
        {
            filterInput.onValueChanged.RemoveAllListeners();
            filterInput.onValueChanged.AddListener(ApplyFilter);
        }

        rowPrefab.gameObject.SetActive(false);
    }

    private void SpawnRows()
    {
        rows.Clear();

        for (int i = 0; i < library.signs.Count; i++)
        {
            string word = library.signs[i].word;
            SignToggleRow row = Instantiate(rowPrefab, contentRoot);
            row.Init(word, false, OnToggleChanged, OnWordClicked);
            rows.Add(row);
        }
    }

    private void BuildClipMap()
    {
        clipByWord.Clear();

        for (int i = 0; i < library.signs.Count; i++)
        {
            var entry = library.signs[i];
            if (entry == null) continue;
            if (string.IsNullOrEmpty(entry.word)) continue;
            if (entry.clip == null) continue;

            if (!clipByWord.ContainsKey(entry.word))
            {
                clipByWord.Add(entry.word, entry.clip);
            }
        }
    }

    private void ApplyFilter(string text)
    {
        string q = string.IsNullOrWhiteSpace(text) ? "" : text.Trim().ToLowerInvariant();

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            if (r == null) continue;

            string w = r.Word;
            bool show = string.IsNullOrEmpty(q) || (w != null && w.ToLowerInvariant().StartsWith(q));
            r.gameObject.SetActive(show);
        }
    }

    private void OnToggleChanged(string word, bool isOn)
    {
        if (isOn) selected.Add(word);
        else selected.Remove(word);
    }

    private void OnWordClicked(string word)
    {
        if (!clipByWord.TryGetValue(word, out var clip) || clip == null)
        {
            Debug.LogWarning("No clip found for: " + word);
            return;
        }

        previewOverlay.SetActive(true);

        videoPlayer.Stop();
        videoPlayer.clip = clip;
        videoPlayer.Prepare();
    }

    private void HidePreview()
    {
        videoPlayer.Stop();
        previewOverlay.SetActive(false);
    }

    public List<string> GetSelectedWords()
    {
        return new List<string>(selected);
    }
}