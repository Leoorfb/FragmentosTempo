using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayerMask;                     // Máscara de camada para detectar colisões com o solo.
    [SerializeField] private GameObject dialogBox;                          // Referência à caixa de diálogo, usada para travar a rotação durante o diálogo.

    [SerializeField] private Camera aimCamera;                                              // Referência à câmera principal, usada para calcular a posição do mouse no mundo.

    public Vector3 aimingTargetPoint;                                       // Ponto de destino para onde o jogador está mirando.

    private void Start()
    {
        //mainCamera = Camera.main;                                           // Inicializa a referência da câmera principal.
    }

    private void Update()
    {
        Aim();                                                              // Chama o método de mira a cada frame.
    }

    private (bool success, Vector3 position) GetMousePosition()             // Método para obter a posição do mouse no mundo, verificando a colisão com o solo.
    {
        var ray = aimCamera.ScreenPointToRay(Input.mousePosition);         // Cria um raio a partir da posição do mouse na tela.

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundLayerMask))     // Verifica se o raio colide com algo no solo, usando o LayerMask para limitar as colisões ao solo.
        {
            return (success: true, position: hitInfo.point);                // Se o raio colidir, retorna a posição do ponto de impacto.
        }
        else
            return (success: false, position: Vector3.zero);                // Se não colidir, retorna false e a posição (0, 0, 0).
    }

    private void Aim()                                                      // Método responsável pela rotação do jogador em direção ao ponto de mira.
    {
        if (dialogBox != null && dialogBox.activeSelf) return;              // Se o diálogo estiver ativo, a rotação é bloqueada (não gira o jogador).

        var (success, aimingTargetPoint) = GetMousePosition();              // Obtém a posição do mouse no mundo, usando o método GetMousePosition.
        if (success)
        {
            Vector3 direction = aimingTargetPoint - transform.position;     // Calcula a direção do jogador para o ponto de mira, mantendo a rotação apenas no eixo horizontal (y = 0).
            direction.y = 0;                                                // Ignora a altura para garantir que a rotação seja apenas horizontal.
            transform.forward = direction;                                  // Atualiza a direção do jogador para olhar para o ponto de mira.
        }
    }
}
