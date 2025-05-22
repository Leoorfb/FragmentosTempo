using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossStateMachine : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Attack,
        JumpAttack,
        Fireball,
        LavaPond,
        ScaldingSmoke
    }

    private State currentState = State.Patrol;
    private State lastAttackState = State.Patrol;
    private int repeatCount = 0;

    private List<State> attackStates = new List<State>
    {
        State.JumpAttack,
        State.Fireball,
        State.LavaPond,
        /*
        State.ScaldingSmoke
        */
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
            case State.ScaldingSmoke: return 3.5f;
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

    [Header("Cone Settings")]
    public GameObject conePrefab; // Prefab do cone a instanciar
    public float coneAngle = 45f;
    public float coneRange = 5f;

    [Header("Rotation Settings")]
    public float pauseBetweenRotations = 1f;
    IEnumerator LavaPondRoutine()
    {
        for (int i = 0; i < 4; i++)
        {
            // Rotaciona 90° no eixo Y
            transform.Rotate(Vector3.up, 90f);

            // Instancia cone na nova direção
            SpawnCone();

            // Espera
            yield return new WaitForSeconds(pauseBetweenRotations);
        }

        void SpawnCone()
        {
            if (conePrefab == null) return;

            GameObject cone = Instantiate(conePrefab, transform.position, transform.rotation);

            // Escala baseada no alcance e ângulo (opcional, depende do modelo)
            // Por exemplo, se o prefab é um cone padrão apontado no eixo Z:
            float scaleFactor = coneRange;
            cone.transform.localScale = new Vector3(
                Mathf.Tan(coneAngle * 0.5f * Mathf.Deg2Rad) * coneRange * 2f, // largura
                1f,
                scaleFactor); // profundidade

            Destroy(cone, pauseBetweenRotations); // remove o cone após x segundos (ou o tempo desejado)
            
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 forward = transform.forward;
        Quaternion leftRayRotation = Quaternion.Euler(0, -coneAngle / 2f, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, coneAngle / 2f, 0);

        Vector3 leftRay = leftRayRotation * forward * coneRange;
        Vector3 rightRay = rightRayRotation * forward * coneRange;

        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);
        Gizmos.DrawLine(transform.position + leftRay, transform.position + rightRay);
    }


}