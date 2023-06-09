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
    Vector2 aimDirection;
    Vector2 aimInput;
    InputAction aimValue;
    bool aiming = false;
    
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
    }

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerInputActions = playerController.playerInputActions;
        playerInputActions.Player.Aim.performed += Aiming;
        playerInputActions.Player.Aim.Enable();
        playerInputActions.Player.AimInput.Enable();
        aimValue = playerInputActions.Player.AimInput;
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
            Debug.Log(aimInput);
            reticle.transform.position = new Vector3(aimInput.x + transform.position.x, aimInput.y + transform.position.y + 1.0f, 0);
        }
        
    }

}
