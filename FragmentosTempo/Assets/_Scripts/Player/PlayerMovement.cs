using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;                          // Velocidade padrão de movimentação do jogador.
    public bool canMove = true;                                             // Controle externo para ativar/desativar movimento (ex: durante diálogos).
    public bool isStunned = false;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;                         // Velocidade durante o dash.
    [SerializeField] private float dashCooldownTime = 1.5f;                 // Tempo de recarga do dash.
    [SerializeField] private float dashDuration = .2f;                      // Duração do dash.
    [SerializeField] private Image dashCDOverlay;                           // UI: overlay que mostra recarga do dash.

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldownTime = 1.0f;               // Tempo de recarga do ataque.
    [SerializeField] private Image attackCDOverlay;                         // UI: overlay que mostra recarga do ataque.
    [SerializeField] private float SecondAtkCooldownTime = 3f;              // Tempo de recarga do ataque secundário.
    [SerializeField] private Image SecondAtkCDOverlay;                      // UI: overlay que mostra recarga do ataque secundário.

    [Header("Ultimate Settings")]
    [SerializeField] private float ultCooldownTime = 10f;                   // Tempo de recarga da ultimate.
    [SerializeField] private Image ultCDOverlay;                            // UI: overlay que mostra recarga da ultimate.

    [Header("Life Potion Settings")]
    [SerializeField] private float lifePotionCooldownTime = 5.0f;           // Tempo de recarga da poção de vida.
    [SerializeField] private Image lifePotionCDOverlay;                     // UI: overlay que mostra recarga da poção.

    [Header("Animation")]
    [SerializeField] private Animator animator;                             // Referência ao Animator do jogador.

    private bool isDashing = false;                                         // Flag para verificar se o jogador está usando Dash.
    private float moveSpeed;                                                // Velocidade atual (pode ser walkSpeed ou dashSpeed).

    private float dashTimer = 0;                                            // Timer para controlar o tempo de recarga do Dash.
    private float attackTimer = 0;                                          // Timer para controlar o tempo de recarga do Attack.
    private float secondAtkTimer = 0;                                       // Timer para controlar o tempo de recarga do ataque secundário.
    private float ultTimer = 0;                                             // Timer para controlar o tempo de recarga da ultimate.
    private float lifePotionTimer = 0;                                      // Timer para controlar o tempo de recarga do uso de LifePot.

    private InputAction moveInput;                                          // Input de movimento.
    private InputAction dashInput;                                          // Input de dash.
    private InputAction healInput;                                          // Input de heal.
    private InputAction attackInput;                                        // Input de attack.
    private InputAction secondAtkInput;                                     // Input do ataque secundário.
    private InputAction ultInput;                                           // Input da ultimate.

    private Rigidbody rb;                                                   // Referência ao Rigidbody para movimentação física.
    public PlayerInputAction playerInputAction;                             // Referência ao sistema de input.

    private PlayerAttack playerAttack;                                      // Referência ao script responsável pelos ataques do jogador.
    private PlayerHealth playerHealth;                                      // Referência ao script responsável pela vida e uso de poções do jogador.
    private DialogManager dialogManager;                                    // Referência ao gerenciador de diálogos, usada para verificar se um diálogo está ativo.

    private Vector3 moveDirection = Vector3.zero;                           // Direção bruta do movimento.
    private Vector3 moveDirectionMag = Vector3.zero;                        // Direção do movimento com magnitude aplicada.

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();                        // Instancia novo esquema de input e inicializa variáveis.
        playerHealth = GetComponent<PlayerHealth>();
        playerAttack = GetComponent<PlayerAttack>();

        rb = GetComponent<Rigidbody>();
        moveSpeed = walkSpeed;

        dashTimer = Time.time - dashCooldownTime;                           // Garante que as habilidades já estejam disponíveis ao iniciar.
        attackTimer = Time.time - attackCooldownTime;
        secondAtkTimer = Time.time - SecondAtkCooldownTime;
        ultTimer = Time.time;
        lifePotionTimer = Time.time - lifePotionCooldownTime;
    }

    private void OnEnable()                                                 // Habilita inputs.
    {
        moveInput = playerInputAction.Player.Move;
        moveInput.Enable();

        dashInput = playerInputAction.Player.Dash;
        dashInput.Enable();
        dashInput.performed += Dash;

        healInput = playerInputAction.Player.Heal;
        healInput.Enable();
        healInput.performed += OnHealPerformed;

        attackInput = playerInputAction.Player.Attack;
        attackInput.Enable();

        secondAtkInput = playerInputAction.Player.SecondaryAttack;
        secondAtkInput.Enable();

        ultInput = playerInputAction.Player.Ultimate;
        ultInput.Enable();
    }

    private void OnDisable()                                                // Desabilita inputs.
    {
        moveInput.Disable();
        dashInput.Disable();
        attackInput.Disable();
        secondAtkInput.Disable();
        ultInput.Disable();
        healInput.performed -= OnHealPerformed;
    }

    private void Update()
    {
        SetAnimation();
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
        UpdateCooldownUI(SecondAtkCDOverlay, secondAtkTimer, SecondAtkCooldownTime);
        UpdateUltCooldownUI();

        if (attackInput.triggered && (dialogManager == null || !dialogManager.IsDialogActive))      // Isso garante que o jogador não possa atacar enquanto estiver avançando falas do tutorial.
        {
            UseAttack();                                                    // Executa o ataque caso o jogador possa atacar no momento.
        }

        if (secondAtkInput.triggered && (dialogManager == null || !dialogManager.IsDialogActive))
        {
            SecondaryAttack();
        }

        if (ultInput.triggered && (dialogManager == null || !dialogManager.IsDialogActive))
        {
            TryUseUltimate();
        }
    }

    private void FixedUpdate()
    {
        if (isStunned)
            return;

        if (!canMove)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        rb.velocity = moveDirectionMag;                                     // Aplica movimento no Rigidbody.
    }

    private void OnHealPerformed(InputAction.CallbackContext context)       // Método para chamar o uso da poção de vida.
    {
        UseLifePotion();
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
        
        if (playerHealth != null)                                           // Ativa a flag de invunerabilidade do jogador no PlayerHealth.
        {
            playerHealth.isInvunerable = true;
        }

        StartCoroutine(DashCooldown());                                     // Inicia dash temporário.
    }

    private IEnumerator DashCooldown()                                      // Corrotina do Cooldown do Dash.
    {
        yield return new WaitForSeconds(dashDuration);                      // Aguarda tempo de Dash, depois volta à velocidade normal.
        isDashing = false;
        moveSpeed = walkSpeed;
        dashTimer = Time.time;                                              // Atualiza o tempo do último Dash.

        if (playerHealth != null)                                           // Desativa a flag de invunerabilidade do jogador no PlayerHealth.
        {
            playerHealth.isInvunerable = false;
        }
    }

    private void UpdateCooldownUI(Image overlay, float timer, float cooldown)       // Método para atualizar UI de cooldowns (dash, ataque, poção).
    {
        if (overlay == null) return;                                        // Se a imagem do overlay não estiver atribuída, sai do método.

        if (overlay == lifePotionCDOverlay)                                 // Verifica se o overlay atual é o da poção de vida.
        {
            if (playerHealth != null && playerHealth.PotionCount <= 0)      // Se o jogador estiver sem poções, esconde o preenchimento.
            {
                overlay.fillAmount = 1f;                                    // Deixa o overlay cheio, como se estivesse sempre em cooldown.
                overlay.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);          // Cinza com transparência
                return;                                                     // Sai do método para não continuar atualizando enquanto não tiver poções.
            }
            else
            {
                overlay.color = Color.white;                                // Cor normal quando há poções disponíveis.
            }
        }

        float timeElapsed = Time.time - timer;                              // Calcula o tempo passado desde que o cooldown começou.
        float fill = Mathf.Clamp01(timeElapsed / cooldown);                 // Preenchimento de 0 a 1.
        overlay.fillAmount = fill;                                          // Atualiza o preenchimento da imagem.
    }

    private void UpdateUltCooldownUI()                                      // Método para atualizar a UI da ultimate.
    {
        if (ultCDOverlay == null) return;

        float timeElapsed = Time.time - ultTimer;
        float fill = Mathf.Clamp01(timeElapsed / ultCooldownTime);

        ultCDOverlay.fillAmount = fill;                                     // Carregamento da barra da ultimate.
    }

    public void UseAttack()                                                 // Método de chamada externa para tentar usar ataque.
    {
        if (dialogManager != null && dialogManager.IsDialogActive) return;  // Impede o ataque se uma caixa de diálogo estiver ativa.

        if (Time.time - attackTimer >= attackCooldownTime)                  // Verifica se o tempo de recarga do ataque já passou.
        {
            attackTimer = Time.time;
            if (playerAttack != null)
            {
                playerAttack.ShootLaser();                                  // Executa o ataque.
            }
        }
    }

    public void SecondaryAttack()                                                 // Método de chamada externa para tentar usar ataque.
    {
        if (dialogManager != null && dialogManager.IsDialogActive) return;        // Impede o ataque se uma caixa de diálogo estiver ativa.

        if (Time.time - secondAtkTimer >= SecondAtkCooldownTime)                  // Verifica se o tempo de recarga do ataque já passou.
        {
            secondAtkTimer = Time.time;
            StartCoroutine(ExecuteSecondAttack());
        }
    }

    private IEnumerator ExecuteSecondAttack()                                   // Corrotina para chamar o uso do ataque secundário.
    {
        if (playerAttack != null)
        {
            playerAttack.FireSecondAttack();
        }

        yield return new WaitForSeconds(2f);                                // Duração do ataque secundário.
    }

    public void TryUseUltimate()                                            // Método para tentar usar a ultimate se disponível.
    {
        if (dialogManager != null && dialogManager.IsDialogActive) return;

        float fill = ultCDOverlay != null ? ultCDOverlay.fillAmount : 1f;   // Verificar se a barra da ultimate está completa.

        if (fill >= 1f)
        {
            ultTimer = Time.time;                                           // Atualiza o timer para o cooldown começar aqui.
            StartCoroutine(UseUltimate());                                  // Chama a corrotina para usar a ultimate.
        }
    }

    private IEnumerator UseUltimate()                                       // Corrotina para usar a ultimate.
    {
        float originalSpeed = moveSpeed;
        moveSpeed = walkSpeed * 0.5f;                                       // Aplicar uma lentidão no jogador enquanto estiver usando a ultimate.

        if (playerAttack != null)
        {
            playerAttack.FireUltimate();                                    // Dispara a ultimate.
        }

        yield return new WaitForSeconds(3f);

        moveSpeed = originalSpeed;                                          // Retorna a velocidade de movimento para o padrão.

    }

    public void UseLifePotion()                                             // Método de chamada externa para tentar usar poção.
    {
        if (Time.time - lifePotionTimer >= lifePotionCooldownTime)          // Isso impede que a poção seja usada antes do cooldown terminar.
        {
            if (playerHealth != null)                                       // Garante que a referência ao script de vida do jogador existe.
            {
                bool used = playerHealth.UsePotion();                       // Tenta usar a poção e armazena o resultado (true se foi usada, false se não).
                if (used)                                                   // Se a poção foi usada com sucesso, atualiza o temporizador para o momento atual.
                {
                    lifePotionTimer = Time.time;
                }
            }
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

    private IEnumerator Stun(float stunDuration)
    {
        isStunned = true;
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
    }
}
