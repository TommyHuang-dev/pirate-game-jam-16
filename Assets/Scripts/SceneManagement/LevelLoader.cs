using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 2.0f;

    private Rigidbody2D _rb;
    private Character _chr;
    private void Start() {
        _rb = this.GetComponent<Rigidbody2D>();
        _chr = this.GetComponent<Character>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("collision!");
        if (other.tag == "RoomExit") { // TODO: Check if room exit is open.
            //SceneManager.LoadScene("Placeholder1"); // TODO: Should be able to load one of the two next scenes.
            //Debug.Log(SceneManager.GetActiveScene().name); 
            StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1)); 
            // Probably useful info: SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1) can load next room in sequence, but that's not really what we want? 
            // Instead, each capillary should hold an int for the room type we'd like to go to. Leaving this here for now tho
        }
    }
    private IEnumerator LoadLevel(int levelIndex) {
        // Play animation, then wait 2 seconds and load the next scene.
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(levelIndex);
    }
}
