using UnityEngine;

public class FloatAndSpin : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private float activationYPosition = 10f;
    [SerializeField] private bool isActive = false;
    
    [Header("Floating")]
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatFrequency = 1f;
    
    [Header("Spinning")]
    [SerializeField] private float spinSpeed = 30f;
    
    // Store the initial position for floating reference
    private Vector3 basePosition;
    
    void Update()
    {
        // Check if the object has reached the activation height
        if (!isActive && transform.position.y >= activationYPosition)
        {
            isActive = true;
            basePosition = transform.position;
        }
        
        // If active, perform floating and spinning
        if (isActive)
        {
            // Floating effect - only affects Y coordinate
            float newY = basePosition.y + floatAmplitude * Mathf.Sin(Time.time * floatFrequency);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            
            // Spinning effect - only rotates around Y axis
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        }
    }
}
