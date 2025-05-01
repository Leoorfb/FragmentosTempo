using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [SerializeField] private Image lifeBarImage;                        // Referência da imagem que representa a barra de vida.

    public void AlterarLifeBar(int currentLife, int maxHealth)          // Atualiza o preenchimento da barra de vida com base na vida atual e máxima.
    {
        lifeBarImage.fillAmount = (float)currentLife / maxHealth;       // Calcula a proporção da vida e aplica ao preenchimento da barra.
    }
}
