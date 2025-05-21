using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [SerializeField] private Image lifeBarImage;                        // Refer�ncia da imagem que representa a barra de vida.
    [SerializeField] private Text bossName;                             // Refer�ncia do nome do boss.

    public void AlterarLifeBar(int currentLife, int maxHealth)          // Atualiza o preenchimento da barra de vida com base na vida atual e m�xima.
    {
        lifeBarImage.fillAmount = (float)currentLife / maxHealth;       // Calcula a propor��o da vida e aplica ao preenchimento da barra.
    }

    public void DefNameBoss(string name)                                // Define o nome do Boss no componente de texto da UI.
    {
        if (bossName != null)
        {
            bossName.text = name;
        }
        else
        {
            Debug.LogWarning("Nome n�o atribuido na UI.");
        }
    }
}
