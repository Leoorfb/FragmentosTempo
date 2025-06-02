using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] public int maxHealth = 100;                // Define a vida máxima do jogador.
    [SerializeField] private int currentHealth;                 // Variável para armazenar a vida atual do jogador.
    [SerializeField] private Image healthBarImage;              // Referência da imagem da barra de vida.
    [SerializeField] private TextMeshProUGUI potionCountText;   // Referência do contador de poções de vida.

    [Header("Potion Settings")]
    [SerializeField] private int potionCount = 3;               // Quantidade inicial de poções.
    [SerializeField] private int potionHealAmount = 30;         // Quantidade de vida recuperada com a poção.
    private Color originalPotionTextColor;                      // Armazena a cor original do texto.

    [Header("VFX Settings")]
    [SerializeField] private GameObject vfxHeal;

    public bool isInvunerable = false;                          // Flag para verificar se o jogador está imune a dano.

    public int PotionCount => potionCount;                      // Retornar a quantidade atual de poções disponíveis.

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;                          // Inicializa a vida atual do jogador com a saúde máxima.

        if (potionCountText != null)                        // Salvar a cor original do texto de contagem de poções.
        {
            originalPotionTextColor = potionCountText.color;
        }

        UpdateHealthUI();                                   // Atualiza a UI com o valor inicial da vida.
        UpdatePotionUI();                                   // Atualiza a UI de poções.
    }

    public void TakeDamage(int damage)                      // Método para aplicar dano ao jogador.
    {
        if (isInvunerable)                                  // Verificar se está invunerável.
        {
            Debug.Log("Dano bloqueado pelo Dash");
            return;
        }

        currentHealth -= damage;                                        // Subtrai o valor do dano da vida atual.
        if (DamagePopUpGenerator.current != null)
            DamagePopUpGenerator.current.CreatePopUp(transform.position, damage.ToString(), Color.yellow);      // Exibe na tela o dano sofrido.
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);       // Garante que a vida não passe de 0 ou da vida máxima.
        UpdateHealthUI();                                               // Atualiza a barra de vida na UI.

        if (currentHealth <= 0)                                         // Se a vida do jogador chegar a 0 chama o método de morte. 
        {
            Die();                                                      // Chama o método para lidar com a morte do jogador.
        }
    }

    public bool UsePotion()                                             // Método para usar poções.
    {
        if (potionCount > 0 && currentHealth < maxHealth)               // Verifica se o jogador tem poções e se a vida atual está abaixo da máxima.
        {
            currentHealth += potionHealAmount;                          // Aumenta a vida com base no valor de cura da poção.
            DamagePopUpGenerator.current.CreatePopUp(transform.position, potionHealAmount.ToString(), Color.green);         // Exibe na tela a vida recuperada.
            SoundManager.Instance.PlaySound3D("DrinkPotion", transform.position);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);   // Garante que a vida não ultrapasse a máxima.
            potionCount--;                                              // Reduz o número de poções disponíveis.
            UpdateHealthUI();                                           // Atualiza a barra de vida na interface.
            Debug.Log("Poção usada!");

            if (vfxHeal != null)
            {
                Instantiate(vfxHeal, transform.position, Quaternion.identity);
            }

            UpdatePotionUI();                                           // Atualiza o texto com a quantidade de poções.
            return true;                                                // Retorna true indicando que a poção foi usada.
        }
        else
        {
            Debug.Log("Sem poções ou vida cheia.");
            return false;                                               // Retorna false se não pôde usar.
        }
    }

    public void AddPotion()                                             // Método para adicionar poções.
    {
        potionCount++;                                                  // Aumenta o número de poções.
        Debug.Log("Poção coletada! Total: " + potionCount);
        UpdatePotionUI();                                               // Atualiza a interface com a nova quantidade.
    }

    private void UpdatePotionUI()                                       // Método para atualizar o texto da UI com a quantidade de poções.
    {
        if (potionCountText != null)
        {
            potionCountText.text = "x" + potionCount;

            if (potionCount <= 0)                                       // Verificar se possui poções, caso não tenha, mudar a cor do texto para vermelho.
            {
                potionCountText.color = Color.red;
            }
            else
            {
                potionCountText.color = originalPotionTextColor;        // Restaura a cor original.
            }
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
        SoundManager.Instance.StopLoop3D();
        EndGameUI.instance?.GameOverScreen();
        Destroy(gameObject);
        Debug.Log("Jogador morreu!");
    }
}
