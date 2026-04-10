using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName = "Sherry's MainGame";

    [Header("Button Settings (Optional)")]
    public Button RightserverButton;

    void Start()
    {

        if (RightserverButton != null)
        {
            RightserverButton.onClick.AddListener(LoadScene);
        }
    }

    // Call this method to load the scene
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene name is not set!");
        }
    }
}