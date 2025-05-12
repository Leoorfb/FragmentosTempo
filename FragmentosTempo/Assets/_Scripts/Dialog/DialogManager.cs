using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [Header("Dialog Settings")]
    [TextArea(3, 10)]
    [SerializeField] private string[] dialogLines;                  // Linhas de di�logo a exibir.
    [SerializeField] private TextMeshProUGUI dialogText;            // Campo de texto da UI.
    [SerializeField] private GameObject dialogBox;                  // Painel da caixa de di�logo.
    [SerializeField] private Button nextButton;                     // Bot�o de avan�ar.
    [SerializeField] private float typingSpeed = 0.05f;             // Velocidade de digita��o do texto.

    [Header("Tutorial Settings")]
    [SerializeField] private PlayerMovement player;                 // Refer�ncia do jogador.
    [SerializeField] private List<MonoBehaviour> bossScripts = new();             // Lista de scripts do boss para controlar os bosses durante o tutorial.

    private List<IBoss> bosses = new();                             // Lista de bosses que implementam a interface IBoss.
    public bool isInTutorial = false;                               // Verificar se est� no tutorial.

    private int currentLineIndex = 0;                               // Linha atual do di�logo.
    private bool isTyping = false;                                  // Flag que verifica se o texto est� sendo digitado.
    private Coroutine typingCoroutine;                              // Refer�ncia para a corrotina que controla a digita��o do texto.


    // Start is called before the first frame update
    void Start()
    {
        foreach (var script in bossScripts)                         // Adiciona os bosses � lista se implementarem a interface IBoss.
        {
            if (script is IBoss boss)
            {
                bosses.Add(boss);
            }
        }

        ShowDialog();                                               // Inicia o di�logo assim que come�a a cena.
        nextButton.onClick.AddListener(NextLine);                   // Atribui a��o ao bot�o.
    }

    private void ShowDialog()                                       // M�todo para exibir a caixa de di�logo.
    {
        isInTutorial = true;

        foreach (IBoss boss in bosses)                              // Desabilita o movimento de todos os bosses durante o tutorial.
        {
            boss.SetCanMove(false);
        }

        if (player != null)                                         // Desabilita o movimento do jogador durante o tutorial.
        {
            player.canMove = false;
        }

        dialogBox.SetActive(true);                                  // Exibe a caixa de di�logo.
        currentLineIndex = 0;                                       // Reinicia o �ndice para a primeira linha.
        StartTyping(dialogLines[currentLineIndex]);                 // Inicia a digita��o da primeira linha do di�logo.
    }

    private void StartTyping(string line)                           // M�todo para iniciar digita��o.
    {
        if (typingCoroutine != null)                                // Cancela a corrotina caso esteja em execu��o.
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeLine(line));           // Inicia a digita��o da linha.
    }

    private IEnumerator TypeLine(string line)                       // Corrotina para digitar as linhas lentamente.
    {
        isTyping = true;
        dialogText.text = "";                                       // Limpa o texto antes de come�ar a digitar.

        foreach (char letter in line.ToCharArray())
        {
            dialogText.text += letter;                              // Adiciona cada letra.
            yield return new WaitForSeconds(typingSpeed);           // Aguarda a pr�xima letra.
        }

        isTyping = false;                                           // Finaliza a digita��o.
    }

    private void NextLine()                                         // M�todo para a pr�xima linha de textos.
    {
        if (isTyping)                                               // Se o texto ainda est� digitando, para a corrotina e exibe o texto completo.
        {
            StopCoroutine(typingCoroutine);
            dialogText.text = dialogLines[currentLineIndex];        // Exibe o texto completo da linha.
            isTyping = false;                                       // Marca que terminou de digitar.
            return;
        }

        currentLineIndex++;                                         // Avan�a para a pr�xima linha.

        if (currentLineIndex < dialogLines.Length)                  // Se ainda houver mais linhas de di�logo, inicia a digita��o da pr�xima linha.
        {
            StartTyping(dialogLines[currentLineIndex]);
        }
        else
        {
            dialogBox.SetActive(false);                             // Oculta a caixa ap�s o fim do di�logo.

            foreach (IBoss boss in bosses)
            {
                boss.SetCanMove(true);                              // Reabilita o movimento dos bosses ap�s o tutorial.
                Debug.Log("Boss ativado");
            }

            if (player != null)
            {
                player.canMove = true;                              // Reabilita o movimento do jogador ap�s o tutorial.
            }

            isInTutorial = false;                                   // Marca que o tutorial terminou.
        }
    }
}
