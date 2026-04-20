using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ClearInputOnClick : MonoBehaviour, IPointerClickHandler, IDeselectHandler
{
    public TMP_InputField inputField;
    public string placeholderText = "Filter signs";

    public void OnPointerClick(PointerEventData eventData)
    {
        SetPlaceholder("");
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (inputField.text == "")
        {
            SetPlaceholder(placeholderText);
        }
    }

    private void SetPlaceholder(string text)
    {
        if (inputField.placeholder == null)
        {
            return;
        }

        TMP_Text placeholder = inputField.placeholder.GetComponent<TMP_Text>();

        if (placeholder != null)
        {
            placeholder.text = text;
        }
    }
}