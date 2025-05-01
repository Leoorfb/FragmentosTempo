using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 100;                                         // Quantidade máxima de vida.
    private int currentHealth;                                          // Vida atual do Boss.
    private BossHealthBarUI healthBarUI;                                // Referência para o script que controla a UI da barra de vida.

    void Start()
    {
        currentHealth = maxHealth;                                      // Define a vida atual como a vida máxima.
        healthBarUI = BossHealthManager.Instance.SpawnBar();            // Instancia a barra de vida e retorna a referência dela.
        UpdateLifeBar();                                                // Chama o método para atualizar a barra de vida na tela com os valores atuais.
    }

    void Update()                                                       // Inicio teste de dano.
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyDamage(10);
        }
    }

    public void ApplyDamage(int amount)                                 // Fim teste de dano.
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateLifeBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateLifeBar()                                        // Método para atualizar a barra de vida com base na vida atual e máxima.
    {
        if (healthBarUI != null)
        {
            healthBarUI.AlterarLifeBar(currentHealth, maxHealth);
        }
    }

    private void Die()                                                  // Teste de morte.
    {
        Debug.Log("Boss morreu!");
        BossHealthManager.Instance.DestroyBar();
        Destroy(gameObject);
    }
}