using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private DodgeSlot dodgeSlot;

    [Header("Player")]
    [SerializeField] private Player player;

    [Header("Timing")]
    [SerializeField] private float dodgeLifetimeSeconds = 1.5f;

    [SerializeField] private float timeBetweenDodgesMin = 5f;
    [SerializeField] private float timeBetweenDodgesMax = 10f;

    [Header("First Dodge Delay")]
    [SerializeField] private float initialDodgeDelayMin = 2f;
    [SerializeField] private float initialDodgeDelayMax = 4f;

    private Coroutine loop;

    private readonly Queue<string> recognizedInbox = new Queue<string>();
    private readonly object inboxLock = new object();

    private bool dodgeResolved;
    private bool dodgeSucceeded;

    private void Awake()
    {
        if (dodgeSlot == null) dodgeSlot = FindFirstObjectByType<DodgeSlot>();
        if (player == null) player = FindFirstObjectByType<Player>();
    }

    private void Update()
    {
        while (true)
        {
            string sign = null;

            lock (inboxLock)
            {
                if (recognizedInbox.Count > 0) sign = recognizedInbox.Dequeue();
            }

            if (string.IsNullOrEmpty(sign)) break;

            TryConsumeDodgeOnMainThread(sign);
        }
    }

    public void EnqueueRecognizedSign(string sign)
    {
        if (string.IsNullOrEmpty(sign)) return;

        lock (inboxLock)
        {
            recognizedInbox.Enqueue(sign);
        }
    }

    public bool TryConsumeDodgeOnMainThread(string sign)
    {
        if (dodgeSlot == null) return false;
        if (!dodgeSlot.IsFilled) return false;
        if (string.IsNullOrEmpty(sign)) return false;

        if (dodgeSlot.CurrentWord.ToLowerInvariant() == sign.ToLowerInvariant())
        {
            dodgeResolved = true;
            dodgeSucceeded = true;
            dodgeSlot.Clear();
            return true;
        }

        return false;
    }

    public void StartAttacks()
    {
        if (loop != null) StopCoroutine(loop);
        loop = StartCoroutine(AttackLoop());
    }

    public void StopAttacks()
    {
        if (loop != null) StopCoroutine(loop);
        loop = null;

        dodgeResolved = false;
        dodgeSucceeded = false;

        if (dodgeSlot != null) dodgeSlot.Clear();
    }

    private IEnumerator AttackLoop()
    {
        float firstWait = Random.Range(initialDodgeDelayMin, initialDodgeDelayMax);
        yield return new WaitForSeconds(firstWait);

        while (true)
        {
            SpawnDodgeWord();

            dodgeResolved = false;
            dodgeSucceeded = false;

            float t = 0f;
            while (t < dodgeLifetimeSeconds && dodgeSlot != null && dodgeSlot.IsFilled && !dodgeResolved)
            {
                t += Time.deltaTime;
                yield return null;
            }

            bool dodgedInTime = dodgeSucceeded;

            if (dodgeSlot != null) dodgeSlot.Clear();

            if (player != null)
            {
                player.ResolveBossAttack(dodgedInTime);

                if (player.Hp <= 0)
                {
                    yield break;
                }
            }

            float wait = Random.Range(timeBetweenDodgesMin, timeBetweenDodgesMax);
            yield return new WaitForSeconds(wait);
        }
    }

    private void SpawnDodgeWord()
    {
        if (dodgeSlot == null) return;
        if (LevelManager.Instance == null) return;

        if (LevelManager.Instance.TryDequeueWord(out string word))
        {
            dodgeSlot.SetWord(word);
        }
    }
}