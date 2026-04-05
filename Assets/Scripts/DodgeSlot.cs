using TMPro;
using UnityEngine;

public class DodgeSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    [Header("Animation")]
    [SerializeField] private ScourgeAttack animationDemo;

    public string CurrentWord { get; private set; } = "";
    public bool IsFilled => !string.IsNullOrEmpty(CurrentWord);
    private bool suppressNextClearAnimation = true;
    private void Reset()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
    }

    private void Awake()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        Clear();
    }

    public void SetWord(string word)
    {
        CurrentWord = word ?? "";

        if (label != null)
        {
            label.text = CurrentWord;
            label.gameObject.SetActive(true);
        }
    }

    public void Clear()
    {
        if (suppressNextClearAnimation)
        {
            suppressNextClearAnimation = false;
        }
        else
        {
            PlayAnimation();
        }

        CurrentWord = "";

        if (label != null)
        {
            label.text = "";
            label.gameObject.SetActive(false);
        }

    }

    private void PlayAnimation()
    {
        if (animationDemo != null)
        {
            animationDemo.Play();
        }
    }
}