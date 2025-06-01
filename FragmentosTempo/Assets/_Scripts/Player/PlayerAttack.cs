using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private GameObject laserPrefab;                // Prefab do projétil (laser) que será instanciado ao atacar.
    [SerializeField] private Transform leftHandTransform;           // Posição na mão esquerda do personagem de onde o projétil será disparado.
    [SerializeField] private float delayShots = 0.1f;               // Tempo de atraso entre os dois disparos.

    [Header("Secondary Attack")]
    [SerializeField] private GameObject secondaryAtkPrefab;         // Prefab do projétil secundário que será instanciado ao atacar.
    [SerializeField] private Transform rightHandTransform;          // Posição na mão direita do personagem de onde o projétil será disparado.

    [Header("Ultimate")]
    [SerializeField] private GameObject ultimatePrefab;             // Prefab da ultimate que será instanciado ao atacar.
    [SerializeField] private Transform ultHandTransform;            // Posição das mãos na frente do personagem de onde a ultimate será disparada.
    [SerializeField] private float delayUltimate = 1.5f;            // Tempo de atraso do disparo da ultimate.

    public void ShootLaser()                                        // Método para usar o ataque básico.
    {
        StartCoroutine(DoubleShotRoutine());                        // Chama a corrotina para disparar um projétil duplo.
    }

    private IEnumerator DoubleShotRoutine()                         // Corrotina para disparo duplo.
    {
        FireLaser(laserPrefab, leftHandTransform);                  // Chama o método para disparar o ataque básico na posição da mão esquerda.
        SoundManager.Instance.PlaySound3D("PlayerAtk", transform.position);     // Toca o som de ataque.
        yield return new WaitForSeconds(delayShots);                // Tempo de atraso para disparar o segundo ataque em sequência.
        FireLaser(laserPrefab, leftHandTransform);                  // Dispara um segundo ataque.
        SoundManager.Instance.PlaySound3D("PlayerAtk", transform.position);     // Toca o som de ataque.
    }

    public void FireSecondAttack()                                  // Método para usar o ataque secundário.
    {
        FireLaser(secondaryAtkPrefab, rightHandTransform);          // Chama o método para disparar um projétil na mão direita.
        SoundManager.Instance.PlaySound3D("PlayerSecondAtk", transform.position);     // Toca o som de ataque secundário.
    }

    public void FireUltimate()                                      // Método para disparar a ultimate.
    {
        StartCoroutine(UltimateRoutine());                          // Chama a corrotina para lidar com a ultimate.
    }

    private IEnumerator UltimateRoutine()                           // Corrotina para disparar a ultimate.
    {
        yield return new WaitForSeconds(delayUltimate);             // Atraso da saída da ultimate.
        FireLaser(ultimatePrefab, ultHandTransform);                // Chama o método para disparar a ultimate na posição a frente do jogador.
        SoundManager.Instance.PlaySound3D("PlayerUltimate", transform.position);     // Toca o som de ultimate.
    }

    private void FireLaser(GameObject prefab, Transform handTransform)              // Método para calcular o disparo dos ataques.
    {
        Quaternion baseRotation = Quaternion.Euler(0f, handTransform.eulerAngles.y, 0f);    // Cria uma rotação base usando apenas o eixo Y da mão do jogador (a direção que ele está olhando horizontalmente).
        Quaternion finalRotation = baseRotation * Quaternion.Euler(90f, 0f, 0f);            // Corrige a rotação para alinhar o laser na direção correta (apontando para frente em vez de para cima).

        Instantiate(prefab, handTransform.position, finalRotation);                    // Instancia o projétil na posição da mão com a rotação ajustada.
    }
}
