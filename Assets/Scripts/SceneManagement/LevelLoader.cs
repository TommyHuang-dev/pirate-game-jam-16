using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.TimeZoneInfo;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 2.0f;

    private void Start() { }

    public enum SceneType {
        MainMenu,
        BasicEnemy,
        EliteEnemy,
        Event,
        Boss
    } private SceneType currentScene = SceneType.MainMenu;

    public void LoadNextLevel() {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }
    public IEnumerator LoadLevel(int levelIndex) {//, SceneType nextSceneType) {
        // Play animation, then wait 2 seconds and load the next scene.
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(levelIndex);
        //AudioManager.Instance.SwapTrack(levelIndex);

    }
}
