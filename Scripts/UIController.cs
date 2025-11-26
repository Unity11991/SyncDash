using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour {

    [SerializeField] private string gameSceneName = "Game";
    public AudioSource audioSrc;
    public void Play() {
        SceneManager.LoadScene(gameSceneName);
    }

    public void PlayAgain() {
        SceneManager.LoadScene(gameSceneName);
    }

    public void PlayOneShotSFX() {
        audioSrc.Play();
    }

    public void Exit() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }


}
