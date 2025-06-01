using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private GameObject laserPrefab;                // Prefab do proj�til (laser) que ser� instanciado ao atacar.
    [SerializeField] private Transform leftHandTransform;           // Posi��o na m�o esquerda do personagem de onde o proj�til ser� disparado.
    [SerializeField] private float delayShots = 0.1f;               // Tempo de atraso entre os dois disparos.

    [Header("Secondary Attack")]
    [SerializeField] private GameObject secondaryAtkPrefab;         // Prefab do proj�til secund�rio que ser� instanciado ao atacar.
    [SerializeField] private Transform rightHandTransform;          // Posi��o na m�o direita do personagem de onde o proj�til ser� disparado.

    [Header("Ultimate")]
    [SerializeField] private GameObject ultimatePrefab;             // Prefab da ultimate que ser� instanciado ao atacar.
    [SerializeField] private Transform ultHandTransform;            // Posi��o das m�os na frente do personagem de onde a ultimate ser� disparada.
    [SerializeField] private float delayUltimate = 1.5f;            // Tempo de atraso do disparo da ultimate.

    public void ShootLaser()                                        // M�todo para usar o ataque b�sico.
    {
        StartCoroutine(DoubleShotRoutine());                        // Chama a corrotina para disparar um proj�til duplo.
    }

    private IEnumerator DoubleShotRoutine()                         // Corrotina para disparo duplo.
    {
        FireLaser(laserPrefab, leftHandTransform);                  // Chama o m�todo para disparar o ataque b�sico na posi��o da m�o esquerda.
        SoundManager.Instance.PlaySound3D("PlayerAtk", transform.position);     // Toca o som de ataque.
        yield return new WaitForSeconds(delayShots);                // Tempo de atraso para disparar o segundo ataque em sequ�ncia.
        FireLaser(laserPrefab, leftHandTransform);                  // Dispara um segundo ataque.
        SoundManager.Instance.PlaySound3D("PlayerAtk", transform.position);     // Toca o som de ataque.
    }

    public void FireSecondAttack()                                  // M�todo para usar o ataque secund�rio.
    {
        FireLaser(secondaryAtkPrefab, rightHandTransform);          // Chama o m�todo para disparar um proj�til na m�o direita.
        SoundManager.Instance.PlaySound3D("PlayerSecondAtk", transform.position);     // Toca o som de ataque secund�rio.
    }

    public void FireUltimate()                                      // M�todo para disparar a ultimate.
    {
        StartCoroutine(UltimateRoutine());                          // Chama a corrotina para lidar com a ultimate.
    }

    private IEnumerator UltimateRoutine()                           // Corrotina para disparar a ultimate.
    {
        yield return new WaitForSeconds(delayUltimate);             // Atraso da sa�da da ultimate.
        FireLaser(ultimatePrefab, ultHandTransform);                // Chama o m�todo para disparar a ultimate na posi��o a frente do jogador.
        SoundManager.Instance.PlaySound3D("PlayerUltimate", transform.position);     // Toca o som de ultimate.
    }

    private void FireLaser(GameObject prefab, Transform handTransform)              // M�todo para calcular o disparo dos ataques.
    {
        Quaternion baseRotation = Quaternion.Euler(0f, handTransform.eulerAngles.y, 0f);    // Cria uma rota��o base usando apenas o eixo Y da m�o do jogador (a dire��o que ele est� olhando horizontalmente).
        Quaternion finalRotation = baseRotation * Quaternion.Euler(90f, 0f, 0f);            // Corrige a rota��o para alinhar o laser na dire��o correta (apontando para frente em vez de para cima).

        Instantiate(prefab, handTransform.position, finalRotation);                    // Instancia o proj�til na posi��o da m�o com a rota��o ajustada.
    }
}
