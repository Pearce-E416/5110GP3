using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenUI : MonoBehaviour
{
    [Header("Navigation")]
    [Tooltip("Name of the main menu scene to return to.")]
    public string mainMenuSceneName = "MainMenu";

    public void OnBackToMenuClicked()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("VictoryScreenUI: mainMenuSceneName is empty.");
        }
    }
}
