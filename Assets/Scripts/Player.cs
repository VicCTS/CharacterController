using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController controller;
    private Animator anim;

    public float speed = 5;

    public float gravity = -9.81f;
    private Vector3 playerVelocity;
    public LayerMask groundLayer;

    

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(x, 0, z);

        float velZ = Vector3.Dot(move.normalized, transform.forward);
        float velX = Vector3.Dot(move.normalized, transform.right);
        anim.SetFloat("VelZ", velZ);
        anim.SetFloat("VelX", velX);

        controller.Move(move * speed * Time.deltaTime);

        if(controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
        }

        if(!controller.isGrounded)
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }
        
        controller.Move(playerVelocity * Time.deltaTime);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 direction = hit.point - transform.position;
            direction.y = 0;
            transform.forward = direction;
        }
    }
}
