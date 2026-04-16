using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Pause : MonoBehaviour
{
    public GameObject pauseUI;
    public Button exitButton;
    public Button resumeButton;

    private bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (pauseUI != null)
            pauseUI.SetActive(false);

        // Connect exit button
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(QuitGame);
            Debug.Log("Exit button connected successfully");
        }
        else
        {
            Debug.LogError("Exit button not assigned!");
        }

        // Connect resume button
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
            Debug.Log("Resume button connected successfully");
        }
        else
        {
            Debug.LogError("Resume button not assigned!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC pressed");
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        Debug.Log("Game Paused");
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        Debug.Log("Game Resumed");
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitGame()
    {
        Debug.Log("!!! QUIT GAME !!!");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}