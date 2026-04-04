using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationDemo : MonoBehaviour
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
    }

    private void Start()
    {
        ResetAnimatorFlags();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            !_sequenceRunning)
        {
            StartCoroutine(AttackSequence());
        }
    }

    private IEnumerator AttackSequence()
    {
        _sequenceRunning = true;

        ResetAnimatorFlags();

        _animator.SetBool(WillAttack, true);

        yield return StartCoroutine(WaitForState("PreAttack", 2f));
        yield return new WaitForSeconds(preAttackBuffer);

        _animator.SetBool(WillAttack, false);
        _animator.SetBool(IsAttacking, true);

        yield return StartCoroutine(WaitForState("Attack", 2f));
        yield return StartCoroutine(WaitForAttackToFinish(3f));

        ResetAnimatorFlags();

        yield return StartCoroutine(WaitForState("Idle", 2f));

        _sequenceRunning = false;
    }

    private IEnumerator WaitForState(string targetStateName, float timeout)
    {
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(targetStateName))
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator WaitForAttackToFinish(float timeout)
    {
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName("Attack") && state.normalizedTime >= 1f)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void ResetAnimatorFlags()
    {
        _animator.SetBool(WillAttack, false);
        _animator.SetBool(IsAttacking, false);
    }
}