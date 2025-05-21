using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;                            // Inst�ncia �nica (Singleton) para acesso global.

    [SerializeField] private MusicLibrary musicLibrary;             // Refer�ncia ao script com a biblioteca de faixas de m�sica
    [SerializeField] private AudioSource musicSource;               // Componente que reproduz a m�sica.

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);                                    // Destroi duplicatas se j� houver uma inst�ncia ativa.
        }
        else
        {
            Instance = this;                                        // Mant�m o objeto ao trocar de cena.
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)          // M�todo para tocar uma m�sica pelo nome, com transi��o suave (fade).
    {
        StartCoroutine(AnimateMusicCrossfade(musicLibrary.GetClipFromName(trackName), fadeDuration));       // Inicia a transi��o suave para a nova m�sica.
    }

    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)       // Corrotina para fazer a transi��o suave entre a m�sica atual e uma nova.
    {
        float percent = 0;
        while (percent < 1)                                         // Fade out da m�sica atual.
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(1f, 0, percent);        // volume de 1 at� 0.
            yield return null;
        }

        musicSource.clip = nextTrack;                               // Troca para a nova faixa.
        musicSource.Play();

        percent = 0;
        while (percent < 1)                                         // Fade in da nova m�sica.
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1f, percent);        // volume de 0 at� 1
            yield return null;
        }
    }

}
