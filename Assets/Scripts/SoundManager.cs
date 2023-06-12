using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour {

	public static SoundManager instance;

	public AudioSource audioSource;
    public AudioClip hit, win, lose, cash, ball, glide, button;
    float pitchTimeOut = 1;

    void Awake(){
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (pitchTimeOut > 0)
        {           
            pitchTimeOut -= 0.05f;
        }
        else
        {
            Spawner.isFeverDecrease = true;
            audioSource.pitch = 1;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
	}

    public void PlaySoundPitch(AudioClip clip)
    {
        pitchTimeOut = 2;
        audioSource.pitch += 0.1f;
        Spawner.instance.Scoring((int)((audioSource.pitch - 1) * 10));
        audioSource.PlayOneShot(clip);
    }
}
