using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [SerializeField] private Image lifeBarImage;                        // Refer�ncia da imagem que representa a barra de vida.

    public void AlterarLifeBar(int currentLife, int maxHealth)          // Atualiza o preenchimento da barra de vida com base na vida atual e m�xima.
    {
        lifeBarImage.fillAmount = (float)currentLife / maxHealth;       // Calcula a propor��o da vida e aplica ao preenchimento da barra.
    }
}
