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
    if (animationDemo == null) animationDemo = GetComponent<ScourgeAttack>();
    if (animationDemo == null) animationDemo = GetComponentInChildren<ScourgeAttack>(true);
    if (animationDemo == null) animationDemo = GetComponentInParent<ScourgeAttack>();

    Debug.Log($"[DodgeSlot] Awake on {gameObject.name}");
    Debug.Log($"[DodgeSlot] animationDemo assigned? {animationDemo != null}");

    Clear();
}

    public void SetWord(string word)
    {
        Debug.Log($"[DodgeSlot] SetWord({word})");

        CurrentWord = word ?? "";

        if (label != null)
        {
            label.text = CurrentWord;
            label.gameObject.SetActive(true);
        }
    }

public void Clear()
{
    Debug.Log($"[DodgeSlot] Clear() called on {gameObject.name}. CurrentWord = '{CurrentWord}'");

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

    Debug.Log("[DodgeSlot] Clear() finished");
}

    private void PlayAnimation()
    {
        Debug.Log($"[DodgeSlot] PlayAnimation() animationDemo assigned? {animationDemo != null}");

        if (animationDemo != null)
        {
            Debug.Log("[DodgeSlot] Calling ScourgeAttack.Play()");
            animationDemo.Play();
        }
        else
        {
            Debug.LogWarning("[DodgeSlot] animationDemo is NULL");
        }
    }
}