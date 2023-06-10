using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowObjects : MonoBehaviour
{
    PlayerController playerController;
    PlayerInputActions playerInputActions;
    [SerializeField] GameObject item;
    [SerializeField] GameObject itemSlot;
    [SerializeField] GameObject reticle;
    [SerializeField] float throwForce;
    public Vector2 aimDirection;
    Vector2 aimInput;
    InputAction aimValue;
    public bool aiming = false;
    [SerializeField] float aimSpeed = 5.0f;

    Rigidbody2D itemRb;
    
    // Start is called before the first frame update

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
                reticle.transform.position = new Vector3(transform.position.x + 1.0f, transform.position.y + 1.0f, 0);
            }
            else
            {
                reticle.transform.position = new Vector3(transform.position.x - 1.0f, transform.position.y + 1.0f, 0);
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
    }

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerInputActions = playerController.playerInputActions;
        playerInputActions.Player.Aim.performed += Aiming;
        playerInputActions.Player.Aim.Enable();
        playerInputActions.Player.AimInput.Enable();
        aimValue = playerInputActions.Player.AimInput;
        playerInputActions.Player.Throw.performed += Throw_performed;
        playerInputActions.Player.Throw.Enable();
    }

    private void Throw_performed(InputAction.CallbackContext context)
    {
        Debug.Log("Thrown");
        item.transform.position = reticle.transform.position;
        itemRb.isKinematic = false;
        item.GetComponent<Collider2D>().enabled = true;
        itemRb.AddForce(aimDirection * throwForce, ForceMode2D.Impulse);
        item.transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {

        if (aiming)
        {
            InputAim();
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Throwable"))
        {
            if (item == null)
            {
                item = collision.gameObject;
                item.GetComponent<CircleCollider2D>().enabled = false;
                itemRb = item.GetComponent<Rigidbody2D>();
                itemRb.velocity = Vector2.zero;
                itemRb.isKinematic = true;
                itemRb.freezeRotation = true;
                item.transform.parent = itemSlot.transform;
                item.transform.position = itemSlot.transform.position;
                item.transform.rotation = Quaternion.identity;
            }

        }
    }

    void InputAim()
    {
        aimInput = aimValue.ReadValue<Vector2>();
        if (aimInput!= Vector2.zero)
        {
            aimDirection = aimInput.normalized;
            //Debug.Log(aimDirection);
            reticle.transform.position = new Vector3(aimDirection.x + transform.position.x, aimDirection.y + transform.position.y + 1.0f, 0);
        }
        
    }


}
