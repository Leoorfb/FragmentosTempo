using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Fireball : MonoBehaviour
{
    public Transform target;
    public float trajectoryHeight = 5f; // Altura máxima do arco
    public int damageAmount = 25;
    private Rigidbody rb;

    [Header("Audio")]
    public AudioClip spawnSound; 
    public AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;

        if (target != null)
        {
            LaunchProjectile();
            // Toca som ao aparecer
            if (spawnSound && audioSource)
                audioSource.PlayOneShot(spawnSound);
        }

        Destroy(gameObject, 5f); // Destrói após 5 segundos
    }

    void LaunchProjectile()
    {
        Vector3 start = transform.position;
        Vector3 end = target.position;

        Vector3 displacementXZ = new Vector3(end.x - start.x, 0f, end.z - start.z);
        float horizontalDistance = displacementXZ.magnitude;
        float verticalOffset = end.y - start.y;

        float gravity = Mathf.Abs(Physics.gravity.y);

        // Calcular tempo de voo baseado na altura do arco
        float apexHeight = Mathf.Max(trajectoryHeight, verticalOffset + 0.1f);
        float timeToApex = Mathf.Sqrt(2 * apexHeight / gravity);
        float totalTime = timeToApex + Mathf.Sqrt(2 * (apexHeight - verticalOffset) / gravity);

        // Velocidade vertical para atingir o ponto mais alto
        float initialVerticalVelocity = Mathf.Sqrt(2 * gravity * apexHeight);

        // Velocidade horizontal para alcançar o alvo no tempo total
        Vector3 horizontalDirection = displacementXZ.normalized;
        float horizontalSpeed = horizontalDistance / totalTime;
        Vector3 initialHorizontalVelocity = horizontalDirection * horizontalSpeed;

        // Vetor de velocidade final
        Vector3 initialVelocity = initialHorizontalVelocity + Vector3.up * initialVerticalVelocity;

        // Aplicar impulso
        rb.AddForce(initialVelocity, ForceMode.Impulse);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damageAmount);
        }
    }


}
