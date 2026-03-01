using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignToggleRow : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Toggle toggle;
    [SerializeField] private Button wordButton;
    [SerializeField] private TMP_Text wordLabel;

    private string word;

    private Action<string, bool> onToggleChanged;
    private Action<string> onWordClicked;

    public string Word => word;

    private void Awake()
    {
        if (toggle == null || wordButton == null || wordLabel == null)
        {
            Debug.LogError("SignToggleRow is missing references. Assign Toggle, Word Button, and Word Label on the prefab.", this);
        }
    }

    public void Init(string wordName, bool isOn, Action<string, bool> toggleChanged, Action<string> wordClicked)
    {
        word = wordName;
        onToggleChanged = toggleChanged;
        onWordClicked = wordClicked;

        if (wordLabel != null)
        {
            wordLabel.text = wordName;
        }

        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(HandleToggleChanged);
            toggle.isOn = isOn;
            toggle.onValueChanged.AddListener(HandleToggleChanged);
        }

        if (wordButton != null)
        {
            wordButton.onClick.RemoveListener(HandleWordClicked);
            wordButton.onClick.AddListener(HandleWordClicked);
        }
    }

    private void HandleToggleChanged(bool isOn)
    {
        onToggleChanged?.Invoke(word, isOn);
    }

    private void HandleWordClicked()
    {
        onWordClicked?.Invoke(word);
    }

    public bool IsOn()
    {
        return toggle != null && toggle.isOn;
    }
}