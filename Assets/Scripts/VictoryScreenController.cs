using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnFromVictoryButton : MonoBehaviour
{
    [SerializeField] private string startSceneName = "StartScreenScene";
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        if (scoreText != null && LevelManager.Instance != null)
        {
            scoreText.text = $"Score: {LevelManager.Instance.LastFightScore:F0}";
        }
    }

    public void OnReturnPressed()
    {
        SceneManager.LoadScene(startSceneName);
    }


}
