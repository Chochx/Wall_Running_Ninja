using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using System.Collections;
using System;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private Collider2D groundCheckPoint;
    [SerializeField] private Collider2D hitBox;
    [SerializeField] private Collider2D attackHitBox;
    [SerializeField] private GameObject deathMenu;
    [SerializeField] private SlashEffect slashEffect;
    [SerializeField] private AudioSource musicAudioSrc; 
    private DifficultyManager difficultyManager;


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
    public bool isGrounded;
    private Vector2 groundCheckOffset;

    [Header ("Attack Settings")]
    public bool isAttacking;
    private SwordController swordController;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    private int airJumpsLeft;
    private bool canJump => jumpBufferCounter > 0f && (coyoteTimeCounter > 0f || airJumpsLeft > 0);

    public bool isAlive = true;
    public bool isJumping;

    private Animator anim; 

    //Private
    private PlayerInput playerInput;
    private InputAction touchPress;
    private InputAction positionAction;
    private Rigidbody2D rb;
    private bool isOverUI = false;
    private float startPosX;
    public bool resetStates;
    private bool wasPerformed;
    private float stateCheckInterval = 0.2f;
    private float lastInputTime;
    private Action<InputAction.CallbackContext> touchHandler;
    private bool isJumpAttacking;
    private float preloadAttackInput;
    private float groundProximityThreshold = 0.7f; 
    private bool wantsToAttack = false;

    void Awake()
    {
        Application.targetFrameRate = 120;
        playerInput = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        swordController = GetComponent<SwordController>();
        touchPress = playerInput.actions["TouchPress"];
        positionAction = playerInput.actions["Position"];
        playerInput.ActivateInput();
        positionAction.Enable();

        touchHandler = ctx => HandleTouch(ctx);
        touchPress.performed += touchHandler;
 
    }

    private void Start()
    {
        DebugManager.instance.enableRuntimeUI = false;
        difficultyManager = DifficultyManager.Instance;
        startPosX = transform.position.x;
        rb = GetComponent<Rigidbody2D>();
        swordController.onEndAnimation += AttackEnded;
        isAlive = true;

    }

    public void AttackEnded()
    {
        isAttacking = false;
    }

    private void OnDestroy()
    {
        touchPress.performed -= touchHandler;
    }

    private void OnDisable()
    {
        touchPress.performed -= touchHandler;
        swordController.onEndAnimation -= AttackEnded;
    }
    private void HandleTouch(InputAction.CallbackContext context)
    {
        if (!isAlive) return;
        Vector2 position = positionAction.ReadValue<Vector2>();
        
        // Now process with updated position
        if (position.x < Screen.width * 0.5f)
        {
            jumpBufferCounter = jumpBufferTime;
            if ((coyoteTimeCounter > 0f || airJumpsLeft > 0))
            {
                ExecuteJump();
            }
        }
        else if (position.x > Screen.width * 0.5f)
        {
            // If grounded, attack immediately
            if (isGrounded && !isAttacking)
            {
                ExecuteAttack();
            }
            // If in air, check distance to ground
            else if (isJumping && !isJumpAttacking)
            {
                // Cast a ray to check distance to ground
                RaycastHit2D hit = Physics2D.Raycast(
                    groundCheckOffset,
                    Vector2.down,
                    groundProximityThreshold,
                    groundLayer
                );

                // If close to ground, set flag for ground attack
                if (hit.collider != null)
                {
                    Debug.Log("Preloading attack");
                    wantsToAttack = true;
                }
                // If far from ground, do jump attack
                else
                {
                    ExecuteJumpAttack();
                }
            }
        }
    }

    private void ExecuteAttack()
    {
        isAttacking = true;
        anim.Play("Attack");
    }

    private void ExecuteJumpAttack()
    {
        isJumpAttacking = true;
        isJumping = false;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.down * 40, ForceMode2D.Impulse);
        anim.Play("JumpAttackDown");
    }

    void FixedUpdate()
    {
        GroundCheck();
        ApplyGravityMultiplier();
        HandleJumpBuffer();
        HandleCoyoteTime();
        HandleAnimations();
        HandleAnimationRunSpeed(); 

        if (transform.position.x < startPosX -1 || transform.position.y < -10) 
        {
            if (!isAlive) return;
            EndGame();
        }
    }

    private void HandleAnimationRunSpeed()
    {
        if (difficultyManager == null) return;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Run"))
        {
            float speedRation = difficultyManager.currentScrollSpeed * 0.05f;
            anim.SetFloat("SpeedMultiplier", speedRation);
        }
    }

    private void HandleAnimations()
    {
        if (isAttacking || !isAlive|| isJumpAttacking) return;
      
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
    private void OnGUI()
    {
#if UNITY_EDITOR
        GUI.Label(new Rect(10, 30, 300, 20), $"Can Jump: {canJump}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Is Grounded: {isGrounded}");
        GUI.Label(new Rect(10, 70, 300, 20), $"Jump Buffer Counter: {jumpBufferCounter}");
        GUI.Label(new Rect(10, 90, 300, 20), $"Coyote Time Counter: {coyoteTimeCounter}");
        GUI.Label(new Rect(10, 110, 300, 20), $"Air Jumps Left: {airJumpsLeft}");
        GUI.Label(new Rect(10, 120, 300, 20), $"Is Jump Attacking: {isJumpAttacking}");
#endif
    }
    private void GroundCheck()
    {
        groundCheckOffset = new Vector2(groundCheckPoint.bounds.center.x, groundCheckPoint.bounds.min.y);
        if (groundCheckPoint != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                groundCheckOffset,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );
            
            isGrounded = hit.collider != null;
            isJumping = hit.collider == null;
            if (hit.collider != null && wantsToAttack && isAlive)
            {
                ExecuteAttack();
                wantsToAttack = false;
            }
            rb.gravityScale = 4;

            Color rayColor = isGrounded ? Color.green : Color.red;
            Debug.DrawRay(groundCheckOffset, Vector2.down * groundCheckDistance, rayColor);
        }
    }
    private void ApplyGravityMultiplier()
    {
        // If falling
        if (rb.velocity.y < 0 && !isJumpAttacking)
        {
            rb.gravityScale = 1;
            rb.velocity += (fallMultiplier - 1) * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
        }
        // If ascending and jump button released
        else if (rb.velocity.y > 0)
        {
            rb.velocity += (lowJumpMultiplier - 1) * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
        }
        else if (isJumpAttacking && !isGrounded)
        {
            rb.gravityScale = 4;
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
            isJumpAttacking = false;
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
            EndGame();
        }

        if (collision.gameObject.CompareTag("Enemy") && attackHitBox.IsTouching(collision))
        {
            Debug.Log("Attack hit");
            Camera mainCamera = Camera.main;

            // Get enemy position
            Vector2 enemyPos = collision.transform.position + new Vector3(0,1);

            // Random start position left of the enemy
            float randomX = UnityEngine.Random.Range(enemyPos.x - 20f, enemyPos.x - 10f); // 5-10 units left of enemy
            float randomY = UnityEngine.Random.Range(enemyPos.y - 3f, enemyPos.y + 3f);  // Random height variation
            Vector2 startPos = new Vector2(randomX, randomY);

            // Calculate direction that ensures we hit the enemy
            Vector2 direction = (enemyPos - startPos).normalized;

            ScoreManager.Instance.OnEnemyKilled(); 

            slashEffect.TriggerEffect(startPos, direction);
        }
    }

    private void HandleDeath()
    {
        deathMenu.SetActive(true);
    }
    private void EndGame()
    {
        playerInput.DeactivateInput();
        ScoreManager.Instance.GameOver();
        isAlive = false;
        playerHasLanded = false;
        anim.Play("Death");
        musicAudioSrc.Stop();
        SoundManager.PlaySound(SoundType.END, 0.3f);
    }
}


