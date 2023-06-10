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
    public Vector2 aimDirection;
    Vector2 aimInput;
    InputAction aimValue;
    public bool aiming = false;
    [SerializeField] float aimSpeed = 5.0f;
    
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
            aimDirection = aimValue.ReadValue<Vector2>();

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
    }

    // Update is called once per frame
    void Update()
    {
        if (item != null)
        {
            item.transform.position = itemSlot.transform.position;
            item.transform.rotation = Quaternion.identity;
        
        }

        if (aiming)
        {
            InputAim();
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Throwable"))
        {
            item = collision.gameObject;
            item.GetComponent<CircleCollider2D>().enabled = false;

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
