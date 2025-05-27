using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeFall : MonoBehaviour
{
    [Header("Tree Setting")]
    [SerializeField] public float FallSpeed = 5f;               // Velocidade de queda da árvore.
    [SerializeField] public float fallDelay = 0.5f;             // Tempo de atraso da queda da árvore.
    [SerializeField] private float fallTimer = 0f;              // Tempo interno para controlar o atraso.

    [Header("Destroy")]
    [SerializeField] private float destroyDelay = 2.5f;         // tempo de espera antes de destruir.

    [Header("Drop Settings")]
    [SerializeField] private GameObject healthPotionPrefab;             // Receber o prefab da poção.
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.5f;    // Porcentagem de chance de dropar poção.

    private bool isFalling = false;                             // Controle se a árvore está em queda.
    private bool hasFallen = false;                             // Controle se a árvore já caiu.
    private bool triceTouching = false;                         // Controle se o Triceratops está em contato.

    private Rigidbody rb;                                       // Referência para o Rigidbody da árvore.
    private Vector3 fallDirection;                              // Direção da queda da árvore.

    private void Start()
    {
        rb = GetComponent<Rigidbody>();                         // Inicia o Rigidbody da árvore.
        if (rb != null)
        {
            rb.isKinematic = true;                              // Impede o Rigidbody de agir sem as físicas incialmente.
        }
    }

    private void Update()
    {
        if (isFalling)                                          // Verifica se a árvore está em queda.
        {
            fallTimer -= Time.deltaTime;                        // Diminui o timer a cada segundo.
            if (fallTimer <= 0f)                                // Quando o timer zerar chama o método de aplicar força de queda.
            {
                ApplyFallForce();
            }
        }
    }

    public void ForceFall(GameObject colliderObject)            // Método para iniciar a queda da árvore.
    {
        if (!isFalling)                                         // Verifica se a árvore não está caindo.
        {
            if (colliderObject.CompareTag("Trice") || colliderObject.CompareTag("TriceHead"))             // Verificar se colidiu com o Triceratops.
            {
                isFalling = true;                               // Marca que a árvore cairá.
                fallTimer = fallDelay;                          // Define o tempo de espera antes da queda.
            }
        }
    }

    private void ApplyFallForce()                               // Método para aplicar a física de queda da árvore.
    {
        if (rb != null)
        {
            rb.isKinematic = false;                             // Deixa o Rigidbody ser controlado pela física.
            rb.freezeRotation = true;                           // Congela a rotação.

            Vector3 fallDirection = Vector3.down;               // Define a direção de queda para baixo.
            rb.AddForce(fallDirection * FallSpeed, ForceMode.Impulse);          // Aplica força impulsiva para baixo.

            StartCoroutine(StopRotationAfterFall());            // Iniciar uma espera para liberar a rotação.
        }
    }

    private IEnumerator StopRotationAfterFall()                 // Libera a rotação da árvore após um curto tempo, permitindo tombar de forma natural.
    {
        yield return new WaitForSeconds(0.3f);
        rb.freezeRotation = false;                              // Descongela a rotação para a árvore tombar livremente no chão.
        hasFallen = true;
    }

    private void OnCollisionEnter(Collision collision)          // Método de entrada de colisões.
    {
        if (hasFallen && collision.gameObject.CompareTag("Trice") || collision.gameObject.CompareTag("TriceHead"))                          // Verifica se a árvore já caiu e se colidiu com o Triceratops.
        {
            triceTouching = true;                                                           // Marca que o Triceratops está colidindo.
        }
    }

    private void OnCollisionExit(Collision collision)           // Método de saída de colisões.
    {
        if (hasFallen && triceTouching && collision.gameObject.CompareTag("Trice") || collision.gameObject.CompareTag("TriceHead"))         // Verifica se a árvore já caiu e se o Triceratops saiu da colisão.
        {
            triceTouching = false;                                                          // Marca que o Triceratops não está colidindo.
            StartCoroutine(DestroyAfterDelay());                                            // Inicia a coroutine para destruir a árvore.
        }
    }

    private IEnumerator DestroyAfterDelay()                     // Coroutine para destruição da árvore.
    {
        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);                                    // Destrói a árvore.

        TrySpawnDrop();
    }

    private void TrySpawnDrop()                                 // Método para tentar usar o spawn de poção com certa porcentagem de chance.
    {
        if (healthPotionPrefab == null) return;                 // Verifica se o prefab da poção de vida foi atribuído. Se não, sai do método.

        float roll = Random.Range(0f, 1f);                      // Gera um número aleatório entre 0.0 e 1.0.
        if (roll <= dropChance)                                 // Se o valor aleatório for menor ou igual à chance de drop, instancia a poção no local atual.
        {
            Instantiate(healthPotionPrefab, transform.position, Quaternion.identity);       // Cria a poção na posição do objeto atual.
        }
    }
}