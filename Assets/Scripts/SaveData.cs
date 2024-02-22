using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    private float audioVolume;
    private bool soundIsOn;
    private bool musicIsOn;

    [SerializeField]
    private List<AudioSource> allSoundFX;
    [SerializeField]
    private List<AudioSource> musicClips;

    [SerializeField]
    private MusicLooper mainLevelMusic;
    [SerializeField]
    private MusicLooper puzzleMusic;

    void Awake()
    {
        LoadOptionPreferences();
        SetVolume();
    }

    public void UpdateObjectsLive(float newVolume, bool soundEnabled, bool musicEnabled) {
        foreach (AudioSource soundSource in allSoundFX) {
            soundSource.volume = newVolume;
            soundSource.mute = !soundEnabled;
        }
        foreach (AudioSource musicSource in musicClips) {
            musicSource.volume = newVolume;
            musicSource.mute = !musicEnabled;
        }
    }

    void LoadOptionPreferences() {
        audioVolume = PlayerPrefs.GetFloat("VolumeValue");
        int soundSetter = PlayerPrefs.GetInt("SoundOn");
        if (soundSetter == 0) {
            soundIsOn = false;
        } else {
            soundIsOn = true;
        }
        int musicSetter = PlayerPrefs.GetInt("MusicOn");
        if (musicSetter == 0) {
            musicIsOn = false;
        } else {
            musicIsOn = true;
        }
    }

    void SetVolume() {
        if (mainLevelMusic != null) {
            mainLevelMusic.maxVolume = audioVolume;
        }
        if (puzzleMusic != null) {
            puzzleMusic.maxVolume = audioVolume;
        }
        foreach (AudioSource soundSource in allSoundFX) {
            soundSource.volume = audioVolume;
            if (!soundIsOn) {
                soundSource.mute = true;
            } else {
                soundSource.mute = false;
            }
        }
        foreach(AudioSource musicSource in musicClips) {
            musicSource.volume = audioVolume;
            if (!musicIsOn) {
                musicSource.mute = true;
            } else {
                musicSource.mute = false;
            }
        }
    }

    public void AddSoundEffects(AudioSource[] sounds) {
        foreach(AudioSource sound in sounds) {
            allSoundFX.Add(sound);
        }
        SetVolume();
    }

    public void RemoveSoundEffects(AudioSource[] sounds) {
        foreach (AudioSource sound in sounds) {
            allSoundFX.Remove(sound);
        }
    }
}
