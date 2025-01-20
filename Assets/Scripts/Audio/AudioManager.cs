using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Singleton audio manager, shared by all scripts
    [SerializeField] private AudioSource audioSource, musicSource1, musicSource2;

    [Header("Scene Music")]
    [SerializeField] private AudioClip[] sceneMusic;
    [SerializeField] private float FadeTime;
    private bool isPlayingMusic1 = false;

    private void Awake() {
        // Can only have one audio manager (singleton)
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){
        SwapTrack(0);
    }

    // Transition between two songs
    public void SwapTrack(int songIdx) {
        StopAllCoroutines();

        StartCoroutine(FadeCurrentMusic(sceneMusic[songIdx]));

        isPlayingMusic1 = !isPlayingMusic1;

    }
    public void ReturnToDefault() {
        // SwapTrack(defaultAmbience);
    }



    // discussions.unity.com/t/fade-out-audio-source/585912/5 may be helpful in the future
    private IEnumerator FadeCurrentMusic(AudioClip newClip) {
        float timeToFade = 1.0f;
        float timeElapsed = 0;

        if (isPlayingMusic1) {
            musicSource2.clip = newClip;
            musicSource2.Play();

            while (timeElapsed < timeToFade) {
                musicSource2.volume = Mathf.Lerp(0, 1, timeElapsed /  timeToFade);
                musicSource1.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            musicSource1.Stop();
        } else {
            musicSource1.clip = newClip;
            musicSource1.Play();

            while (timeElapsed < timeToFade) {
                musicSource1.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                musicSource2.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            musicSource2.Stop();
        }
    }
}
