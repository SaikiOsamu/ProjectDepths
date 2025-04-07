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

    public void SetSource(AudioSource _source)
    {
        source = _source;
        source.clip = clip;
        source.loop = loop;
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
    public static AudioManager instance;

    [SerializeField]
    Sound[] sounds;

    void Awake()
    {
        if(instance != null)
        {
            if(instance != this)
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
            //_go.transform.SetParent(this.transform);
            sounds[i].SetSource (_go.AddComponent<AudioSource>());
            
        }
        PlaySound("BGM_MainMenu");
    }

    public void PlaySound(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if(sounds[i].name == _name)
            {
                sounds[i].Play();
                return;
            }
        }
        // No sound with the name
        Debug.LogWarning("AudioManager: Sound not found: " + _name);
    }

    public void StopSound(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].Stop();
                return;
            }
        }
        // No sound with the name
        //Debug.LogWarning("AudioManager: Sound not found: " + _name);
    }

}
