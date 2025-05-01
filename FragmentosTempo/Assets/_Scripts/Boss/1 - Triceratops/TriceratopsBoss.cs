using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TriceratopsBoss : MonoBehaviour
{
    [Header("Player Target")]
    public Transform player;                            // Referência ao Transform do jogador.

    [Header("Movement Settings")]
    public float chaseSpeed = 5f;                       // Velocidade ao perseguir o jogador.
    public float detectionRange = 10f;                  // Detecção para começar a perseguir.

    [Header("Charge Attack")]
    public float chargeSpeed = 20f;                     // Velocidade durante a investida.
    public float chargeRange = 10f;                     // Distância para começar a investir.
    public float maxChargeDistance = 15f;               // Distância máxima que a investida pode percorrer.
    public float chargePrepareTime = 1.5f;              // Tempo de preparação da investida.
    public float chargeCooldown = 3f;                   // Tempo de recarga da investida.
    private float nextChargeTime = 0f;                  // Marca o tempo para usar a próxima investida.

    [Header("Collision Settings")]
    public float stuckTime = 3f;                        // Tempo que o Triceratops fica preso após bater numa árvore.
    public float pushForce = 700f;                      // Força horizontal ao empurrar o jogador.
    public float pushUpForce = 150f;                    // Força vertical ao empurrar o jogador.

    [Header("Tail Attack")]
    public float tailAttackRange = 5f;                  // Distância mínima para atacar com a cauda.
    public float tailAttackCooldown = 5f;               // Tempo de recarga do golpe de cauda.
    public float tailAttackDuration = 2f;               // Duração do efeito do golpe de cauda.

    [Header("Earthquake Attack")]
    public float earthquakeRadius = 12f;                // Raio de alcance do ataque terremoto.
    public float earthquakeCooldown = 8f;               // Tempo de recarga do terremoto.
    public float earthquakeDuration = 1.5f;             // Duração do efeito do terremoto.
    public LayerMask playerMask;                        // Layer que detecta o jogador para o terremoto.

    [Header("References")]
    public Transform headPosition;                      // Posição da cabeça para detectar colisões frontais.
    public LayerMask obstacleMask;                      // Layer que detecta obstáculos.

    private Vector3 chargeStartPosition;                // Posição inicial da investida.
    private Vector3 chargeDirection;                    // Direção da investida.
    private Vector3 chargeTarget;                       // Posição alvo da investida.

    private Rigidbody rb;                               // Rigidbody do Triceratops.

    private bool isStuck = false;                       // Está preso?
    private float stuckTimer;                           // Contador de tempo preso.

    private bool isCharging = false;                    // Está em investida?
    private bool isPreparingCharge = false;             // Está se preparando para investir?
    private bool hasChargedDamage = false;              // Verifica se o dano já foi aplicado.

    private bool isTailAttacking = false;               // Está usando o ataque de cauda?
    private float nextTailAttackTime = 0f;              // Próximo tempo que poderá usar golpe de cauda.

    private bool isEarthquaking = false;                // Está usando terremoto?
    private float nextEarthquakeTime = 0f;              // Próximo tempo que poderá usar terremoto.
    private bool earthquakeImpactDone = false;          // Verifica se o impacto já aconteceu.
    private bool isOnGround = true;                     // Verifica se está no chão.

    void Start()
    {
        rb = GetComponent<Rigidbody>();                 // Pega o Rigidbody no início do jogo.
    }

    void Update()
    {
        if (player == null)                             // Se não tem jogador, não faz nada.
        {
            return;
        }

        if (isEarthquaking || isTailAttacking)          // Se está usando terremoto ou golpe de cauda, não realiza outras ações.
        {
            return;
        }

        if (isStuck)                                    // Se estiver preso chama o método para lidar com o aprisionamento.
        {
            HandleStuck();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);         // Calcula distância até o jogador.

        if (Time.time >= nextEarthquakeTime && distanceToPlayer <= 7f)
        {
            Earthquake();                           // Se o jogador está perto, e o terremoto está disponível.
        }

        if (distanceToPlayer <= tailAttackRange && PlayerIsBehind() && Time.time >= nextTailAttackTime)
        {
            TailAttack();                           // Se o jogador está atrás, dentro do alcance e o ataque de cauda está liberado.
            return;
        }

        if (isPreparingCharge)                          // Preparando para investida.
        {
            RotateTowards(player.position);
            return;
        }

        if (isCharging)
        {
            Charge();                                   // Se está investindo, continua o movimento de investida.
            return;
        }
                
        if (distanceToPlayer <= chargeRange && Time.time >= nextChargeTime)         // Se o jogador está perto para iniciar a investida.
        {
            PrepareCharge();
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();                          // Se está perto o suficiente, persegue o jogador.
        }

        if (distanceToPlayer <= detectionRange && !isCharging)          // Se o jogador está perto e não estiver usando investida, olhar em direção ao jogador.
        {
            RotateTowards(player.position);
        }
    }
    
    bool PlayerIsBehind() => Vector3.Angle(transform.forward, (player.position - transform.position).normalized) > 120f;        // Método para verificar se o jogador está na parte de trás.

    void RotateTowards(Vector3 target)                      // Método para rotacionar em direção ao jogador.
    {
        Vector3 lookDirection = (target - transform.position).normalized;
        lookDirection.y = 0;                                                                                    // Ignora rotação no eixo Y (para não virar para cima ou para baixo).
        if (lookDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            float rotationSpeed = 3f;
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);       // Rotação suave.
        }
    }

    void ChasePlayer()                                      // Método para perseguir o player.
    {
        Vector3 direction = (player.position - transform.position).normalized;
        rb.MovePosition(transform.position + direction * chaseSpeed * Time.deltaTime);
        RotateTowards(player.position);
    }

    void PrepareCharge()                                    // Método para iniciar o carregamento da investida.
    {
        if (isPreparingCharge || Time.time < nextChargeTime) return;

        isPreparingCharge = true;
        rb.velocity = Vector3.zero;

        chargeTarget = player.position;
        chargeDirection = (chargeTarget - transform.position).normalized;

        Debug.Log("Preparando investida...");

        Invoke(nameof(StartCharge), chargePrepareTime);
    }

    void StartCharge()                                      // Método para iniciar a investida.
    {
        Debug.Log("Inciando investida!");
        isPreparingCharge = false;
        isCharging = true;
        nextChargeTime = Time.time + chargeCooldown;        // Define o cooldown para a próxima investida.
    }

    void Charge()                                           // Método para realizar a investida.
    {
        if (Physics.Raycast(headPosition.position, chargeDirection, out RaycastHit hit, 2f, obstacleMask))          // Raycast para detectar colisão frontal com obstáculo
        {
            if (hit.collider.CompareTag("Tree"))
            {
                GetStuck();                         // Se acertar uma árvore, fica preso.
                return;
            }
        }

        rb.MovePosition(transform.position + chargeDirection * chargeSpeed * Time.deltaTime);       // Movimento da investida              

        float chargeDistance = Vector3.Distance(transform.position, chargeTarget);           // Verifica se já andou a distância máxima
        if (chargeDistance >= maxChargeDistance)
        {
            EndCharge();                                    // Finaliza a investida.
        }
    }

    void EndCharge()                                        // Método para finalizar a investida.
    {
        isCharging = false;
        hasChargedDamage = false;                               // Reseta para permitir dano na próxima investida.
        Debug.Log("Trice terminou a investida");
        RotateTowards(player.position);
    }

    void TailAttack()                                       // Método para iniciar o golpe da cauda.
    {
        if (isTailAttacking) return;                                                                // Se já estiver atacando, não iniciar outro ataque.

        Debug.Log("Triceratops deu um golpe de cauda!");
        isTailAttacking = true;
        nextTailAttackTime = Time.time + tailAttackCooldown;                                        // Ativa cooldown do golpe de cauda.

        rb.velocity = Vector3.zero;                                                                 // Para o movimento do Triceratops.

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, tailAttackRange, playerMask);       // Detecta o jogador atrás e dentro da área de ataque da cauda.
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
                    Vector3 pushDirection = (hit.transform.position - transform.position).normalized;       // Calcula a direção para empurrar o jogador.
                    playerRB.AddForce(pushDirection * pushForce + Vector3.up * pushUpForce);                // Aplica força empurrando o jogador.
                    Debug.Log("Player empurrado pelo golpe de cauda!");
                }
            }
        }

        Invoke(nameof(EndTailAttack), tailAttackDuration);                                          // Chama o método EndTailAttack() para encerrar o ataque.
    }

    void EndTailAttack()                                    // Método para terminar o golpe de cauda.
    {
        isTailAttacking = false;
        Debug.Log("Triceratops terminou o golpe de cauda!");
    }

    void Earthquake()                                       // Método para iniciar o terremoto.
    {
        if (isEarthquaking || !isOnGround) return;                      // Se já está em terremoto e não está no chão, ignora.

        isEarthquaking = true;
        earthquakeImpactDone = false;                                   // Reseta o controle.
        Debug.Log("Triceratops usou TERREMOTO!");
        nextEarthquakeTime = Time.time + earthquakeCooldown;            // Ativa cooldown do terremoto.

        rb.velocity = Vector3.zero;                                     // Para o movimento.
        rb.AddForce(Vector3.up * 8f, ForceMode.Impulse);

        Invoke(nameof(ImpactEarthquake), 0.7f);
    }

    void ImpactEarthquake()                                 // Método para realizar o impacto do terremoto.
    {
        Debug.Log("Triceratops causou o impacto do terremoto!");
        earthquakeImpactDone = true;                                    // Marca que o impacto do terremoto foi realizado.

        Invoke(nameof(EndEarthquake), earthquakeDuration);              // Chama o método EndEarthquake() para encerrar o ataque.
    }

    void EndEarthquake()                                    // Método para terminar o Terremoto.
    {
        if (earthquakeImpactDone && isOnGround)             // Se o impacto foi feito e o Triceratops está no chão.
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, earthquakeRadius, playerMask);      // Detecta todos jogadores atingidos pelo terremoto
            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("Player"))                                                           // Verifica se o objeto detectado tem a tag "Player".
                {
                    TriceratopsDamageDealer damageDealer = GetComponent<TriceratopsDamageDealer>();     // Pega o componente que lida com dano do Triceratops.
                    damageDealer.DealEarthquakeDamage(transform.position);                              // Aplica o dano de terremoto ao jogador.

                    Debug.Log("Jogador atingido");

                    Rigidbody hitRB = hit.GetComponent<Rigidbody>();                                    // Pega o Rigidbody do jogador para aplicar força.
                    if (hitRB != null)
                    {
                        Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                        hitRB.AddForce(pushDir * 500f + Vector3.up * 300f);                             // Empurra o jogador.
                    }
                }
            }
        }

        isEarthquaking = false;                                                                         // Marca que o terremoto acabou.
        isOnGround = true;                                                                              // Marca que está no chão.
    }
        
    void GetStuck()                                         // Método para aprisonar o Boss.
    {
        Debug.Log("Triceratops esta preso na arvore!");
        isStuck = true;
        isCharging = false;
        isPreparingCharge = false;
        stuckTimer = stuckTime;                                                                     // Inicia o contador para se soltar.
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;                                                                      // Deixa o Rigidbody sem física para não ficar deslizando.
    }

    void HandleStuck()                                      // Método para lidar com o aprisionar.
    {
        Debug.Log("Contando tempo preso: " + stuckTimer.ToString("F2"));
        stuckTimer -= Time.deltaTime;               // Contagem regressiva do tempo preso.
        if (stuckTimer <= 0)
        {
            Unstuck();                              // Se tempo acabar, solta o Triceratops.
        }
    }

    void Unstuck()                                          // Método para se soltar após aprisionado.
    {
        Debug.Log("Triceratops se soltou!");
        isStuck = false;
        rb.isKinematic = false;                                                                     // Volta o Rigidbody para a física normal.
        rb.velocity = Vector3.zero;

        Vector3 backward = -transform.forward * 5f + Vector3.up * 2f;                               // Impulso para trás
        rb.AddForce(backward, ForceMode.Impulse);

        transform.Rotate(0, 180f, 0);                                                               // Gira para trás (180°)
    }

    private void OnCollisionEnter(Collision collision)      // Método de colisões.
    {
        if (collision.gameObject.CompareTag("Ground"))      // Verificar se está no chão.
        {
            isOnGround = true;                              // Marca que está no chão.
        }

        if (isCharging)                                     // Se está investindo.
        {
            if (collision.gameObject.CompareTag("Tree"))            // Se colidir com árvore enquanto investe, fica preso.
            {
                if (isCharging)                                     // Garantir que só fique preso se colidir enquanto usa a investida.
                {
                    GetStuck();

                    TreeFall tree = collision.gameObject.GetComponent<TreeFall>();
                    if (tree != null)
                    {
                        tree.ForceFall(gameObject);   // Chamamos a função para derrubar a árvore imediatamente
                    }
                }                
            }
        }

        if (collision.gameObject.CompareTag("Player"))     // Se colidiu com o jogador.
        {
            if (!hasChargedDamage)                              // Se ainda não causou dano da investida.
            {
                hasChargedDamage = true;                        // Marca que já causou dano.

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

    private void OnDrawGizmosSelected()                     // Método para mostrar os raios de alcance no Editor para facilitar ajustes.
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
