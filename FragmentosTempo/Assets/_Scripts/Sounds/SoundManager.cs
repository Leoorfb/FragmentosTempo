using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;                                    // Inst�ncia �nica para acesso global.

    private AudioSource loopAudioSource;                                    // Som de Loop.

    [SerializeField] private SoundLibrary sfxLibrary;                       // Biblioteca de efeitos sonoros dispon�veis.
    [SerializeField] private AudioSource sfx2DSource;                       // Fonte de �udio usada para sons 2D.

    private void Awake()
    {
        if (Instance != null)                                               // Garante que apenas uma inst�ncia exista na cena.
        {
            Destroy(gameObject);                                            // Destroi esta inst�ncia se j� houver outra.
        }
        else
        {
            Instance = this;                                                // Define esta como a inst�ncia principal.
            DontDestroyOnLoad(gameObject);                                  // Mant�m o objeto entre mudan�as de cena.
        }
    }

    private void Start()
    {
        loopAudioSource = gameObject.AddComponent<AudioSource>();           // Cria um AudioSource para sons em loop.
        loopAudioSource.spatialBlend = 1f;                                  // Define como som 3D (1 = totalmente 3D).
        loopAudioSource.loop = true;                                        // Configura para repetir automaticamente.
    }

    public void PlaySound3D(AudioClip clip, Vector3 pos)                    // M�todo para tocar um som 3D na posi��o especificada.
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos);                         // Toca o som 3D na posi��o fornecida.
        }
    }

    public void PlaySound3D(string soundName, Vector3 pos)                  // M�todo para tocar um som 3D usando o nome do efeito na biblioteca.
    {
        PlaySound3D(sfxLibrary.GetClipFromName(soundName), pos);            // Busca e toca o som.
    }

    public void PlaySound2D(string soundName)                               // M�todo para tocar um som 2D na fonte de �udio dedicada.
    {
        sfx2DSource.PlayOneShot(sfxLibrary.GetClipFromName(soundName));     // Toca o som 2D.
    }

    public AudioClip GetClipByName(string soundName)                        // M�todo para obter um clipe de �udio pelo nome do grupo.
    {
        return sfxLibrary.GetClipFromName(soundName);                       // Retorna um clipe aleat�rio do grupo especificado.
    }

    public void PlayLoop3D(string soundName, Vector3 pos)                   // M�todo para tocar um som 3D em loop.
    {
        AudioClip clip = sfxLibrary.GetClipFromName(soundName);             // Busca o clipe correspondente.
        if (clip != null)                                                   // Verifica se encontrou o clip.
        {
            loopAudioSource.clip = clip;                                    // Define o clipe no AudioSource de loop.
            loopAudioSource.transform.position = pos;                       // Define a posi��o 3D do som.
            if (!loopAudioSource.isPlaying)                                 // Se n�o estiver tocando ainda, come�a a tocar em loop.
            {
                loopAudioSource.Play();
            }
        }
    }

    public void StopLoop3D()                                                // M�todo para parar o som em loop.
    {
        if (loopAudioSource.isPlaying)                                      // Se estiver tocando, para o som.
        {
            loopAudioSource.Stop();
        }
    }

    public void UpdateLoop3DPosition(Vector3 pos)                           // M�todo para atualizar a posi��o do som em loop.
    {
        if (loopAudioSource != null && loopAudioSource.isPlaying)           // Se o AudioSource existe e est� tocando, atualiza a posi��o.
        {
            loopAudioSource.transform.position = pos;
        }
    }
}
