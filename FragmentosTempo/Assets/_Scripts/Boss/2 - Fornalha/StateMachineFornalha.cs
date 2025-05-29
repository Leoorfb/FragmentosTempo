using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossStateMachine : MonoBehaviour
{
    public Rigidbody rb;
    public enum State
    {
        Patrol,
        Attack,
        JumpAttack,
        Fireball,
        LavaPond,
    }

    private State currentState = State.Patrol;
    private State lastAttackState = State.Patrol;
    private int repeatCount = 0;

    private List<State> attackStates = new List<State>
    {
        State.JumpAttack,
        State.Fireball,
        State.LavaPond,
        
    };

    private Coroutine stateRoutine;

    void Start()
    {
        ChangeState(State.Patrol);
    }

    void ChangeState(State newState)
    {
        if (stateRoutine != null)
            StopCoroutine(stateRoutine);

        currentState = newState;
        stateRoutine = StartCoroutine(RunState(newState));
    }

    IEnumerator RunState(State state)
    {
        Debug.Log("Entrou no estado: " + state);

        // Atualiza contadores para ataque
        if (IsAttackState(state))
        {
            if (state == lastAttackState)
                repeatCount++;
            else
            {
                repeatCount = 1;
                lastAttackState = state;
            }
        }
        /*      ANTIGO 
        // Simulação de duração dos estados
        float duration = GetStateDuration(state);
        yield return new WaitForSeconds(duration);

        // Decidir próximo estado
        if (state == State.Patrol)
        {
            ChangeState(State.Attack);
        }
        else if (state == State.Attack)
        {
            ChangeState(GetNextAttackState());
        }
        else
        {
            ChangeState(State.Attack);
        }
        */
        switch (state)
        {
            
            case State.Fireball:
                yield return StartCoroutine(FireballRoutine());
                break;
            
            case State.LavaPond:
                yield return StartCoroutine(LavaPondRoutine());
                break;
            case State.JumpAttack:
                yield return StartCoroutine(JumpAttackRoutine());
                break;
            // Criar case Para todas os outros estados
            //No momento so RotateFireball
            default:
                yield return new WaitForSeconds(GetStateDuration(state));
                break;
        }

        // Transição
        if (state == State.Patrol)
            ChangeState(State.Attack);
        else if (state == State.Attack)
            ChangeState(GetNextAttackState());
        else
            ChangeState(State.Attack);
    }

    State GetNextAttackState()
    {
        List<State> possibleStates = new List<State>(attackStates);
        if (repeatCount >= 2)
            possibleStates.Remove(lastAttackState);

        return possibleStates[Random.Range(0, possibleStates.Count)];
    }

    bool IsAttackState(State state) => attackStates.Contains(state);

    float GetStateDuration(State state)
    {
        switch (state)
        {
            case State.Patrol: return 2f;
            case State.Attack: return 0.5f;
            case State.JumpAttack: return 3f;
            case State.Fireball: return 4f;
            case State.LavaPond: return 5f;
            default: return 1f;
        }
    }


    /* ----------- Essa parte e do Fireball ---------------- */

    [SerializeField] private Transform target; // O jogador
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float rotationDuration = 2f;
    [SerializeField] private float rotationSpeed = 90f; // graus por segundo
    [SerializeField] private Transform spawnPoint;       // Ponto de onde o projétil será disparado


    IEnumerator FireballRoutine()
    {
        float elapsed = 0f;
        int shotsFired = 0;
        float shotInterval = rotationDuration / 3f;

        while (elapsed < rotationDuration)
        {
            // Chegou o momento de disparar?
            if (shotsFired < 3 && elapsed >= shotInterval * shotsFired)
            {
                // Pausa o giro contínuo e mira no jogador
                yield return StartCoroutine(RotateToFaceTargetY(target));

                //  Dispara na direção atual (que agora está mirando no player)
                Fire();

                shotsFired++;
            }

            // Continua a girar horizontalmente
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }



    IEnumerator RotateToFaceTargetY(Transform target)
    {
        float angleThreshold = 1f; // Quanto de precisão aceitável
        float rotationSpeedY = 180f; // velocidade para mirar

        while (true)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f; // só considera horizontal

            if (direction.sqrMagnitude < 0.001f)
                break;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion currentRotation = transform.rotation;

            transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, rotationSpeedY * Time.deltaTime);

            float angle = Quaternion.Angle(currentRotation, targetRotation);
            if (angle < angleThreshold)
                break;

            yield return null;
        }
    }





    void Fire()
    {
        GameObject projectile = Instantiate(fireballPrefab, spawnPoint.position, Quaternion.identity);
        Fireball arc = projectile.GetComponent<Fireball>();
        arc.target = target.GetComponent<Transform>();
    }




    /* ---------------- FIM do RotateFireball ------------- */

    /*----------------------Inicio COne ---------------------*/

    [Header("Cone Settings")]
    public GameObject conePrefab; // Prefab do cone a instanciar
    public float coneAngle = 45f;
    public float coneRange = 5f;

    [Header("Rotation Settings")]
    public float pauseBetweenRotations = 1f;
    public float rotationSpeedDegreesPerSecond = 180f;
    IEnumerator LavaPondRoutine()
    {
        for (int i = 0; i < 4; i++)
        {
            Quaternion startRot = transform.rotation;
            Quaternion targetRot = Quaternion.Euler(0f, transform.eulerAngles.y + 90f, 0f);

            // Rota suavemente
            while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRot,
                    rotationSpeedDegreesPerSecond * Time.deltaTime);

                yield return null; // espera o próximo frame
            }

            // Garante rotação exata
            transform.rotation = targetRot;

            // Instancia cone
            SpawnCone();

            // Espera
            yield return new WaitForSeconds(pauseBetweenRotations);
        }
    }

    void SpawnCone()
    {
        if (conePrefab == null) return;

        GameObject cone = Instantiate(conePrefab, transform.position, transform.rotation);
        Destroy(cone, pauseBetweenRotations); // Dura o tempo da pausa
    }

    /*------------------------------Fim do Cone-------------------------------*/

    /*------------------------------Inicio Pulo-----------------------------*/

    [Header("Jump Config")]
    public float jumpForce = 10f;
    public float airTime = 1.5f;
    public float trackingTime = 0.7f;
    public float trackingSpeed = 5f;
    [Header("References")]
    public GameObject blobShadowPrefab;
    public GameObject impactHexPrefab;
    public LayerMask groundMask;

    [Header("Impact Config")]
    public float pushForce = 5f;
    public float impactRadius = 5f;
    public int impactDamage = 30;
    public float aoeDuration = 3f;
    public int dps = 5;

    private GameObject blobShadow;
    private bool isJumping;
    IEnumerator JumpAttackRoutine()
    {
            isJumping = true;
            rb.useGravity = true;
            rb.velocity = Vector3.up * jumpForce;

            GetComponent<Collider>().enabled = false;

            // Corrige rotação
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            rb.angularVelocity = Vector3.zero;

            // Instancia sombra
            blobShadow = Instantiate(blobShadowPrefab, transform.position, Quaternion.identity);
            blobShadow.transform.localScale = Vector3.zero;

            float t = 0f;

            // Acompanha o player
            while (t < trackingTime)
            {
                if (target && blobShadow)
                {
                    Vector3 targetPos = new Vector3(target.position.x, 0, target.position.z);
                    blobShadow.transform.position = Vector3.Lerp(blobShadow.transform.position, targetPos, Time.deltaTime * trackingSpeed);
                    blobShadow.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / trackingTime);
                }

                t += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(airTime - trackingTime);

            // Teleporta para posição de queda
            Vector3 landPos = blobShadow.transform.position + Vector3.up * 15f;
            transform.position = landPos;
            rb.velocity = Vector3.zero;
            rb.useGravity = true;
            GetComponent<Collider>().enabled = true;

            // Espera cair no chão
            while (!Physics.Raycast(transform.position, Vector3.down, 1f, groundMask))
            {
                
                yield return null;
            }

            LandingTrigger trigger = GetComponentInChildren<LandingTrigger>();
            if (trigger != null)
            {
                Collider col = trigger.GetComponent<Collider>();
                if (col != null)
                {
                StartCoroutine(EnableTriggerTemporarily(col, 3f)); // só ativa por 0.2s
                }
            }

        Destroy(blobShadow, 0.3f);
            isJumping = false;

            // Instancia o círculo de dano ao cair
            if (impactHexPrefab != null)
            {
                Instantiate(impactHexPrefab, transform.position, Quaternion.identity);
            }
            



        
    }

    IEnumerator EnableTriggerTemporarily(Collider col, float duration)
    {
        col.enabled = true;
        yield return new WaitForSeconds(duration);
        col.enabled = false;
    }



    /*------------------------------Fim do Pulo----------------------------*/
}