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
        // Can further add more effects here: time fade out
        source.Stop();
    }
}

public class AudioManager : MonoBehaviour
{
    private bool isMuted = false;
    private string currentBGM = ""; // Track current BGM

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
            DontDestroyOnLoad(this); // Uncommented this line
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

            // Optional debug log
            // Debug.Log("Initialized sound: " + sounds[i].name + " | Loop: " + sounds[i].loop);
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
        // Check if this is a BGM (assuming BGM names start with "BGM_")
        bool isBGM = _name.StartsWith("BGM_");

        // If this is a BGM, stop the current BGM if there is one
        if (isBGM && !string.IsNullOrEmpty(currentBGM))
        {
            StopSound(currentBGM);
        }

        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].Play();

                // If this is a BGM, remember it as the current BGM
                if (isBGM)
                {
                    currentBGM = _name;
                }

                return;
            }
        }

        // No sound with the name
        Debug.LogWarning("AudioManager: Sound not found: " + _name);
    }

    public void StopSound(string _name)
    {
        // If we're stopping the current BGM, clear the currentBGM variable
        if (_name == currentBGM)
        {
            currentBGM = "";
        }

        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].Stop();
                return;
            }
        }

        // Optional warning for missing sounds
        // Debug.LogWarning("AudioManager: Sound not found: " + _name);
    }
}