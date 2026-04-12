using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Pause : MonoBehaviour
{
    public GameObject pauseUI;
    public Button exitButton; // Drag your button here

    private bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (pauseUI != null)
            pauseUI.SetActive(false);

        // Connect button
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(QuitGame);
            Debug.Log("Button connected successfully");
        }
        else
        {
            Debug.LogError("Exit button not assigned!");
        }
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Over UI? " + EventSystem.current.IsPointerOverGameObject());
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
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse clicked at: " + Input.mousePosition);
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