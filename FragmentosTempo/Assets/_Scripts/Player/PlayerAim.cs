using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayerMask;                     // Máscara de camada para detectar colisões com o solo.
    [SerializeField] private GameObject dialogBox;                          // Referência à caixa de diálogo, usada para travar a rotação durante o diálogo.
    [SerializeField] private Camera aimCamera;                              // Referência à câmera principal, usada para calcular a posição do mouse no mundo.
    [SerializeField] private Transform aimIndicator;                        // Refrência ao objeto visual da mira.
    [SerializeField] private GameObject menuInGame;                         // Referência ao menu.

    private InputAction aimInput;                                           // Input analógico direito.
    private PlayerInputAction playerInputAction;                            // Referência aos inputs.
    private bool isUsingGamepad = false;                                    // Flag para saber se está usando Gamepad.

    [SerializeField] private float joystickAimThreshold = 0.2f;             // Sensibilidade para considerar que está mirando.

    public Vector3 aimingTargetPoint;                                       // Ponto de destino para onde o jogador está mirando.

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();

        aimInput = playerInputAction.Player.Aim;
    }

    private void OnEnable()
    {
        aimInput.Enable();
    }

    private void OnDisable()
    {
        aimInput.Disable();
    }

    private void Update()
    {
        if ((dialogBox != null && dialogBox.activeSelf) || (menuInGame != null && menuInGame.activeSelf)) return;

        Vector2 aimDirection = aimInput.ReadValue<Vector2>();

        if (aimDirection.magnitude >= joystickAimThreshold)
        {
            isUsingGamepad = true;
            AimWithJoystick(aimDirection);
        }
        else
        {
            isUsingGamepad = false;
            AimWithMouse();
        }
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

    private void AimWithJoystick(Vector2 aimDirection)                      // Método responsável pela rotação do jogador em direção ao ponto de mira.
    {
        Vector3 direction = new Vector3(aimDirection.x, 0, aimDirection.y);
        if (direction.sqrMagnitude < 0.01f) return;

        transform.forward = direction;

        if (aimIndicator != null)                                       // Mover a mira para o ponto de mira.
        {
            Vector3 aimPosition = transform.position + direction.normalized * 2f;
            aimPosition.y += 0.1f;
            aimIndicator.position = aimPosition;
        }
    }

    private void AimWithMouse()
    {
        var (success, point) = GetMousePosition();
        if (success)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0;
            transform.forward = direction;

            if (aimIndicator != null)
            {
                Vector3 aimPosition = point;
                aimPosition.y += 0.1f;
                aimIndicator.position = aimPosition;
            }
        }
    }
}
