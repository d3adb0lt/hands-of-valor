using TMPro;
using UnityEngine;

public class WordSlot : MonoBehaviour
{
    public enum WordKind
    {
        Normal,
        Critical,
        Parry
    }

    [SerializeField] private TMP_Text label;

    public string CurrentWord { get; private set; } = "";
    public bool IsFilled => !string.IsNullOrEmpty(CurrentWord);

    public WordKind Kind { get; private set; } = WordKind.Normal;

    public bool IsCritical => Kind == WordKind.Critical;
    public bool IsParry => Kind == WordKind.Parry;

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
        SetWord(word, WordKind.Normal);
    }

    public void SetWord(string word, bool isCritical)
    {
        SetWord(word, isCritical ? WordKind.Critical : WordKind.Normal);
    }

    public void SetWord(string word, WordKind kind)
    {
        CurrentWord = word ?? "";
        Kind = kind;

        if (label != null)
        {
            label.text = CurrentWord;

            if (hasNormalColor)
            {
                if (Kind == WordKind.Critical) label.color = Color.red;
                else if (Kind == WordKind.Parry) label.color = Color.yellow;
                else label.color = normalColor;
            }

            label.gameObject.SetActive(true);
        }
    }

    public void Clear()
    {
        CurrentWord = "";
        Kind = WordKind.Normal;

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