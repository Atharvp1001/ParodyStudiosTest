using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Essentials")]
    public Transform cam;
    CharacterController controller;
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    Animator anim;

    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    private bool sprinting;
    Vector2 movement;
    private float trueSpeed;

    [Header("Jumping")]
    public float jumpHeight;
    public float gravity;
    private bool isGrounded;
    Vector3 velocity;

    void Start()
    {
        trueSpeed = walkSpeed;
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(transform.position, 0.1f, 1);
        anim.SetBool("isGrounded", isGrounded);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            trueSpeed = sprintSpeed;
            sprinting = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            trueSpeed = walkSpeed;
            sprinting = false;
        }

        anim.transform.localPosition = Vector3.zero;
        anim.transform.localEulerAngles = Vector3.zero;

        movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 direction = new Vector3(movement.x, 0, movement.y).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * trueSpeed * Time.deltaTime);

            if (sprinting == true)
            {
                anim.SetFloat("Speed", 2);
            }
            else
            {
                anim.SetFloat("Speed", 1);
            }
        }

        else
        {
            anim.SetFloat("Speed", 0);
        }

        //jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt((jumpHeight * 10) * -2f * gravity);
        }

        if (velocity.y > -20)
        {
            velocity.y += (gravity * 10) * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
