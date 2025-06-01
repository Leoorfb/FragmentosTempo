using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;                                    // Instância única para acesso global.

    private AudioSource loopAudioSource;                                    // Som de Loop.

    [SerializeField] private SoundLibrary sfxLibrary;                       // Biblioteca de efeitos sonoros disponíveis.
    [SerializeField] private AudioSource sfx2DSource;                       // Fonte de áudio usada para sons 2D.

    private void Awake()
    {
        if (Instance != null)                                               // Garante que apenas uma instância exista na cena.
        {
            Destroy(gameObject);                                            // Destroi esta instância se já houver outra.
        }
        else
        {
            Instance = this;                                                // Define esta como a instância principal.
            DontDestroyOnLoad(gameObject);                                  // Mantém o objeto entre mudanças de cena.
        }
    }

    private void Start()
    {
        loopAudioSource = gameObject.AddComponent<AudioSource>();           // Cria um AudioSource para sons em loop.
        loopAudioSource.spatialBlend = 1f;                                  // Define como som 3D (1 = totalmente 3D).
        loopAudioSource.loop = true;                                        // Configura para repetir automaticamente.
    }

    public void PlaySound3D(AudioClip clip, Vector3 pos)                    // Método para tocar um som 3D na posição especificada.
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos);                         // Toca o som 3D na posição fornecida.
        }
    }

    public void PlaySound3D(string soundName, Vector3 pos)                  // Método para tocar um som 3D usando o nome do efeito na biblioteca.
    {
        PlaySound3D(sfxLibrary.GetClipFromName(soundName), pos);            // Busca e toca o som.
    }

    public void PlaySound2D(string soundName)                               // Método para tocar um som 2D na fonte de áudio dedicada.
    {
        sfx2DSource.PlayOneShot(sfxLibrary.GetClipFromName(soundName));     // Toca o som 2D.
    }

    public AudioClip GetClipByName(string soundName)                        // Método para obter um clipe de áudio pelo nome do grupo.
    {
        return sfxLibrary.GetClipFromName(soundName);                       // Retorna um clipe aleatório do grupo especificado.
    }

    public void PlayLoop3D(string soundName, Vector3 pos)                   // Método para tocar um som 3D em loop.
    {
        AudioClip clip = sfxLibrary.GetClipFromName(soundName);             // Busca o clipe correspondente.
        if (clip != null)                                                   // Verifica se encontrou o clip.
        {
            loopAudioSource.clip = clip;                                    // Define o clipe no AudioSource de loop.
            loopAudioSource.transform.position = pos;                       // Define a posição 3D do som.
            if (!loopAudioSource.isPlaying)                                 // Se não estiver tocando ainda, começa a tocar em loop.
            {
                loopAudioSource.Play();
            }
        }
    }

    public void StopLoop3D()                                                // Método para parar o som em loop.
    {
        if (loopAudioSource.isPlaying)                                      // Se estiver tocando, para o som.
        {
            loopAudioSource.Stop();
        }
    }

    public void UpdateLoop3DPosition(Vector3 pos)                           // Método para atualizar a posição do som em loop.
    {
        if (loopAudioSource != null && loopAudioSource.isPlaying)           // Se o AudioSource existe e está tocando, atualiza a posição.
        {
            loopAudioSource.transform.position = pos;
        }
    }
}
