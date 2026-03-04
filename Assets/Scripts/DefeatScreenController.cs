using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnFromDefeatButton : MonoBehaviour
{
    [SerializeField] private string startSceneName = "StartScreenScene";

    public void OnReturnPressed()
    {
        SceneManager.LoadScene(startSceneName);
    }


}
