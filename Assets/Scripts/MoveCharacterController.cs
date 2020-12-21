using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCharacterController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 5f;
    private float rotationSpeed = 90f;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        float move = Input.GetAxis("Vertical");
        float rotate = Input.GetAxis("Horizontal");
        controller.Move(transform.forward * move * Time.deltaTime * playerSpeed);

        if(rotate != 0)
        {
            transform.Rotate(new Vector3(0, rotate * rotationSpeed * Time.deltaTime, 0));
        }

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
