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
    [SerializeField] private string defeatSceneName = "DefeatScene";

    [Header("Selected words are provided by the start screen")]
    [SerializeField] private List<string> selectedWords = new List<string>();

    [Header("Score Weights")]
    [SerializeField] private float scoreMultiplier = 1000f;
    [SerializeField] private float wordsWeight = 1f;
    [SerializeField] private float hpWeight = 1f;
    [SerializeField] private float timeWeight = 1f;

    public float LastFightScore { get; private set; }

    [Header("Boss Fight Queue")]
    [SerializeField] private int bagsToPreload = 30;

    [Tooltip("When the queue falls below this number, a new shuffled bag is appended.")]
    [SerializeField] private int refillThreshold = 5;

    public IReadOnlyList<string> SelectedWords => selectedWords;

    private readonly Queue<string> fightQueue = new Queue<string>();
    private float fightStartTime;

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

    private void CalculateScore()
    {
        var player = FindFirstObjectByType<Player>();
        float timeTaken = Mathf.Max(Time.time - fightStartTime, 0.1f);
        float hpPercentage = (player != null) ? (float) player.Hp / player.MaxHp : 0f;

        LastFightScore = scoreMultiplier * (selectedWords.Count * wordsWeight) * (hpPercentage * hpWeight) / (timeTaken * timeWeight);
        LastFightScore = Mathf.Max(LastFightScore, 1f);
        Debug.Log($"Score: {LastFightScore:F0} (words={selectedWords.Count}, hp%={hpPercentage:P0}, time={timeTaken:F1}s)");
    }

    public void StartBossFight()
    {
        PrepareBossFightQueue();
        fightStartTime = Time.time;

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

        var webcamTextire = FindFirstObjectByType<WebCamTexture>();
        if (webcamTextire != null) webcamTextire.Stop();

        if (wonBattle) {
            CalculateScore();
            SceneManager.LoadScene(victorySceneName);
        } else {
            SceneManager.LoadScene(defeatSceneName);
        }
        
    }
}