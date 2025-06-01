using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriceratopsBoss : MonoBehaviour, IBoss
{
    private bool canMove = true;                    // Vari�vel privada para controlar se Boss pode se mover.

    public void SetCanMove(bool value)              // M�todo que permite definir se o Trice pode se mover ou n�o.
    {
        canMove = value;                            // Atualiza a vari�vel de controle de movimento com o valor recebido
        Debug.Log("Trice can move: " + canMove);

        if (!canMove)                               // Cancela a��es atuais.
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
    public Transform player;                             // Refer�ncia ao Transform do jogador.

    [Header("Movement Settings")]
    public float chaseSpeed = 5f;                       // Velocidade ao perseguir o jogador.
    public float detectionRange = 10f;                  // Detec��o para come�ar a perseguir.

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;                    // Refer�ncia ao Transform do pontos de patrulha.
    public float patrolSpeed = 3f;                      // Velocidade da patrulha.
    private int currentPatrolIndex = 0;                 // Atual ponto de patrulha.
    public float waitTimeAtPoint = 2f;                  // Tempo de espera no ponto de patrulha.
    private float waitTimer;                            // Temporizador para o tempo de patrulha.

    [Header("Charge Attack")]
    public float chargeSpeed = 20f;                     // Velocidade durante a investida.
    public float chargeRange = 10f;                     // Dist�ncia para come�ar a investir.
    public float maxChargeDistance = 15f;               // Dist�ncia m�xima que a investida pode percorrer.
    public float chargePrepareTime = 1.5f;              // Tempo de prepara��o da investida.
    public float chargeCooldown = 3f;                   // Tempo de recarga da investida.
    private float nextChargeTime = 0f;                  // Marca o tempo para usar a pr�xima investida.

    [Header("Collision Settings")]
    public float stuckTime = 3f;                        // Tempo que o Triceratops fica preso ap�s bater em uma �rvore.
    public float pushForce = 1500f;                     // For�a horizontal ao empurrar o jogador.
    public float pushUpForce = 600f;                    // For�a vertical ao empurrar o jogador.
    public float rotationSpeed = 2f;                    // Velocidade de rota��o ao sair do aprisionamento.

    [Header("Tail Attack")]
    public float tailAttackRange = 5f;                  // Dist�ncia m�nima para atacar com a cauda.
    public float tailAttackCooldown = 5f;               // Tempo de recarga do golpe de cauda.
    public float tailAttackDuration = 2f;               // Dura��o do efeito do golpe de cauda.

    [Header("Earthquake Attack")]
    public float earthquakeRadius = 12f;                // Raio de alcance do ataque terremoto.
    public float earthquakeCooldown = 8f;               // Tempo de recarga do terremoto.
    public float earthquakeDuration = 1.5f;             // Dura��o do efeito do terremoto.
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
    public Transform tailPosition;                      // Posi��o da cauda para detectar colis�es traseiras.
    public Transform headPosition;                      // Posi��o da cabe�a para detectar colis�es frontais.
    public LayerMask obstacleMask;                      // Layer que detecta obst�culos.

    private Vector3 chargeStartPosition;                // Posi��o inicial da investida.
    private Vector3 chargeDirection;                    // Dire��o da investida.
    private Vector3 chargeTarget;                       // Posi��o alvo da investida.

    private Rigidbody rb;                               // Rigidbody do Triceratops.
    private TriceratopsStateMachine stateMachine;       // Refer�ncia ao script de State Machine.

    private bool isPatrolling = true;                   // Verifica��o se est� em patrulha.

    private bool isStuck = false;                       // Est� preso?
    private float stuckTimer;                           // Contador de tempo preso.
    private bool isRotatingAfterUnstuck = false;        // Verifica��o se rotacionou ap�s sair do aprisionamento.
    private Quaternion targetRotation;                  // Posi��o para rotacionar.

    private bool isCharging = false;                    // Vertifica se est� em investida.
    private bool isPreparingCharge = false;             // Vertifica se est� se preparando para investir.
    private bool hasChargedDamage = false;              // Verifica se o dano j� foi aplicado.
    private GameObject chargeVFXInstance;

    private bool isTailAttacking = false;               // Vertifica se est� usando o ataque de cauda.
    private float nextTailAttackTime = 0f;              // Pr�ximo tempo que poder� usar golpe de cauda.

    private bool isEarthquaking = false;                // Vertifica se est� usando terremoto.
    private float nextEarthquakeTime = 0f;              // Pr�ximo tempo que poder� usar terremoto.
    private bool earthquakeImpactDone = false;          // Verifica se o impacto j� aconteceu.
    private bool isOnGround = true;                     // Verifica se est� no ch�o.

    void Awake()
    {
        rb = GetComponent<Rigidbody>();                             // Pega o Rigidbody no in�cio do jogo.
        stateMachine = GetComponent<TriceratopsStateMachine>();     // Pega os States Machines no in�cio do jogo. 
    }

    void Update()
    {
        if (player == null)                             // Se n�o tem jogador, entrar em State Machine de "Patrulha".
        {
            Patrol();
        }

        if (!canMove)                                   // Verifica se o movimento est� desabilitado.
        {
            StopKinematic();
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

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);             // Calcula dist�ncia at� o jogador.

        if (distanceToPlayer <= detectionRange && !isCharging && !isPreparingCharge && !isStuck)    // Se est� perto o suficiente, n�o est� investindo ou preso, persegue o jogador.
        {
            isPatrolling = false;
            ChasePlayer();                              // Chama o m�todo de perseguir o jogador.
        }

        if (distanceToPlayer <= detectionRange && !isCharging && !isPreparingCharge)          // Se o jogador est� perto e n�o estiver usando investida, olhar em dire��o ao jogador.
        {
            RotateTowards(player.position);             // Chama o m�todo de rotacionar para o jogador.
        }

        if (distanceToPlayer <= chargeRange && Time.time >= nextChargeTime)         // Se o jogador est� perto, iniciar a investida.
        {
            isPatrolling = false;
            PrepareCharge();                            // Chama o m�todo para preparar a investida.
            return;
        }

        if (Time.time >= nextEarthquakeTime && distanceToPlayer <= 7f && !isPreparingCharge && !isCharging && !isTailAttacking)     // Se o jogador est� perto, e o terremoto est� dispon�vel.
        {
            isPatrolling = false;
            Earthquake();                               // Chama o m�todo de usar o terremoto.
        }

        if (distanceToPlayer <= tailAttackRange && PlayerIsBehind() && Time.time >= nextTailAttackTime && !isStuck)         // Se o jogador est� atr�s, dentro do alcance e o ataque de cauda est� liberado.
        {
            isPatrolling = false;
            TailAttack();                           // Chama o m�todo de usar o ataque de cauda.
            return;
        }

        isPatrolling = true;                        // Ativa o State Machine de patrulha.

        if (isPatrolling && distanceToPlayer > detectionRange)      // Se estiver em patrulha ativado e a dist�ncia para o jogador � maior do que a dist�ncia de detec��o.
        {
            Patrol();                               // Chama o m�todo para patrulhar.
        }

        if (isRotatingAfterUnstuck)                 // Verifica se o Triceratops est� rotacionando ap�s se soltar.
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);      // Suaviza a rota��o atual em dire��o � rota��o alvo usando Slerp.

            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)      // Verifica se o �ngulo entre a rota��o atual e a rota��o alvo � menor que 1 grau.
            {
                transform.rotation = targetRotation;                            // Define a rota��o diretamente como a rota��o alvo.
                isRotatingAfterUnstuck = false;                                 // Desativa a rota��o ap�s aprisionamento, indicando que a rota��o foi conclu�da.
            }
        }
    }

    private void FixedUpdate()
    {
        if (isCharging)
        {
            if (Physics.Raycast(headPosition.position, chargeDirection, out RaycastHit hit, 2f, obstacleMask))          // Raycast para detectar colis�o frontal com obst�culo.
            {
                if (hit.collider.CompareTag("Tree"))                    // Se acertar uma �rvore, fica preso.
                {
                    GetStuck();
                    return;
                }
            }

            rb.MovePosition(rb.position + chargeDirection * chargeSpeed * Time.deltaTime);

            float chargeDistance = Vector3.Distance(transform.position, chargeTarget);
            if (chargeDistance >= maxChargeDistance)                                                // Verifica se j� andou a dist�ncia m�xima.
            {
                EndCharge();                                    // Chama o m�todo para finalizar a investida.
            }
        }
    }

    bool PlayerIsBehind() => Vector3.Angle(transform.forward, (player.position - transform.position).normalized) > 120f;        // M�todo para verificar se o jogador est� na parte de tr�s.

    public void Idle()                                  // M�todo para deixar o Triceratops im�vel em State Machine de Idle.
    {
        rb.velocity = Vector3.zero;
    }

    public void RotateTowards(Vector3 target)           // M�todo para rotacionar em dire��o ao jogador.
    {
        Vector3 lookDirection = (target - transform.position).normalized;
        lookDirection.y = 0;                                                            // Ignora rota��o no eixo Y (para n�o virar para cima ou para baixo).
        if (lookDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            float rotationSpeed = 5f;
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);       // Rota��o suave.
        }
    }

    public void Patrol()                                // M�todo para entrar em State Machine de patrulha.
    {
        if (patrolPoints.Length == 0) return;                                               // Se n�o h� pontos de patrulha, encerra o m�todo.

        stateMachine.ChangeState(TriceratopsState.Patrol);                                  // Entra no State Machine de patrulha.

        Transform targetPoint = patrolPoints[currentPatrolIndex];                           // Define o ponto de patrulha atual como alvo.
        Vector3 direction = (targetPoint.position - transform.position).normalized;         // Calcula a dire��o normalizada at� o ponto de patrulha.

        RotateTowards(targetPoint.position);                                                // Rotaciona o Triceratops em dire��o ao ponto de patrulha.

        rb.MovePosition(transform.position + direction * patrolSpeed * Time.deltaTime);     // Move o Triceratops em dire��o ao ponto de patrulha.

        float distance = Vector3.Distance(transform.position, targetPoint.position);        // Calcula a dist�ncia at� o ponto de patrulha.

        if (distance < 1f)                                                                  // Se est� pr�ximo do ponto de patrulha.
        {
            if (waitTimer <= 0)                                                             // Se o tempo de espera terminou.
            {
                int newPatrolIndex = currentPatrolIndex;                                    // Inicializa o novo �ndice igual ao atual.
                while (newPatrolIndex == currentPatrolIndex && patrolPoints.Length > 1)     // Garante que o novo �ndice seja diferente do atual.
                {
                    newPatrolIndex = Random.Range(0, patrolPoints.Length);                  // Escolhe um novo ponto aleat�rio.
                }
                currentPatrolIndex = newPatrolIndex;                                        // Atualiza o �ndice de patrulha.

                waitTimer = waitTimeAtPoint;                                                // Reseta o temporizador de espera.
            }
            else
            {
                waitTimer -= Time.deltaTime;                                                // Reduz o temporizador com o tempo.
                rb.velocity = Vector3.zero;                                                 // Para o movimento enquanto espera.
            }
        }
    }

    public void ChasePlayer()                           // M�todo para perseguir o player.
    {
        stateMachine.ChangeState(TriceratopsState.Chase);                                   // Entra no State Machine de Perseguir.

        Vector3 direction = (player.position - transform.position).normalized;              // Calcula a dire��o normalizada at� o jogador.
        rb.MovePosition(transform.position + direction * chaseSpeed * Time.deltaTime);      // Move o Triceratops na dire��o do jogador.

        RotateTowards(player.position);                                                     // Rotaciona o Triceratops para olhar na dire��o do jogador.
    }

    public void PrepareCharge()                         // M�todo para iniciar o carregamento da investida.
    {

        if (isPreparingCharge || Time.time < nextChargeTime) return;            // Se j� est� preparando a investida ou ainda n�o passou o tempo de recarga, sai do m�todo.

        SetCanMove(false);                                                      // Desativa a movimenta��o do Triceratops.
        isPreparingCharge = true;                                               // Ativa que o Triceratops est� em prepara��o da investida.
        rb.velocity = Vector3.zero;                                             // Para o movimento atual, zerando a velocidade.

        stateMachine.ChangeState(TriceratopsState.PrepareCharge);               // Entra do State Machine de Prepara��o de Investida.

        chargeTarget = player.position;                                         // Guarda a posi��o atual do jogador como alvo da investida.
        chargeDirection = (chargeTarget - transform.position).normalized;       // Calcula a dire��o da investida, do Triceratops at� o jogador.

        Debug.Log("Preparando investida...");

        if (prepareVFX != null)
        {
            GameObject vfxInstance = Instantiate(prepareVFX, vfxSpawnPrepare != null ? vfxSpawnPrepare.position : transform.position, Quaternion.identity);
            Destroy(vfxInstance, 5f);
        }

        Invoke(nameof(Charge), chargePrepareTime);                              // Agenda a chamada do m�todo Charge ap�s um tempo de prepara��o.
    }

    public void Charge()                                // M�todo para realizar a investida.
    {
        if (chargeVFX != null)
        {
            chargeVFXInstance = Instantiate(chargeVFX, vfxSpawnCharge != null ? vfxSpawnCharge.position : transform.position, Quaternion.identity);
            chargeVFXInstance.transform.SetParent(transform);       // Faz o VFX seguir o Triceratops
        }

        Debug.Log("Inciando investida!");
        isPreparingCharge = false;                                  // Desativa a prepara��o de investida.
        isCharging = true;                                          // Ativa a investida.

        stateMachine.ChangeState(TriceratopsState.Charge);          // Entra no State Machine de Investida.        
    }

    public void EndCharge()                             // M�todo para finalizar a investida.
    {
        isCharging = false;                                 // Desativa a investida.
        hasChargedDamage = false;                           // Reseta para permitir dano na pr�xima investida.
        stateMachine.ChangeState(TriceratopsState.Idle);    // Entra no State Machine de Idle.
        Debug.Log("Trice terminou a investida");
        RotateTowards(player.position);                     // Rotaciona para o jogador.

        SetCanMove(true);                                   // Reativa os movimentos do Triceratops.

        nextChargeTime = Time.time + chargeCooldown;        // Define o cooldown para a pr�xima investida.

        if (chargeVFXInstance != null)
        {
            Destroy(chargeVFXInstance);
            chargeVFXInstance = null;
        }
    }

    public void TailAttack()                            // M�todo para iniciar o golpe da cauda.
    {
        if (isTailAttacking) return;                                                                // Se j� estiver atacando, n�o iniciar outro ataque.

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

        Collider[] hitColliders = Physics.OverlapSphere(tailPosition.position, tailAttackRange, playerMask);       // Detecta o jogador atr�s e dentro da �rea de ataque da cauda.
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
                    Vector3 pushDirection = (hit.transform.position - transform.position).normalized;       // Calcula a dire��o para empurrar o jogador.
                    playerRB.AddForce(pushDirection * pushForce + Vector3.up * pushUpForce);                // Aplica for�a empurrando o jogador.
                    Debug.Log("Player empurrado pelo golpe de cauda!");
                }
            }
        }

        Invoke(nameof(EndTailAttack), tailAttackDuration);                                          // Chama o m�todo para encerrar o Golpe de Cauda.
    }

    public void EndTailAttack()                         // M�todo para terminar o golpe de cauda.
    {
        isTailAttacking = false;                                // Desativa o golpe de cauda.
        Debug.Log("Triceratops terminou o golpe de cauda!");

        stateMachine.ChangeState(TriceratopsState.Idle);        // Entra no State Machine de Idle.

        SetCanMove(true);                                       // Reativa os movimentos.
    }

    public void Earthquake()                            // M�todo para iniciar o terremoto.
    {

        if (isEarthquaking || !isOnGround || isStuck) return;           // Se j� est� em terremoto, n�o est� no ch�o ou preso, ignora.

        SetCanMove(false);                                              // Desativa os movimentos.
        isEarthquaking = true;                                          // Ativa o terremoto.
        earthquakeImpactDone = false;                                   // Deixa desativado o impacto do terremoto.
        Debug.Log("Triceratops usou TERREMOTO!");
        nextEarthquakeTime = Time.time + earthquakeCooldown;            // Ativa cooldown do terremoto.

        rb.velocity = Vector3.zero;                                     // Para o movimento.
        stateMachine.ChangeState(TriceratopsState.Earthquake);          // Entra no State Machine de Terremoto.

        Invoke(nameof(ImpactEarthquake), 0.7f);                         // Chama o m�todo de Impacto do Terremoto.
    }

    public void ImpactEarthquake()                      // M�todo para realizar o impacto do terremoto.
    {
        Debug.Log("Triceratops causou o impacto do terremoto!");
        earthquakeImpactDone = true;                              // Marca que o impacto do terremoto foi realizado.

        Invoke(nameof(EndEarthquake), earthquakeDuration);        // Chama o m�todo para encerrar o terremoto.
    }

    public void EndEarthquake()                         // M�todo para terminar o Terremoto.
    {
        SoundManager.Instance.PlaySound3D("TriceEarthquake", transform.position);                       // Chama o efeito sonoro de terremoto.
        if (earthquakeImpactDone && isOnGround)             // Se o impacto foi feito e o Triceratops est� no ch�o.
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, earthquakeRadius, playerMask);      // Detecta todos jogadores atingidos pelo terremoto
            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("Player"))                                                           // Verifica se atingiu o jogador.
                {
                    TriceratopsDamageDealer damageDealer = GetComponent<TriceratopsDamageDealer>();     // Pega o componente que lida com dano do Triceratops.
                    damageDealer.DealEarthquakeDamage(transform.position);                              // Aplica o dano de terremoto ao jogador.

                    Debug.Log("Jogador atingido");

                    Rigidbody hitRB = hit.GetComponent<Rigidbody>();                                    // Pega o Rigidbody do jogador para aplicar for�a.
                    if (hitRB != null)
                    {
                        Vector3 pushDir = (hit.transform.position - transform.position).normalized;     // Dire��o para empurrar o jogador.
                        hitRB.AddForce(pushDir * pushForce + Vector3.up * pushUpForce);                 // Empurra o jogador.
                    }
                }
            }
        }

        isEarthquaking = false;                                        // Marca que o terremoto acabou.

        stateMachine.ChangeState(TriceratopsState.Idle);               // Entra no State Machine de Idle.
        isOnGround = true;                                             // Marca que est� no ch�o.

        SetCanMove(true);                                              // Reativa os movimentos.

        PlayVFX(earthquakeVFX, vfxSpawnEarthquake.position, vfxSpawnEarthquake.rotation, 3f);
    }

    public void GetStuck()                              // M�todo para aprisonar o Triceratops.
    {
        Debug.Log("Triceratops esta preso na arvore!");
        isStuck = true;                                         // Ativa como preso.
        SoundManager.Instance.PlaySound3D("TriceStuck", transform.position);
        isCharging = false;                                     // Desativa a investida.
        isPreparingCharge = false;                              // Desativa a prepara��o da investida.
        stuckTimer = stuckTime;                                 // Inicia o contador para se soltar.
        StopKinematic();
        rb.isKinematic = true;                                  // Deixa o Rigidbody sem f�sica para n�o ficar deslizando.
        stateMachine.ChangeState(TriceratopsState.Stuck);       // Entra no State Machine de Aprisionado.
    }

    public void HandleStuck()                           // M�todo para lidar com o aprisionar.
    {
        Debug.Log("Contando tempo preso: " + stuckTimer.ToString("F2"));
        stuckTimer -= Time.deltaTime;               // Contagem regressiva do tempo preso.
        if (stuckTimer <= 0)
        {
            Unstuck();                              // Se tempo acabar, chama o m�todo para soltar o Triceratops.
        }
    }

    public void Unstuck()                               // M�todo para se soltar ap�s aprisionado.
    {
        Debug.Log("Triceratops se soltou!");
        isStuck = false;                                                            // Destiva o aprisionar.
        rb.isKinematic = false;                                                     // Volta o Rigidbody para a f�sica normal.
        rb.velocity = Vector3.zero;

        RotateTowards(player.position);

        Vector3 backward = -transform.forward * 5f + Vector3.up * 2f;               // Impulso para tr�s.
        rb.AddForce(backward, ForceMode.Impulse);

        targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180f, 0);    // Aplica uma rota��o ap�s se soltar.
        isRotatingAfterUnstuck = true;                                              // Ativa a rota��o ap�s se soltar.

        stateMachine.ChangeState(TriceratopsState.Idle);                            // Entra no State Machine de Idle.

        canMove = true;                                                             // Reativa os movimentos.
    }

    private void OnCollisionEnter(Collision collision)  // M�todo de colis�es.
    {
        if (collision.gameObject.CompareTag("Ground"))      // Verificar se est� no ch�o.
        {
            isOnGround = true;                              // Marca que est� no ch�o.
        }

        if (isCharging && collision.gameObject.CompareTag("Tree"))     // Se est� investindo e acertou uma �rvore.
        {
            GetStuck();                                                // Chama o m�todo de aprisionar.

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
            if (!hasChargedDamage)                              // Se ainda n�o causou dano da investida.
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

    private void StopKinematic()                        // M�todo para garantir que o Rigidbody n�o continue com velocidade ao sair do modo Kinematic.
    {
        if (!rb.isKinematic)                            // Se o Rigidbody N�O estiver no modo kinematic.
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

    private void OnDrawGizmosSelected()                 // M�todo para mostrar os raios de alcance no Editor para facilitar ajustes.
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
