using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 100;                                         // Quantidade máxima de vida.
    public int currentHealth;                                           // Vida atual do Boss.
    private BossHealthBarUI healthBarUI;                                // Referência para o script que controla a UI da barra de vida.
    private string bossName;                                            // Nome do Boss atual.

    public BossHealthBarUI HealthBarUI => healthBarUI;

    public int nextSceneID;                                             // ID da próxima cena a ser carregada após a morte.

    private readonly Dictionary<string, string> bossNameByScene = new Dictionary<string, string>()      // Dicionário que associa cenas aos nomes dos bosses.
    {
        { "BossTrice", "Triceratops" },
        { "BossFornalha", "Fornalha"},
    };

    void Start()
    {
        currentHealth = maxHealth;                                      // Define a vida atual como a vida máxima.
        BossHealthManager.Instance.OnBarSpawned += InitHealthBar;       // Registra o método InitHealthBar no evento OnBarSpawned para inicializar a UI assim que ela for criada.

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != "BossTrice")
        {
            BossHealthManager.Instance.SpawnBar();                          // Intância a barra de vida.
            BossHealthManager.Instance.ShowBar(0.5f);                       // Mostra a barra de vida.
        }
    }

    public void InitHealthBar(BossHealthBarUI barUI)                   // Método para inicializar a barra de vida com base na cena atual e associa ao nome do boss.
    {
        healthBarUI = barUI;

        string sceneName = SceneManager.GetActiveScene().name;          // Obtém o nome da cena atual.

        if (!bossNameByScene.TryGetValue(sceneName, out bossName))      // Tenta encontrar o nome do boss com base na cena atual
        {
            bossName = "Boss Desconhecido";
            Debug.LogWarning($"Cena '{sceneName}' não encontrada no dicionário de nomes de boss.");
        }

        if (healthBarUI != null)                                        // Se a UI foi atribuída corretamente, define o nome nela.
        {
            healthBarUI.DefNameBoss(bossName);                          // Atualiza o nome no UI.
        }

        UpdateLifeBar();                                                // Chama o método para atualizar a barra de vida na tela com os valores atuais.

    }

    public void ApplyDamage(int amount)                                 // Aplica dano ao boss e verifica se ele deve morrer.
    {
        currentHealth -= amount;                                        // Reduz a vida.
        DamagePopUpGenerator.current.CreatePopUp(transform.position, amount.ToString(), Color.red);         // Exibe na tela o dano sofrido.
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);       // Garante que a vida fique entre 0 e o máximo.
        UpdateLifeBar();                                                // Atualiza a UI

        if (currentHealth <= 0)                                         // Se a vida chegou a 0, executa a morte do boss.
        {
            Die();
        }
    }

    private void UpdateLifeBar()                                        // Método para atualizar a barra de vida com base na vida atual e máxima.
    {
        if (healthBarUI != null)
        {
            healthBarUI.AlterarLifeBar(currentHealth, maxHealth);       // Passa os valores atualizados para o script da UI.
        }
    }

    private void Die()                                                  // Método de morte.
    {
        Debug.Log("Boss morreu!");
        BossHealthManager.Instance.DestroyBar();

        if (nextSceneID != 3)
            LoadingScreenManager.Instance.SwitchToScene(nextSceneID);
        else
            EndGameUI.instance?.WinScreen();

        Destroy(gameObject);
    }
}