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

    [Header("Parry Words")]
    [SerializeField] private float parryChance = 0.2f;

    [Header("Boss")]
    [SerializeField] private BossHealth bossHealth;

    [Header("Player")]
    [SerializeField] private Player player;

    [Header("VFX")]
    [SerializeField] private SlashVFX slashLeftPrefab; 
    [SerializeField] private SlashVFX slashRightPrefab; 
    [SerializeField] private SlashVFX criticalSlashPrefab; 

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

        if (player == null) player = FindFirstObjectByType<Player>();

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

        bool spawnCriticalThisRound = Random.value < criticalChance;
        bool spawnParryThisRound = Random.value < parryChance;

        int criticalIndex = spawnCriticalThisRound ? Random.Range(0, countToSpawn) : -1;
        int parryIndex = spawnParryThisRound ? Random.Range(0, countToSpawn) : -1;

        if (criticalIndex != -1 && parryIndex == criticalIndex)
        {
            parryIndex = (parryIndex + 1) % countToSpawn;
        }

        for (int i = 0; i < countToSpawn; i++)
        {
            if (!LevelManager.Instance.TryDequeueWord(out string word)) break;

            int randomSlotIndex = Random.Range(0, emptySlots.Count);
            WordSlot slot = emptySlots[randomSlotIndex];
            emptySlots.RemoveAt(randomSlotIndex);

            bool isCritical = i == criticalIndex;
            bool isParry = i == parryIndex;

            WordSlot.WordKind kind = WordSlot.WordKind.Normal;
            float life = lifetimeSeconds;

            if (isCritical)
            {
                kind = WordSlot.WordKind.Critical;
                life = lifetimeSeconds * criticalLifetimeMultiplier;
            }
            else if (isParry)
            {
                kind = WordSlot.WordKind.Parry;
                life = lifetimeSeconds;
            }

            SpawnInSlot(slot, word, life, kind);
        }
    }

    private void SpawnInSlot(WordSlot slot, string word, float lifetime, WordSlot.WordKind kind)
    {
        slot.SetWord(word, kind);

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

        WordSlot matchCritical = null;
        WordSlot matchNormal = null;
        WordSlot matchParry = null;

        for (int i = 0; i < slots.Count; i++)
        {
            WordSlot slot = slots[i];
            if (!slot.IsFilled) continue;

            if (slot.CurrentWord.ToLowerInvariant() != s) continue;

            if (slot.Kind == WordSlot.WordKind.Critical)
            {
                matchCritical = slot;
                break;
            }

            if (slot.Kind == WordSlot.WordKind.Normal)
            {
                if (matchNormal == null) matchNormal = slot;
            }
            else if (slot.Kind == WordSlot.WordKind.Parry)
            {
                if (matchParry == null) matchParry = slot;
            }
        }

        WordSlot chosen = matchCritical != null ? matchCritical : (matchNormal != null ? matchNormal : matchParry);
        if (chosen == null) return;

        bool wasCritical = chosen.Kind == WordSlot.WordKind.Critical;
        bool wasParry = chosen.Kind == WordSlot.WordKind.Parry;

        SpawnSlashVFX(chosen);

        chosen.Clear();

        if (expiry.TryGetValue(chosen, out Coroutine routine) && routine != null)
        {
            StopCoroutine(routine);
            expiry.Remove(chosen);
        }

        if (wasParry)
        {
            if (player != null) player.ResolveParry();
            return;
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
    }

    private void SpawnSlashVFX(WordSlot slot)
    {
        if (slot == null) return;
        if (slot.Kind == WordSlot.WordKind.Parry) return;

        SlashVFX prefab = slot.Kind == WordSlot.WordKind.Critical
            ? criticalSlashPrefab
            : (Random.value < 0.5f ? slashLeftPrefab : slashRightPrefab);

        if (prefab == null) return;

        var vfx = Instantiate(prefab, slot.transform, false);
        var rect = vfx.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            rect.localScale = new Vector3(0.5f, 0.5f, 1f);
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