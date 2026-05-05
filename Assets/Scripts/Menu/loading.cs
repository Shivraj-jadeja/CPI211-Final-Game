using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoLoadingScreen : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "Sherry's MainGame";

    private VideoPlayer videoPlayer;

    private void Start()
    {
        // Get the VideoPlayer component
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            // Subscribe to the video finished event
            videoPlayer.loopPointReached += OnVideoFinished;

            // Start playing the video
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("No VideoPlayer component found!");
            LoadNextScene();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Load the next scene when video is done
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}