using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string trackName;                                // Nome identificador da m�sica
    public AudioClip clip;                                  // Refer�ncia ao arquivo de �udio (AudioClip)
}

public class MusicLibrary : MonoBehaviour
{
    public MusicTrack[] tracks;                             // Array de faixas de m�sica dispon�veis na biblioteca
    public AudioClip GetClipFromName(string trackName)      // M�todo para retornar o AudioClip correspondente ao nome informado.
    {
        foreach (var track in tracks)                       // Percorre todas as faixas dispon�veis.
        {
            if (track.trackName == trackName)               // Compara o nome informado com o nome da faixa atual.
            {
                return track.clip;                          // Retorna o clip correspondente.
            }
        }

        return null;                                        // Retorna null se nenhuma faixa com o nome for encontrada.
    }
}
