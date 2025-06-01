using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeFall : MonoBehaviour
{
    [Header("Tree Setting")]
    [SerializeField] private float fallSpeed = 20f;             // Velocidade com que a �rvore cai.
    [SerializeField] private float fallAngle = 90f;             // �ngulo total que a �rvore deve rotacionar ao cair.
    [SerializeField] private float currentAngle = 0f;           // Controle do �ngulo atual da �rvore enquanto est� caindo.

    [Header("Destroy")]
    [SerializeField] private float destroyDelay = 2.5f;         // tempo de espera antes de destruir.

    [Header("Drop Settings")]
    [SerializeField] private GameObject healthPotionPrefab;             // Receber o prefab da po��o.
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.5f;    // Porcentagem de chance de dropar po��o.

    public bool hasFallen = false;                              // Controle se a �rvore j� caiu.
    private bool isFalling = false;                             // Controle se a �rvore est� no processo de queda.

    public Vector3 fallDirection;                               // Dire��o para onde a �rvore vai cair.

    private void OnCollisionEnter(Collision collision)          // M�todo de colis�es.
    {
        if (collision.gameObject.CompareTag("Trice") || collision.gameObject.CompareTag("TriceHead") && !isFalling)     // Quando colide com o "Trice" ou "TriceHead" e ainda n�o est� caindo
        {
            if (!isFalling && !hasFallen)
            {
                Vector3 collisionDirection = (collision.transform.position - transform.position).normalized;            // Calcula a dire��o oposta da colis�o, para determinar a dire��o de queda
                fallDirection = -collisionDirection;
                fallDirection.y = 0;                            // Zera a altura para evitar inclina��es verticais.
                fallDirection.Normalize();

                isFalling = true;                               // Come�a a cair.
                SoundManager.Instance.PlaySound3D("TreeFalling", transform.position);
            }
            else if (hasFallen)
            {
                StartCoroutine(DestroyAfterDelay());            // Se j� caiu, inicia processo de destrui��o.
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isFalling && currentAngle < fallAngle)                                  // Se estiver caindo e ainda n�o alcan�ou o �ngulo m�ximo.
        {
            float rotationStep = fallSpeed * Time.deltaTime;                        // Calcula o quanto rotacionar neste frame.
            float rotation = Mathf.Min(rotationStep, fallAngle - currentAngle);     // Garante que n�o passe do �ngulo final.

            Vector3 rotationAxis = Vector3.Cross(Vector3.up, fallDirection);        // Define o eixo de rota��o cruzando o "up" com a dire��o de queda.

            transform.Rotate(rotationAxis, rotation, Space.World);                  // Rotaciona a �rvore em torno do eixo calculado.
            currentAngle += rotation;

            if (currentAngle >= fallAngle)                                          // Quando alcan�a o �ngulo final, finaliza a queda.
            {
                isFalling = false;
                hasFallen = true;
            }
        }
    }

    private IEnumerator DestroyAfterDelay()                     // Corrotina que aguarda o tempo antes de destruir a �rvore.
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
        Debug.Log("Arvore destruida!");

        TrySpawnDrop();                                         // Tenta gerar o drop ap�s destrui��o.
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
