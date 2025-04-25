using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;

    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashCooldownTime = 1.5f;
    [SerializeField] private float dashDuration = .2f;

    private bool isDashing = false;
    private float dashTimer = 0;

    private float moveSpeed;

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
        if (isDashing || dashTimer - Time.time > dashCooldownTime) return;

        isDashing = true;
        dashTimer = Time.time;
        moveSpeed = dashSpeed;

        StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        moveSpeed = walkSpeed;
    }
}
