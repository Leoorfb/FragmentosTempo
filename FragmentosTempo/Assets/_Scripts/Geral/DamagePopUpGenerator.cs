using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopUpGenerator : MonoBehaviour
{
    public static DamagePopUpGenerator current;                                     // Refer�ncia est�tica para permitir f�cil acesso ao gerador.
    public GameObject prefab;                                                       // Prefab do pop-up que ser� instanciado.

    private void Awake()
    {
        current = this;                                                             // Define a inst�ncia atual para acesso global.
    }

    public void CreatePopUp(Vector3 position, string text, Color color)             // M�todo para criar o pop-up de dano.
    {
        var popup = Instantiate(prefab, position, Quaternion.identity);             // Instancia o prefab na posi��o indicada, sem rota��o.
        var temp = popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();     // Acessa o TextMeshProUGUI no primeiro filho do prefab.
        temp.text = text;                                                           // Define o texto que ser� exibido.
        temp.faceColor = color;                                                     // Define a cor do texto.

        Destroy(popup, 1f);                                                         // Destroi automaticamente o pop-up ap�s 1 segundo para evitar ac�mulo.
    }
}
