using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] public int maxHealth = 100;            // Define a vida m�xima do jogador.
    [SerializeField] private int currentHealth;             // Vari�vel para armazenar a vida atual do jogador.

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;                          // Inicializa a vida atual do jogador com a sa�de m�xima.
    }

    public void TakeDamage(int damage)                      // M�todo para aplicar dano ao jogador.
    {
        currentHealth -= damage;                            // Subtrai o valor do dano da vida atual.
        if (currentHealth <= 0)                             // Se a vida do jogador chegar a 0 chama o m�todo de morte. 
        {
            Die();                                          // Chama o m�todo para lidar com a morte do jogador.
        }
    }

    void Die()                                              // M�todo para lidar com a morte do jogador.
    {
        Destroy(gameObject);
        Debug.Log("Jogador morreu!");
    }
}
