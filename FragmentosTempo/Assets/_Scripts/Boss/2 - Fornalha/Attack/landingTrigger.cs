using System.Collections;
using UnityEngine;

public class LandingTrigger : MonoBehaviour
{
    public int damage = 20;
    public float pushForce = 5000f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Aplica dano
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);

            // Aplica empurrão com impulso
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Salva constraints originais
                RigidbodyConstraints originalConstraints = rb.constraints;

                // Libera apenas posição
                rb.constraints = RigidbodyConstraints.FreezeRotation;

                // Direção de empurrão (horizontal)
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                pushDir.y = 0.2f;

                rb.AddForce(pushDir * pushForce, ForceMode.Impulse);

                // Reaplica constraints após tempo
                StartCoroutine(ReapplyConstraints(rb, originalConstraints));
            }

            // Desativa controle do player temporariamente
            PlayerMovement controller = other.GetComponent<PlayerMovement>();
            if (controller != null)
            {
                controller.enabled = false;
                StartCoroutine(ReenableController(controller, 0.5f));
            }
        }
    }

    IEnumerator ReapplyConstraints(Rigidbody rb, RigidbodyConstraints original, float delay = 0.5f)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null)
            rb.constraints = original;
    }

    IEnumerator ReenableController(MonoBehaviour controller, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (controller != null)
            controller.enabled = true;
    }
}
