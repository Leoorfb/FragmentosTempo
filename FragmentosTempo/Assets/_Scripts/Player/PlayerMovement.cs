using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;                          // Velocidade padr�o de movimenta��o do jogador.
    public bool canMove = true;                                             // Controle externo para ativar/desativar movimento (ex: durante di�logos).

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;                         // Velocidade durante o dash.
    [SerializeField] private float dashCooldownTime = 1.5f;                 // Tempo de recarga do dash.
    [SerializeField] private float dashDuration = .2f;                      // Dura��o do dash.
    [SerializeField] private Image dashCDOverlay;                           // UI: overlay que mostra recarga do dash.

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldownTime = 1.0f;               // Tempo de recarga do ataque.
    [SerializeField] private Image attackCDOverlay;                         // UI: overlay que mostra recarga do ataque.

    [Header("Life Potion Settings")]
    [SerializeField] private float lifePotionCooldownTime = 5.0f;           // Tempo de recarga da po��o de vida.
    [SerializeField] private Image lifePotionCDOverlay;                     // UI: overlay que mostra recarga da po��o.

    private bool isDashing = false;                                         // Flag para verificar se o jogador est� usando Dash.
    private float moveSpeed;                                                // Velocidade atual (pode ser walkSpeed ou dashSpeed).

    private float dashTimer = 0;                                            // Timer para controlar o tempo de recarga do Dash.
    private float attackTimer = 0;                                          // Timer para controlar o tempo de recarga do Attack.
    private float lifePotionTimer = 0;                                      // Timer para controlar o tempo de recarga do uso de LifePot;

    private InputAction moveInput;                                          // Input de movimento.
    private InputAction dashInput;                                          // Input de dash.
    private Rigidbody rb;
    [SerializeField] private Animator animator;                                                   // Refer�ncia ao Rigidbody para movimenta��o f�sica.

    public PlayerInputAction playerInputAction;                             // Refer�ncia ao sistema de input.

    private Vector3 moveDirection = Vector3.zero;                           // Dire��o bruta do movimento.
    private Vector3 moveDirectionMag = Vector3.zero;                        // Dire��o do movimento com magnitude aplicada.

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();                        // Instancia novo esquema de input e inicializa vari�veis.
        rb = GetComponent<Rigidbody>();
        moveSpeed = walkSpeed;

        dashTimer = Time.time - dashCooldownTime;                           // Garante que as habilidades j� estejam dispon�veis ao iniciar.
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
        SetAnimation();
        if (!canMove) return;                                               // Impede atualiza��es se o jogador n�o puder se mover.

        if (!isDashing)                                                     // Se n�o estiver dando Dash, atualiza a dire��o de movimento com base no input.
        {
            moveDirection.x = moveInput.ReadValue<Vector2>().x;
            moveDirection.z = moveInput.ReadValue<Vector2>().y;
        }

        moveDirectionMag.x = moveDirection.x * moveSpeed;                   // Aplica magnitude da velocidade � dire��o.
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

    private void Dash()                                                     // M�todo para realizar Dash.
    {
        if (isDashing || Time.time - dashTimer < dashCooldownTime) return;  // Verifica se j� est� dashing ou se o dash ainda est� em recarga.

        isDashing = true;
        moveSpeed = dashSpeed;

        StartCoroutine(DashCooldown());                                     // Inicia dash tempor�rio.
    }

    private IEnumerator DashCooldown()                                      // Corrotina do Cooldown do Dash.
    {
        yield return new WaitForSeconds(dashDuration);                      // Aguarda tempo de Dash, depois volta � velocidade normal.
        isDashing = false;
        moveSpeed = walkSpeed;
        dashTimer = Time.time;                                              // Atualiza o tempo do �ltimo Dash.
    }

    private void UpdateCooldownUI(Image overlay, float timer, float cooldown)       // M�todo para atualizar UI de cooldowns (dash, ataque, po��o).
    {
        if (overlay == null) return;

        float timeElapsed = Time.time - timer;
        float fill = Mathf.Clamp01(timeElapsed / cooldown);                 // Preenchimento de 0 a 1.
        overlay.fillAmount = fill;
    }

    public void UseAttack()                                                 // M�todo de chamada externa para tentar usar ataque.
    {
        if (Time.time - attackTimer >= attackCooldownTime)
        {
            attackTimer = Time.time;
            // L�gica de Ataque.
        }
    }

    public void UseLifePotion()                                             // M�todo de chamada externa para tentar usar po��o.
    {
        if (Time.time - lifePotionTimer >= lifePotionCooldownTime)
        {
            lifePotionTimer = Time.time;
            // L�gica de cura.
        }
    }

    private void SetAnimation()
    {
        if (moveDirection == Vector3.zero)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        animator.SetBool("isWalking", true);

        float dotX = Vector3.Dot(transform.forward, moveDirection);
        float dotZ = Vector3.Dot(transform.right, moveDirection);

        animator.SetFloat("speedX", dotX);
        animator.SetFloat("speedZ", dotZ);
    }
}
