using UnityEngine;
using Engine;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Engine")]
    [SerializeField] private SimpleExecutionEngine engine;

    private bool isPaused;

    public void TogglePause()
    {
        if (isPaused) {
            Resume();
        }
        else 
        {
            Pause();
        }
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (engine != null) engine.Pause();

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (engine != null) engine.Poll();

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    public void QuitToMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        LevelManager.Instance.StopBossFight(false);
    }
}
