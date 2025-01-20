using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Threading.Tasks;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private float inputThersholdPoint = 1000;
    [SerializeField] private Collider2D groundCheckPoint;
    [SerializeField] private Collider2D hitBox;
    [SerializeField] private GameObject deathMenu;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private int maxAirJumps = 0;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    public bool playerHasLanded;
    private bool isGrounded;
    private Vector2 groundCheckOffset;

    [Header ("Attack Settings")]
    private bool isAttacking;
    private SwordController swordController;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    private int airJumpsLeft;
    private bool canJump => jumpBufferCounter > 0f && (coyoteTimeCounter > 0f || airJumpsLeft > 0);
    public bool isAlive = true;
    private bool isJumping;

    private Animator anim; 

    //Private
    private PlayerInput playerInput;
    private InputAction jumpPress;
    private InputAction attackPress;
    private InputAction positionAction;
    private Rigidbody2D rb;
    private bool isOverUI = false; 

    // Start is called before the first frame update
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        swordController = GetComponent<SwordController>();
        jumpPress = playerInput.actions["JumpTouch"];
        attackPress = playerInput.actions["AttackTouch"];
        positionAction = playerInput.actions["Position"];
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Application.targetFrameRate = 60;
        swordController.onEndAnimation += AttackEnded;

        var eventSystem = EventSystem.current;
        var eventTrigger = eventSystem.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = eventSystem.gameObject.AddComponent<EventTrigger>();
        }

        // Create pointer down entry
        var pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        var downEvent = new EventTrigger.TriggerEvent();
        downEvent.AddListener((data) => isOverUI = true);
        pointerDown.callback = downEvent;

        // Create pointer up entry
        var pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        var upEvent = new EventTrigger.TriggerEvent();
        upEvent.AddListener((data) => isOverUI = false);
        pointerUp.callback = upEvent;

        // Add both entries to the trigger
        eventTrigger.triggers.Add(pointerDown);
        eventTrigger.triggers.Add(pointerUp);

        isAlive = true;
    }

    private void AttackEnded()
    {
        isAttacking = false;
    }

    private void OnEnable()
    {
        jumpPress.performed += Jump;
        attackPress.performed += Attack;
    }

    private void Attack(InputAction.CallbackContext context)
    {
        if (isOverUI) return;
        if (!isAlive) return;
        Vector2 position = positionAction.ReadValue<Vector2>();
        if (position.x > Screen.width * 0.5f && isGrounded && !isAttacking)
        {
            isAttacking = true;
            anim.Play("Attack");
        }
    }

    private void OnDisable()
    {
        jumpPress.performed -= Jump;
        attackPress.performed -= Attack;
        swordController.onEndAnimation -= AttackEnded;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (isOverUI) return;
        if (!isAlive) return;
        Vector2 position = positionAction.ReadValue<Vector2>();
        if (position.x < Screen.width * 0.5f)
        {
            jumpBufferCounter = jumpBufferTime;

            if (canJump && !isAttacking)
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
        if (isAttacking) return;
        if (!isAlive) return;

        string currentAnimation = GetCurrentAnimation(); 
        anim.Play(currentAnimation);
    }

    private string GetCurrentAnimation()
    {
        if (!isGrounded)
        {
            return rb.velocity.y > 0 ? "JumpStart" : "JumpFall";
        }

        return "Run"; 
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
        if (collision.collider.CompareTag("Ground"))
        {
            playerHasLanded = true;
        }

        // Reset jumping state when hitting something above
        if (collision.contacts[0].normal.y < -0.7f)
        {
            isJumping = false;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && hitBox.IsTouching(collision))
        {
            Debug.Log($"Enemy Collision Detected - Current Score: {ScoreManager.Instance.TotalScore}");

            isAlive = false;
            playerHasLanded = false;
            anim.Play("Death");

            ScoreManager.Instance.GameOver();
        }
    }

    private void HandleDeath()
    {
        deathMenu.SetActive(true);
    }
}
