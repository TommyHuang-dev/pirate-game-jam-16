using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public class Song {
        public List<AudioClip> clips;
        public int BPM;

        public Song(List<AudioClip> clips, int BPM) {
            this.clips = clips;
            this.BPM = BPM;
        }
    }
    public static AudioManager Instance; // Singleton audio manager, shared by all scripts
    [SerializeField] private AudioSource audioSource, musicSourceIntro1, musicSource1, musicSourceIntro2, musicSource2, musicSourceOutro;

    [Header("Scene Music")]
    [SerializeField] List<AudioClip> mainTheme, battle, evilBattle, boss, finalBoss;
    private Song[] songs = new Song[5];

    [SerializeField] private float FadeTime;
    private bool isPlayingMusic1 = false;

    [Header("SFX")]
    [SerializeField] List<AudioClip> SFX;
    

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
    void Start() {
        // Gross lol
        songs[0] = new Song(mainTheme, 84); songs[1] = new Song(battle, 136); songs[2] = new Song(evilBattle, 0);
        songs[3] = new Song(boss, 138); songs[4] = new Song(finalBoss, 0);
        SwapTrack(0);
    }

    // Transition between two songs
    public void SwapTrack(int levelIndex) {
        StopAllCoroutines();

        int songIdx = GetSongIndex(levelIndex);
        StartCoroutine(SwapAndFadeMusic(songIdx));

        isPlayingMusic1 = !isPlayingMusic1;
    }
    public void ReturnToDefault() {
        // SwapTrack(defaultAmbience);
    }

    public void PlaySFX(SoundEffects sfx, float pitch, float volume) {
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        audioSource.PlayOneShot(SFX[(int)sfx]);
    }

    // Play win or loss music corresponding to current level.
    public void PlayWinLoss(bool hasWon, int levelIndex) {
        int songIdx = GetSongIndex(levelIndex);
        Song curr_song = songs[songIdx];
        Debug.Log("Play win " + hasWon + " curr song idx: " + songIdx);
        if (curr_song.clips.Count <= 1) { Debug.Log("Wrong song?"); return; }

        //StopAllCoroutines();

        // Calculate the duration of a bar in 4/4 - not planning on other time signatures.
        double barDuration = (60d / curr_song.BPM * 4) * (4/4);
        //double barDuration = (60d / curr_song.BPM); // This is the beat duration
        // This line works out how far you are through the current bar
        double remainder;
        if (isPlayingMusic1) {
            remainder = ((double)musicSource1.timeSamples / musicSource1.clip.frequency) % (barDuration);
        } else {
            remainder = ((double)musicSource2.timeSamples / musicSource2.clip.frequency) % (barDuration);
        }
        // This line works out when the next bar will occur
        double nextBarTime = AudioSettings.dspTime + barDuration - remainder;
        // Set the current Clip to end on the next bar
        if (isPlayingMusic1) {
            musicSource1.SetScheduledEndTime(nextBarTime);
        } else {
            musicSource2.SetScheduledEndTime(nextBarTime);
        }
        // Play the win/loss clip
        if (hasWon) { 
            musicSourceOutro.clip = curr_song.clips[(int)BattleSongParts.Win];
        } else {
            musicSourceOutro.clip = curr_song.clips[(int)BattleSongParts.Loss];
        }
        // Schedule an ending clip to start on the next bar
        musicSourceOutro.PlayScheduled(nextBarTime);
    }
    private int GetSongIndex(int levelIndex) {
        if (levelIndex == 2 || levelIndex == 3) { // Battle
            return 1;
        } else if (levelIndex == 5) { // Boss
            return 3;
        } else {
            return 0;
        }
    }

    // discussions.unity.com/t/fade-out-audio-source/585912/5 may be helpful in the future
    private IEnumerator SwapAndFadeMusic(int songIdx) {
        float timeToFade = 1.0f;
        float timeElapsed = 0;

        List<AudioClip> song = songs[songIdx].clips;

        if (isPlayingMusic1) {
            // If this is a battle theme with multiple sections, play the intro and loop the main part.
            if (song.Count > 1) {
                // Most precise duration calculation possible.
                double introDuration = ((double)song[0].samples / song[0].frequency);
                double startTime = AudioSettings.dspTime + 0.2;
                musicSourceIntro2.clip = song[0]; musicSource2.clip = song[1];
                musicSource2.loop = true;

                musicSourceIntro2.PlayScheduled(startTime);
                musicSource2.PlayScheduled(startTime + introDuration);
            } else { // Otherwise, just play the song.
                musicSourceIntro2.clip = song[0];
                musicSourceIntro2.Play();
            }

            while (timeElapsed < timeToFade) {
                musicSource2.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                musicSourceIntro2.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                musicSource1.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                musicSourceIntro1.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            musicSourceIntro1.Stop();
            musicSource1.Stop();
        } else {
            if (song.Count > 1) {
                double introDuration = ((double)song[0].samples / song[0].frequency);
                double startTime = AudioSettings.dspTime + 0.2;
                musicSourceIntro1.clip = song[0]; musicSource1.clip = song[1];
                musicSource1.loop = true;

                musicSourceIntro1.PlayScheduled(startTime);
                musicSource1.PlayScheduled(startTime + introDuration);
            } else { // Otherwise, just play the song.
                musicSource1.clip = song[0];
                musicSource1.Play(); musicSource2.loop = false;
            }

            while (timeElapsed < timeToFade) {
                musicSource1.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                musicSourceIntro1.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                musicSource2.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                musicSourceIntro2.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            musicSourceIntro2.Stop();
            musicSource2.Stop(); musicSource2.loop = false;
        }
    }

    public enum BattleSongParts {
        Intro,
        Loop,
        Win,
        Loss,
    }

    public enum SoundEffects {
        Shoot,
        EnemyHit,
        PlayerHurt,
        Upgrade
    }
}
