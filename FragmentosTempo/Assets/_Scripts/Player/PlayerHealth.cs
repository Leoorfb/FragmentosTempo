using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] public int maxHealth = 100;            // Define a vida máxima do jogador.
    [SerializeField] private int currentHealth;             // Variável para armazenar a vida atual do jogador.
    [SerializeField] private Image healthBarImage;          // Referência da imagem da barra de vida.

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;                          // Inicializa a vida atual do jogador com a saúde máxima.
        UpdateHealthUI();                                   // Atualiza a UI com o valor inicial da vida.
    }

    // Teste de dano.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            TakeDamage(10);
        }
    } // Fim teste de dano.

    public void TakeDamage(int damage)                      // Método para aplicar dano ao jogador.
    {
        currentHealth -= damage;                                        // Subtrai o valor do dano da vida atual.
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);       // Garante que a vida não passe de 0 ou da vida máxima.
        UpdateHealthUI();                                               // Atualiza a barra de vida na UI.

        if (currentHealth <= 0)                                         // Se a vida do jogador chegar a 0 chama o método de morte. 
        {
            Die();                                                      // Chama o método para lidar com a morte do jogador.
        }
    }

    void UpdateHealthUI()                                   // Atualiza a imagem da barra de vida de acordo com a porcentagem atual de vida.
    {
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = (float)currentHealth / maxHealth;       // Preenche a imagem proporcional à vida atual.
        }
    }

    void Die()                                              // Método para lidar com a morte do jogador.
    {
        Destroy(gameObject);
        Debug.Log("Jogador morreu!");
    }
}
