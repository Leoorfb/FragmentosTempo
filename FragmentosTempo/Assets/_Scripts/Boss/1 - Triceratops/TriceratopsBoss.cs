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
    public float detectionRange = 10f;                  // Detec��o para come�ar a perseguir.

    [Header("Charge Attack")]
    public float chargeSpeed = 20f;                     // Velocidade durante a investida.
    public float chargeRange = 10f;                     // Dist�ncia para come�ar a investir.
    public float maxChargeDistance = 15f;               // Dist�ncia m�xima que a investida pode percorrer.
    public float chargePrepareTime = 1.5f;              // Tempo de prepara��o da investida.
    public float chargeCooldown = 3f;                   // Tempo de recarga da investida.
    private float nextChargeTime = 0f;                  // Marca o tempo para usar a pr�xima investida.

    [Header("Collision Settings")]
    public float stuckTime = 3f;                        // Tempo que o Triceratops fica preso ap�s bater numa �rvore.
    public float pushForce = 700f;                      // For�a horizontal ao empurrar o jogador.
    public float pushUpForce = 150f;                    // For�a vertical ao empurrar o jogador.

    [Header("Tail Attack")]
    public float tailAttackRange = 5f;                  // Dist�ncia m�nima para atacar com a cauda.
    public float tailAttackCooldown = 5f;               // Tempo de recarga do golpe de cauda.
    public float tailAttackDuration = 2f;               // Dura��o do efeito do golpe de cauda.

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

    private bool isStuck = false;                       // Est� preso?
    private float stuckTimer;                           // Contador de tempo preso.

    private bool isCharging = false;                    // Est� em investida?
    private bool isPreparingCharge = false;             // Est� se preparando para investir?
    private bool hasChargedDamage = false;              // Verifica se o dano j� foi aplicado.

    private bool isTailAttacking = false;               // Est� usando o ataque de cauda?
    private float nextTailAttackTime = 0f;              // Pr�ximo tempo que poder� usar golpe de cauda.

    private bool isEarthquaking = false;                // Est� usando terremoto?
    private float nextEarthquakeTime = 0f;              // Pr�ximo tempo que poder� usar terremoto.
    private bool earthquakeImpactDone = false;          // Verifica se o impacto j� aconteceu.
    private bool isOnGround = true;                     // Verifica se est� no ch�o.

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

        if (isEarthquaking || isTailAttacking)          // Se est� usando terremoto ou golpe de cauda, n�o realiza outras a��es.
        {
            return;
        }

        if (isStuck)                                    // Se estiver preso chama o m�todo para lidar com o aprisionamento.
        {
            HandleStuck();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);         // Calcula dist�ncia at� o jogador.

        if (Time.time >= nextEarthquakeTime && distanceToPlayer <= 7f)
        {
            Earthquake();                           // Se o jogador est� perto, e o terremoto est� dispon�vel.
        }

        if (distanceToPlayer <= tailAttackRange && PlayerIsBehind() && Time.time >= nextTailAttackTime)
        {
            TailAttack();                           // Se o jogador est� atr�s, dentro do alcance e o ataque de cauda est� liberado.
            return;
        }

        if (isPreparingCharge)                          // Preparando para investida.
        {
            RotateTowards(player.position);
            return;
        }

        if (isCharging)
        {
            Charge();                                   // Se est� investindo, continua o movimento de investida.
            return;
        }
                
        if (distanceToPlayer <= chargeRange && Time.time >= nextChargeTime)         // Se o jogador est� perto para iniciar a investida.
        {
            PrepareCharge();
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();                          // Se est� perto o suficiente, persegue o jogador.
        }

        if (distanceToPlayer <= detectionRange && !isCharging)          // Se o jogador est� perto e n�o estiver usando investida, olhar em dire��o ao jogador.
        {
            RotateTowards(player.position);
        }
    }
    
    bool PlayerIsBehind() => Vector3.Angle(transform.forward, (player.position - transform.position).normalized) > 120f;        // M�todo para verificar se o jogador est� na parte de tr�s.

    void RotateTowards(Vector3 target)                      // M�todo para rotacionar em dire��o ao jogador.
    {
        Vector3 lookDirection = (target - transform.position).normalized;
        lookDirection.y = 0;                                                                                    // Ignora rota��o no eixo Y (para n�o virar para cima ou para baixo).
        if (lookDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            float rotationSpeed = 3f;
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);       // Rota��o suave.
        }
    }

    void ChasePlayer()                                      // M�todo para perseguir o player.
    {
        Vector3 direction = (player.position - transform.position).normalized;
        rb.MovePosition(transform.position + direction * chaseSpeed * Time.deltaTime);
        RotateTowards(player.position);
    }

    void PrepareCharge()                                    // M�todo para iniciar o carregamento da investida.
    {
        if (isPreparingCharge || Time.time < nextChargeTime) return;

        isPreparingCharge = true;
        rb.velocity = Vector3.zero;

        chargeTarget = player.position;
        chargeDirection = (chargeTarget - transform.position).normalized;

        Debug.Log("Preparando investida...");

        Invoke(nameof(StartCharge), chargePrepareTime);
    }

    void StartCharge()                                      // M�todo para iniciar a investida.
    {
        Debug.Log("Inciando investida!");
        isPreparingCharge = false;
        isCharging = true;
        nextChargeTime = Time.time + chargeCooldown;        // Define o cooldown para a pr�xima investida.
    }

    void Charge()                                           // M�todo para realizar a investida.
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

        float chargeDistance = Vector3.Distance(transform.position, chargeTarget);           // Verifica se j� andou a dist�ncia m�xima
        if (chargeDistance >= maxChargeDistance)
        {
            EndCharge();                                    // Finaliza a investida.
        }
    }

    void EndCharge()                                        // M�todo para finalizar a investida.
    {
        isCharging = false;
        hasChargedDamage = false;                               // Reseta para permitir dano na pr�xima investida.
        Debug.Log("Trice terminou a investida");
        RotateTowards(player.position);
    }

    void TailAttack()                                       // M�todo para iniciar o golpe da cauda.
    {
        if (isTailAttacking) return;                                                                // Se j� estiver atacando, n�o iniciar outro ataque.

        Debug.Log("Triceratops deu um golpe de cauda!");
        isTailAttacking = true;
        nextTailAttackTime = Time.time + tailAttackCooldown;                                        // Ativa cooldown do golpe de cauda.

        rb.velocity = Vector3.zero;                                                                 // Para o movimento do Triceratops.

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, tailAttackRange, playerMask);       // Detecta o jogador atr�s e dentro da �rea de ataque da cauda.
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                TriceratopsDamageDealer damageDealer = GetComponent<TriceratopsDamageDealer>();     // Pega o componente de dano do Triceratops.
                if (damageDealer != null)
                {
                    damageDealer.DealTailDamage(hit.gameObject);                                    // Aplica o dano de golpe da cauda.
                }

                Rigidbody playerRB = hit.GetComponent<Rigidbody>();
                if (playerRB != null)
                {
                    Vector3 pushDirection = (hit.transform.position - transform.position).normalized;       // Calcula a dire��o para empurrar o jogador.
                    playerRB.AddForce(pushDirection * pushForce + Vector3.up * pushUpForce);                // Aplica for�a empurrando o jogador.
                    Debug.Log("Player empurrado pelo golpe de cauda!");
                }
            }
        }

        Invoke(nameof(EndTailAttack), tailAttackDuration);                                          // Chama o m�todo EndTailAttack() para encerrar o ataque.
    }

    void EndTailAttack()                                    // M�todo para terminar o golpe de cauda.
    {
        isTailAttacking = false;
        Debug.Log("Triceratops terminou o golpe de cauda!");
    }

    void Earthquake()                                       // M�todo para iniciar o terremoto.
    {
        if (isEarthquaking || !isOnGround) return;                      // Se j� est� em terremoto e n�o est� no ch�o, ignora.

        isEarthquaking = true;
        earthquakeImpactDone = false;                                   // Reseta o controle.
        Debug.Log("Triceratops usou TERREMOTO!");
        nextEarthquakeTime = Time.time + earthquakeCooldown;            // Ativa cooldown do terremoto.

        rb.velocity = Vector3.zero;                                     // Para o movimento.
        rb.AddForce(Vector3.up * 8f, ForceMode.Impulse);

        Invoke(nameof(ImpactEarthquake), 0.7f);
    }

    void ImpactEarthquake()                                 // M�todo para realizar o impacto do terremoto.
    {
        Debug.Log("Triceratops causou o impacto do terremoto!");
        earthquakeImpactDone = true;                                    // Marca que o impacto do terremoto foi realizado.

        Invoke(nameof(EndEarthquake), earthquakeDuration);              // Chama o m�todo EndEarthquake() para encerrar o ataque.
    }

    void EndEarthquake()                                    // M�todo para terminar o Terremoto.
    {
        if (earthquakeImpactDone && isOnGround)             // Se o impacto foi feito e o Triceratops est� no ch�o.
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, earthquakeRadius, playerMask);      // Detecta todos jogadores atingidos pelo terremoto
            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("Player"))                                                           // Verifica se o objeto detectado tem a tag "Player".
                {
                    TriceratopsDamageDealer damageDealer = GetComponent<TriceratopsDamageDealer>();     // Pega o componente que lida com dano do Triceratops.
                    damageDealer.DealEarthquakeDamage(transform.position);                              // Aplica o dano de terremoto ao jogador.

                    Debug.Log("Jogador atingido");

                    Rigidbody hitRB = hit.GetComponent<Rigidbody>();                                    // Pega o Rigidbody do jogador para aplicar for�a.
                    if (hitRB != null)
                    {
                        Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                        hitRB.AddForce(pushDir * 500f + Vector3.up * 300f);                             // Empurra o jogador.
                    }
                }
            }
        }

        isEarthquaking = false;                                                                         // Marca que o terremoto acabou.
        isOnGround = true;                                                                              // Marca que est� no ch�o.
    }
        
    void GetStuck()                                         // M�todo para aprisonar o Boss.
    {
        Debug.Log("Triceratops esta preso na arvore!");
        isStuck = true;
        isCharging = false;
        isPreparingCharge = false;
        stuckTimer = stuckTime;                                                                     // Inicia o contador para se soltar.
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;                                                                      // Deixa o Rigidbody sem f�sica para n�o ficar deslizando.
    }

    void HandleStuck()                                      // M�todo para lidar com o aprisionar.
    {
        Debug.Log("Contando tempo preso: " + stuckTimer.ToString("F2"));
        stuckTimer -= Time.deltaTime;               // Contagem regressiva do tempo preso.
        if (stuckTimer <= 0)
        {
            Unstuck();                              // Se tempo acabar, solta o Triceratops.
        }
    }

    void Unstuck()                                          // M�todo para se soltar ap�s aprisionado.
    {
        Debug.Log("Triceratops se soltou!");
        isStuck = false;
        rb.isKinematic = false;                                                                     // Volta o Rigidbody para a f�sica normal.
        rb.velocity = Vector3.zero;

        Vector3 backward = -transform.forward * 5f + Vector3.up * 2f;                               // Impulso para tr�s
        rb.AddForce(backward, ForceMode.Impulse);

        transform.Rotate(0, 180f, 0);                                                               // Gira para tr�s (180�)
    }

    private void OnCollisionEnter(Collision collision)      // M�todo de colis�es.
    {
        if (collision.gameObject.CompareTag("Ground"))      // Verificar se est� no ch�o.
        {
            isOnGround = true;                              // Marca que est� no ch�o.
        }

        if (isCharging)                                     // Se est� investindo.
        {
            if (collision.gameObject.CompareTag("Tree"))            // Se colidir com �rvore enquanto investe, fica preso.
            {
                if (isCharging)                                     // Garantir que s� fique preso se colidir enquanto usa a investida.
                {
                    GetStuck();

                    TreeFall tree = collision.gameObject.GetComponent<TreeFall>();
                    if (tree != null)
                    {
                        tree.ForceFall(gameObject);   // Chamamos a fun��o para derrubar a �rvore imediatamente
                    }
                }                
            }
        }

        if (collision.gameObject.CompareTag("Player"))     // Se colidiu com o jogador.
        {
            if (!hasChargedDamage)                              // Se ainda n�o causou dano da investida.
            {
                hasChargedDamage = true;                        // Marca que j� causou dano.

                Rigidbody playerRB = collision.gameObject.GetComponent<Rigidbody>();                // Se colidir com jogador enquanto investe, empurra o jogador
                if (playerRB != null)
                {
                    Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
                    playerRB.AddForce(pushDirection * pushForce + Vector3.up * pushUpForce);
                    Debug.Log("Player empurrado!");

                    var damageDealer = GetComponent<TriceratopsDamageDealer>();         // Pega o componente de dano do Triceratops.
                    if (damageDealer != null)
                    {
                        damageDealer.DealChargeDamage(collision.gameObject);            // Aplica o dano de investida ao jogador.
                        Debug.Log("Dano da investida aplicado!");
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()                     // M�todo para mostrar os raios de alcance no Editor para facilitar ajustes.
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chargeRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxChargeDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, tailAttackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, earthquakeRadius);
    }
}
