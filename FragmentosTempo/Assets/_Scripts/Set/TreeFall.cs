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
    [SerializeField] private GameObject healthPotionPrefab;             // Receber o prefab da po��o.
    [SerializeField][Range(0f, 1f)] private float dropChance = 0.5f;    // Porcentagem de chance de dropar po��o.

    public bool hasFallen = false;                             // Controle se a �rvore j� caiu.
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
