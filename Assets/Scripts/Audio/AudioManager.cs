using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0f, 2f)]
    public float pitch = 1f;

    [Range(0f, 0.5f)]
    public float volumeRandomness = 0.1f;
    [Range(0f, 0.5f)]
    public float pitchRandomness = 0.1f;

    public bool loop = false;

    private AudioSource source;

    public void Mute(bool mute)
    {
        if (source != null)
        {
            source.mute = mute;
        }
    }

    public void SetSource(AudioSource _source)
    {
        source = _source;
        source.clip = clip;
        source.loop = loop;
        source.spatialBlend = 0f; // Make sure it's 2D audio for BGM
        source.playOnAwake = false; // Don't play until explicitly called

        // For background music, set priority to high (low number)
        if (loop)
        {
            source.priority = 0;
        }
    }

    public void Play()
    {
        source.volume = volume * (1 + Random.Range(-volumeRandomness / 2f, volumeRandomness / 2f));
        source.pitch = pitch * (1 + Random.Range(-pitchRandomness / 2f, pitchRandomness / 2f));
        source.Play();
    }

    public void Stop()
    {
        // Add a null check before stopping
        if (source != null)
        {
            source.Stop();
        }
    }

    // Added method to check if sound is currently playing
    public bool IsPlaying()
    {
        return source != null && source.isPlaying;
    }

    // Added method to get/set volume directly
    public float GetVolume()
    {
        return source != null ? source.volume : 0f;
    }

    public void SetVolume(float newVolume)
    {
        if (source != null)
        {
            source.volume = newVolume;
        }
    }
}

public class AudioManager : MonoBehaviour
{
    private bool isMuted = false;

    // Track currently playing BGMs
    private string[] activeBGMs = new string[2]; // We'll only track up to 2 active BGMs

    // Special BGM groups
    private readonly string BGM_MAIN = "BGM_MainMenu";
    private readonly string BGM_PLAY = "BGM_PlayScene";
    private readonly string BGM_PAUSE = "BGM_PauseMenu";

    public static AudioManager instance;

    [SerializeField]
    Sound[] sounds;

    void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Destroy(this.gameObject);
            }
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    void Start()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            GameObject _go = new GameObject("Sound_" + i + "_" + sounds[i].name);
            // transform sound to the playerCharacter to make it cleaner
            _go.transform.SetParent(this.transform);
            sounds[i].SetSource(_go.AddComponent<AudioSource>());
        }
    }

    public void MuteAll()
    {
        isMuted = true;
        foreach (var sound in sounds)
        {
            sound.Mute(true);
        }
    }

    public void UnmuteAll()
    {
        isMuted = false;
        foreach (var sound in sounds)
        {
            sound.Mute(false);
        }
    }

    public void ToggleMute()
    {
        if (isMuted)
        {
            UnmuteAll();
        }
        else
        {
            MuteAll();
        }
    }

    public void PlaySound(string _name)
    {
        bool isBGM = _name.StartsWith("BGM_");

        if (isBGM)
        {
            // Handle BGM special cases
            HandleBGMPlayback(_name);
        }
        else
        {
            // Regular sound effect - play normally
            for (int i = 0; i < sounds.Length; i++)
            {
                if (sounds[i].name == _name)
                {
                    sounds[i].Play();
                    return;
                }
            }

            // No sound with the name
            Debug.LogWarning("AudioManager: Sound not found: " + _name);
        }
    }

    private void HandleBGMPlayback(string bgmName)
    {
        // 1. If MainMenu BGM is requested, stop any gameplay music
        if (bgmName == BGM_MAIN)
        {
            StopSound(BGM_PLAY);
            StopSound(BGM_PAUSE);
            PlayBGM(bgmName);

            // Update tracking
            activeBGMs[0] = bgmName;
            activeBGMs[1] = null;
        }
        // 2. If Play or Pause Scene BGM is requested
        else if (bgmName == BGM_PLAY || bgmName == BGM_PAUSE)
        {
            // Stop main menu music if it's playing
            StopSound(BGM_MAIN);

            // Play the requested BGM without stopping the other gameplay BGM
            PlayBGM(bgmName);

            // Update tracking - find the next available slot or replace same type
            if (bgmName == activeBGMs[0] || activeBGMs[0] == null)
            {
                activeBGMs[0] = bgmName;
            }
            else if (bgmName == activeBGMs[1] || activeBGMs[1] == null)
            {
                activeBGMs[1] = bgmName;
            }
            else
            {
                // Both slots are taken by different BGMs - replace the one that isn't the other gameplay BGM
                if (activeBGMs[0] != BGM_PLAY && activeBGMs[0] != BGM_PAUSE)
                {
                    activeBGMs[0] = bgmName;
                }
                else
                {
                    activeBGMs[1] = bgmName;
                }
            }
        }
        // 3. Any other BGM - treat as exclusive
        else
        {
            // Stop all other BGMs
            for (int i = 0; i < activeBGMs.Length; i++)
            {
                if (activeBGMs[i] != null)
                {
                    StopSound(activeBGMs[i]);
                    activeBGMs[i] = null;
                }
            }

            PlayBGM(bgmName);
            activeBGMs[0] = bgmName;
        }
    }

    private void PlayBGM(string bgmName)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == bgmName)
            {
                // Only play if not already playing
                if (!sounds[i].IsPlaying())
                {
                    sounds[i].Play();
                }
                return;
            }
        }

        // BGM not found
        Debug.LogWarning("AudioManager: BGM not found: " + bgmName);
    }

    public void StopSound(string _name)
    {
        // Update tracking if it's a BGM
        if (_name != null && _name.StartsWith("BGM_"))
        {
            for (int i = 0; i < activeBGMs.Length; i++)
            {
                if (activeBGMs[i] == _name)
                {
                    activeBGMs[i] = null;
                }
            }
        }

        // Find the sound and stop it if it exists
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].Stop();
                return;
            }
        }

        // Optional: You can add debug logging if the sound wasn't found
        Debug.LogWarning("Sound not found to stop: " + _name);
    }

    // New method to check if a sound is playing
    public bool IsSoundPlaying(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                return sounds[i].IsPlaying();
            }
        }
        return false;
    }

    // New method to adjust volume of a specific sound
    public void SetSoundVolume(string _name, float volume)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].SetVolume(volume);
                return;
            }
        }
    }
}