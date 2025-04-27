using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] public int maxHealth = 100;            // Define a vida máxima do jogador.
    [SerializeField] private int currentHealth;             // Variável para armazenar a vida atual do jogador.

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;                          // Inicializa a vida atual do jogador com a saúde máxima.
    }

    public void TakeDamage(int damage)                      // Método para aplicar dano ao jogador.
    {
        currentHealth -= damage;                            // Subtrai o valor do dano da vida atual.
        if (currentHealth <= 0)                             // Se a vida do jogador chegar a 0 chama o método de morte. 
        {
            Die();                                          // Chama o método para lidar com a morte do jogador.
        }
    }

    void Die()                                              // Método para lidar com a morte do jogador.
    {
        Destroy(gameObject);
        Debug.Log("Jogador morreu!");
    }
}
