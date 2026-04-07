using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDelay = 0.15f;
    [SerializeField] private float maxAlpha = 0.35f;
    [SerializeField] private float fadeInTime = 0.05f;
    [SerializeField] private float fadeOutTime = 0.2f;

    private Coroutine flashRoutine;

    private void Awake()
    {
        if (flashImage == null)
        {
            flashImage = GetComponent<Image>();
        }

        if (flashImage != null)
        {
            flashImage.color = new Color(1f, 0f, 0f, 0f);
        }
    }

    public void FlashRed()
    {
        if (flashImage == null) return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        flashImage.color = new Color(1f, 0f, 0f, 0f);

        yield return new WaitForSeconds(flashDelay);

        Color c = flashImage.color;

        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, maxAlpha, t / fadeInTime);
            flashImage.color = c;
            yield return null;
        }

        c.a = maxAlpha;
        flashImage.color = c;

        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(maxAlpha, 0f, t / fadeOutTime);
            flashImage.color = c;
            yield return null;
        }

        flashImage.color = new Color(1f, 0f, 0f, 0f);
        flashRoutine = null;
    }
}