using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [SerializeField] private Image lifeBarImage;                        // Referência da imagem que representa a barra de vida.
    [SerializeField] private Text bossName;                             // Referência do nome do boss.

    public void AlterarLifeBar(int currentLife, int maxHealth)          // Atualiza o preenchimento da barra de vida com base na vida atual e máxima.
    {
        lifeBarImage.fillAmount = (float)currentLife / maxHealth;       // Calcula a proporção da vida e aplica ao preenchimento da barra.
    }

    public void DefNameBoss(string name)                                // Define o nome do Boss no componente de texto da UI.
    {
        if (bossName != null)
        {
            bossName.text = name;
        }
        else
        {
            Debug.LogWarning("Nome não atribuido na UI.");
        }
    }
}
