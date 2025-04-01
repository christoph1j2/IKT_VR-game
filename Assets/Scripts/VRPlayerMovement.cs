using UnityEngine;
using Valve.VR;

public class VRPlayerMovement : MonoBehaviour
{
    public SteamVR_Action_Vector2 moveAction; // Assign in Inspector
    public float speed = 2.0f;
    public AudioSource footstepAudio; // Assign in Inspector
    public AudioClip[] footstepClips; // Array of footstep sounds
    public float footstepDelay = 0.5f; // Delay between steps

    private CharacterController characterController;
    private Transform head;
    private float footstepTimer = 0f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        head = Camera.main?.transform;

        if (characterController == null)
            Debug.LogError("CharacterController missing on Player!");

        if (footstepAudio == null)
            Debug.LogError("AudioSource missing on Player!");
    }

    private void Update()
    {
        Vector3 direction = GetInput();
        Move(direction);
        HandleFootsteps(direction);
    }

    private Vector3 GetInput()
    {
        if (head == null || moveAction == null) return Vector3.zero;

        Vector2 input = moveAction.axis;
        if (input.magnitude < 0.1f) return Vector3.zero; // Ignore tiny movements

        Vector3 forward = head.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = head.right;
        right.y = 0;
        right.Normalize();

        return (forward * input.y + right * input.x).normalized;
    }

    private void Move(Vector3 direction)
    {
        if (characterController == null) return;
        characterController.Move(direction * speed * Time.deltaTime);
    }

    private void HandleFootsteps(Vector3 movement)
    {
        if (movement.magnitude > 0.1f) // Player is moving
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepDelay)
            {
                PlayFootstep();
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepAudio == null || footstepClips.Length == 0) return;

        int randomIndex = Random.Range(0, footstepClips.Length);
        footstepAudio.PlayOneShot(footstepClips[randomIndex]);
    }
}
