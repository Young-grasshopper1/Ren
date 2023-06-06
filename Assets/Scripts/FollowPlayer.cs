using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Transform playerTrans;
    [SerializeField] float xOffset = 0;
    [SerializeField] float yOffset = 2.38f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(playerTrans.position.x + xOffset, playerTrans.position.y + yOffset, transform.position.z);
    }
}
