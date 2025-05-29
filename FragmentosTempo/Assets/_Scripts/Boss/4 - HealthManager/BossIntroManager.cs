using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossIntroManager : MonoBehaviour
{
    public float delayToActivate = 2f;                          // Tempo de atraso antes de ativar a barra de vida do Boss.
    public GameObject dialogBox;                                // Referência ao objeto da caixa de diálogo que será usada para controlar o tempo de espera.

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForDialogToEnd());                   // Inicia uma coroutine que aguarda o fim do diálogo antes de ativar a barra de vida do Boss.
    }

    private IEnumerator WaitForDialogToEnd()                    // Coroutine que espera até o fim do diálogo para ativar a barra de vida do Boss.
    {
        yield return new WaitUntil(() => !dialogBox.activeSelf);    // Espera até que a caixa de diálogo esteja desativada.

        BossHealthManager.Instance.SpawnBar();                      // Quando o diálogo acabar, cria a barra de vida do Boss.

        yield return new WaitForSeconds(delayToActivate);           // Aguarda o tempo especificado antes de mostrar a barra de vida do Boss.

        BossHealthManager.Instance.ShowBar();                       // Após o atraso, exibe a barra de vida do Boss.
    }
}