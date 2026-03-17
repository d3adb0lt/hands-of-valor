using UnityEngine;

public class SlashVFX : MonoBehaviour
{
    [SerializeField] private float fallbackLifetime = 1f;

    private void OnEnable()
    {
        float t = fallbackLifetime;

        var animator = GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            if (clips != null && clips.Length > 0)
            {
                t = clips[0].length;
            }
        }

        Destroy(gameObject, t);
    }
}
