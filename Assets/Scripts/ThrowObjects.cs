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
    
    // Start is called before the first frame update

    private void Awake()
    {
        
    }

    private void Aiming(InputAction.CallbackContext context)
    {
        Debug.Log("Aiming");
    }

    private void OnDisable()
    {
        playerInputActions.Player.Aim.Disable();
    }

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerInputActions = playerController.playerInputActions;
        playerInputActions.Player.Aim.performed += Aiming;
        playerInputActions.Player.Aim.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (item != null)
        {
            item.transform.position = itemSlot.transform.position;
        
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

}
