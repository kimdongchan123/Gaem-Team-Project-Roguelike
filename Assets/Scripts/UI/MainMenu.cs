using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] 
    private string gameSceneName = "GameScene";

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
