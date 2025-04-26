using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TriceratopsBoss : MonoBehaviour
{
    [Header("Player Target")]
    public Transform player;                            // Refer�ncia ao Transform do jogador.

    [Header("Movement Settings")]
    public float chaseSpeed = 5f;                       // Velocidade ao perseguir o jogador.
    public float chargeSpeed = 10f;                     // Velocidade durante a investida.
    public float detectionRange = 10f;                  // Dist�ncia para come�ar a perseguir.
    public float chargeRange = 10f;                     // Dist�ncia para come�ar a investir.
    public float maxChargeDistance = 15f;               // Dist�ncia m�xima que a investida pode percorrer.

    [Header("Collision Settings")]
    public float stuckTime = 3f;                        // Tempo que o Triceratops fica preso ap�s bater numa �rvore.
    public float pushForce = 700f;                      // For�a horizontal ao empurrar o jogador.
    public float pushUpForce = 150f;                    // For�a vertical ao empurrar o jogador.

    [Header("Tail Attack")]
    public float tailAttackRange = 5f;                  // Dist�ncia m�nima para atacar com a cauda.
    public float tailAttackCooldown = 5f;               // Tempo de recarga do golpe de cauda.

    [Header("Earthquake Attack")]
    public float earthquakeRadius = 12f;                // Raio de alcance do ataque terremoto.
    public float earthquakeCooldown = 8f;               // Tempo de recarga do terremoto.
    public float earthquakeDuration = 1.5f;             // Dura��o do efeito do terremoto.
    public LayerMask playerMask;                        // Layer que detecta o jogador para o terremoto.

    [Header("References")]
    public Transform headPosition;                      // Posi��o da cabe�a para detectar colis�es frontais.
    public LayerMask obstacleMask;                      // Layer que detecta obst�culos.

    private Vector3 chargeStartPosition;                // Posi��o inicial da investida.
    private Vector3 chargeDirection;                    // Dire��o da investida.
    private Vector3 chargeTarget;                       // Posi��o alvo da investida.

    private Rigidbody rb;                               // Rigidbody do Triceratops.
    private bool isCharging = false;                    // Est� em investida?
    private bool isStuck = false;                       // Est� preso?
    private float stuckTimer;                           // Contador de tempo preso.
    private bool isEarthquaking = false;                // Est� usando terremoto?

    private float nextEarthquakeTime = 0f;              // Pr�ximo tempo que poder� usar terremoto.
    private float nextTailAttackTime = 0f;              // Pr�ximo tempo que poder� usar golpe de cauda.

    void Start()
    {
        rb = GetComponent<Rigidbody>();                 // Pega o Rigidbody no in�cio do jogo.
    }

    void Update()
    {
        if (player == null)                             // Se n�o tem jogador, n�o faz nada.
        {
            return;
        }

        if (isEarthquaking)                             // Se est� usando terremoto, n�o realiza outras a��es.
        {
            return;
        }

        if (isStuck)
        {
            Debug.Log("Contando tempo preso: " + stuckTimer.ToString("F2"));
            stuckTimer -= Time.deltaTime;               // Contagem regressiva do tempo preso.
            if (stuckTimer <= 0)                        
            {
                Unstuck();                              // Se tempo acabar, solta o Triceratops.
            }
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);         // Calcula dist�ncia at� o jogador.

        if (isCharging)
        {
            Charge();                                   // Se est� investindo, continua o movimento de investida.
        }
        else
        {
            if (distanceToPlayer <= chargeRange)
            {
                StartCharge();                         // Se o jogador est� na dist�ncia de investida, come�a a investida. 
            }
            else if (distanceToPlayer <= detectionRange)
            {
                ChasePlayer();                        // Se est� perto o suficiente, persegue o jogador.
            }
        }

        if (distanceToPlayer <= tailAttackRange && PlayerIsBehind() && Time.time >= nextTailAttackTime)
        {
            TailAttack();                           // Se o jogador est� atr�s, dentro do alcance e o ataque de cauda est� liberado.
        }

        if (Time.time >= nextEarthquakeTime && PlayerInFront() && distanceToPlayer <= 7f)       
        {
            Earthquake();                           // Se o jogador est� na frente, perto, e o terremoto est� dispon�vel
        }
    }

    bool PlayerInFront()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toPlayer);
        return angle < 60f;                         // Considera jogador na frente se o �ngulo � menor que 60 graus.
    }
    bool PlayerIsBehind()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toPlayer);
        return angle > 120f;                        // Considera jogador atr�s se o �ngulo � maior que 120 graus.
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        rb.MovePosition(transform.position + direction * chaseSpeed * Time.deltaTime);      // Move na dire��o do jogador.
        RotateTowards(player.position);                                                     // Gira para olhar o jogador.
    }

    void StartCharge()
    {
        isCharging = true;
        chargeTarget = player.position;
        chargeStartPosition = transform.position;
        chargeDirection = (chargeTarget - transform.position).normalized;                   // Define dire��o da investida.
    }

    void Charge()
    {
        if (Physics.Raycast(headPosition.position, chargeDirection, out RaycastHit hit, 2f, obstacleMask))          // Raycast para detectar colis�o frontal com obst�culo
        {
            if (hit.collider.CompareTag("Tree"))
            {
                GetStuck();                         // Se acertar uma �rvore, fica preso.
                return;
            }
        }

        rb.MovePosition(transform.position + chargeDirection * chargeSpeed * Time.deltaTime);       // Movimento da investida

        float chargeDistance = Vector3.Distance(chargeStartPosition, transform.position);           // Verifica se j� andou a dist�ncia m�xima
        if (chargeDistance >= maxChargeDistance)
        {
            Debug.Log("Trice terminou a investida");
            isCharging = false;
        }

        RotateTowards(chargeTarget);                                                                // Continua rotacionando para o alvo.
    }

    void RotateTowards(Vector3 target)
    {
        Vector3 lookDirection = (target - transform.position).normalized;
        lookDirection.y = 0;                                                                                    // Ignora rota��o no eixo Y (para n�o virar para cima ou para baixo).
        if (lookDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);       // Rota��o suave.
        }
    }

    void TailAttack()
    {
        Debug.Log("Triceratops deu um golpe de cauda!");
        nextTailAttackTime = Time.time + tailAttackCooldown;                                        // Ativa cooldown do golpe de cauda.
    }
    void Earthquake()
    {
        if (isEarthquaking) return;                                                                 // Se j� est� em terremoto, ignora.

        isEarthquaking = true;
        Debug.Log("Triceratops usou TERREMOTO!");
        nextEarthquakeTime = Time.time + earthquakeCooldown;                                        // Ativa cooldown do terremoto.

        rb.velocity = Vector3.zero;                                                                 // Para o movimento.

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, earthquakeRadius, playerMask);      // Detecta todos jogadores atingidos pelo terremoto
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Jogador atingido");

                Rigidbody hitRB = hit.GetComponent<Rigidbody>();
                if (hitRB != null)
                {
                    Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                    hitRB.AddForce(pushDir * 500f + Vector3.up * 200f);                             // Empurra o jogador.
                }
            }
        }

        Invoke(nameof(EndEarthquake), 1f);                                                          // Termina o terremoto depois de 1 segundo.
    }

    void EndEarthquake()
    {
        isEarthquaking = false;
    }
        
    void GetStuck()
    {
        Debug.Log("Triceratops esta preso na arvore!");
        isStuck = true;
        isCharging = false;
        stuckTimer = stuckTime;                                                                     // Inicia o contador para se soltar.
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;                                                                      // Deixa o Rigidbody sem f�sica para n�o ficar deslizando.
    }

    void Unstuck()
    {
        Debug.Log("Triceratops se soltou!");
        isStuck = false;
        rb.isKinematic = false;                                                                     // Volta o Rigidbody para a f�sica normal.
        rb.velocity = Vector3.zero;

        Vector3 backward = -transform.forward * 5f + Vector3.up * 2f;                               // Impulso para tr�s
        rb.AddForce(backward, ForceMode.Impulse);

        transform.Rotate(0, 180f, 0);                                                               // Gira para tr�s (180�)
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isCharging)
        {
            if (collision.gameObject.CompareTag("Tree"))
            {
                GetStuck();                                                                         // Se colidir com �rvore enquanto investe, fica preso.
            }
            else if (collision.gameObject.CompareTag("Player"))
            {
                Rigidbody playerRB = collision.gameObject.GetComponent<Rigidbody>();                // Se colidir com jogador enquanto investe, empurra o jogador
                if (playerRB != null)
                {
                    Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
                    playerRB.AddForce(pushDirection * pushForce + Vector3.up * pushUpForce);
                    Debug.Log("Player empurrado!");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()                                             // Mostra os raios de alcance no Editor para facilitar ajustes.
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chargeRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, tailAttackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, earthquakeRadius);
    }
}
