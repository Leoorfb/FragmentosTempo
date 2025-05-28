using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriceratopsAttack : MonoBehaviour
{
    private bool canAttack = true;

    public void SetCanAttack(bool value)
    {
        canAttack = value;
        Debug.Log("Trice can attack: " + canAttack);

        if (!canAttack)
        {
            isCharging =false;
            isPreparingCharge = false;
            isTailAttacking = false;
            isEarthquaking = false;
            earthquakeImpactDone = false;
        }
    }

    private TriceratopsBoss boss;

    [Header("Player")]
    public Transform player;

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


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stateMachine = GetComponent<TriceratopsStateMachine>();
        boss = GetComponent<TriceratopsBoss>();
    }

    void Update()
    {
        if (player == null) return;

        if (!canAttack)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        if (isEarthquaking || isTailAttacking)
        {
            return;
        }

        if (isStuck)
        {
            HandleStuck();
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= chargeRange && Time.time >= nextChargeTime)
        {
            PrepareCharge();
            return;
        }

        if (Time.time >= nextEarthquakeTime && distanceToPlayer <= 7f && !isPreparingCharge && !isCharging && !isTailAttacking)
        {
            Earthquake();
        }

        if (distanceToPlayer <= tailAttackRange && PlayerBehind() && Time.time >= nextTailAttackTime && !isStuck)
        {
            TailAttack();
            return;
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

    bool PlayerBehind() => Vector3.Angle(transform.forward, (player.position - transform.position).normalized) > 120f;

    public void PrepareCharge()
    {
        if (isPreparingCharge || Time.time < nextChargeTime) return;

        isPreparingCharge = true;
        rb.velocity = Vector3.zero;

        stateMachine.ChangeState(TriceratopsState.PrepareCharge);

        chargeTarget = player.position;
        chargeDirection = (chargeTarget - transform.position).normalized;

        Debug.Log("Preparando investida...");

        Invoke(nameof(Charge), chargePrepareTime);
    }

    public void Charge()
    {
        Debug.Log("Iniciando investida!");
        isPreparingCharge = false;
        isCharging = true;

        stateMachine.ChangeState(TriceratopsState.Charge);

        if (Physics.Raycast(headPosition.position, chargeDirection, out RaycastHit hit, 2f, obstacleMask))
        {
            if (hit.collider.CompareTag("Tree"))
            {
                GetStuck();
                stateMachine.ChangeState(TriceratopsState.Stuck);
                return;
            }
        }

        rb.MovePosition(transform.position + chargeDirection * chargeSpeed * Time.deltaTime);

        float chargeDistance = Vector3.Distance(transform.position, chargeTarget);
        if (chargeDistance >= maxChargeDistance)
        {
            EndCharge();
        }

        nextChargeTime = Time.time + chargeCooldown;
    }

    public void EndCharge()
    {
        isCharging = false;
        hasChargedDamage = false;
        stateMachine.ChangeState(TriceratopsState.Idle);
        Debug.Log("Trice terminou a investida");
        boss.RotateTowards(player.position);
    }

    public void TailAttack()
    {
        if (isTailAttacking || isStuck) return;

        Debug.Log("Triceratops deu um golpe de cauda!");
        isTailAttacking = true;
        nextTailAttackTime = Time.time + tailAttackCooldown;

        rb.velocity = Vector3.zero;

        stateMachine.ChangeState(TriceratopsState.TailAttack);

        Collider[] hitColliders = Physics.OverlapSphere(tailPosition.position, tailAttackRange, playerMask);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                TriceratopsDamageDealer damageDealer = GetComponent<TriceratopsDamageDealer>();
                if (damageDealer != null)
                {
                    damageDealer.DealTailDamage(hit.gameObject);
                }

                Rigidbody playerRB = hit.GetComponent<Rigidbody>();
                if (playerRB != null)
                {
                    Vector3 pushDirection = (hit.transform.position - transform.position).normalized;
                    playerRB.AddForce(pushDirection * pushForce + Vector3.up * pushUpForce);
                    Debug.Log("Player empurrado pelo golpe de cauda!");
                }
            }
        }

        Invoke(nameof(EndTailAttack), tailAttackDuration);
    }

    public void EndTailAttack()
    {
        isTailAttacking = false;
        Debug.Log("Triceratops terminou o golpe de cauda!");

        stateMachine.ChangeState(TriceratopsState.Idle);
    }

    public void Earthquake()
    {
        if (isEarthquaking || !isOnGround || isStuck) return;

        isEarthquaking = true;
        earthquakeImpactDone = false;
        Debug.Log("Triceratops usou TERREMOTO!");
        nextEarthquakeTime = Time.time + earthquakeCooldown;

        rb.velocity = Vector3.zero;
        stateMachine.ChangeState(TriceratopsState.Earthquake);

        Invoke(nameof(ImpactEarthquake), 0.7f);
    }

    public void ImpactEarthquake()
    {
        Debug.Log("Triceratops causou o impacto do terremoto!");
        earthquakeImpactDone = true;

        Invoke(nameof(EndEarthquake), earthquakeDuration);
    }

    public void EndEarthquake()
    {
        if (earthquakeImpactDone && isOnGround)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, earthquakeRadius, playerMask);
            foreach (var hit in hitColliders)
            {
                if (hit.CompareTag("Player"))
                {
                    TriceratopsDamageDealer damageDealer = GetComponent<TriceratopsDamageDealer>();
                    damageDealer.DealEarthquakeDamage(transform.position);

                    Debug.Log("Jogador atingido");

                    Rigidbody hitRB = hit.GetComponent<Rigidbody>();
                    if (hitRB != null)
                    {
                        Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                        hitRB.AddForce(pushDir * 500f + Vector3.up * 300f);
                    }
                }
            }
        }

        isEarthquaking = false;

        stateMachine.ChangeState(TriceratopsState.Idle);
        isOnGround = true;
    }

    public void GetStuck()
    {
        Debug.Log("Triceratops está preso na árvore!");
        isStuck = true;
        isCharging = false;
        isPreparingCharge = false;
        stuckTimer = stuckTime;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        stateMachine.ChangeState(TriceratopsState.Stuck);
    }

    public void HandleStuck()
    {
        Debug.Log("Contando tempo preso: " + stuckTimer.ToString("F2"));
        stuckTimer -= Time.deltaTime;
        if (stuckTimer <= 0)
        {
            Unstuck();
        }
    }

    public  void Unstuck()
    {
        Debug.Log("Triceratops se soltou!");
        isStuck = false;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;

        Vector3 backward = -transform.forward * 5f + Vector3.up * 2f;
        rb.AddForce(backward, ForceMode.Impulse);

        targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180f, 0);
        isRotatingAfterUnstuck = true;

        stateMachine.ChangeState(TriceratopsState.Idle);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
        }

        if (isCharging)
        {
            if (collision.gameObject.CompareTag("Tree"))
            {
                if (isCharging)
                {
                    GetStuck();
                }
            }
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (!hasChargedDamage)
            {
                hasChargedDamage = true;

                Rigidbody playerRB = collision.gameObject.GetComponent<Rigidbody>();
                if (playerRB != null)
                {
                    Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
                    playerRB.AddForce(pushDirection * pushForce + Vector3.up * pushUpForce);
                    Debug.Log("Player empurrado!");

                    var damageDealer = GetComponent<TriceratopsDamageDealer>();
                    if (damageDealer != null)
                    {
                        damageDealer.DealChargeDamage(collision.gameObject);
                        Debug.Log("Dano da investida aplicado!");
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
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
