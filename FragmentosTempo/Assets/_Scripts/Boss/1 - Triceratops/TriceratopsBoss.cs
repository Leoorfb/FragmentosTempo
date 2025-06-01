using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriceratopsBoss : MonoBehaviour, IBoss
{
    private bool canMove = true;                    // Variável privada para controlar se Boss pode se mover.

    public void SetCanMove(bool value)              // Método que permite definir se o Trice pode se mover ou não.
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
    public Transform player;                             // Referência ao Transform do jogador.

    [Header("Movement Settings")]
    public float chaseSpeed = 5f;                       // Velocidade ao perseguir o jogador.
    public float detectionRange = 10f;                  // Detecção para começar a perseguir.

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;                    // Referência ao Transform do pontos de patrulha.
    public float patrolSpeed = 3f;                      // Velocidade da patrulha.
    private int currentPatrolIndex = 0;                 // Atual ponto de patrulha.
    public float waitTimeAtPoint = 2f;                  // Tempo de espera no ponto de patrulha.
    private float waitTimer;                            // Temporizador para o tempo de patrulha.

    [Header("Charge Attack")]
    public float chargeSpeed = 20f;                     // Velocidade durante a investida.
    public float chargeRange = 10f;                     // Distância para começar a investir.
    public float maxChargeDistance = 15f;               // Distância máxima que a investida pode percorrer.
    public float chargePrepareTime = 1.5f;              // Tempo de preparação da investida.
    public float chargeCooldown = 3f;                   // Tempo de recarga da investida.
    private float nextChargeTime = 0f;                  // Marca o tempo para usar a próxima investida.

    [Header("Collision Settings")]
    public float stuckTime = 3f;                        // Tempo que o Triceratops fica preso após bater em uma árvore.
    public float pushForce = 1500f;                     // Força horizontal ao empurrar o jogador.
    public float pushUpForce = 600f;                    // Força vertical ao empurrar o jogador.
    public float rotationSpeed = 2f;                    // Velocidade de rotação ao sair do aprisionamento.

    [Header("Tail Attack")]
    public float tailAttackRange = 5f;                  // Distância mínima para atacar com a cauda.
    public float tailAttackCooldown = 5f;               // Tempo de recarga do golpe de cauda.
    public float tailAttackDuration = 2f;               // Duração do efeito do golpe de cauda.

    [Header("Earthquake Attack")]
    public float earthquakeRadius = 12f;                // Raio de alcance do ataque terremoto.
    public float earthquakeCooldown = 8f;               // Tempo de recarga do terremoto.
    public float earthquakeDuration = 1.5f;             // Duração do efeito do terremoto.
    public LayerMask playerMask;                        // Layer que detecta o jogador para o terremoto.

    [Header("VFX Settings")]
    [SerializeField] private GameObject prepareVFX;
    [SerializeField] private Transform vfxSpawnPrepare;
    [SerializeField] private GameObject chargeVFX;
    [SerializeField] private Transform vfxSpawnCharge;
    [SerializeField] private GameObject tailVFX;
    [SerializeField] private Transform vfxSpawnTail;
    [SerializeField] private GameObject earthquakeVFX;
    [SerializeField] private Transform vfxSpawnEarthquake;
    [SerializeField] private GameObject stuckVFX;
    [SerializeField] private Transform vfxSpawnStuck;


    [Header("References")]
    public Transform tailPosition;                      // Posição da cauda para detectar colisões traseiras.
    public Transform headPosition;                      // Posição da cabeça para detectar colisões frontais.
    public LayerMask obstacleMask;                      // Layer que detecta obstáculos.

    private Vector3 chargeStartPosition;                // Posição inicial da investida.
    private Vector3 chargeDirection;                    // Direção da investida.
    private Vector3 chargeTarget;                       // Posição alvo da investida.

    private Rigidbody rb;                               // Rigidbody do Triceratops.
    private TriceratopsStateMachine stateMachine;       // Referência ao script de State Machine.

    private bool isPatrolling = true;                   // Verificação se está em patrulha.

    private bool isStuck = false;                       // Está preso?
    private float stuckTimer;                           // Contador de tempo preso.
    private bool isRotatingAfterUnstuck = false;        // Verificação se rotacionou após sair do aprisionamento.
    private Quaternion targetRotation;                  // Posição para rotacionar.

    private bool isCharging = false;                    // Vertifica se está em investida.
    private bool isPreparingCharge = false;             // Vertifica se está se preparando para investir.
    private bool hasChargedDamage = false;              // Verifica se o dano já foi aplicado.
    private GameObject chargeVFXInstance;

    private bool isTailAttacking = false;               // Vertifica se está usando o ataque de cauda.
    private float nextTailAttackTime = 0f;              // Próximo tempo que poderá usar golpe de cauda.

    private bool isEarthquaking = false;                // Vertifica se está usando terremoto.
    private float nextEarthquakeTime = 0f;              // Próximo tempo que poderá usar terremoto.
    private bool earthquakeImpactDone = false;          // Verifica se o impacto já aconteceu.
    private bool isOnGround = true;                     // Verifica se está no chão.

    void Awake()
    {
        rb = GetComponent<Rigidbody>();                             // Pega o Rigidbody no início do jogo.
        stateMachine = GetComponent<TriceratopsStateMachine>();     // Pega os States Machines no início do jogo. 
    }

    void Update()
    {
        if (player == null)                             // Se não tem jogador, entrar em State Machine de "Patrulha".
        {
            Patrol();
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

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);             // Calcula distância até o jogador.

        if (distanceToPlayer <= detectionRange && !isCharging && !isPreparingCharge && !isStuck)    // Se está perto o suficiente, não está investindo ou preso, persegue o jogador.
        {
            isPatrolling = false;
            ChasePlayer();                              // Chama o método de perseguir o jogador.
        }

        if (distanceToPlayer <= detectionRange && !isCharging && !isPreparingCharge)          // Se o jogador está perto e não estiver usando investida, olhar em direção ao jogador.
        {
            RotateTowards(player.position);             // Chama o método de rotacionar para o jogador.
        }

        if (distanceToPlayer <= chargeRange && Time.time >= nextChargeTime)         // Se o jogador está perto, iniciar a investida.
        {
            isPatrolling = false;
            PrepareCharge();                            // Chama o método para preparar a investida.
            return;
        }

        if (Time.time >= nextEarthquakeTime && distanceToPlayer <= 7f && !isPreparingCharge && !isCharging && !isTailAttacking)     // Se o jogador está perto, e o terremoto está disponível.
        {
            isPatrolling = false;
            Earthquake();                               // Chama o método de usar o terremoto.
        }

        if (distanceToPlayer <= tailAttackRange && PlayerIsBehind() && Time.time >= nextTailAttackTime && !isStuck)         // Se o jogador está atrás, dentro do alcance e o ataque de cauda está liberado.
        {
            isPatrolling = false;
            TailAttack();                           // Chama o método de usar o ataque de cauda.
            return;
        }

        isPatrolling = true;                        // Ativa o State Machine de patrulha.

        if (isPatrolling && distanceToPlayer > detectionRange)      // Se estiver em patrulha ativado e a distância para o jogador é maior do que a distância de detecção.
        {
            Patrol();                               // Chama o método para patrulhar.
        }

        if (isRotatingAfterUnstuck)                 // Verifica se o Triceratops está rotacionando após se soltar.
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);      // Suaviza a rotação atual em direção à rotação alvo usando Slerp.

            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)      // Verifica se o ângulo entre a rotação atual e a rotação alvo é menor que 1 grau.
            {
                transform.rotation = targetRotation;                            // Define a rotação diretamente como a rotação alvo.
                isRotatingAfterUnstuck = false;                                 // Desativa a rotação após aprisionamento, indicando que a rotação foi concluída.
            }
        }
    }

    private void FixedUpdate()
    {
        if (isCharging)
        {
            if (Physics.Raycast(headPosition.position, chargeDirection, out RaycastHit hit, 2f, obstacleMask))          // Raycast para detectar colisão frontal com obstáculo.
            {
                if (hit.collider.CompareTag("Tree"))                    // Se acertar uma árvore, fica preso.
                {
                    GetStuck();
                    return;
                }
            }

            rb.MovePosition(rb.position + chargeDirection * chargeSpeed * Time.deltaTime);

            float chargeDistance = Vector3.Distance(transform.position, chargeTarget);
            if (chargeDistance >= maxChargeDistance)                                                // Verifica se já andou a distância máxima.
            {
                EndCharge();                                    // Chama o método para finalizar a investida.
            }
        }
    }

    bool PlayerIsBehind() => Vector3.Angle(transform.forward, (player.position - transform.position).normalized) > 120f;        // Método para verificar se o jogador está na parte de trás.

    public void Idle()                                  // Método para deixar o Triceratops imóvel em State Machine de Idle.
    {
        rb.velocity = Vector3.zero;
    }

    public void RotateTowards(Vector3 target)           // Método para rotacionar em direção ao jogador.
    {
        Vector3 lookDirection = (target - transform.position).normalized;
        lookDirection.y = 0;                                                            // Ignora rotação no eixo Y (para não virar para cima ou para baixo).
        if (lookDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            float rotationSpeed = 5f;
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);       // Rotação suave.
        }
    }

    public void Patrol()                                // Método para entrar em State Machine de patrulha.
    {
        if (patrolPoints.Length == 0) return;                                               // Se não há pontos de patrulha, encerra o método.

        stateMachine.ChangeState(TriceratopsState.Patrol);                                  // Entra no State Machine de patrulha.

        Transform targetPoint = patrolPoints[currentPatrolIndex];                           // Define o ponto de patrulha atual como alvo.
        Vector3 direction = (targetPoint.position - transform.position).normalized;         // Calcula a direção normalizada até o ponto de patrulha.

        RotateTowards(targetPoint.position);                                                // Rotaciona o Triceratops em direção ao ponto de patrulha.

        rb.MovePosition(transform.position + direction * patrolSpeed * Time.deltaTime);     // Move o Triceratops em direção ao ponto de patrulha.

        float distance = Vector3.Distance(transform.position, targetPoint.position);        // Calcula a distância até o ponto de patrulha.

        if (distance < 1f)                                                                  // Se está próximo do ponto de patrulha.
        {
            if (waitTimer <= 0)                                                             // Se o tempo de espera terminou.
            {
                int newPatrolIndex = currentPatrolIndex;                                    // Inicializa o novo índice igual ao atual.
                while (newPatrolIndex == currentPatrolIndex && patrolPoints.Length > 1)     // Garante que o novo índice seja diferente do atual.
                {
                    newPatrolIndex = Random.Range(0, patrolPoints.Length);                  // Escolhe um novo ponto aleatório.
                }
                currentPatrolIndex = newPatrolIndex;                                        // Atualiza o índice de patrulha.

                waitTimer = waitTimeAtPoint;                                                // Reseta o temporizador de espera.
            }
            else
            {
                waitTimer -= Time.deltaTime;                                                // Reduz o temporizador com o tempo.
                rb.velocity = Vector3.zero;                                                 // Para o movimento enquanto espera.
            }
        }
    }

    public void ChasePlayer()                           // Método para perseguir o player.
    {
        stateMachine.ChangeState(TriceratopsState.Chase);                                   // Entra no State Machine de Perseguir.

        Vector3 direction = (player.position - transform.position).normalized;              // Calcula a direção normalizada até o jogador.
        rb.MovePosition(transform.position + direction * chaseSpeed * Time.deltaTime);      // Move o Triceratops na direção do jogador.

        RotateTowards(player.position);                                                     // Rotaciona o Triceratops para olhar na direção do jogador.
    }

    public void PrepareCharge()                         // Método para iniciar o carregamento da investida.
    {

        if (isPreparingCharge || Time.time < nextChargeTime) return;            // Se já está preparando a investida ou ainda não passou o tempo de recarga, sai do método.

        SetCanMove(false);                                                      // Desativa a movimentação do Triceratops.
        isPreparingCharge = true;                                               // Ativa que o Triceratops está em preparação da investida.
        rb.velocity = Vector3.zero;                                             // Para o movimento atual, zerando a velocidade.

        stateMachine.ChangeState(TriceratopsState.PrepareCharge);               // Entra do State Machine de Preparação de Investida.

        chargeTarget = player.position;                                         // Guarda a posição atual do jogador como alvo da investida.
        chargeDirection = (chargeTarget - transform.position).normalized;       // Calcula a direção da investida, do Triceratops até o jogador.

        Debug.Log("Preparando investida...");

        if (prepareVFX != null)
        {
            GameObject vfxInstance = Instantiate(prepareVFX, vfxSpawnPrepare != null ? vfxSpawnPrepare.position : transform.position, Quaternion.identity);
            Destroy(vfxInstance, 5f);
        }

        Invoke(nameof(Charge), chargePrepareTime);                              // Agenda a chamada do método Charge após um tempo de preparação.
    }

    public void Charge()                                // Método para realizar a investida.
    {
        if (chargeVFX != null)
        {
            chargeVFXInstance = Instantiate(chargeVFX, vfxSpawnCharge != null ? vfxSpawnCharge.position : transform.position, Quaternion.identity);
            chargeVFXInstance.transform.SetParent(transform);       // Faz o VFX seguir o Triceratops
        }

        Debug.Log("Inciando investida!");
        isPreparingCharge = false;                                  // Desativa a preparação de investida.
        isCharging = true;                                          // Ativa a investida.

        stateMachine.ChangeState(TriceratopsState.Charge);          // Entra no State Machine de Investida.        
    }

    public void EndCharge()                             // Método para finalizar a investida.
    {
        isCharging = false;                                 // Desativa a investida.
        hasChargedDamage = false;                           // Reseta para permitir dano na próxima investida.
        stateMachine.ChangeState(TriceratopsState.Idle);    // Entra no State Machine de Idle.
        Debug.Log("Trice terminou a investida");
        RotateTowards(player.position);                     // Rotaciona para o jogador.

        SetCanMove(true);                                   // Reativa os movimentos do Triceratops.

        nextChargeTime = Time.time + chargeCooldown;        // Define o cooldown para a próxima investida.

        if (chargeVFXInstance != null)
        {
            Destroy(chargeVFXInstance);
            chargeVFXInstance = null;
        }
    }

    public void TailAttack()                            // Método para iniciar o golpe da cauda.
    {
        if (isTailAttacking) return;                                                                // Se já estiver atacando, não iniciar outro ataque.

        SetCanMove(false);                                                 // Desativa os movimentos.
        isTailAttacking = true;                                            // Ativa o ataque de cauda.
        Debug.Log("Triceratops deu um golpe de cauda!");
        nextTailAttackTime = Time.time + tailAttackCooldown;               // Ativa cooldown do golpe de cauda.

        rb.velocity = Vector3.zero;                                        // Para o movimento do Triceratops.

        stateMachine.ChangeState(TriceratopsState.TailAttack);             // Entra no State Machine de Golpe de Cauda.

        if (tailVFX != null)
        {
            GameObject vfxInstance = Instantiate(tailVFX, vfxSpawnTail != null ? vfxSpawnTail.position : transform.position, Quaternion.identity);
            Destroy(vfxInstance , 5f);
        }

        Collider[] hitColliders = Physics.OverlapSphere(tailPosition.position, tailAttackRange, playerMask);       // Detecta o jogador atrás e dentro da área de ataque da cauda.
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))                                                           // Se acertar o jogador.
            {
                TriceratopsDamageDealer damageDealer = GetComponent<TriceratopsDamageDealer>();     // Pega o componente de dano do Triceratops.
                if (damageDealer != null)
                {
                    damageDealer.DealTailDamage(hit.gameObject);                                    // Aplica o dano de golpe da cauda.
                }

                Rigidbody playerRB = hit.GetComponent<Rigidbody>();                                         // Pega o componente de Rigidbody do jogador.
                if (playerRB != null)
                {
                    Vector3 pushDirection = (hit.transform.position - transform.position).normalized;       // Calcula a direção para empurrar o jogador.
                    playerRB.AddForce(pushDirection * pushForce + Vector3.up * pushUpForce);                // Aplica força empurrando o jogador.
                    Debug.Log("Player empurrado pelo golpe de cauda!");
                }
            }
        }

        Invoke(nameof(EndTailAttack), tailAttackDuration);                                          // Chama o método para encerrar o Golpe de Cauda.
    }

    public void EndTailAttack()                         // Método para terminar o golpe de cauda.
    {
        isTailAttacking = false;                                // Desativa o golpe de cauda.
        Debug.Log("Triceratops terminou o golpe de cauda!");

        stateMachine.ChangeState(TriceratopsState.Idle);        // Entra no State Machine de Idle.

        SetCanMove(true);                                       // Reativa os movimentos.
    }

    public void Earthquake()                            // Método para iniciar o terremoto.
    {

        if (isEarthquaking || !isOnGround || isStuck) return;           // Se já está em terremoto, não está no chão ou preso, ignora.

        SetCanMove(false);                                              // Desativa os movimentos.
        isEarthquaking = true;                                          // Ativa o terremoto.
        earthquakeImpactDone = false;                                   // Deixa desativado o impacto do terremoto.
        Debug.Log("Triceratops usou TERREMOTO!");
        nextEarthquakeTime = Time.time + earthquakeCooldown;            // Ativa cooldown do terremoto.

        rb.velocity = Vector3.zero;                                     // Para o movimento.
        stateMachine.ChangeState(TriceratopsState.Earthquake);          // Entra no State Machine de Terremoto.

        Invoke(nameof(ImpactEarthquake), 0.7f);                         // Chama o método de Impacto do Terremoto.
    }

    public void ImpactEarthquake()                      // Método para realizar o impacto do terremoto.
    {
        Debug.Log("Triceratops causou o impacto do terremoto!");
        earthquakeImpactDone = true;                              // Marca que o impacto do terremoto foi realizado.

        Invoke(nameof(EndEarthquake), earthquakeDuration);        // Chama o método para encerrar o terremoto.
    }

    public void EndEarthquake()                         // Método para terminar o Terremoto.
    {
        SoundManager.Instance.PlaySound3D("TriceEarthquake", transform.position);                       // Chama o efeito sonoro de terremoto.
        if (earthquakeImpactDone && isOnGround)             // Se o impacto foi feito e o Triceratops está no chão.
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, earthquakeRadius, playerMask);      // Detecta todos jogadores atingidos pelo terremoto
            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("Player"))                                                           // Verifica se atingiu o jogador.
                {
                    TriceratopsDamageDealer damageDealer = GetComponent<TriceratopsDamageDealer>();     // Pega o componente que lida com dano do Triceratops.
                    damageDealer.DealEarthquakeDamage(transform.position);                              // Aplica o dano de terremoto ao jogador.

                    Debug.Log("Jogador atingido");

                    Rigidbody hitRB = hit.GetComponent<Rigidbody>();                                    // Pega o Rigidbody do jogador para aplicar força.
                    if (hitRB != null)
                    {
                        Vector3 pushDir = (hit.transform.position - transform.position).normalized;     // Direção para empurrar o jogador.
                        hitRB.AddForce(pushDir * pushForce + Vector3.up * pushUpForce);                 // Empurra o jogador.
                    }
                }
            }
        }

        isEarthquaking = false;                                        // Marca que o terremoto acabou.

        stateMachine.ChangeState(TriceratopsState.Idle);               // Entra no State Machine de Idle.
        isOnGround = true;                                             // Marca que está no chão.

        SetCanMove(true);                                              // Reativa os movimentos.

        PlayVFX(earthquakeVFX, vfxSpawnEarthquake.position, vfxSpawnEarthquake.rotation, 3f);
    }

    public void GetStuck()                              // Método para aprisonar o Triceratops.
    {
        Debug.Log("Triceratops esta preso na arvore!");
        isStuck = true;                                         // Ativa como preso.
        SoundManager.Instance.PlaySound3D("TriceStuck", transform.position);
        isCharging = false;                                     // Desativa a investida.
        isPreparingCharge = false;                              // Desativa a preparação da investida.
        stuckTimer = stuckTime;                                 // Inicia o contador para se soltar.
        StopKinematic();
        rb.isKinematic = true;                                  // Deixa o Rigidbody sem física para não ficar deslizando.
        stateMachine.ChangeState(TriceratopsState.Stuck);       // Entra no State Machine de Aprisionado.
    }

    public void HandleStuck()                           // Método para lidar com o aprisionar.
    {
        Debug.Log("Contando tempo preso: " + stuckTimer.ToString("F2"));
        stuckTimer -= Time.deltaTime;               // Contagem regressiva do tempo preso.
        if (stuckTimer <= 0)
        {
            Unstuck();                              // Se tempo acabar, chama o método para soltar o Triceratops.
        }
    }

    public void Unstuck()                               // Método para se soltar após aprisionado.
    {
        Debug.Log("Triceratops se soltou!");
        isStuck = false;                                                            // Destiva o aprisionar.
        rb.isKinematic = false;                                                     // Volta o Rigidbody para a física normal.
        rb.velocity = Vector3.zero;

        RotateTowards(player.position);

        Vector3 backward = -transform.forward * 5f + Vector3.up * 2f;               // Impulso para trás.
        rb.AddForce(backward, ForceMode.Impulse);

        targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180f, 0);    // Aplica uma rotação após se soltar.
        isRotatingAfterUnstuck = true;                                              // Ativa a rotação após se soltar.

        stateMachine.ChangeState(TriceratopsState.Idle);                            // Entra no State Machine de Idle.

        canMove = true;                                                             // Reativa os movimentos.
    }

    private void OnCollisionEnter(Collision collision)  // Método de colisões.
    {
        if (collision.gameObject.CompareTag("Ground"))      // Verificar se está no chão.
        {
            isOnGround = true;                              // Marca que está no chão.
        }

        if (isCharging && collision.gameObject.CompareTag("Tree"))     // Se está investindo e acertou uma árvore.
        {
            GetStuck();                                                // Chama o método de aprisionar.

            if (stuckVFX != null)
            {
                GameObject vfxInstance = Instantiate(stuckVFX, vfxSpawnStuck != null ? vfxSpawnStuck.position : transform.position, Quaternion.identity);
                vfxInstance.transform.SetParent(transform);
                Debug.Log("Stuck VFX");
            }

            return;
        }

        if (collision.gameObject.CompareTag("Player"))          // Se colidiu com o jogador.
        {
            if (!hasChargedDamage)                              // Se ainda não causou dano da investida.
            {
                hasChargedDamage = true;                        // Marca que causou dano.

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

    private void StopKinematic()                        // Método para garantir que o Rigidbody não continue com velocidade ao sair do modo Kinematic.
    {
        if (!rb.isKinematic)                            // Se o Rigidbody NÃO estiver no modo kinematic.
        {
            rb.velocity = Vector3.zero;                 // Zera a velocidade atual para evitar deslizamentos indesejados.
        }
    }

    private void PlayVFX(GameObject vfxObject, Vector3 position, Quaternion rotation, float duration)
    {
        if (vfxObject == null) return;

        vfxObject.transform.position = position;
        vfxObject.transform.rotation = rotation;
        vfxObject.SetActive(true);

        StartCoroutine(DisableVFXAfterTime(vfxObject, duration));
    }

    private IEnumerator DisableVFXAfterTime(GameObject vfxObject, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (vfxObject != null)
        {
            vfxObject.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()                 // Método para mostrar os raios de alcance no Editor para facilitar ajustes.
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
