using TMPro;
using UnityEngine;

public class WordSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    public string CurrentWord { get; private set; } = "";
    public bool IsFilled => !string.IsNullOrEmpty(CurrentWord);

    public bool IsCritical { get; private set; }

    private Color normalColor;
    private bool hasNormalColor;

    private void Reset()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
    }

    private void Awake()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);

        if (label != null)
        {
            normalColor = label.color;
            hasNormalColor = true;
        }

        Clear();
    }

    public void SetWord(string word)
    {
        SetWord(word, false);
    }

    public void SetWord(string word, bool isCritical)
    {
        CurrentWord = word ?? "";
        IsCritical = isCritical;

        if (label != null)
        {
            label.text = CurrentWord;

            if (hasNormalColor)
            {
                label.color = IsCritical ? Color.red : normalColor;
            }

            label.gameObject.SetActive(true);
        }
    }

    public void Clear()
    {
        CurrentWord = "";
        IsCritical = false;

        if (label != null)
        {
            label.text = "";

            if (hasNormalColor)
            {
                label.color = normalColor;
            }

            label.gameObject.SetActive(false);
        }
    }
}