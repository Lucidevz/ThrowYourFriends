using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MusicLooper : MonoBehaviour
{
    [SerializeField]
    private AudioSource introClip;
    [SerializeField]
    private AudioSource loopingClip;
    [SerializeField]
    private AudioSource loopingClip2;

    [SerializeField]
    private float delayAfterIntro;
    [SerializeField]
    private float loopDelay;

    public float maxVolume;

    private float resumeMusicAfterPause;

    [SerializeField]
    private PauseMenu pauseMenu;

    public enum LoopingTypes {
        dontPlayAnything,
        loop1Clip, // Loops the same cip over and over again
        introWithLoopingClip // Plays an intro, then follows it up with a clip that loops infinitely
    }

    public LoopingTypes loopType;

    public bool dontPlayOnStart;

    void Start()
    {
        StopAllMusic();
        maxVolume = 1;
        resumeMusicAfterPause = delayAfterIntro;
        if (!dontPlayOnStart) {
            if (loopType == LoopingTypes.loop1Clip) {
                StartCoroutine(LoopClip1WithDelay(loopDelay));
            } else if (loopType == LoopingTypes.introWithLoopingClip) {
                introClip.Play();
                StartCoroutine(LoopClip1WithDelay(delayAfterIntro));
            }
        }
    }

    private void Update() {
        if (pauseMenu != null && !pauseMenu.IsTheGamePaused()) {
            resumeMusicAfterPause -= Time.deltaTime;
        }
    }

    public void StartFromIntro() {
        StartCoroutine(PlayIntroClip());
    }

    public void StartMusicLoop(float delay) {
        StartCoroutine(LoopClip1WithDelay(delay));
    }

    public void StopAllMusic() {
        loopingClip.Stop();
        loopingClip2.Stop();
        if (introClip != null) {
            introClip.Stop();
        }
        StopAllCoroutines();
    }

    public void PauseAllMusic() {
        loopingClip.Pause();
        loopingClip2.Pause();
        if (introClip != null) {
            introClip.Pause();
        }
        StopAllCoroutines();
    }

    public void ResumeAllMusic() {
        loopingClip.UnPause();
        loopingClip2.UnPause();
        if (introClip != null) {
            introClip.UnPause();
        }
        if (introClip != null) {
            if (introClip.isPlaying) {
                StartCoroutine(LoopClip1WithDelay(resumeMusicAfterPause));
            }
        }
        if (loopingClip.isPlaying) {
            StartCoroutine(LoopClip2WithDelay(resumeMusicAfterPause));
        }
        if (loopingClip2.isPlaying) {
            StartCoroutine(LoopClip1WithDelay(resumeMusicAfterPause));
        }
    }

    IEnumerator PlayIntroClip() {
        introClip.Play();
        yield return null;
        StartCoroutine(LoopClip1WithDelay(delayAfterIntro));
    }

    IEnumerator LoopClip1WithDelay(float delay) {
        resumeMusicAfterPause = delay;
        yield return new WaitForSeconds(delay);
        loopingClip.Play();
        StartCoroutine(LoopClip2WithDelay(loopDelay));
    }

    IEnumerator LoopClip2WithDelay(float delay) {
        resumeMusicAfterPause = delay;
        yield return new WaitForSeconds(delay);
        loopingClip2.Play();
        StartCoroutine(LoopClip1WithDelay(loopDelay));
    }

    public void FadeInMusic(float duration) {
        StartCoroutine(FadeInAudio(duration));
    }

    public void FadeOutMusic(float duration) {
        StartCoroutine(FadeOutAudio(duration));
    }

    public void SetAllAudioVolume(float volume) {
        if(volume > maxVolume) {
            volume = maxVolume;
        }
        loopingClip.volume = volume;
        loopingClip2.volume = volume;
        if (introClip != null) {
            introClip.volume = volume;
        }
    }

    IEnumerator FadeInAudio(float duration) {
        // As all clips have same volume, I can use one clip to determine the volume of all of them
        while (loopingClip.volume < maxVolume) {
            // Only fade the intro if this looper script has an intro as part of it
            if (introClip != null) {
                introClip.volume += Time.deltaTime / duration;
            }
            loopingClip.volume += Time.deltaTime / duration;
            loopingClip2.volume += Time.deltaTime / duration;
            yield return new WaitForSeconds(Time.deltaTime / duration);
        }
        yield return null;
    }

    IEnumerator FadeOutAudio(float duration) {
        // As all clips have same volume, I can use one clip to determine the volume of all of them
        while (loopingClip.volume >= 0) {
            // Only fade the intro if this looper script has an intro as part of it
            if(introClip != null) {
                introClip.volume -= Time.deltaTime / duration;
            }
            loopingClip.volume -= Time.deltaTime / duration;
            loopingClip2.volume -= Time.deltaTime / duration;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        loopingClip.Stop();
        loopingClip2.Stop();
        if (introClip != null) {
            introClip.Stop();
        }
        yield return null;
    }
}
