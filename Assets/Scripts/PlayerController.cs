using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private ThrowObjects throwObjectsScript;

    // Basic Player Movement Variables
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

    // ThrowObjects variables
    [SerializeField] GameObject item;
    [SerializeField] GameObject itemSlot;
    Rigidbody2D itemRb;
    Collider2D itemCollider;
    [SerializeField] GameObject reticle;
    [SerializeField] LayerMask throwableLayer;
    [SerializeField] float throwForce;
    public Vector2 aimDirection;
    Vector2 aimInput;
    InputAction aimValue;
    public bool aiming = false;
    [SerializeField] float aimSpeed = 5.0f;
    bool hasItem = false;
    [SerializeField] float minYClamp = 0;
    [SerializeField] float maxYClamp = 1.0f;
    [SerializeField] float minXClamp = 0.0f;
    [SerializeField] float maxXClamp = 1.0f;
    [SerializeField] float pickUpOffset = 1.0f;
    



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
        playerInputActions.Player.Aim.performed += Aiming;
        playerInputActions.Player.Aim.Enable();
        playerInputActions.Player.AimInput.Enable();
        aimValue = playerInputActions.Player.AimInput;
        playerInputActions.Player.Throw.performed += Throw_performed;
        playerInputActions.Player.Throw.Enable();
        playerInputActions.Player.PickUp.performed += PickUp_performed;
        playerInputActions.Player.PickUp.Enable();
        playerInputActions.Player.Teleport.performed += Teleport_performed;
        playerInputActions.Player.Teleport.canceled += Teleport_canceled;
        playerInputActions.Player.Teleport.Enable();

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
        playerInputActions.Player.Aim.Disable();
        playerInputActions.Player.AimInput.Disable();
        playerInputActions.Player.Throw.Disable();
        playerInputActions.Player.PickUp.Disable();
        playerInputActions.Player.Teleport.Disable();
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
        //checks if aiming
        if (aiming)
        {
            InputAim();
        }

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
        if (aiming)
        {
            if (aimDirection.x < 0 && transform.eulerAngles.y == 0)
            {
                transform.Rotate(Vector2.up, 180);
            }
            if (aimDirection.x > 0 && transform.eulerAngles.y == 180)
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

    //////////////////////////throwable items functions

    private void Aiming(InputAction.CallbackContext context)
    {
        if (!aiming)
        {
            Debug.Log("Aiming");
            reticle.gameObject.SetActive(true);
            aimDirection = new Vector2(0, 0);
            if (transform.eulerAngles.y == 0)
            {
                reticle.transform.position = new Vector3(transform.position.x + 1.5f, transform.position.y + 1.5f, 0);
            }
            else
            {
                reticle.transform.position = new Vector3(transform.position.x - 1.5f, transform.position.y + 1.5f, 0);
            }

            aiming = true;
        }
        else if (aiming)
        {
            Debug.Log("Not Aiming");
            reticle.gameObject.SetActive(false);
            aiming = false;
        }

        Debug.Log(context.phase);

    }
    private void Teleport_canceled(InputAction.CallbackContext obj)
    {

    }

    private void Teleport_performed(InputAction.CallbackContext context)
    {
        Vector2 positionTemp;
        Vector2 speedTemp;
        if (item != null && !hasItem)
        {
            Debug.Log("Teleport");

            // switch positions
            positionTemp = transform.position;
            transform.position = item.transform.position;
            item.transform.position = positionTemp;

            //switch speeds
            speedTemp = playerRb.velocity;
            playerRb.velocity = itemRb.velocity;
            itemRb.velocity = speedTemp;
        }
    }

    private void PickUp_performed(InputAction.CallbackContext context)
    {
        float forwardDirection;
        if (transform.rotation.eulerAngles.y == 0)
        {
            forwardDirection = 1;
        }
        else
        {
            forwardDirection = -1;
        }
        Debug.Log(pickUpOffset);
        Vector2 cornerA = new Vector2(playerCollider.bounds.center.x, playerCollider.bounds.center.y - playerCollider.bounds.extents.y);
        Vector2 cornerB = new Vector2(playerCollider.bounds.center.x + (pickUpOffset * forwardDirection), playerCollider.bounds.center.y + playerCollider.bounds.extents.y);
        Collider2D[] itemsList = Physics2D.OverlapAreaAll(cornerA, cornerB, throwableLayer);
        //check if there is an item
        if (itemsList.Length != 0)
        {
            // if there is an item and player item is filled, drop current item
            if (item != null)
            {
                itemCollider.enabled = true;
                item.transform.parent = null;
                itemRb.isKinematic = false;
                itemRb.freezeRotation = false;
                item = null;
            }

            // pick up new item
            item = itemsList[0].gameObject;
            hasItem = true;
            itemCollider = item.GetComponent<CircleCollider2D>();
            itemCollider.enabled = false;
            itemRb = item.GetComponent<Rigidbody2D>();
            itemRb.velocity = Vector2.zero;
            itemRb.isKinematic = true;
            itemRb.freezeRotation = true;
            item.transform.parent = itemSlot.transform;
            item.transform.position = itemSlot.transform.position;
            item.transform.rotation = Quaternion.identity;
        }
        else
        {
            if (item != null)
            {
                hasItem = false;
                itemCollider.enabled = true;
                item.transform.parent = null;
                itemRb.isKinematic = false;
                itemRb.freezeRotation = false;
                item = null;
            }
        }


    }

    private void Throw_performed(InputAction.CallbackContext context)
    {
        if (hasItem)
        {
            hasItem = false;
            Debug.Log("Thrown");
            item.transform.position = reticle.transform.position;
            itemRb.isKinematic = false;
            itemCollider.enabled = true;
            itemRb.AddForce(aimDirection * throwForce, ForceMode2D.Impulse);
            item.transform.parent = null;
        }

    }

    void InputAim()
    {
        aimInput = aimValue.ReadValue<Vector2>();
        if (aimInput != Vector2.zero)
        {
            aimDirection = aimInput.normalized;
            if (aimDirection.y < minYClamp)
            {
                aimDirection.x = Mathf.Clamp(Mathf.Abs(aimDirection.x), minXClamp, maxXClamp) * Mathf.Sign(aimDirection.x);
            }
            //Debug.Log(aimDirection);
            aimDirection.y = Mathf.Clamp(aimDirection.y, minYClamp, maxYClamp);
            reticle.transform.position = new Vector3(aimDirection.x + transform.position.x, aimDirection.y + transform.position.y + 1.5f, 0);
        }

    }
}
