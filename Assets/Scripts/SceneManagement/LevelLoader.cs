using System.Collections;
using Unity.VisualScripting;
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
        // Go from win scene to main menu. TODO: Win screen logic should flow better and not be split like this... what is it even doing in here
        if (SaveData.Instance.data.currentRoomNumber == 13) {
            SaveData.Instance.data.currentRoomNumber++;
            AudioManager.Instance.PlayWinLoss(true, 2);
            StartCoroutine(LoadFromWin());
        }
    }

    private IEnumerator LoadFromWin() {
        yield return new WaitForSeconds(10.0f);
        SaveData.Instance.data.currentRoomType = (int)SceneType.MainMenu;
        SceneManager.LoadScene((int)SceneType.MainMenu);
    }
    public void LoadNextLevel(SceneType nextType) {
        StartCoroutine(LoadLevel((int) nextType));
    }
    public IEnumerator LoadLevel(int levelIndex) {//, SceneType nextSceneType) {
         // Play animation (trigger fade start), then load the next scene after some time.
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        // Go to win screen if the player made it. TODO: Win screen logic should flow better and not be split like this
        if (SaveData.Instance.data.currentRoomNumber == 12) {
            SaveData.Instance.data.currentRoomNumber++;
            Debug.Log("load win");
            SceneManager.LoadScene("WinScene");
            yield break;
        }
        SceneManager.LoadScene(levelIndex);
        if (currentScene != SceneType.MainMenu) {
            // Increment player progression
            SaveData.Instance.data.currentRoomNumber++;
            SaveData.Instance.data.currentRoomType = levelIndex;
            SaveData.Instance.data.currentRoomCompleted = false;
            
            Debug.Log("Current Room: " + SaveData.Instance.data.currentRoomNumber);
        }
         
        AudioManager.Instance.SwapTrack(levelIndex);
    }
}
