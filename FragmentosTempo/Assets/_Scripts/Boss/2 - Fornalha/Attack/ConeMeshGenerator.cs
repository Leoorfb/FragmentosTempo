using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ConeMeshGenerator : MonoBehaviour
{
    [Header("Cone Settings")]
    public float coneAngle = 60f;
    public float coneRange = 5f;
    public int segments = 30;

    void Start()
    {
        GenerateFlatCone();
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

        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = null;
        mc.sharedMesh = mesh;
        mc.convex = true;
        mc.isTrigger = true;
    }


    [SerializeField] private int damage = 10;
    [SerializeField] private float duration = 3f;
    [SerializeField] private LayerMask targetLayers; // Defina quais camadas devem ser afetadas (ex: "Inimigo")

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return; // Ignora objetos fora da camada desejada

        // Exemplo: aplica dano se tiver um script "Health"
        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Configurações do cone
        float angle = coneAngle;
        float range = coneRange;
        int steps = 30;

        Gizmos.color = Color.red;
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        // Desenha linhas radiais do cone
        for (int i = 0; i <= steps; i++)
        {
            float stepAngle = -angle / 2 + (angle * i / steps);
            Quaternion rot = Quaternion.Euler(0, stepAngle, 0);
            Vector3 dir = rot * forward;
            Gizmos.DrawLine(origin, origin + dir * range);
        }

        // Desenha arco externo (base do cone)
        Vector3 lastPoint = Vector3.zero;
        for (int i = 0; i <= steps; i++)
        {
            float stepAngle = -angle / 2 + (angle * i / steps);
            Quaternion rot = Quaternion.Euler(0, stepAngle, 0);
            Vector3 dir = rot * forward;
            Vector3 point = origin + dir * range;

            if (i > 0)
                Gizmos.DrawLine(lastPoint, point);

            lastPoint = point;
        }
    }

}
