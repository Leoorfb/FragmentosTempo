using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeFall : MonoBehaviour
{
    [Header("Tree Setting")]
    [SerializeField] private float fallSpeed = 20f;             // Velocidade com que a árvore cai.
    [SerializeField] private float fallAngle = 90f;             // Ângulo total que a árvore deve rotacionar ao cair.
    [SerializeField] private float currentAngle = 0f;           // Controle do ângulo atual da árvore enquanto está caindo.

    [Header("Destroy")]
    [SerializeField] private float destroyDelay = 2.5f;         // tempo de espera antes de destruir.

    [Header("Drop Settings")]
    [SerializeField] private GameObject healthPotionPrefab;             // Receber o prefab da poção.
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.5f;    // Porcentagem de chance de dropar poção.

    public bool hasFallen = false;                              // Controle se a árvore já caiu.
    private bool isFalling = false;                             // Controle se a árvore está no processo de queda.

    public Vector3 fallDirection;                               // Direção para onde a árvore vai cair.

    private void OnCollisionEnter(Collision collision)          // Método de colisões.
    {
        if (collision.gameObject.CompareTag("Trice") || collision.gameObject.CompareTag("TriceHead") && !isFalling)     // Quando colide com o "Trice" ou "TriceHead" e ainda não está caindo
        {
            if (!isFalling && !hasFallen)
            {
                Vector3 collisionDirection = (collision.transform.position - transform.position).normalized;            // Calcula a direção oposta da colisão, para determinar a direção de queda
                fallDirection = -collisionDirection;
                fallDirection.y = 0;                            // Zera a altura para evitar inclinações verticais.
                fallDirection.Normalize();

                isFalling = true;                               // Começa a cair.
                SoundManager.Instance.PlaySound3D("TreeFalling", transform.position);
            }
            else if (hasFallen)
            {
                StartCoroutine(DestroyAfterDelay());            // Se já caiu, inicia processo de destruição.
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isFalling && currentAngle < fallAngle)                                  // Se estiver caindo e ainda não alcançou o ângulo máximo.
        {
            float rotationStep = fallSpeed * Time.deltaTime;                        // Calcula o quanto rotacionar neste frame.
            float rotation = Mathf.Min(rotationStep, fallAngle - currentAngle);     // Garante que não passe do ângulo final.

            Vector3 rotationAxis = Vector3.Cross(Vector3.up, fallDirection);        // Define o eixo de rotação cruzando o "up" com a direção de queda.

            transform.Rotate(rotationAxis, rotation, Space.World);                  // Rotaciona a árvore em torno do eixo calculado.
            currentAngle += rotation;

            if (currentAngle >= fallAngle)                                          // Quando alcança o ângulo final, finaliza a queda.
            {
                isFalling = false;
                hasFallen = true;
            }
        }
    }

    private IEnumerator DestroyAfterDelay()                     // Corrotina que aguarda o tempo antes de destruir a árvore.
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
        Debug.Log("Arvore destruida!");

        TrySpawnDrop();                                         // Tenta gerar o drop após destruição.
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
