using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;                                    // Inst�ncia �nica para acesso global.

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
}
