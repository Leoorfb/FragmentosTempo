using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotionPickup : MonoBehaviour
{
    [Header("Potion Settings")]
    public float rotationSpeed = 80f;                                           // Velocidade de rotação em graus por segundos.
    public float floatAmplitude = 0.25f;                                        // Altura máxima que a poção sobe/desce.
    public float floatFrequency = 6f;                                           // Velocidade do movimento de sobe/desce.

    private Vector3 startPos;                                                   // Posição inicial da poção.

    private void Start()
    {
        startPos = transform.position;                                          // Armazena a posição incial da poção.
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);           // Rotação contínua.

        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;       // Movimento de sobe e desce.
        transform.position = new Vector3(transform.position.x, newY, startPos.z);
    }

    private void OnTriggerEnter(Collider other)                                 // Método chamado automaticamente quando outro Collider entra em contato com o Collider deste objeto.
    {
        if (other.CompareTag("Player"))                                         // Verifica se o objeto que colidiu possui a tag "Player".
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();     // Tenta obter o componente PlayerHealth do objeto que colidiu.

            if (playerHealth != null)                                           // Se o jogador tiver o script PlayerHealth, adiciona uma poção.
            {
                playerHealth.AddPotion();                                       // Adiciona uma poção ao inventário.
                Destroy(gameObject);                                            // Destroi o item após a coleta.
            }
        }
    }
}
