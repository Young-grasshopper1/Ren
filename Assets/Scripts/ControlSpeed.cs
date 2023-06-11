using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSpeed : MonoBehaviour
{
    [SerializeField] float maxSpeed = 6.0f;

    Rigidbody2D objectRb;
    // Start is called before the first frame update
    void Start()
    {
        objectRb = GetComponent<Rigidbody2D>();
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 velocityDirection = objectRb.velocity.normalized;
        if (objectRb.velocity.magnitude > maxSpeed)
        {
            objectRb.velocity = velocityDirection * maxSpeed;
        }
    }
}
