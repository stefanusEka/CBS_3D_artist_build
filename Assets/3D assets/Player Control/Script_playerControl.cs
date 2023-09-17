using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class Script_playerControl : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float playerMass = 2.0f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float rotationSpeed = 15.0f;

    private CharacterController controller;
    private PlayerInput playerInput;
    private Vector3 moveVelocity;
    private bool isGrounded;
    public float currentMoveSpeed;

    private Transform cameraTransform;
    private Transform playerTransform;

    // -- inputs --
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private InputAction zoomAction;

    // -- zoom control --
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private CinemachineComponentBase componentBase;
    float cameraDistance;
    [SerializeField] private float cameraZoomModifier = 5.0f;
    [SerializeField] private float minZoomDistance = 3.0f;
    [SerializeField] private float maxZoomDistance = 8.0f;
    [SerializeField] private bool invertedScroll = false;
    


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;
        playerTransform = this.transform;

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        crouchAction = playerInput.actions["Crouch"];
        zoomAction = playerInput.actions["Zoom"];

        currentMoveSpeed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        LockHideCursor();
        CheckGrounded();
        PlayerMove();
        RotateBasedOnCamera();
        PlayerJump();
        // --- crouch --- (pake yg dah ada aja)
        ApplyGravity();

        if (Input.GetAxis("Mouse ScrollWheel") != 0) { // temporary, dia cuma detect scroll spesifik, kalo zoom diganti button lain ga bisa detect
            //if (input.zoomAction != 0.0f){
            ZoomCamera();
        }

    }


    // --- locks and hides cursor ---
    private void LockHideCursor()
    {        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    // --- checks if player is grounded ---
    private void CheckGrounded()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && moveVelocity.y < 0)
        {
            moveVelocity.y = 0f;
        }
    }

    // --- move player ---
    // - TODO limits movement when not grounded, but keep momentum - 
    private void PlayerMove()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0, input.y);

        // - camera follow mouse look -
        move = move.x * cameraTransform.right.normalized + (move.z * playerTransform.forward.normalized);
        //move = cameraTransform.TransformDirection(move);
        move.y = 0f;
        controller.Move(move * Time.deltaTime * currentMoveSpeed);
    }


    // --- rotate player ---
    // TODO player model rotates based on camera look, a relative forward of camera, s relative back of camera, etc
    // implementasi sekarang, player selalu ngadep depan kemanapun hadapan kamera nya
    private void RotateBasedOnCamera()
    {
        Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }


    // --- jump ability ---
    private void PlayerJump()
    { 
        if (jumpAction.triggered && isGrounded)
        {
            moveVelocity.y += Mathf.Sqrt(-jumpHeight * (playerMass * gravityValue));
        }
    }


    // --- zoom mechanic ---
    private void ZoomCamera()
    {
        // -- get cinemachine body --
        if (componentBase == null)
        {
            componentBase = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        }

        // actual zoom mechanic. taking account if the control is inverted, sensitivity, and min max distance
        (componentBase as CinemachineFramingTransposer).m_CameraDistance = 
            Mathf.Clamp((componentBase as CinemachineFramingTransposer).m_CameraDistance -
                (invertedScroll ? zoomAction.ReadValue<float>() : zoomAction.ReadValue<float>() / cameraZoomModifier),
                minZoomDistance,
                maxZoomDistance);
    }


    // --- apply constant gravity ---
    private void ApplyGravity()
    {
        moveVelocity.y += (playerMass * gravityValue) * Time.deltaTime;
        controller.Move(moveVelocity * Time.deltaTime);
    }
}



/*
 * todo
 * - limit/disable player move on air control + rigidbody
 */