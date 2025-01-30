using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1.5f;

    public enum SceneType {
        MainMenu,
        NewRunRoom,
        BasicEnemy,
        EliteEnemy,
        Event,
        Boss
    } [SerializeField] public SceneType currentScene;

    private void Start() {
        currentScene = (SceneType)SceneManager.GetActiveScene().buildIndex;
        if (currentScene != SceneType.MainMenu) { // Don't overwrite stuff from main menu
            SaveData.Instance.data.currentRoomType = SceneManager.GetActiveScene().buildIndex;
        }
    }

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
            SaveData.Instance.data.currentRoomNumber++;
            SaveData.Instance.data.currentRoomCompleted = false;
            
            Debug.Log("Current Room: " + SaveData.Instance.data.currentRoomNumber);
        }
         
        AudioManager.Instance.SwapTrack(levelIndex);
    }
}
