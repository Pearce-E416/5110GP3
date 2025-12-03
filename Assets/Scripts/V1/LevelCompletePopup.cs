using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelCompletePopup : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;    // LevelCompletePanel
    public TextMeshProUGUI messageText;    // MessageText
    public Button nextButton;   // NextButton

    [Header("Navigation")]
    [Tooltip("Name of the scene to load when Next is pressed")]
    public string nextSceneName;

    void Awake()
    {
        if (panel != null)
            panel.SetActive(false); // start hidden

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);
    }

    public void Show(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (panel != null)
            panel.SetActive(true);
    }

    void OnNextClicked()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // If no next scene, just hide the panel
            if (panel != null)
                panel.SetActive(false);
        }
    }
}
