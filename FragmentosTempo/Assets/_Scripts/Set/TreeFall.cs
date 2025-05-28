using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeFall : MonoBehaviour
{
    [Header("Tree Setting")]
    [SerializeField] private float fallSpeed = 20f;
    [SerializeField] private float fallAngle = 90f;
    [SerializeField] private float currentAngle = 0f;

    [Header("Destroy")]
    [SerializeField] private float destroyDelay = 2.5f;         // tempo de espera antes de destruir.

    [Header("Drop Settings")]
    [SerializeField] private GameObject healthPotionPrefab;             // Receber o prefab da poção.
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.5f;    // Porcentagem de chance de dropar poção.

    public bool hasFallen = false;                             // Controle se a árvore já caiu.
    private bool isFalling = false;

    public Vector3 fallDirection;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Trice") || collision.gameObject.CompareTag("TriceHead") && !isFalling)
        {
            if (!isFalling && !hasFallen)
            {
                Vector3 collisionDirection = (collision.transform.position - transform.position).normalized;
                fallDirection = -collisionDirection;
                fallDirection.y = 0;
                fallDirection.Normalize();

                isFalling = true;
            }
            else if (hasFallen)
            {
                StartCoroutine(DestroyAfterDelay());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isFalling && currentAngle < fallAngle)
        {
            float rotationStep = fallSpeed * Time.deltaTime;
            float rotation = Mathf.Min(rotationStep, fallAngle - currentAngle);

            Vector3 rotationAxis = Vector3.Cross(Vector3.up, fallDirection);
            transform.Rotate(rotationAxis, rotation, Space.World);
            currentAngle += rotation;

            if (currentAngle >= fallAngle)
            {
                isFalling = false;
                hasFallen = true;
            }
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
        Debug.Log("Arvore destruida!");

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
