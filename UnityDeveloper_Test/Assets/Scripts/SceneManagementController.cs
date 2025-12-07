using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagementController : MonoBehaviour
{
    // Restart the current scene
    public void RestartScene()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    // Load next scene (must be added in Build Settings)
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        // If next index is out of range, reload the same scene or do nothing
        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("No next scene found in Build Settings!");
            return;
        }

        SceneManager.LoadScene(nextIndex);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // stop play mode in editor
#else
        Application.Quit(); // quit game in build
#endif
    }
}
