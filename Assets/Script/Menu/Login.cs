using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SimpleLogin : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public UnityEngine.UI.Button loginButton;
    public string nextSceneName = "Server";

    void Start()
    {
        loginButton.onClick.AddListener(OnLogin);
    }

    void OnLogin()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.Log("Please enter both fields");
            return;
        }

        PlayerPrefs.SetString("PlayerUsername", username);
        SceneManager.LoadScene(nextSceneName);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnLogin();
        }
    }
}