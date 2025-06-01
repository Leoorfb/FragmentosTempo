using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;                                               // Referência ao AudioMixer que controla os volumes da música e efeitos sonoros.

    public Slider musicSlider;                                                  // Slider para controlar o volume da música.
    public Slider sfxSlider;                                                    // Slider para controlar o volume dos efeitos sonoros.

    private void Start()
    {
        LoadVolume();                                                           // Carrega os volumes salvos previamente.
        MusicManager.Instance.PlayMusic("Main Menu");                           // Toca a música do menu principal ao iniciar.
    }

    public void Play()                                                          // Método chamado quando o jogador clica no botão "Play".
    {
        LoadingScreenManager.Instance.SwitchToScene(1);                         // Inicia a troca de cena para a cena de índice 1, utilizando a tela de carregamento.        
    }

    public void Quit()                                                          // Método chamado quando o jogador clica no botão "Quit".
    {
        Application.Quit();                                                     // Encerra a aplicação.
        Debug.Log("Jogo encerrado!");
    }

    public void UpdateMusicVolume(float volume)                                 // Método para atualizar o volume da música com base no valor do slider.
    {
        audioMixer.SetFloat("MusicVolume", volume);                             // Atualiza o volume da música no AudioMixer.
    }

    public void UpdateEffectsVolume(float volume)                               // Método para atualizar o volume dos efeitos sonoros com base no valor do slider.
    {
        audioMixer.SetFloat("SFXVolume", volume);                               // Atualiza o volume dos efeitos sonoros no AudioMixer.
    }

    public void SaveVolume()                                                    // Método para salvar os volumes atuais de música e efeitos sonoros.
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);              // Obtém o volume atual da música.
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);                       // Salva o volume da música usando PlayerPrefs.

        audioMixer.GetFloat("SFXVolume", out float sfxVolume);                  // Obtém o volume atual dos efeitos sonoros.
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);                           // Salva o volume dos efeitos sonoros usando PlayerPrefs.
    }

    public void LoadVolume()                                                    // Método para carregar os volumes salvos ao iniciar o menu.
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");                // Define o valor do slider de música com o volume salvo.
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");                    // Define o valor do slider de efeitos sonoros com o volume salvo.
    }
}
