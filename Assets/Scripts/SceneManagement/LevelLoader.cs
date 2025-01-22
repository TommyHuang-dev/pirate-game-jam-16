using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 2.0f;

    public enum SceneType {
        MainMenu,
        BasicEnemy=2, // Start from 2, new run room = 1.
        EliteEnemy,
        Event,
        Boss
    } [SerializeField] private SceneType currentScene;

    public void LoadNextLevel(SceneType nextType) {
        StartCoroutine(LoadLevel((int) nextType));
    }
    public IEnumerator LoadLevel(int levelIndex) {//, SceneType nextSceneType) {
        // Play animation (trigger fade start), then load the next scene after some time.
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(levelIndex);
        if (currentScene != SceneType.MainMenu) {
            // Increment player progression
            PlayerPrefs.SetInt(
                "currentRoomNumber",
                PlayerPrefs.GetInt("currentRoomNumber", 0) + 1
            );
            Debug.Log("Current Room: " + PlayerPrefs.GetInt("currentRoomNumber", 0));
        }
         
        //TODO: Make this an enum maybe
        if (levelIndex == 2 || levelIndex == 3 || levelIndex == 5) {
            AudioManager.Instance.SwapTrack(1);
        } else {
            AudioManager.Instance.SwapTrack(0);
        }
    }
}
