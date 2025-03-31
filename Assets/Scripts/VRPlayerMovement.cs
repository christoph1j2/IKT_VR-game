using UnityEngine;
using Valve.VR;

public class VRPlayerMovement : MonoBehaviour
{
    public SteamVR_Action_Vector2 moveAction;  // Assign in Inspector
    public float speed = 2.0f;

    private Transform cameraRig;
    private Transform head;

    private void Start()
    {
        cameraRig = GameObject.Find("Player")?.transform;
        head = Camera.main?.transform;

        if (cameraRig == null) Debug.LogError("❌ cameraRig is NULL! Make sure [CameraRig] exists in the scene.");
        if (head == null) Debug.LogError("❌ head is NULL! Ensure there's a Main Camera in the scene.");
    }


    private void Update()
    {
        if (moveAction == null)
        {
            Debug.LogError("MoveAction is null! Assign it in the Inspector.");
            return;
        }

        Vector3 direction = GetInput();
        Move(direction);
    }


    private Vector3 GetInput()
    {
        if (moveAction == null)
        {
            Debug.LogError("❌ moveAction is NULL! Make sure it's assigned in the Inspector.");
            return Vector3.zero;
        }

        Vector2 inputVector = moveAction.axis; // Potential null issue
        Debug.Log($"✅ Move Input: {inputVector}");

        Vector3 direction = new Vector3(inputVector.x, 0, inputVector.y);
        return direction;
    }


    private void Move(Vector3 direction)
    {
        cameraRig.position += direction * speed * Time.deltaTime;
    }
}
