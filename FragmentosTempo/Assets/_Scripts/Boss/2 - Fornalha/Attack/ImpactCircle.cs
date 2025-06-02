using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Este Script cria um hexagono ao boss fornalha finalizar seu ataque de pulo ao cair no chão, dando dano contiuamente por DURATION segundos

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ImpactCircle : MonoBehaviour
{
    public float duration = 3f;
    public float radius = 3f;
    public int damageAmount = 10;
    public int sides = 6; // hexágono 
    public float tickRate = 1f; // segundos entre cada dano

    private HashSet<PlayerHealth> playersInZone = new HashSet<PlayerHealth>();
    private float timeElapsed = 0f;


    [Header("Audio")]
    public AudioClip spawnSound; public AudioSource audioSource;

    void Start()
    {
        GenerateMesh();
        // Toca som ao aparecer
        if (spawnSound && audioSource)
            audioSource.PlayOneShot(spawnSound);
        StartCoroutine(DamageRoutine());


    }

    void Update()
    {
        timeElapsed += Time.deltaTime;


        if (timeElapsed >= duration)
            Destroy(gameObject);
    }

    IEnumerator DamageRoutine()
    {
        while (true)
        {
            foreach (PlayerHealth player in playersInZone)
            {
                if (player != null)
                    player.TakeDamage(damageAmount);
            }

            yield return new WaitForSeconds(tickRate);
        }
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

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damageAmount);
        }
    }

    void GenerateMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[sides + 1];
        int[] triangles = new int[sides * 3];

        vertices[0] = Vector3.zero; // centro
        float angleStep = 360f / sides;

        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[i + 1] = new Vector3(x, -0.5f, z);
        }

        for (int i = 0; i < sides; i++)
        {
            int current = i + 1;
            int next = (i + 1) % sides + 1;

            triangles[i * 3 + 0] = 0;         // centro
            triangles[i * 3 + 1] = next;
            triangles[i * 3 + 2] = current;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        mc.convex = false;
        mc.isTrigger = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);

        Vector3 center = transform.position;
        float angleStep = 360f / sides;
        Vector3 prevPoint = Vector3.zero;

        for (int i = 0; i <= sides; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 point = new Vector3(x, 0, z) + center;

            if (i > 0)
            {
                Gizmos.DrawLine(prevPoint, point);
            }

            prevPoint = point;
        }

        // Fecha o polígono
        Vector3 firstPoint = new Vector3(Mathf.Cos(0) * radius, 0, Mathf.Sin(0) * radius) + center;
        Gizmos.DrawLine(prevPoint, firstPoint);
    }
}
