using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool gamePaused = false;
    public GameObject pauseUI;

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (gamePaused) {
                Resume();
            } else {
                Pause();
            }
        }
    }

    public void Resume() {
        pauseUI.SetActive(false);
        gamePaused = false;
        Time.timeScale = 1.0f;
    }
    public void Pause() {
        pauseUI.SetActive(true);
        gamePaused = true;
        Time.timeScale = 0.0f;
    }
    public void QuitGame() {
        Application.Quit();
    }
}

