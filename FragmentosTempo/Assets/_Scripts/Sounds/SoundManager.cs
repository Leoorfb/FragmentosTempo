using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;                                    // Instância única para acesso global.

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
}
