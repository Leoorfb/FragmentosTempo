using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [Header("Dialog Settings")]
    [TextArea(3, 10)]
    [SerializeField] private string[] dialogLines;                  // Linhas de diálogo a exibir.
    [SerializeField] private TextMeshProUGUI dialogText;            // Campo de texto da UI.
    [SerializeField] private GameObject dialogBox;                  // Painel da caixa de diálogo.
    [SerializeField] private Button nextButton;                     // Botão de avançar.
    [SerializeField] private float typingSpeed = 0.05f;             // Velocidade de digitação do texto.

    [Header("Tutorial Settings")]
    [SerializeField] private PlayerMovement player;                 // Referência do jogador.
    [SerializeField] private List<MonoBehaviour> bossScripts = new();             // Lista de scripts do boss para controlar os bosses durante o tutorial.

    private List<IBoss> bosses = new();                             // Lista de bosses que implementam a interface IBoss.
    public bool isInTutorial = false;                               // Verificar se está no tutorial.

    private int currentLineIndex = 0;                               // Linha atual do diálogo.
    private bool isTyping = false;                                  // Flag que verifica se o texto está sendo digitado.
    private Coroutine typingCoroutine;                              // Referência para a corrotina que controla a digitação do texto.


    // Start is called before the first frame update
    void Start()
    {
        foreach (var script in bossScripts)                         // Adiciona os bosses à lista se implementarem a interface IBoss.
        {
            if (script is IBoss boss)
            {
                bosses.Add(boss);
            }
        }

        ShowDialog();                                               // Inicia o diálogo assim que começa a cena.
        nextButton.onClick.AddListener(NextLine);                   // Atribui ação ao botão.
    }

    private void ShowDialog()                                       // Método para exibir a caixa de diálogo.
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

        dialogBox.SetActive(true);                                  // Exibe a caixa de diálogo.
        currentLineIndex = 0;                                       // Reinicia o índice para a primeira linha.
        StartTyping(dialogLines[currentLineIndex]);                 // Inicia a digitação da primeira linha do diálogo.
    }

    private void StartTyping(string line)                           // Método para iniciar digitação.
    {
        if (typingCoroutine != null)                                // Cancela a corrotina caso esteja em execução.
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeLine(line));           // Inicia a digitação da linha.
    }

    private IEnumerator TypeLine(string line)                       // Corrotina para digitar as linhas lentamente.
    {
        isTyping = true;
        dialogText.text = "";                                       // Limpa o texto antes de começar a digitar.

        foreach (char letter in line.ToCharArray())
        {
            dialogText.text += letter;                              // Adiciona cada letra.
            yield return new WaitForSeconds(typingSpeed);           // Aguarda a próxima letra.
        }

        isTyping = false;                                           // Finaliza a digitação.
    }

    private void NextLine()                                         // Método para a próxima linha de textos.
    {
        if (isTyping)                                               // Se o texto ainda está digitando, para a corrotina e exibe o texto completo.
        {
            StopCoroutine(typingCoroutine);
            dialogText.text = dialogLines[currentLineIndex];        // Exibe o texto completo da linha.
            isTyping = false;                                       // Marca que terminou de digitar.
            return;
        }

        currentLineIndex++;                                         // Avança para a próxima linha.

        if (currentLineIndex < dialogLines.Length)                  // Se ainda houver mais linhas de diálogo, inicia a digitação da próxima linha.
        {
            StartTyping(dialogLines[currentLineIndex]);
        }
        else
        {
            dialogBox.SetActive(false);                             // Oculta a caixa após o fim do diálogo.

            foreach (IBoss boss in bosses)
            {
                boss.SetCanMove(true);                              // Reabilita o movimento dos bosses após o tutorial.
                Debug.Log("Boss ativado");
            }

            if (player != null)
            {
                player.canMove = true;                              // Reabilita o movimento do jogador após o tutorial.
            }

            isInTutorial = false;                                   // Marca que o tutorial terminou.
        }
    }
}
