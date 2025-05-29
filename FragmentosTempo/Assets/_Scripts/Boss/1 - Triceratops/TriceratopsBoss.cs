using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriceratopsBoss : MonoBehaviour, IBoss
{
    private bool canMove = true;                    // Variável privada para controlar se Boss pode se mover.

    public void SetCanMove(bool value)                  // Método que permite definir se o Trice pode se mover ou não.
    {
        canMove = value;                            // Atualiza a variável de controle de movimento com o valor recebido
        Debug.Log("Trice can move: " + canMove);

        if (!canMove)                               // Cancela ações atuais.
        {
            rb.velocity = Vector3.zero;
            isCharging = false;
            isPreparingCharge = false;
            isTailAttacking = false;
            isEarthquaking = false;
            earthquakeImpactDone = false;

            StopAllCoroutines();                    // Interrompe todas as corrotinas em andamento.
        }
    }

    [Header("Player Target")]
    public Transform player;                            // Referência ao Transform do jogador.

    [Header("Movement Settings")]
    public float chaseSpeed = 5f;                       // Velocidade ao perseguir o jogador.
    public float detectionRange = 10f;                  // Detecção para começar a perseguir.

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3f;
    private int currentPatrolIndex = 0;
    public float waitTimeAtPoint = 2f;
    private float waitTimer;

    [Header("Charge Attack")]
    public float chargeSpeed = 20f;                     // Velocidade durante a investida.
    public float chargeRange = 10f;                     // Distância para começar a investir.
    public float maxChargeDistance = 15f;               // Distância máxima que a investida pode percorrer.
    public float chargePrepareTime = 1.5f;              // Tempo de preparação da investida.
    public float chargeCooldown = 3f;                   // Tempo de recarga da investida.
    private float nextChargeTime = 0f;                  // Marca o tempo para usar a próxima investida.

    [Header("Collision Settings")]
    public float stuckTime = 3f;                        // Tempo que o Triceratops fica preso após bater numa árvore.
    public float pushForce = 1500f;                      // Força horizontal ao empurrar o jogador.
    public float pushUpForce = 600f;                    // Força vertical ao empurrar o jogador.
    public float rotationSpeed = 2f;

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
    public Transform tailPosition;    
    public Transform headPosition;                      // Posição da cabeça para detectar colisões frontais.
    public LayerMask obstacleMask;                      // Layer que detecta obstáculos.

    private Vector3 chargeStartPosition;                // Posição inicial da investida.
    private Vector3 chargeDirection;                    // Direção da investida.
    private Vector3 chargeTarget;                       // Posição alvo da investida.

    private Rigidbody rb;                               // Rigidbody do Triceratops.
    private TriceratopsStateMachine stateMachine;

    private bool isPatrolling = true;

    private bool isStuck = false;                       // Está preso?
    private float stuckTimer;                           // Contador de tempo preso.
    private bool isRotatingAfterUnstuck = false;
    private Quaternion targetRotation;

    private bool isCharging = false;                    // Está em investida?
    private bool isPreparingCharge = false;             // Está se preparando para investir?
    private bool hasChargedDamage = false;              // Verifica se o dano já foi aplicado.

    private bool isTailAttacking = false;               // Está usando o ataque de cauda?
    private float nextTailAttackTime = 0f;              // Próximo tempo que poderá usar golpe de cauda.

    private bool isEarthquaking = false;                // Está usando terremoto?
    private float nextEarthquakeTime = 0f;              // Próximo tempo que poderá usar terremoto.
    private bool earthquakeImpactDone = false;          // Verifica se o impacto já aconteceu.
    private bool isOnGround = true;                     // Verifica se está no chão.

    void Awake()
    {
        rb = GetComponent<Rigidbody>();                 // Pega o Rigidbody no início do jogo.
        stateMachine = GetComponent<TriceratopsStateMachine>();       
    }

    void Update()
    {
        if (player == null)                             // Se não tem jogador, não faz nada.
        {
            return;
        }

        if (!canMove)                                   // Verifica se o movimento está desabilitado.
        {
            StopKinematic();
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

        if (distanceToPlayer <= detectionRange && !isCharging && !isPreparingCharge && !isStuck)
        {
            isPatrolling = false;
            ChasePlayer();                          // Se está perto o suficiente, persegue o jogador.
        }

        if (distanceToPlayer <= detectionRange && !isCharging && !isPreparingCharge)          // Se o jogador está perto e não estiver usando investida, olhar em direção ao jogador.
        {
            RotateTowards(player.position);
        }

        if (distanceToPlayer <= chargeRange && Time.time >= nextChargeTime)         // Se o jogador está perto para iniciar a investida.
        {
            isPatrolling = false;
            PrepareCharge();
            return;
        }

        if (Time.time >= nextEarthquakeTime && distanceToPlayer <= 7f && !isPreparingCharge && !isCharging && !isTailAttacking)
        {
            isPatrolling = false;
            Earthquake();                           // Se o jogador está perto, e o terremoto está disponível.
        }

        if (distanceToPlayer <= tailAttackRange && PlayerIsBehind() && Time.time >= nextTailAttackTime && !isStuck)
        {
            isPatrolling = false;
            TailAttack();                           // Se o jogador está atrás, dentro do alcance e o ataque de cauda está liberado.
            return;
        }

        isPatrolling = true;

        if (isPatrolling && distanceToPlayer > detectionRange)
        {
            Patrol();
        }

        if (isRotatingAfterUnstuck)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                transform.rotation = targetRotation;
                isRotatingAfterUnstuck = false;
            }
        }
    }

    bool PlayerIsBehind() => Vector3.Angle(transform.forward, (player.position - transform.position).normalized) > 120f;        // Método para verificar se o jogador está na parte de trás.

    public void Idle()
    {
        rb.velocity = Vector3.zero;
    }

    public void RotateTowards(Vector3 target)                      // Método para rotacionar em direção ao jogador.
    {
        Vector3 lookDirection = (target - transform.position).normalized;
        lookDirection.y = 0;                                                                                    // Ignora rotação no eixo Y (para não virar para cima ou para baixo).
        if (lookDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            float rotationSpeed = 5f;
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);       // Rotação suave.
        }
    }

    public void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        stateMachine.ChangeState(TriceratopsState.Patrol);

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;

        RotateTowards(targetPoint.position);

        rb.MovePosition(transform.position + direction * patrolSpeed * Time.deltaTime);

        float distance = Vector3.Distance(transform.position, targetPoint.position);

        if (distance < 1f)
        {
            if (waitTimer <= 0)
            {
                int newPatrolIndex = currentPatrolIndex;
                while (newPatrolIndex == currentPatrolIndex && patrolPoints.Length > 1)
                {
                    newPatrolIndex = Random.Range(0, patrolPoints.Length);
                }
                currentPatrolIndex = newPatrolIndex;

                waitTimer = waitTimeAtPoint;
            }
            else
            {
                waitTimer -= Time.deltaTime;
                rb.velocity = Vector3.zero;
            }
        }
    }

    public void ChasePlayer()                                      // Método para perseguir o player.
    {
        stateMachine.ChangeState(TriceratopsState.Chase);

        Vector3 direction = (player.position - transform.position).normalized;
        rb.MovePosition(transform.position + direction * chaseSpeed * Time.deltaTime);

        RotateTowards(player.position);
    }

    public void PrepareCharge()                                    // Método para iniciar o carregamento da investida.
    {

        if (isPreparingCharge || Time.time < nextChargeTime) return;            // Se já está preparando a investida ou ainda não passou o tempo de recarga, sai do método.

        SetCanMove(false);
        isPreparingCharge = true;                                               // Define que o Trice agora está no estado de preparação da investida
        rb.velocity = Vector3.zero;                                             // Para o movimento atual, zerando a velocidade

        stateMachine.ChangeState(TriceratopsState.PrepareCharge);

        chargeTarget = player.position;                                         // Guarda a posição atual do jogador como alvo da investida.
        chargeDirection = (chargeTarget - transform.position).normalized;       // Calcula a direção da investida, do inimigo até o jogador.

        Debug.Log("Preparando investida...");

        Invoke(nameof(Charge), chargePrepareTime);                         // Agenda a chamada do método StartCharge após um tempo de preparação
    }

    public void Charge()                                           // Método para realizar a investida.
    {
        Debug.Log("Inciando investida!");
        isPreparingCharge = false;
        isCharging = true;

        stateMachine.ChangeState(TriceratopsState.Charge);

        if (Physics.Raycast(headPosition.position, chargeDirection, out RaycastHit hit, 2f, obstacleMask))          // Raycast para detectar colisão frontal com obstáculo
        {
            if (hit.collider.CompareTag("Tree"))
            {
                GetStuck();                         // Se acertar uma árvore, fica preso.
                stateMachine.ChangeState(TriceratopsState.Stuck);
                return;
            }
        }

        rb.MovePosition(transform.position + chargeDirection * chargeSpeed * Time.deltaTime);       // Movimento da investida              

        float chargeDistance = Vector3.Distance(transform.position, chargeTarget);           // Verifica se já andou a distância máxima
        if (chargeDistance >= maxChargeDistance)
        {
            EndCharge();                                    // Finaliza a investida.
        }

        nextChargeTime = Time.time + chargeCooldown;        // Define o cooldown para a próxima investida.
    }

    public void EndCharge()                                        // Método para finalizar a investida.
    {
        isCharging = false;
        hasChargedDamage = false;                           // Reseta para permitir dano na próxima investida.
        stateMachine.ChangeState(TriceratopsState.Idle);
        Debug.Log("Trice terminou a investida");
        RotateTowards(player.position);

        SetCanMove(true);
    }

    public void TailAttack()                                       // Método para iniciar o golpe da cauda.
    {
        if (isTailAttacking || isStuck) return;                                                                // Se já estiver atacando, não iniciar outro ataque.

        SetCanMove(false);
        isTailAttacking = true;
        Debug.Log("Triceratops deu um golpe de cauda!");
        nextTailAttackTime = Time.time + tailAttackCooldown;                                        // Ativa cooldown do golpe de cauda.

        rb.velocity = Vector3.zero;                                                                 // Para o movimento do Triceratops.

        stateMachine.ChangeState(TriceratopsState.TailAttack);

        Collider[] hitColliders = Physics.OverlapSphere(tailPosition.position, tailAttackRange, playerMask);       // Detecta o jogador atrás e dentro da área de ataque da cauda.
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

    public void EndTailAttack()                                    // Método para terminar o golpe de cauda.
    {
        isTailAttacking = false;
        Debug.Log("Triceratops terminou o golpe de cauda!");

        stateMachine.ChangeState(TriceratopsState.Idle);

        SetCanMove(true);
    }

    public void Earthquake()                                       // Método para iniciar o terremoto.
    {

        if (isEarthquaking || !isOnGround || isStuck) return;                      // Se já está em terremoto e não está no chão, ignora.

        SetCanMove(false);
        isEarthquaking = true;
        earthquakeImpactDone = false;                                   // Reseta o controle.
        Debug.Log("Triceratops usou TERREMOTO!");
        nextEarthquakeTime = Time.time + earthquakeCooldown;            // Ativa cooldown do terremoto.

        rb.velocity = Vector3.zero;                                     // Para o movimento.
        stateMachine.ChangeState(TriceratopsState.Earthquake);

        Invoke(nameof(ImpactEarthquake), 0.7f);
    }

    public void ImpactEarthquake()                                 // Método para realizar o impacto do terremoto.
    {
        Debug.Log("Triceratops causou o impacto do terremoto!");
        earthquakeImpactDone = true;                                    // Marca que o impacto do terremoto foi realizado.

        Invoke(nameof(EndEarthquake), earthquakeDuration);              // Chama o método EndEarthquake() para encerrar o ataque.
    }

    public void EndEarthquake()                                    // Método para terminar o Terremoto.
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
                        hitRB.AddForce(pushDir * pushForce + Vector3.up * pushUpForce);                             // Empurra o jogador.
                    }
                }
            }
        }

        isEarthquaking = false;                                                                         // Marca que o terremoto acabou.

        stateMachine.ChangeState(TriceratopsState.Idle);
        isOnGround = true;                                                                              // Marca que está no chão.

        SetCanMove(true);
    }

    public void GetStuck()                                         // Método para aprisonar o Boss.
    {
        Debug.Log("Triceratops esta preso na arvore!");
        isStuck = true;
        isCharging = false;
        isPreparingCharge = false;
        stuckTimer = stuckTime;                                                                     // Inicia o contador para se soltar.
        StopKinematic();
        rb.isKinematic = true;                                                                      // Deixa o Rigidbody sem física para não ficar deslizando.
        stateMachine.ChangeState(TriceratopsState.Stuck);
    }

    public void HandleStuck()                                      // Método para lidar com o aprisionar.
    {
        Debug.Log("Contando tempo preso: " + stuckTimer.ToString("F2"));
        stuckTimer -= Time.deltaTime;               // Contagem regressiva do tempo preso.
        if (stuckTimer <= 0)
        {
            Unstuck();                              // Se tempo acabar, solta o Triceratops.
        }
    }

    public void Unstuck()                                          // Método para se soltar após aprisionado.
    {
        Debug.Log("Triceratops se soltou!");
        isStuck = false;
        rb.isKinematic = false;                                                                     // Volta o Rigidbody para a física normal.
        rb.velocity = Vector3.zero;

        Vector3 backward = -transform.forward * 5f + Vector3.up * 2f;                               // Impulso para trás
        rb.AddForce(backward, ForceMode.Impulse);

        targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180f, 0);
        isRotatingAfterUnstuck = true;

        stateMachine.ChangeState(TriceratopsState.Idle);

        canMove = true;
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

    private void StopKinematic()
    {
        if (!rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
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
