using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{

    public Button PlayButton;
    public Button QuitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayButton.onClick.AddListener(Play);
        QuitButton.onClick.AddListener(Quit);
    }

    private void Play()
    {
        SceneManager.LoadScene(1);
    }

    private void Quit()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

}
