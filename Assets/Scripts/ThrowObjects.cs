using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowObjects : MonoBehaviour
{
    PlayerController playerController;
    PlayerInputActions playerInputActions;
    Collider2D playerCollider;
    [SerializeField] GameObject item;
    [SerializeField] GameObject itemSlot;
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
    [SerializeField ]float pickUpOffset = 1.0f;

    Collider2D itemCollider;
    public Rigidbody2D itemRb;
    Rigidbody2D playerRb;

    private void Awake()
    {
        
    }

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

    private void OnDisable()
    {
        playerInputActions.Player.Aim.Disable();
        playerInputActions.Player.AimInput.Disable();
        playerInputActions.Player.Throw.Disable();
        playerInputActions.Player.PickUp.Disable();
        playerInputActions.Player.Teleport.Disable();
    }

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerRb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        playerInputActions = playerController.playerInputActions;
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

    private void Teleport_canceled(InputAction.CallbackContext obj)
    {
        
    }

    private void Teleport_performed(InputAction.CallbackContext context)
    {
        Vector2 positionTemp;
        Vector2 speedTemp;
        if(item != null && !hasItem)
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
            if(item != null)
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

    // Update is called once per frame
    void Update()
    {

        if (aiming)
        {
            InputAim();
        }
        
    }


    void InputAim()
    {
        aimInput = aimValue.ReadValue<Vector2>();
        if (aimInput!= Vector2.zero)
        {
            aimDirection = aimInput.normalized;
            if (aimDirection.y < minYClamp)
            {
                aimDirection.x = Mathf.Clamp(Mathf.Abs(aimDirection.x), minXClamp, maxXClamp) * Mathf.Sign(aimDirection.x);
            }
            //Debug.Log(aimDirection);
            aimDirection.y = Mathf.Clamp(aimDirection.y, minYClamp,maxYClamp);
            reticle.transform.position = new Vector3(aimDirection.x + transform.position.x, aimDirection.y + transform.position.y + 1.5f, 0);
        }
        
    }


}
