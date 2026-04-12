using UnityEngine;
using UnityEngine.SceneManagement;

public class Control : MonoBehaviour
{
    public string mainMenuSceneName = "UI_MainMenu";

    public void GoBackToMainMenu()
    {
        Debug.Log("Back button clicked. Loading: " + mainMenuSceneName);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}