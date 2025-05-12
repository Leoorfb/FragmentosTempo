using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;                          // Velocidade padrão de movimentação do jogador.
    public bool canMove = true;                                             // Controle externo para ativar/desativar movimento (ex: durante diálogos).

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;                         // Velocidade durante o dash.
    [SerializeField] private float dashCooldownTime = 1.5f;                 // Tempo de recarga do dash.
    [SerializeField] private float dashDuration = .2f;                      // Duração do dash.
    [SerializeField] private Image dashCDOverlay;                           // UI: overlay que mostra recarga do dash.

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldownTime = 1.0f;               // Tempo de recarga do ataque.
    [SerializeField] private Image attackCDOverlay;                         // UI: overlay que mostra recarga do ataque.

    [Header("Life Potion Settings")]
    [SerializeField] private float lifePotionCooldownTime = 5.0f;           // Tempo de recarga da poção de vida.
    [SerializeField] private Image lifePotionCDOverlay;                     // UI: overlay que mostra recarga da poção.

    private bool isDashing = false;                                         // Flag para verificar se o jogador está usando Dash.
    private float moveSpeed;                                                // Velocidade atual (pode ser walkSpeed ou dashSpeed).

    private float dashTimer = 0;                                            // Timer para controlar o tempo de recarga do Dash.
    private float attackTimer = 0;                                          // Timer para controlar o tempo de recarga do Attack.
    private float lifePotionTimer = 0;                                      // Timer para controlar o tempo de recarga do uso de LifePot;

    private InputAction moveInput;                                          // Input de movimento.
    private InputAction dashInput;                                          // Input de dash.
    private Rigidbody rb;                                                   // Referência ao Rigidbody para movimentação física.

    public PlayerInputAction playerInputAction;                             // Referência ao sistema de input.

    private Vector3 moveDirection = Vector3.zero;                           // Direção bruta do movimento.
    private Vector3 moveDirectionMag = Vector3.zero;                        // Direção do movimento com magnitude aplicada.

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();                        // Instancia novo esquema de input e inicializa variáveis.
        rb = GetComponent<Rigidbody>();
        moveSpeed = walkSpeed;

        dashTimer = Time.time - dashCooldownTime;                           // Garante que as habilidades já estejam disponíveis ao iniciar.
        attackTimer = Time.time - attackCooldownTime;
        lifePotionTimer = Time.time - lifePotionCooldownTime;
    }

    private void OnEnable()                                                 // Habilita inputs e associa evento de Dash.
    {
        moveInput = playerInputAction.Player.Move;                          
        moveInput.Enable();

        dashInput = playerInputAction.Player.Dash;
        dashInput.Enable();
        dashInput.performed += Dash;
    }

    private void OnDisable()                                                // Desabilita inputs.
    {
        moveInput.Disable();
        dashInput.Disable();
    }


    private void Update()
    {
        if (!canMove) return;                                               // Impede atualizações se o jogador não puder se mover.

        if (!isDashing)                                                     // Se não estiver dando Dash, atualiza a direção de movimento com base no input.
        {
            moveDirection.x = moveInput.ReadValue<Vector2>().x;
            moveDirection.z = moveInput.ReadValue<Vector2>().y;
        }

        moveDirectionMag.x = moveDirection.x * moveSpeed;                   // Aplica magnitude da velocidade à direção.
        moveDirectionMag.z = moveDirection.z * moveSpeed;

        UpdateCooldownUI(dashCDOverlay, dashTimer, dashCooldownTime);                       // Atualiza as UIs de cooldowns.
        UpdateCooldownUI(attackCDOverlay, attackTimer, attackCooldownTime);
        UpdateCooldownUI(lifePotionCDOverlay, lifePotionTimer, lifePotionCooldownTime);
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        rb.velocity = moveDirectionMag;                                     // Aplica movimento no Rigidbody
    }

    private void Dash(InputAction.CallbackContext obj)                      // Evento chamado pelo input de dash.
    {
        Dash();
    }

    private void Dash()                                                     // Método para realizar Dash.
    {
        if (isDashing || Time.time - dashTimer < dashCooldownTime) return;  // Verifica se já está dashing ou se o dash ainda está em recarga.

        isDashing = true;
        moveSpeed = dashSpeed;

        StartCoroutine(DashCooldown());                                     // Inicia dash temporário.
    }

    private IEnumerator DashCooldown()                                      // Corrotina do Cooldown do Dash.
    {
        yield return new WaitForSeconds(dashDuration);                      // Aguarda tempo de Dash, depois volta à velocidade normal.
        isDashing = false;
        moveSpeed = walkSpeed;
        dashTimer = Time.time;                                              // Atualiza o tempo do último Dash.
    }

    private void UpdateCooldownUI(Image overlay, float timer, float cooldown)       // Método para atualizar UI de cooldowns (dash, ataque, poção).
    {
        if (overlay == null) return;

        float timeElapsed = Time.time - timer;
        float fill = Mathf.Clamp01(timeElapsed / cooldown);                 // Preenchimento de 0 a 1.
        overlay.fillAmount = fill;
    }

    public void UseAttack()                                                 // Método de chamada externa para tentar usar ataque.
    {
        if (Time.time - attackTimer >= attackCooldownTime)
        {
            attackTimer = Time.time;
            // Lógica de Ataque.
        }
    }

    public void UseLifePotion()                                             // Método de chamada externa para tentar usar poção.
    {
        if (Time.time - lifePotionTimer >= lifePotionCooldownTime)
        {
            lifePotionTimer = Time.time;
            // Lógica de cura.
        }
    }
}
