using UnityEngine;
using Valve.VR;

public class VRPlayerMovement : MonoBehaviour
{
    public SteamVR_Action_Vector2 moveAction;
    public float speed = 2.0f;
    [Tooltip("Reference to the body collider if not on the same object")]
    public GameObject BodyCollider;
    [Tooltip("Use transform-based movement instead of physics")]
    public bool useDirectMovement = false;

    private Transform cameraRig;
    private Transform head;
    private Rigidbody rb;
    private Vector3 currentMoveDirection = Vector3.zero;
    private bool setupComplete = false;

    private void Start()
    {
        Debug.Log("VRPlayerMovement: Starting setup...");
        
        // Find the player transform
        cameraRig = GameObject.Find("Player")?.transform;
        if (cameraRig == null) {
            Debug.LogError("❌ Player not found! Trying to use parent...");
            cameraRig = transform.parent;
        }
        
        // Find the camera
        head = Camera.main?.transform;
        if (head == null) {
            Debug.LogError("❌ Main Camera not found! Trying to find VRCamera...");
            head = GameObject.Find("VRCamera")?.transform;
        }

        // Find the rigidbody
        if (BodyCollider != null) {
            rb = BodyCollider.GetComponent<Rigidbody>();
            Debug.Log($"Looking for Rigidbody on specified BodyCollider: {(rb != null ? "Found" : "Not found")}");
        }
        
        if (rb == null) {
            rb = GetComponent<Rigidbody>();
            Debug.Log($"Looking for Rigidbody on this object: {(rb != null ? "Found" : "Not found")}");
        }
        
        if (rb == null && cameraRig != null) {
            rb = cameraRig.GetComponent<Rigidbody>();
            Debug.Log($"Looking for Rigidbody on Player object: {(rb != null ? "Found" : "Not found")}");
        }

        if (rb != null) {
            // Log Rigidbody settings
            Debug.Log($"Rigidbody found: IsKinematic={rb.isKinematic}, UseGravity={rb.useGravity}, " +
                     $"Constraints={rb.constraints}");
        }

        // Check SteamVR input
        if (moveAction == null) {
            Debug.LogError("❌ No SteamVR moveAction assigned!");
        } else {
            Debug.Log("SteamVR moveAction is assigned");
        }

        setupComplete = (cameraRig != null && head != null);
        Debug.Log($"Setup complete: {setupComplete}. CameraRig: {(cameraRig != null)}, Head: {(head != null)}");
    }

    private void Update()
    {
        if (!setupComplete) return;

        // Check for input
        currentMoveDirection = GetInput();
        
        // If we're using direct movement, apply it immediately
        if (useDirectMovement && currentMoveDirection != Vector3.zero) {
            MoveDirectly(currentMoveDirection);
        }
    }

    private void FixedUpdate()
    {
        if (!setupComplete || useDirectMovement) return;

        // Apply physics-based movement
        if (rb != null && currentMoveDirection != Vector3.zero) {
            MoveWithPhysics(currentMoveDirection);
        }
    }

    private Vector3 GetInput()
    {
        if (moveAction == null) return Vector3.zero;

        Vector2 inputVector = moveAction.axis;
        
        // Debug log when we get input
        if (inputVector.magnitude > 0.1f) {
            Debug.Log($"Input detected: {inputVector}");
        }

        return new Vector3(inputVector.x, 0, inputVector.y);
    }

    private void MoveWithPhysics(Vector3 direction)
    {
        if (rb == null) return;
        
        // Calculate movement direction based on head orientation
        Vector3 headForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        Vector3 headRight = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;
        Vector3 moveDirection = (headForward * direction.z + headRight * direction.x);
        
        // Apply movement
        Vector3 targetPosition = rb.position + moveDirection * speed * Time.fixedDeltaTime;
        Debug.Log($"Moving with physics: Current={rb.position}, Target={targetPosition}");
        rb.MovePosition(targetPosition);
    }

    private void MoveDirectly(Vector3 direction)
    {
        // Calculate movement direction based on head orientation
        Vector3 headForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        Vector3 headRight = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;
        Vector3 moveDirection = (headForward * direction.z + headRight * direction.x);
        
        // Apply direct transform movement
        Vector3 movement = moveDirection * speed * Time.deltaTime;
        Debug.Log($"Moving directly: {movement}");
        cameraRig.position += movement;
    }
}
