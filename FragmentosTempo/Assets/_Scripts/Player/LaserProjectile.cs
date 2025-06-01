using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    public float speed = 20f;                                                   // Velocidade de movimento do projétil.
    public int damage = 10;                                                     // Dano que o projétil causa ao inimigo.
    public float lifeTime = 2f;                                                 // Tempo de vida do projétil antes de ser destruído automaticamente.

    private Vector3 moveDirection;                                              // Direção em que o projétil vai se mover.
    public bool headHit = false;                                                // Flag para verificar se atingiu a cabeça do Trice.

    // Start is called before the first frame update
    void Start()
    {
        moveDirection = transform.up;                                           // Define a direção inicial do projétil.
        Destroy(gameObject, lifeTime);                                          // Destroi o projétil automaticamente após o tempo de vida definido.
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;           // Move o projétil continuamente na direção especificada.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TriceHead"))                                      // Verificar se atingiu a cabeça do Trice e não aplicar dano.
        {
            Debug.Log("Acertou a cabeça - sem dano!");
            headHit = true;
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Trice"))                                          // Verifica se colidiu com o inimigo específico (tag "Trice" representa o boss).
        {
            if (headHit)
            {
                return;
            }

            BossHealth bossHealth = other.GetComponent<BossHealth>();           // Tenta acessar o componente de vida do boss.
            if (bossHealth != null)
            {
                bossHealth.ApplyDamage(damage);                                 // Aplica dano ao boss.
            }
            Destroy(gameObject);                                                // Destroi o projétil após atingir o inimigo.
        }


        if (other.CompareTag("Fornalha"))                                          // Verifica se colidiu com o inimigo específico (tag "Trice" representa o boss).
        {

            BossHealth bossHealth = other.GetComponent<BossHealth>();           // Tenta acessar o componente de vida do boss.
            if (bossHealth != null)
            {
                bossHealth.ApplyDamage(damage);                                 // Aplica dano ao boss.
            }
            Destroy(gameObject);                                                // Destroi o projétil após atingir o inimigo.
        }
    }
}
