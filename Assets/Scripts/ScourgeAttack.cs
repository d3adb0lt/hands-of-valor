using System.Collections;
using UnityEngine;

public class ScourgeAttack : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float preAttackBuffer = 0.05f;

    private static readonly int WillAttack = Animator.StringToHash("willAttack");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");

    private bool _sequenceRunning;

    private void Reset()
    {
        _animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        Debug.Log($"[ScourgeAttack] Awake on {gameObject.name}, animator assigned? {_animator != null}");
    }

    private void Start()
    {
        ResetAnimatorFlags();
    }

    public void Play()
    {
        Debug.Log($"[ScourgeAttack] Play() called on {gameObject.name}");

        if (!_sequenceRunning)
            StartCoroutine(PlayAttackSequence());
    }

    private IEnumerator PlayAttackSequence()
    {
        if (_sequenceRunning)
        {
            Debug.Log("[ScourgeAttack] Sequence already running, aborting");
            yield break;
        }

        if (_animator == null)
        {
            Debug.LogWarning("[ScourgeAttack] Animator is NULL, cannot play animation");
            yield break;
        }

        Debug.Log("[ScourgeAttack] Sequence start");

        _sequenceRunning = true;

        ResetAnimatorFlags();

        Debug.Log("[ScourgeAttack] Set willAttack = true");
        _animator.SetBool(WillAttack, true);

        yield return WaitForState("PreAttack", 2f);
        yield return new WaitForSeconds(preAttackBuffer);

        Debug.Log("[ScourgeAttack] Set willAttack = false, isAttacking = true");
        _animator.SetBool(WillAttack, false);
        _animator.SetBool(IsAttacking, true);

        yield return WaitForState("Attack", 2f);
        yield return WaitForAttackToFinish(3f);

        Debug.Log("[ScourgeAttack] Reset animator flags");
        ResetAnimatorFlags();

        yield return WaitForState("Idle", 2f);

        _sequenceRunning = false;

        Debug.Log("[ScourgeAttack] Sequence end");
    }

    private IEnumerator WaitForState(string targetStateName, float timeout)
    {
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(targetStateName))
            {
                Debug.Log($"[ScourgeAttack] Reached state {targetStateName}");
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning($"[ScourgeAttack] Timed out waiting for state {targetStateName}");
    }

    private IEnumerator WaitForAttackToFinish(float timeout)
    {
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName("Attack") && state.normalizedTime >= 1f)
            {
                Debug.Log("[ScourgeAttack] Attack finished");
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning("[ScourgeAttack] Timed out waiting for attack to finish");
    }

    private void ResetAnimatorFlags()
    {
        if (_animator == null)
            return;

        _animator.SetBool(WillAttack, false);
        _animator.SetBool(IsAttacking, false);
    }
}