using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossIntroManager : MonoBehaviour
{
    public float delayToActivate = 2f;                          // Tempo de atraso antes de ativar a barra de vida do Boss.
    public GameObject dialogBox;                                // Refer�ncia ao objeto da caixa de di�logo que ser� usada para controlar o tempo de espera.

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForDialogToEnd());                   // Inicia uma coroutine que aguarda o fim do di�logo antes de ativar a barra de vida do Boss.
    }

    private IEnumerator WaitForDialogToEnd()                    // Coroutine que espera at� o fim do di�logo para ativar a barra de vida do Boss.
    {
        yield return new WaitUntil(() => !dialogBox.activeSelf);    // Espera at� que a caixa de di�logo esteja desativada.

        BossHealthManager.Instance.SpawnBar();                      // Quando o di�logo acabar, cria a barra de vida do Boss.

        yield return new WaitForSeconds(delayToActivate);           // Aguarda o tempo especificado antes de mostrar a barra de vida do Boss.

        BossHealthManager.Instance.ShowBar();                       // Ap�s o atraso, exibe a barra de vida do Boss.
    }
}