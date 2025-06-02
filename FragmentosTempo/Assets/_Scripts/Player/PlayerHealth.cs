using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] public int maxHealth = 100;                // Define a vida m�xima do jogador.
    [SerializeField] private int currentHealth;                 // Vari�vel para armazenar a vida atual do jogador.
    [SerializeField] private Image healthBarImage;              // Refer�ncia da imagem da barra de vida.
    [SerializeField] private TextMeshProUGUI potionCountText;   // Refer�ncia do contador de po��es de vida.

    [Header("Potion Settings")]
    [SerializeField] private int potionCount = 3;               // Quantidade inicial de po��es.
    [SerializeField] private int potionHealAmount = 30;         // Quantidade de vida recuperada com a po��o.
    private Color originalPotionTextColor;                      // Armazena a cor original do texto.

    [Header("VFX Settings")]
    [SerializeField] private GameObject vfxHeal;

    public bool isInvunerable = false;                          // Flag para verificar se o jogador est� imune a dano.

    public int PotionCount => potionCount;                      // Retornar a quantidade atual de po��es dispon�veis.

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;                          // Inicializa a vida atual do jogador com a sa�de m�xima.

        if (potionCountText != null)                        // Salvar a cor original do texto de contagem de po��es.
        {
            originalPotionTextColor = potionCountText.color;
        }

        UpdateHealthUI();                                   // Atualiza a UI com o valor inicial da vida.
        UpdatePotionUI();                                   // Atualiza a UI de po��es.
    }

    public void TakeDamage(int damage)                      // M�todo para aplicar dano ao jogador.
    {
        if (isInvunerable)                                  // Verificar se est� invuner�vel.
        {
            Debug.Log("Dano bloqueado pelo Dash");
            return;
        }

        currentHealth -= damage;                                        // Subtrai o valor do dano da vida atual.
        if (DamagePopUpGenerator.current != null)
            DamagePopUpGenerator.current.CreatePopUp(transform.position, damage.ToString(), Color.yellow);      // Exibe na tela o dano sofrido.
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);       // Garante que a vida n�o passe de 0 ou da vida m�xima.
        UpdateHealthUI();                                               // Atualiza a barra de vida na UI.

        if (currentHealth <= 0)                                         // Se a vida do jogador chegar a 0 chama o m�todo de morte. 
        {
            Die();                                                      // Chama o m�todo para lidar com a morte do jogador.
        }
    }

    public bool UsePotion()                                             // M�todo para usar po��es.
    {
        if (potionCount > 0 && currentHealth < maxHealth)               // Verifica se o jogador tem po��es e se a vida atual est� abaixo da m�xima.
        {
            currentHealth += potionHealAmount;                          // Aumenta a vida com base no valor de cura da po��o.
            DamagePopUpGenerator.current.CreatePopUp(transform.position, potionHealAmount.ToString(), Color.green);         // Exibe na tela a vida recuperada.
            SoundManager.Instance.PlaySound3D("DrinkPotion", transform.position);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);   // Garante que a vida n�o ultrapasse a m�xima.
            potionCount--;                                              // Reduz o n�mero de po��es dispon�veis.
            UpdateHealthUI();                                           // Atualiza a barra de vida na interface.
            Debug.Log("Po��o usada!");

            if (vfxHeal != null)
            {
                Instantiate(vfxHeal, transform.position, Quaternion.identity);
            }

            UpdatePotionUI();                                           // Atualiza o texto com a quantidade de po��es.
            return true;                                                // Retorna true indicando que a po��o foi usada.
        }
        else
        {
            Debug.Log("Sem po��es ou vida cheia.");
            return false;                                               // Retorna false se n�o p�de usar.
        }
    }

    public void AddPotion()                                             // M�todo para adicionar po��es.
    {
        potionCount++;                                                  // Aumenta o n�mero de po��es.
        Debug.Log("Po��o coletada! Total: " + potionCount);
        UpdatePotionUI();                                               // Atualiza a interface com a nova quantidade.
    }

    private void UpdatePotionUI()                                       // M�todo para atualizar o texto da UI com a quantidade de po��es.
    {
        if (potionCountText != null)
        {
            potionCountText.text = "x" + potionCount;

            if (potionCount <= 0)                                       // Verificar se possui po��es, caso n�o tenha, mudar a cor do texto para vermelho.
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
            healthBarImage.fillAmount = (float)currentHealth / maxHealth;       // Preenche a imagem proporcional � vida atual.
        }
    }

    void Die()                                              // M�todo para lidar com a morte do jogador.
    {
        SoundManager.Instance.StopLoop3D();
        EndGameUI.instance?.GameOverScreen();
        Destroy(gameObject);
        Debug.Log("Jogador morreu!");
    }
}
