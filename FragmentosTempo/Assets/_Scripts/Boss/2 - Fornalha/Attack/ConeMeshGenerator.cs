using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConeMeshGenerator : MonoBehaviour
{
    [Header("Cone Settings")]
    public float coneAngle = 60f;
    public float coneRange = 5f;
    public int segments = 30;

    [Header("Damage Settings")]
    public int damage = 10;
    public float duration = 3f;
    public float damageTickRate = 1f;
    public LayerMask targetLayers;

    private float timer = 0f;
    private List<Vector3> coneVertices = new List<Vector3>();

    [Header("Audio")]
    public AudioClip spawnSound; public AudioSource audioSource;

    void Start()
    {
        GenerateFlatCone();
        // Toca som ao aparecer
        if (spawnSound && audioSource)
            audioSource.PlayOneShot(spawnSound);
        StartCoroutine(DamageOverTime());
    }

    void GenerateFlatCone()
    {
        Mesh mesh = new Mesh();
        mesh.name = "FlatConeMesh";

        Vector3[] vertices = new Vector3[segments + 2]; // center + arc points
        int[] triangles = new int[segments * 3];

        // Center point at origin
        vertices[0] = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(-coneAngle / 2f, coneAngle / 2f, t);
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Sin(rad) * coneRange;
            float z = Mathf.Cos(rad) * coneRange;

            vertices[i + 1] = new Vector3(x, 0, z);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Aplicar ao MeshFilter e MeshCollider
        GetComponent<MeshFilter>().mesh = mesh;
    }


    IEnumerator DamageOverTime()
    {
        while (timer < duration)
        {
            ApplyConeDamage();
            yield return new WaitForSeconds(damageTickRate);
            timer += damageTickRate;
        }

        Destroy(gameObject);
    }

    void ApplyConeDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, coneRange, targetLayers);

        foreach (var hit in hits)
        {
            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            float dist = Vector3.Distance(transform.position, hit.transform.position);

            if (angleToTarget <= coneAngle / 2f && dist <= coneRange)
            {
                PlayerHealth hp = hit.GetComponent<PlayerHealth>();
                if (hp != null)
                {
                    hp.TakeDamage(damage);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 forward = transform.forward * coneRange;
        Quaternion leftRot = Quaternion.Euler(0, -coneAngle / 2f, 0);
        Quaternion rightRot = Quaternion.Euler(0, coneAngle / 2f, 0);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftDir);
        Gizmos.DrawLine(transform.position, transform.position + rightDir);

        int steps = 20;
        for (int i = 0; i < steps; i++)
        {
            float t1 = i / (float)steps;
            float t2 = (i + 1) / (float)steps;

            float angle1 = Mathf.Lerp(-coneAngle / 2f, coneAngle / 2f, t1) * Mathf.Deg2Rad;
            float angle2 = Mathf.Lerp(-coneAngle / 2f, coneAngle / 2f, t2) * Mathf.Deg2Rad;

            Vector3 point1 = transform.position + new Vector3(Mathf.Sin(angle1), 0, Mathf.Cos(angle1)) * coneRange;
            Vector3 point2 = transform.position + new Vector3(Mathf.Sin(angle2), 0, Mathf.Cos(angle2)) * coneRange;

            Gizmos.DrawLine(point1, point2);
        }
    }

}
