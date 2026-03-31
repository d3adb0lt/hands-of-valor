using UnityEngine;

public class BarUI : MonoBehaviour
{
    public float width, height;
    [SerializeField] private RectTransform bar;

    private float maxValue;

    public void SetMaxValue(float max)
    {
        maxValue = max;
        SetValue(max);
    }

    public void SetValue(float current)
    {
        float newWidth = (current / maxValue) * width;
        bar.sizeDelta = new Vector2(newWidth, height);
    }
}
