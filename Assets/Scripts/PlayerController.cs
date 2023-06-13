using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private ThrowObjects throwObjectsScript;

    // Start is called before the first frame update
    public PlayerInputActions playerInputActions;
    private InputAction movement;
    private Rigidbody2D playerRb;
    public CapsuleCollider2D playerCollider;
    public LayerMask groundLayer;
    private Animator playerAnimator;
    private int isRunning;
    private int isJumping;
    private int isFalling;

    public float speed = 5.0f;
    public float height;
    private float jumpForce;
    private Vector2 playerMovement;
    public bool grounded;
    public float fallGravity = 5.0f;
    private bool jumping;
    private bool falling;
    private float playerInitialHeight;
    private float downwardForceHeight;
    public float offset = 1.0f;



    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerRb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        throwObjectsScript = GetComponent<ThrowObjects>();
    }

    private void OnEnable()
    {
        movement = playerInputActions.Player.Movement;
        movement.Enable();

        playerInputActions.Player.Jump.performed += DoJump;
        playerInputActions.Player.Jump.canceled += Jump_canceled;
        playerInputActions.Player.Jump.Enable();
        
    }

    private void Jump_canceled(InputAction.CallbackContext context)
    {

        if (transform.position.y < downwardForceHeight && jumping)
        {
            Debug.Log("Jump Cancelled");
            playerRb.AddForce(Vector2.down * jumpForce, ForceMode2D.Impulse);
            jumping = false;
            playerAnimator.SetBool(isJumping, false);
            falling = true;
        }
    }

    private void DoJump(InputAction.CallbackContext context)
    {

        if (context.performed && grounded)
        {
            //Debug.Log("Jump");
            // store the players current height then the players max height before stopping force applied
            playerInitialHeight = transform.position.y;
            downwardForceHeight = height + playerInitialHeight;

            // set the jump force based off the height you want (2x to increase the speed, handle the fall somewhere else to account for increase) and the gravity scale 
            jumpForce = Mathf.Sqrt(3 * height * Physics.gravity.y * -2);
            playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumping = true;
            playerAnimator.SetBool(isJumping, true);
        }

    }

    private void OnDisable()
    {
        movement.Disable();
        playerInputActions.Player.Jump.Disable();
    }

    void FixedUpdate()
    {
        //sets the x velocity based off input
        playerRb.velocity = new Vector2(playerMovement.x * speed, playerRb.velocity.y);
        GroundCheck();

    }

    void Start()
    {
        // assign animation parameter hashes
        isRunning = Animator.StringToHash("isRunning");
        isFalling = Animator.StringToHash("isFalling");
        isJumping = Animator.StringToHash("isJumping");
    }


    // Update is called once per frame
    void Update()
    {
       
        //read players movement
        playerMovement = movement.ReadValue<Vector2>();
        if (playerMovement.x != 0)
        {
            playerAnimator.SetBool(isRunning, true);
        }
        else
        {
            playerAnimator.SetBool(isRunning, false);
        }

        if(jumping || falling)
        {
            HandleFalling();
        }

    }

    private void LateUpdate()
    {
        HandleRotation();
    }

    void HandleRotation()
    {
        // rotates the player by 180 degrees in the y axis
        if (throwObjectsScript.aiming)
        {
            if (throwObjectsScript.aimDirection.x < 0 && transform.eulerAngles.y == 0)
            {
                transform.Rotate(Vector2.up, 180);
            }
            if (throwObjectsScript.aimDirection.x > 0 && transform.eulerAngles.y == 180)
            {
                transform.Rotate(Vector2.up, -180);
            }
        }
        else
        {
            if (playerMovement.x < 0 && transform.eulerAngles.y == 0)
            {
                transform.Rotate(Vector2.up, 180);
            }
            if (playerMovement.x > 0 && transform.eulerAngles.y == 180)
            {
                transform.Rotate(Vector2.up, -180);
            }
        }
    }

    void HandleFalling()
    {
        if (transform.position.y >= downwardForceHeight && jumping)
        {
            //apply downward force
            playerRb.AddForce(Vector2.down * jumpForce,ForceMode2D.Impulse);
            jumping = false;
            playerAnimator.SetBool(isJumping, false);
            falling = true;
        }
        if (falling)
        {
            playerRb.gravityScale = fallGravity;
            playerAnimator.SetBool(isFalling, true);
        }
    }
    void GroundCheck()
    {
        //implement a check better than one ray
        if (Physics2D.Raycast(playerCollider.bounds.center, Vector2.down, (offset + playerCollider.bounds.extents.y), groundLayer))
        {
            grounded = true;
            falling = false;
            playerRb.gravityScale = 1.0f;
            playerAnimator.SetBool(isFalling, false);
        }
        else
        {
            grounded = false;
            if (playerRb.velocity.y < 0)
            {
                jumping = false;
                playerAnimator.SetBool(isJumping, false);
                falling = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(playerCollider.bounds.center, (offset + playerCollider.bounds.extents.y) * Vector2.down);
    }


}
