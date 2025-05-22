using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotionPickup : MonoBehaviour
{
    [Header("Potion Settings")]
    public float rotationSpeed = 80f;                                           // Velocidade de rota��o em graus por segundos.
    public float floatAmplitude = 0.25f;                                        // Altura m�xima que a po��o sobe/desce.
    public float floatFrequency = 6f;                                           // Velocidade do movimento de sobe/desce.

    private Vector3 startPos;                                                   // Posi��o inicial da po��o.

    private void Start()
    {
        startPos = transform.position;                                          // Armazena a posi��o incial da po��o.
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);           // Rota��o cont�nua.

        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;       // Movimento de sobe e desce.
        transform.position = new Vector3(transform.position.x, newY, startPos.z);
    }

    private void OnTriggerEnter(Collider other)                                 // M�todo chamado automaticamente quando outro Collider entra em contato com o Collider deste objeto.
    {
        if (other.CompareTag("Player"))                                         // Verifica se o objeto que colidiu possui a tag "Player".
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();     // Tenta obter o componente PlayerHealth do objeto que colidiu.

            if (playerHealth != null)                                           // Se o jogador tiver o script PlayerHealth, adiciona uma po��o.
            {
                playerHealth.AddPotion();                                       // Adiciona uma po��o ao invent�rio.
                Destroy(gameObject);                                            // Destroi o item ap�s a coleta.
            }
        }
    }
}
