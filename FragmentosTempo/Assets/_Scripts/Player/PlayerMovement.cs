using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private InputAction moveInput;
    private InputAction dashInput;
    private Rigidbody rb;
    
    public PlayerInputAction playerInputAction; // mover para uma classe Player quando criada


    private Vector3 moveDirection = Vector3.zero;

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        moveInput = playerInputAction.Player.Move;
        moveInput.Enable();

        dashInput = playerInputAction.Player.Dash;
        dashInput.Enable();
    }

    private void OnDisable()
    {
        moveInput.Disable();
    }


    private void Update()
    {
        moveDirection.x = moveInput.ReadValue<Vector2>().x * speed;
        moveDirection.z = moveInput.ReadValue<Vector2>().y * speed;

    }

    private void FixedUpdate()
    {
        rb.velocity = moveDirection;
    }

}
