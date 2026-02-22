using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenController : MonoBehaviour
{
    [SerializeField] private string bossFightSceneName = "BossTemplateScene";

    [SerializeField]
    private List<string> hardcodedWords = new List<string>();

    private bool waitingForBossScene;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnStartFightPressed()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetSelectedWords(new List<string>(hardcodedWords));
        }
        else
        {
            Debug.LogError("LevelManager.Instance is null. Make sure LevelManager exists in StartScreen.");
        }

        waitingForBossScene = true;
        SceneManager.LoadScene(bossFightSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!waitingForBossScene) return;
        if (scene.name != bossFightSceneName) return;

        waitingForBossScene = false;

        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager.Instance is null in boss scene.");
            return;
        }

        LevelManager.Instance.StartBossFight();

        Destroy(gameObject);
    }
}