using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerControllerOld : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float jumpHeightPercent = 0.1f; // % of screen height
    [SerializeField] private Camera gameCamera;

    private PlayerInput playerInput;
    private InputAction pressAction;

    private WallManager wallManager;
    private bool isJumping;
    private float jumpTimer;
    private int currentWall = 1; // 1 = right wall, -1 = left wall
    private float screenHeight;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        pressAction = playerInput.actions["Touch"];
    }

    private void OnEnable()
    {
        pressAction.performed += InitJump;
    }

    private void OnDisable()
    {
        pressAction.performed -= InitJump;
    }

    private void InitJump(InputAction.CallbackContext context)
    {
        StartWallJump();
    }

    private void Start()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;

        wallManager = FindObjectOfType<WallManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        CalculateScreenDimensions();
        SnapToWall();
    }

    private void CalculateScreenDimensions()
    {
        Vector2 bottomLeft = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, gameCamera.nearClipPlane));
        Vector2 topRight = gameCamera.ViewportToWorldPoint(new Vector3(1, 1, gameCamera.nearClipPlane));
        screenHeight = topRight.y - bottomLeft.y;
    }

    private void Update()
    {
        // Handle input
        //if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && !isJumping)
        //{
        //    StartWallJump();
        //}

        if (isJumping)
        {
            UpdateJump();
        }

        // Update player facing direction
        spriteRenderer.flipX = currentWall < 0;
    }

    private void StartWallJump()
    {
        isJumping = true;
        jumpTimer = 0;
        currentWall *= -1; // Switch walls
    }

    private void UpdateJump()
    {
        jumpTimer += Time.deltaTime;
        float jumpProgress = jumpTimer / jumpDuration;

        if (jumpProgress >= 1f)
        {
            isJumping = false;
            SnapToWall();
        }
        else
        {
            // Horizontal movement
            float xPos = Mathf.Lerp(
                wallManager.HorizontalSnapDistance * -currentWall,
                wallManager.HorizontalSnapDistance * currentWall,
                jumpProgress
            );

            // Vertical arc
            float yOffset = Mathf.Sin(jumpProgress * Mathf.PI) * (screenHeight * jumpHeightPercent);

            transform.position = new Vector3(xPos, yOffset, 0);
        }
    }

    private void SnapToWall()
    {
        transform.position = new Vector3(
            wallManager.HorizontalSnapDistance * currentWall,
            0,
            0
        );
    }

    private void OnValidate()
    {
        jumpHeightPercent = Mathf.Clamp(jumpHeightPercent, 0f, 0.5f);
    }
}