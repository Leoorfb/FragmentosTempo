using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerFornalha : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip spawnSound; public AudioSource audioSource;

    void Start()
    {
        // Toca som ao aparecer
        if (spawnSound && audioSource)
            audioSource.PlayOneShot(spawnSound);
    }

}
