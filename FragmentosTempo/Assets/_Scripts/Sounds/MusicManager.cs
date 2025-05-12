using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;                            // Instância única (Singleton) para acesso global.

    [SerializeField] private MusicLibrary musicLibrary;             // Referência ao script com a biblioteca de faixas de música
    [SerializeField] private AudioSource musicSource;               // Componente que reproduz a música.

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);                                    // Destroi duplicatas se já houver uma instância ativa.
        }
        else
        {
            Instance = this;                                        // Mantém o objeto ao trocar de cena.
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)          // Método para tocar uma música pelo nome, com transição suave (fade).
    {
        StartCoroutine(AnimateMusicCrossfade(musicLibrary.GetClipFromName(trackName), fadeDuration));       // Inicia a transição suave para a nova música.
    }

    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)       // Corrotina para fazer a transição suave entre a música atual e uma nova.
    {
        float percent = 0;
        while (percent < 1)                                         // Fade out da música atual.
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(1f, 0, percent);        // volume de 1 até 0.
            yield return null;
        }

        musicSource.clip = nextTrack;                               // Troca para a nova faixa.
        musicSource.Play();

        percent = 0;
        while (percent < 1)                                         // Fade in da nova música.
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1f, percent);        // volume de 0 até 1
            yield return null;
        }
    }

}
