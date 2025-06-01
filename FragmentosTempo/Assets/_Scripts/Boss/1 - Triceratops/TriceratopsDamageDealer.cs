using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriceratopsDamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int chargeDamage = 20;                                 // Define o valor de dano causado pela investida.
    [SerializeField] private int tailDamage = 10;                                   // Define o valor de dano causado pelo golpe de cauda.
    [SerializeField] private int earthquakeDamage = 30;                             // Define o valor de dano causado pelo terremoto.

    public void DealChargeDamage(GameObject target)                                 // Método para causar dano da investida no jogador.
    {
        var playerHealth = target.GetComponent<PlayerHealth>();                     // Obtém o componente PlayerHealth do alvo.
        if (playerHealth != null)                                                   // Se o componente PlayerHealth for encontrado:
        {
            playerHealth.TakeDamage(chargeDamage);                                  // Aplica o dano de investida ao jogador.
            Debug.Log("Jogador tomou " + chargeDamage + " de dano de investida!");
        }
    }

    public void DealTailDamage(GameObject target)                                   // Método para causar dano do golpe de cauda no jogador.
    {
        var playerHealth = target.GetComponent<PlayerHealth>();                     // Obtém o componente PlayerHealth do alvo.
        if (playerHealth != null)                                                   // Se o componente PlayerHealth for encontrado:
        {
            playerHealth.TakeDamage(tailDamage);                                    // Aplica o dano do golpe de cauda ao jogador.
            Debug.Log("Jogador tomou " + tailDamage + " de dano do golpe de cauda!");
        }
    }

    public void DealEarthquakeDamage(Vector3 epicenter)                             // Método para causar dano de terremoto ao redor de um ponto de epicentro.
    {
        Collider[] hitColliders = Physics.OverlapSphere(epicenter, 12f, LayerMask.GetMask("Player"));       // Detecta todos os coliders dentro de um raio de 12 unidades a partir do epicentro, com a camada "Player".

        foreach (var hit in hitColliders)                                           // Itera sobre cada colididor detectado.
        {
            if (hit.CompareTag("Player"))                                           // Se o colididor for um jogador:
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();       // Obtém o componente PlayerHealth do jogador.
                if (playerHealth != null)                                           // Se o componente PlayerHealth for encontrado:
                {
                    playerHealth.TakeDamage(earthquakeDamage);                      // Aplica o dano do terremoto ao jogador.
                    Debug.Log("Jogador tomou " + earthquakeDamage + " de dano do terremoto!");
                }
            }
        }
    }
}
