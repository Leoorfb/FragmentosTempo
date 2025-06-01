using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopUpGenerator : MonoBehaviour
{
    public static DamagePopUpGenerator current;                                     // Referência estática para permitir fácil acesso ao gerador.
    public GameObject prefab;                                                       // Prefab do pop-up que será instanciado.

    private void Awake()
    {
        current = this;                                                             // Define a instância atual para acesso global.
    }

    public void CreatePopUp(Vector3 position, string text, Color color)             // Método para criar o pop-up de dano.
    {
        var popup = Instantiate(prefab, position, Quaternion.identity);             // Instancia o prefab na posição indicada, sem rotação.
        var temp = popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();     // Acessa o TextMeshProUGUI no primeiro filho do prefab.
        temp.text = text;                                                           // Define o texto que será exibido.
        temp.faceColor = color;                                                     // Define a cor do texto.

        Destroy(popup, 1f);                                                         // Destroi automaticamente o pop-up após 1 segundo para evitar acúmulo.
    }
}
