using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordSpawnManager : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private List<WordSlot> slots = new List<WordSlot>();

    [Header("Round Settings")]
    [SerializeField] private int wordsPerRound = 3;

    [Header("Timing")]
    [SerializeField] private float lifetimeSeconds = 2.5f;
    [SerializeField] private float timeBetweenRounds = 0.25f;

    [Header("Critical Words")]
    [SerializeField] private float criticalChance = 0.2f;
    [SerializeField] private float criticalLifetimeMultiplier = 0.5f;

    [Header("Boss")]
    [SerializeField] private BossHealth bossHealth;

    private readonly Dictionary<WordSlot, Coroutine> expiry = new Dictionary<WordSlot, Coroutine>();

    private readonly Queue<string> recognizedInbox = new Queue<string>();
    private readonly object inboxLock = new object();

    private Coroutine fightLoop;

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

            HandleRecognizedOnMainThread(sign);
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

    public void StartBossFight()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("No LevelManager.Instance found.");
            return;
        }

        if (bossHealth == null) bossHealth = FindFirstObjectByType<BossHealth>();

        if (fightLoop != null) StopCoroutine(fightLoop);
        fightLoop = StartCoroutine(FightLoop());
    }

    public void StopBossFight()
    {
        if (fightLoop != null) StopCoroutine(fightLoop);
        fightLoop = null;

        ClearAll();
    }

    private IEnumerator FightLoop()
    {
        while (true)
        {
            SpawnRound();

            float t = 0f;
            while (t < lifetimeSeconds && AnySlotsFilled())
            {
                t += Time.deltaTime;
                yield return null;
            }

            ClearAll();
            yield return new WaitForSeconds(timeBetweenRounds);
        }
    }

    private bool AnySlotsFilled()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsFilled) return true;
        }
        return false;
    }

    private void SpawnRound()
    {
        if (LevelManager.Instance == null) return;

        var emptySlots = new List<WordSlot>();
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsFilled) emptySlots.Add(slots[i]);
        }

        int countToSpawn = Mathf.Min(wordsPerRound, emptySlots.Count);
        if (countToSpawn <= 0) return;

        bool spawnCritical = Random.value < criticalChance;

        for (int i = 0; i < countToSpawn; i++)
        {
            if (!LevelManager.Instance.TryDequeueWord(out string word)) break;

            int randomSlotIndex = Random.Range(0, emptySlots.Count);
            WordSlot slot = emptySlots[randomSlotIndex];
            emptySlots.RemoveAt(randomSlotIndex);

            bool isCriticalThisOne = spawnCritical && i == 0;
            float lifetime = isCriticalThisOne ? lifetimeSeconds * criticalLifetimeMultiplier : lifetimeSeconds;

            SpawnInSlot(slot, word, lifetime, isCriticalThisOne);
        }
    }

    private void SpawnInSlot(WordSlot slot, string word, float lifetime, bool isCritical)
    {
        slot.SetWord(word, isCritical);

        if (expiry.TryGetValue(slot, out Coroutine existing) && existing != null)
        {
            StopCoroutine(existing);
            expiry.Remove(slot);
        }

        expiry[slot] = StartCoroutine(ExpireSlotAfter(slot, lifetime));
    }

    private IEnumerator ExpireSlotAfter(WordSlot slot, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (slot.IsFilled) slot.Clear();

        expiry.Remove(slot);
    }

    private void HandleRecognizedOnMainThread(string sign)
    {
        var bossAttacks = FindFirstObjectByType<BossAttackManager>();
        if (bossAttacks != null)
        {
            if (bossAttacks.TryConsumeDodgeOnMainThread(sign))
            {
                return;
            }
        }

        string s = sign.ToLowerInvariant();

        for (int i = 0; i < slots.Count; i++)
        {
            WordSlot slot = slots[i];
            if (!slot.IsFilled) continue;

            if (slot.CurrentWord.ToLowerInvariant() == s)
            {
                bool wasCritical = slot.IsCritical;

                slot.Clear();

                if (expiry.TryGetValue(slot, out Coroutine routine) && routine != null)
                {
                    StopCoroutine(routine);
                    expiry.Remove(slot);
                }

                if (bossHealth != null)
                {
                    int damage = wasCritical ? 2 : 1;
                    bossHealth.TakeDamage(damage);

                    if (wasCritical) Debug.Log("Critical hit");

                    if (bossHealth.CurrentHp == 0 && LevelManager.Instance != null)
                    {
                        LevelManager.Instance.StopBossFight(wonBattle: true);
                    }
                }

                break;
            }
        }
    }

    private void ClearAll()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Clear();
        }

        foreach (var kv in expiry)
        {
            if (kv.Value != null) StopCoroutine(kv.Value);
        }
        expiry.Clear();
    }
}