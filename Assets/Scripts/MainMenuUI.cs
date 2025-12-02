using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Navigation")]
    [Tooltip("Name of the first gameplay scene (e.g., Aries level).")]
    public string firstLevelSceneName = "AriesLevel";

    public void OnPlayClicked()
    {
        if (!string.IsNullOrEmpty(firstLevelSceneName))
        {
            SceneManager.LoadScene(firstLevelSceneName);
        }
        else
        {
            Debug.LogError("MainMenuUI: firstLevelSceneName is empty.");
        }
    }

    public void OnExitClicked()
    {
        Debug.Log("MainMenuUI: Exit requested.");

        // Works in a built game
        Application.Quit();

        // In the Unity editor, Application.Quit() does nothing.
        // To simulate, you can stop Play Mode manually.
    }
}
