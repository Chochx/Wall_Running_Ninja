using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{


    [SerializeField] private float inputThersholdPoint = 1000;
    [SerializeField]private Collider2D groundCheckPoint;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private int maxAirJumps = 0;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    private Vector2 groundCheckOffset;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    private int airJumpsLeft;
    private bool canJump => jumpBufferCounter > 0f && (coyoteTimeCounter > 0f || airJumpsLeft > 0);
    private bool isJumping;

    private Animator anim; 


    //Private
    private PlayerInput playerInput;
    private InputAction jumpPress;
    private InputAction attackPress;
    private InputAction positionAction;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        jumpPress = playerInput.actions["JumpTouch"];
        attackPress = playerInput.actions["AttackTouch"];
        positionAction = playerInput.actions["Position"];
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Application.targetFrameRate = 60; 
    }

    private void OnEnable()
    {
        jumpPress.performed += Jump;
        attackPress.performed += Attack;
    }

    private void Attack(InputAction.CallbackContext context)
    {
        Vector2 position = positionAction.ReadValue<Vector2>();
        if (position.x > Screen.width * 0.5f)
        {
            Debug.Log("SLICE!!");
        }
    }

    private void OnDisable()
    {
        jumpPress.performed -= Jump;
        attackPress.performed -= Attack;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        Vector2 position = positionAction.ReadValue<Vector2>();
        if (position.x < Screen.width * 0.5f)
        {
            jumpBufferCounter = jumpBufferTime;

            if (canJump)
            {
                ExecuteJump();
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GroundCheck();
        ApplyGravityMultiplier();
        HandleJumpBuffer();
        HandleCoyoteTime();
        HandleAnimations(); 
        
    }

    private void HandleAnimations()
    {
        if (rb.velocity.y > 0 && !isGrounded)
        {
            anim.Play("JumpStart");
        }
        if (rb.velocity.y < 0.5f && !isGrounded)
        {
            anim.Play("JumpFall");
        }

        if (isGrounded) 
        { 
            anim.Play("Run"); 
        }
    }

    private void GroundCheck()
    {
        groundCheckOffset = new Vector2(groundCheckPoint.bounds.center.x, groundCheckPoint.bounds.min.y);
        if (groundCheckPoint != null)
        {
            // Option 1: Using Raycast
            RaycastHit2D hit = Physics2D.Raycast(
                groundCheckOffset,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );
            isGrounded = hit.collider != null;
            rb.gravityScale = 4;
            Color rayColor = isGrounded ? Color.green : Color.red;
            Debug.DrawRay(groundCheckOffset, Vector2.down * groundCheckDistance, rayColor);
        }

    }
    private void ApplyGravityMultiplier()
    {
        // If falling
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = 1;
            rb.velocity += (fallMultiplier - 1) * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
        }
        // If ascending and jump button released
        else if (rb.velocity.y > 0 && !jumpPress.IsPressed())
        {
            rb.velocity += (lowJumpMultiplier - 1) * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
        }
    }
    private void ExecuteJump()
    {
        // Reset vertical velocity before jumping for consistency
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        if (!isGrounded)
        {
            airJumpsLeft--;
        }

        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        isJumping = true;
    }
    private void HandleJumpBuffer()
    {
        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
        }
    }

    private void HandleCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            airJumpsLeft = maxAirJumps;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Reset jumping state when hitting something above
        if (collision.contacts[0].normal.y < -0.7f)
        {
            isJumping = false;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }
}
