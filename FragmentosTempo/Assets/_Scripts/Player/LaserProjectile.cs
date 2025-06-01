using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    public float speed = 20f;                                                   // Velocidade de movimento do proj�til.
    public int damage = 10;                                                     // Dano que o proj�til causa ao inimigo.
    public float lifeTime = 2f;                                                 // Tempo de vida do proj�til antes de ser destru�do automaticamente.

    private Vector3 moveDirection;                                              // Dire��o em que o proj�til vai se mover.
    public bool headHit = false;                                                // Flag para verificar se atingiu a cabe�a do Trice.

    // Start is called before the first frame update
    void Start()
    {
        moveDirection = transform.up;                                           // Define a dire��o inicial do proj�til.
        Destroy(gameObject, lifeTime);                                          // Destroi o proj�til automaticamente ap�s o tempo de vida definido.
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;           // Move o proj�til continuamente na dire��o especificada.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TriceHead"))                                      // Verificar se atingiu a cabe�a do Trice e n�o aplicar dano.
        {
            Debug.Log("Acertou a cabe�a - sem dano!");
            headHit = true;
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Trice"))                                          // Verifica se colidiu com o inimigo espec�fico (tag "Trice" representa o boss).
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
            Destroy(gameObject);                                                // Destroi o proj�til ap�s atingir o inimigo.
        }


        if (other.CompareTag("Fornalha"))                                          // Verifica se colidiu com o inimigo espec�fico (tag "Trice" representa o boss).
        {

            BossHealth bossHealth = other.GetComponent<BossHealth>();           // Tenta acessar o componente de vida do boss.
            if (bossHealth != null)
            {
                bossHealth.ApplyDamage(damage);                                 // Aplica dano ao boss.
            }
            Destroy(gameObject);                                                // Destroi o proj�til ap�s atingir o inimigo.
        }
    }
}
