using System.Collections;
using UnityEngine;

public class LandingTrigger : MonoBehaviour
{
    public int damage = 20;
    public float pushForce = 50f;
    public float stunDuration = 5f;
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player colidindo");
            // Aplica dano
            PlayerHealth health = other.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);

            // Aplica empurrão com impulso
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log("Player Tem rigidbody");
                // Salva constraints originais
                RigidbodyConstraints originalConstraints = rb.constraints;

                // Libera apenas posição
                rb.constraints = RigidbodyConstraints.FreezeRotation;

                // Direção de empurrão (horizontal)
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                pushDir.y = 0.2f;

                // Desativa controle do player temporariamente
                PlayerMovement controller = other.GetComponent<PlayerMovement>();
               
                StartCoroutine(controller.Stun(stunDuration));
                rb.AddForce(pushDir * pushForce, ForceMode.Impulse);

                // Reaplica constraints após tempo
                StartCoroutine(ReapplyConstraints(rb, originalConstraints));
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
