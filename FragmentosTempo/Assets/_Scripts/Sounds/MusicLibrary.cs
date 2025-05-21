using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string trackName;                                // Nome identificador da música
    public AudioClip clip;                                  // Referência ao arquivo de áudio (AudioClip)
}

public class MusicLibrary : MonoBehaviour
{
    public MusicTrack[] tracks;                             // Array de faixas de música disponíveis na biblioteca
    public AudioClip GetClipFromName(string trackName)      // Método para retornar o AudioClip correspondente ao nome informado.
    {
        foreach (var track in tracks)                       // Percorre todas as faixas disponíveis.
        {
            if (track.trackName == trackName)               // Compara o nome informado com o nome da faixa atual.
            {
                return track.clip;                          // Retorna o clip correspondente.
            }
        }

        return null;                                        // Retorna null se nenhuma faixa com o nome for encontrada.
    }
}
