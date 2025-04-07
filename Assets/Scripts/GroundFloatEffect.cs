using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;

public class GroundFloatEffect : MonoBehaviour
{
    public float floatHeight = 0.2f;
    public float bobFrequency = 2f;
    public float bobAmplitude = 0.05f;
    public float rotationSpeed = 50f;
    public float floatUpSpeed = 2f;
    public float startDelay = 0.5f; // Delay before starting the floating effect
    public LayerMask groundLayer;

    private Rigidbody rb;
    private Interactable interactable;
    private bool isFloating = false;
    private bool startFloating = false;
    private Vector3 floatTargetPosition;
    private float baseY;

    private AudioSource audioSource;
    public AudioClip floatSound; // Sound to play when starting to float
    public AudioClip dropSound; // Sound to play when landing
    public AudioClip pickupSound; // Sound to play when picked up

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        interactable = GetComponent<Interactable>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (startFloating)
        {
            // Smooth float up to target position
            Vector3 pos = transform.position;
            pos = Vector3.MoveTowards(pos, floatTargetPosition, floatUpSpeed * Time.deltaTime);
            transform.position = pos;

            if (Vector3.Distance(pos, floatTargetPosition) < 0.01f)
            {
                startFloating = false;
                isFloating = true;
                baseY = floatTargetPosition.y;
            }
        }

        if (isFloating)
        {
            // Bobbing & rotation
            float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            Vector3 newPos = new Vector3(transform.position.x, baseY + bobOffset, transform.position.z);
            transform.position = newPos;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isFloating && !interactable.attachedToHand && ((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            PlaySound(dropSound); // Play landing sound
            BeginFloating();
        }
    }

    private void BeginFloating()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        floatTargetPosition = transform.position + Vector3.up * floatHeight;
        startFloating = true;

        PlaySound(floatSound); // Play floating sound
        // Optionally, you can add a small delay before starting the floating effect

        if (startDelay > 0)
        {
            StartCoroutine(DelayedFloatStart());
        }
        else
        {
            startFloating = true;
        }
    }

    private IEnumerator DelayedFloatStart()
    {
        yield return new WaitForSeconds(startDelay);
        startFloating = true;
    }

    public void OnAttachedToHand(Hand hand)
    {
        StopFloating();
        PlaySound(pickupSound); // Play pickup sound
    }

    public void OnDetachedFromHand(Hand hand)
    {
        // Let gravity take over again until grounded
        rb.isKinematic = false;
        rb.useGravity = true;
        isFloating = false;
        startFloating = false;
    }

    private void StopFloating()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        isFloating = false;
        startFloating = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}