using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;                                               // Refer�ncia ao AudioMixer que controla os volumes da m�sica e efeitos sonoros.

    public Slider musicSlider;                                                  // Slider para controlar o volume da m�sica.
    public Slider sfxSlider;                                                    // Slider para controlar o volume dos efeitos sonoros.

    private void Start()
    {
        LoadVolume();                                                           // Carrega os volumes salvos previamente.
        MusicManager.Instance.PlayMusic("Main Menu");                           // Toca a m�sica do menu principal ao iniciar.
    }

    public void Play()                                                          // M�todo chamado quando o jogador clica no bot�o "Play".
    {
        LoadingScreenManager.Instance.SwitchToScene(1);                         // Inicia a troca de cena para a cena de �ndice 1, utilizando a tela de carregamento.        
    }

    public void Quit()                                                          // M�todo chamado quando o jogador clica no bot�o "Quit".
    {
        Application.Quit();                                                     // Encerra a aplica��o.
        Debug.Log("Jogo encerrado!");
    }

    public void UpdateMusicVolume(float volume)                                 // M�todo para atualizar o volume da m�sica com base no valor do slider.
    {
        audioMixer.SetFloat("MusicVolume", volume);                             // Atualiza o volume da m�sica no AudioMixer.
    }

    public void UpdateEffectsVolume(float volume)                               // M�todo para atualizar o volume dos efeitos sonoros com base no valor do slider.
    {
        audioMixer.SetFloat("SFXVolume", volume);                               // Atualiza o volume dos efeitos sonoros no AudioMixer.
    }

    public void SaveVolume()                                                    // M�todo para salvar os volumes atuais de m�sica e efeitos sonoros.
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);              // Obt�m o volume atual da m�sica.
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);                       // Salva o volume da m�sica usando PlayerPrefs.

        audioMixer.GetFloat("SFXVolume", out float sfxVolume);                  // Obt�m o volume atual dos efeitos sonoros.
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);                           // Salva o volume dos efeitos sonoros usando PlayerPrefs.
    }

    public void LoadVolume()                                                    // M�todo para carregar os volumes salvos ao iniciar o menu.
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");                // Define o valor do slider de m�sica com o volume salvo.
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");                    // Define o valor do slider de efeitos sonoros com o volume salvo.
    }
}
