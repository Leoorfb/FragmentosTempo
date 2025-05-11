using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashCooldownTime = 1.5f;
    [SerializeField] private float dashDuration = .2f;
    [SerializeField] private Image dashCDOverlay;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldownTime = 1.0f;
    [SerializeField] private Image attackCDOverlay;

    [Header("Life Potion Settings")]
    [SerializeField] private float lifePotionCooldownTime = 5.0f;
    [SerializeField] private Image lifePotionCDOverlay;

    private bool isDashing = false;
    private float moveSpeed;

    private float dashTimer = 0;
    private float attackTimer = 0;
    private float lifePotionTimer = 0;

    private InputAction moveInput;
    private InputAction dashInput;
    private Rigidbody rb;
    
    public PlayerInputAction playerInputAction; // mover para uma classe Player quando criada


    private Vector3 moveDirection = Vector3.zero;
    private Vector3 moveDirectionMag = Vector3.zero;

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();
        rb = GetComponent<Rigidbody>();
        moveSpeed = walkSpeed;

        dashTimer = Time.time - dashCooldownTime;
        attackTimer = Time.time - attackCooldownTime;
        lifePotionTimer = Time.time - lifePotionCooldownTime;
    }

    private void OnEnable()
    {
        moveInput = playerInputAction.Player.Move;
        moveInput.Enable();

        dashInput = playerInputAction.Player.Dash;
        dashInput.Enable();
        dashInput.performed += Dash;
    }

    private void OnDisable()
    {
        moveInput.Disable();
        dashInput.Disable();
    }


    private void Update()
    {
        if (!isDashing)
        {
            moveDirection.x = moveInput.ReadValue<Vector2>().x;
            moveDirection.z = moveInput.ReadValue<Vector2>().y;
        }

        moveDirectionMag.x = moveDirection.x * moveSpeed;
        moveDirectionMag.z = moveDirection.z * moveSpeed;

        UpdateCooldownUI(dashCDOverlay, dashTimer, dashCooldownTime);
        UpdateCooldownUI(attackCDOverlay, attackTimer, attackCooldownTime);
        UpdateCooldownUI(lifePotionCDOverlay, lifePotionTimer, lifePotionCooldownTime);
    }

    private void FixedUpdate()
    {
        rb.velocity = moveDirectionMag;
    }

    private void Dash(InputAction.CallbackContext obj)
    {
        Dash();
    }

    private void Dash()
    {
        if (isDashing || Time.time - dashTimer < dashCooldownTime) return;

        isDashing = true;
        moveSpeed = dashSpeed;

        StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        moveSpeed = walkSpeed;
        dashTimer = Time.time;
    }

    private void UpdateCooldownUI(Image overlay, float timer, float cooldown)
    {
        if (overlay == null) return;

        float timeElapsed = Time.time - timer;
        float fill = Mathf.Clamp01(timeElapsed / cooldown);
        overlay.fillAmount = fill;
    }

    public void UseAttack()
    {
        if (Time.time - attackTimer >= attackCooldownTime)
        {
            attackTimer = Time.time;
            // Lógica de Ataque.
        }
    }

    public void UseLifePotion()
    {
        if (Time.time - lifePotionTimer >= lifePotionCooldownTime)
        {
            lifePotionTimer = Time.time;
            // Lógica de cura.
        }
    }
}
