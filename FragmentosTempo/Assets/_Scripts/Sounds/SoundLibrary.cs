using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SoundEffect
{
    public string groupID;                                  // Identificador do grupo de efeitos sonoros.
    public AudioClip[] clips;                               // Lista de clipes de �udio pertencentes ao grupo.
}

public class SoundLibrary : MonoBehaviour
{
    public SoundEffect[] soundEffects;                      // Lista de todos os efeitos sonoros dispon�veis.

    public AudioClip GetClipFromName(string name)           // M�todo para retornar um clipe de �udio aleat�rio do grupo especificado.
    {
        foreach (var soundEffect in soundEffects)           // Procura pelo grupo com o nome especificado.
        {
            if (soundEffect.groupID == name)
            {
                return soundEffect.clips[Random.Range(0, soundEffect.clips.Length)];        // Retorna um clipe aleat�rio da lista do grupo.
            }
        }

        return null;                                        // Retorna null se o grupo n�o for encontrado.
    }
}
