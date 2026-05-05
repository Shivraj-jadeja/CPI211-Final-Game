using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lose : MonoBehaviour
{
    [Header("Button References (Optional)")]
    public Button retryButton;
    public Button quitButton;


    [Header("Scene Settings")]
    public string mainMenuSceneName = "Main Menu";

    private void Start()
    {
        // Automatically find and assign buttons if not manually set
        if (retryButton == null)
            retryButton = GameObject.Find("RetryButton")?.GetComponent<Button>();

        if (quitButton == null)
            quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();

        // Add listeners to buttons (if they exist)
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryPressed);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitPressed);
    }

    // Called when Retry button is clicked
    public void OnRetryPressed()
    {
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);


    }

    // Called when Quit button is clicked
    public void OnQuitPressed()
    {
        // For editor: stops play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // For built game: quits the application
            Application.Quit();
#endif
    }
}