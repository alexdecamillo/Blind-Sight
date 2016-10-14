using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Player : MonoBehaviour {

    [Serializable]
    public class MovementSettings
    {
        public float ForwardSpeed = 6.0f;   // Speed when walking forward
        public float BackwardSpeed = 4.0f;  // Speed when walking backwards
        public float StrafeSpeed = 4.0f;    // Speed when walking sideways
        public float CurrentTargetSpeed;

        public void UpdateDesiredTargetSpeed(Vector2 input) {
            if (input == Vector2.zero) return;
            if (input.x > 0 || input.x < 0) {
                //strafe
                CurrentTargetSpeed = StrafeSpeed;
            }
            if (input.y < 0) {
                //backwards
                CurrentTargetSpeed = BackwardSpeed;
            }
            if (input.y > 0) {
                //forwards
                //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                CurrentTargetSpeed = ForwardSpeed;
            }
        }
    }

    public Camera cam;
    public MovementSettings movementSettings = new MovementSettings();
    public MouseLook mouseLook;
    public Click ping;

    private Rigidbody m_RigidBody;
    private CapsuleCollider m_Capsule;
    private float m_YRotation;

    public float facingDir;

    private void Start() {
        m_RigidBody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        mouseLook.Init(transform, cam.transform);
    }


    private void Update() {
        RotateView();

        // finds direction facing and sets arrow to this
        facingDir = Mathf.Abs(transform.eulerAngles.y);
        if (facingDir > 360) facingDir = facingDir % 360;
        FindObjectOfType<UI>().Direction(facingDir);
    }
    

    private void FixedUpdate() {
        Vector2 input = GetInput();
        Boolean click = Input.GetMouseButtonDown(0);

        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon)) {
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, Vector3.up).normalized;

            // updates target position vectors to adjust for target speed
            desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
            desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
            desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;

            // Moves m_RigidBody in direction of desiredMove while less than max speed (CurrentTargetSpeed)
            if (m_RigidBody.velocity.sqrMagnitude <
                (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed)) {
                m_RigidBody.AddForce(desiredMove, ForceMode.Impulse);
            }

        }

        if (click)
            ping.Shoot();

        if (Input.GetButtonDown("Jump")) {
            //FindObjectOfType<UI>().Win();
        }
    }

    // receives input for movement
    private Vector2 GetInput() {

        Vector2 input = new Vector2 {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };
        movementSettings.UpdateDesiredTargetSpeed(input);
        return input;
    }

    // Looks based on mouse position
    private void RotateView() {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        mouseLook.LookRotation(transform, cam.transform);
    }

    void OnCollisionEnter() {
        Debug.Log("collided");
    }

}
