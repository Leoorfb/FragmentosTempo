using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayerMask;                     // M�scara de camada para detectar colis�es com o solo.
    [SerializeField] private GameObject dialogBox;                          // Refer�ncia � caixa de di�logo, usada para travar a rota��o durante o di�logo.

    [SerializeField] private Camera aimCamera;

    public Vector3 aimingTargetPoint;                                       // Ponto de destino para onde o jogador est� mirando.


    private void Update()
    {
        Aim();                                                              // Chama o m�todo de mira a cada frame.
    }

    private (bool success, Vector3 position) GetMousePosition()             // M�todo para obter a posi��o do mouse no mundo, verificando a colis�o com o solo.
    {
        var ray = aimCamera.ScreenPointToRay(Input.mousePosition);         // Cria um raio a partir da posi��o do mouse na tela.

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundLayerMask))     // Verifica se o raio colide com algo no solo, usando o LayerMask para limitar as colis�es ao solo.
        {
            return (success: true, position: hitInfo.point);                // Se o raio colidir, retorna a posi��o do ponto de impacto.
        }
        else
            return (success: false, position: Vector3.zero);                // Se n�o colidir, retorna false e a posi��o (0, 0, 0).
    }

    private void Aim()                                                      // M�todo respons�vel pela rota��o do jogador em dire��o ao ponto de mira.
    {
        if (dialogBox != null && dialogBox.activeSelf) return;              // Se o di�logo estiver ativo, a rota��o � bloqueada (n�o gira o jogador).

        var (success, aimingTargetPoint) = GetMousePosition();              // Obt�m a posi��o do mouse no mundo, usando o m�todo GetMousePosition.
        if (success)
        {
            Vector3 direction = aimingTargetPoint - transform.position;     // Calcula a dire��o do jogador para o ponto de mira, mantendo a rota��o apenas no eixo horizontal (y = 0).
            direction.y = 0;                                                // Ignora a altura para garantir que a rota��o seja apenas horizontal.
            transform.forward = direction;                                  // Atualiza a dire��o do jogador para olhar para o ponto de mira.
        }
    }
}
