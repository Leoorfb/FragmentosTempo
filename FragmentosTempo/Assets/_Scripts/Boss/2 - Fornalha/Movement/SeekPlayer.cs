using UnityEngine;

public class FollowAndRotate : MonoBehaviour
{
    public Transform target;           // Objeto a ser seguido
    public float rotationSpeed = 5f;   // Velocidade de rotação
    public float moveSpeed = 3f;       // Velocidade de movimento

    void Update()
    {
        if (target == null) return;

        SeekRotate();

        /*
        //Código temporario pra teste
        if (Vector3.Distance(transform.position, target.position) > 3f)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        */
    }


    void SeekRotate()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // Ignora diferenças de altura (bloqueia rotação no X/Z)

        if (direction.sqrMagnitude < 0.001f) return; // Evita problemas com zero

        // Rotação desejada apenas no eixo Y
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Rotação suave apenas no Y
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);

    }
}
