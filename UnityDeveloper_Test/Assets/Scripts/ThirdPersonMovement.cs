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

    [Header("Fall / Notify")]
    [Tooltip("How many seconds ungrounded before notifying game manager")]
    public float fallGrace = 7f;
    float fallTimer = 0f;

    [Tooltip("Assign your CollectableCollector (game manager) here so it can be notified on fall")]
    public CollectableCollector gameManager;

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
        // If the world is rotating, block horizontal movement but keep gravity & ground checks intact
        if (MarkerSelector.IsRotating)
        {
            // Do NOT change isGrounded calculation here — preserve original behavior
            // Keep gravity applied so player will fall if in the air
            // Stop horizontal movement and zero animation speed
            ApplyGravity();                    // updates velocity and moves vertically
            anim.SetFloat("Speed", 0f);

            UpdateFallTimerAndNotify();

            return;
        }

        // --- ORIGINAL ground check logic (unchanged) ---
        isGrounded = Physics.CheckSphere(transform.position, 0.1f, 1);
        anim.SetBool("isGrounded", isGrounded);

        UpdateFallTimerAndNotify();

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

            anim.SetFloat("Speed", sprinting ? 2f : 1f);
        }
        else
        {
            anim.SetFloat("Speed", 0f);
        }

        // jumping - same as original
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt((jumpHeight * 10f) * -2f * gravity);
        }

        // gravity (same as original)
        if (velocity.y > -20f)
        {
            velocity.y += (gravity * 10f) * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // Extracted gravity logic so we can reuse it while rotating (so player won't stick)
    void ApplyGravity()
    {
        // keep original isGrounded check so we don't change behavior
        isGrounded = Physics.CheckSphere(transform.position, 0.1f, 1);
        anim.SetBool("isGrounded", isGrounded);

        if (isGrounded && velocity.y < 0)
            velocity.y = -1;

        // apply gravity
        if (velocity.y > -20f)
            velocity.y += (gravity * 10f) * Time.deltaTime;

        // vertical-only move while rotating (no horizontal movement)
        controller.Move(new Vector3(0f, velocity.y, 0f) * Time.deltaTime);
    }

    void UpdateFallTimerAndNotify()
    {
        if (!isGrounded)
        {
            fallTimer += Time.deltaTime;

            if (fallTimer >= fallGrace)
            {
                // Notify the game manager once (Make sure gameManager assigned in Inspector)
                if (gameManager != null)
                {
                    gameManager.OnPlayerFellTooLong();
                }

                // clamp so we call only once until grounded again (or you can reset elsewhere)
                fallTimer = 0f;
            }
        }
        else
        {
            fallTimer = 0f;
        }
    }
}
