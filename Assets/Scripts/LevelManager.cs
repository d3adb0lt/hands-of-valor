using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Engine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string startScreenSceneName = "StartScreenScene";
    [SerializeField] private string victorySceneName = "VictoryScene";

    [Header("Selected words are provided by the start screen")]
    [SerializeField] private List<string> selectedWords = new List<string>();

    [Header("Boss Fight Queue")]
    [SerializeField] private int bagsToPreload = 30;

    [Tooltip("When the queue falls below this number, a new shuffled bag is appended.")]
    [SerializeField] private int refillThreshold = 5;

    public IReadOnlyList<string> SelectedWords => selectedWords;

    private readonly Queue<string> fightQueue = new Queue<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSelectedWords(List<string> words)
    {
        selectedWords = words ?? new List<string>();
    }

    public void PrepareBossFightQueue()
    {
        fightQueue.Clear();

        if (selectedWords == null || selectedWords.Count == 0)
        {
            Debug.LogError("SelectedWords is empty. Cannot prepare fight queue.");
            return;
        }

        for (int i = 0; i < bagsToPreload; i++)
        {
            AppendShuffledBag();
        }
    }

    public bool TryDequeueWord(out string word)
    {
        EnsureRefill();

        if (fightQueue.Count == 0)
        {
            word = null;
            return false;
        }

        word = fightQueue.Dequeue();
        EnsureRefill();
        return true;
    }

    public int FightQueueCount => fightQueue.Count;

    private void EnsureRefill()
    {
        if (selectedWords == null || selectedWords.Count == 0) return;

        if (fightQueue.Count < refillThreshold)
        {
            AppendShuffledBag();
        }
    }

    private void AppendShuffledBag()
    {
        var bag = new List<string>(selectedWords);
        Shuffle(bag);

        for (int i = 0; i < bag.Count; i++)
        {
            fightQueue.Enqueue(bag[i]);
        }
    }

    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public void StartBossFight()
    {
        PrepareBossFightQueue();

        var spawner = FindFirstObjectByType<WordSpawnManager>();
        if (spawner != null) spawner.StartBossFight();

        var attacks = FindFirstObjectByType<BossAttackManager>();
        if (attacks != null) attacks.StartAttacks();
    }

    public void StopBossFight(bool wonBattle)
    {
        var spawner = FindFirstObjectByType<WordSpawnManager>();
        if (spawner != null) spawner.StopBossFight();

        var attacks = FindFirstObjectByType<BossAttackManager>();
        if (attacks != null) attacks.StopAttacks();

        var engine = FindFirstObjectByType<SimpleExecutionEngine>();
        if (engine != null) engine.Pause();

        if (wonBattle) {
            SceneManager.LoadScene(victorySceneName);
        } else {
            SceneManager.LoadScene(startScreenSceneName);
        }
        
    }
}