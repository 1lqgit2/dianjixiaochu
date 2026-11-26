using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioSource winSound;

    public AudioSource clickSound;

    public AudioSource bounceSound;

    public AudioSource rotateSound;

    public AudioSource sawSound;

    public AudioSource coinSound;

    public AudioSource bombSound;

    public AudioSource hammerSound;

    public AudioSource backgroundMusic;

    public AudioSource[] comboSound;

    public AudioSource[] soundList;

    public int comboSoundIndex;

    public static AudioManager instance;

    [HideInInspector]
    public int musicState, soundState, hapticState;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("Music"))
            PlayerPrefs.SetInt("Music", 1);
        if (!PlayerPrefs.HasKey("Sound"))
            PlayerPrefs.SetInt("Sound", 1);
        if (!PlayerPrefs.HasKey("Haptic"))
            PlayerPrefs.SetInt("Haptic", 1);

        //music
        if (PlayerPrefs.GetInt("Music") == 1)
            musicState = 1;
        else musicState = 0;

        if (musicState == 1)
        {        
            ToogleMusic(true);
        }
        else
        {          
            ToogleMusic(false);
        }
        //sound
        if (PlayerPrefs.GetInt("Sound") == 1)
            soundState = 1;
        else soundState = 0;

        if (soundState == 1)
        {
            ToogleSound(true);
        }
        else
        {
            ToogleSound(false);
        }
        //haptic
        if (PlayerPrefs.GetInt("Haptic") == 1)
            hapticState = 1;
        else
            hapticState = 0;

        if (hapticState == 1)
        {
            ToogleHaptic(true);
        }
        else
        {
            ToogleHaptic(false);
        }


        if (FindObjectsOfType(typeof(AudioManager)).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        comboSoundIndex = 0;

        instance = this;

       
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToogleMusic(bool toogle)
    {
        if(toogle)
        {
            musicState = 1;
            backgroundMusic.volume = 0.5f;
            PlayerPrefs.SetInt("Music", 1);
        }
         
        else
        {
            musicState = 0;
            backgroundMusic.volume = 0.0f;
            PlayerPrefs.SetInt("Music", 0);
        }
           
    }

    public void ToogleSound(bool toogle)
    {
        if (toogle)
        {

            for (int i = 0; i < soundList.Length; i++)
                soundList[i].volume = 1.0f;
            soundState = 1;
            PlayerPrefs.SetInt("Sound", 1);
        }

        else
        {
            for (int i = 0; i < soundList.Length; i++)
                soundList[i].volume = 0.0f;
            soundState = 0;
            PlayerPrefs.SetInt("Music", 0);
        }
    }
    public void ToogleHaptic(bool toogle)
    {
        if (toogle)
        {
            hapticState = 1;
            PlayerPrefs.SetInt("Haptic", 1);
        }

        else
        {
            hapticState = 0;
            PlayerPrefs.SetInt("Haptic", 0);
        }

    }

   public void PlayComboSound()
    {
        comboSound[comboSoundIndex].Play();

        if(comboSoundIndex == comboSound.Length - 1)
        {
            comboSoundIndex = 0;
        }
        else
        {
            comboSoundIndex++;
        }
    }
}
