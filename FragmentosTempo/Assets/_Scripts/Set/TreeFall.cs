using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeFall : MonoBehaviour
{
    [Header("Tree Setting")]
    [SerializeField] public float FallSpeed = 5f;               // Velocidade de queda da �rvore.
    [SerializeField] public float fallDelay = 0.5f;             // Tempo de atraso da queda da �rvore.
    [SerializeField] private float fallTimer = 0f;              // Tempo interno para controlar o atraso.

    [Header("Destroy")]
    [SerializeField] private float destroyDelay = 2.5f;         // tempo de espera antes de destruir.

    [Header("Drop Settings")]
    [SerializeField] private GameObject healthPotionPrefab;             // Receber o prefab da po��o.
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.5f;    // Porcentagem de chance de dropar po��o.

    private bool isFalling = false;                             // Controle se a �rvore est� em queda.
    private bool hasFallen = false;                             // Controle se a �rvore j� caiu.
    private bool triceTouching = false;                         // Controle se o Triceratops est� em contato.

    private Rigidbody rb;                                       // Refer�ncia para o Rigidbody da �rvore.
    private Vector3 fallDirection;                              // Dire��o da queda da �rvore.

    private void Start()
    {
        rb = GetComponent<Rigidbody>();                         // Inicia o Rigidbody da �rvore.
        if (rb != null)
        {
            rb.isKinematic = true;                              // Impede o Rigidbody de agir sem as f�sicas incialmente.
        }
    }

    private void Update()
    {
        if (isFalling)                                          // Verifica se a �rvore est� em queda.
        {
            fallTimer -= Time.deltaTime;                        // Diminui o timer a cada segundo.
            if (fallTimer <= 0f)                                // Quando o timer zerar chama o m�todo de aplicar for�a de queda.
            {
                ApplyFallForce();
            }
        }
    }

    public void ForceFall(GameObject colliderObject)            // M�todo para iniciar a queda da �rvore.
    {
        if (!isFalling)                                         // Verifica se a �rvore n�o est� caindo.
        {
            if (colliderObject.CompareTag("Trice") || colliderObject.CompareTag("TriceHead"))             // Verificar se colidiu com o Triceratops.
            {
                isFalling = true;                               // Marca que a �rvore cair�.
                fallTimer = fallDelay;                          // Define o tempo de espera antes da queda.
            }
        }
    }

    private void ApplyFallForce()                               // M�todo para aplicar a f�sica de queda da �rvore.
    {
        if (rb != null)
        {
            rb.isKinematic = false;                             // Deixa o Rigidbody ser controlado pela f�sica.
            rb.freezeRotation = true;                           // Congela a rota��o.

            Vector3 fallDirection = Vector3.down;               // Define a dire��o de queda para baixo.
            rb.AddForce(fallDirection * FallSpeed, ForceMode.Impulse);          // Aplica for�a impulsiva para baixo.

            StartCoroutine(StopRotationAfterFall());            // Iniciar uma espera para liberar a rota��o.
        }
    }

    private IEnumerator StopRotationAfterFall()                 // Libera a rota��o da �rvore ap�s um curto tempo, permitindo tombar de forma natural.
    {
        yield return new WaitForSeconds(0.3f);
        rb.freezeRotation = false;                              // Descongela a rota��o para a �rvore tombar livremente no ch�o.
        hasFallen = true;
    }

    private void OnCollisionEnter(Collision collision)          // M�todo de entrada de colis�es.
    {
        if (hasFallen && collision.gameObject.CompareTag("Trice") || collision.gameObject.CompareTag("TriceHead"))                          // Verifica se a �rvore j� caiu e se colidiu com o Triceratops.
        {
            triceTouching = true;                                                           // Marca que o Triceratops est� colidindo.
        }
    }

    private void OnCollisionExit(Collision collision)           // M�todo de sa�da de colis�es.
    {
        if (hasFallen && triceTouching && collision.gameObject.CompareTag("Trice") || collision.gameObject.CompareTag("TriceHead"))         // Verifica se a �rvore j� caiu e se o Triceratops saiu da colis�o.
        {
            triceTouching = false;                                                          // Marca que o Triceratops n�o est� colidindo.
            StartCoroutine(DestroyAfterDelay());                                            // Inicia a coroutine para destruir a �rvore.
        }
    }

    private IEnumerator DestroyAfterDelay()                     // Coroutine para destrui��o da �rvore.
    {
        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);                                    // Destr�i a �rvore.

        TrySpawnDrop();
    }

    private void TrySpawnDrop()                                 // M�todo para tentar usar o spawn de po��o com certa porcentagem de chance.
    {
        if (healthPotionPrefab == null) return;                 // Verifica se o prefab da po��o de vida foi atribu�do. Se n�o, sai do m�todo.

        float roll = Random.Range(0f, 1f);                      // Gera um n�mero aleat�rio entre 0.0 e 1.0.
        if (roll <= dropChance)                                 // Se o valor aleat�rio for menor ou igual � chance de drop, instancia a po��o no local atual.
        {
            Instantiate(healthPotionPrefab, transform.position, Quaternion.identity);       // Cria a po��o na posi��o do objeto atual.
        }
    }
}