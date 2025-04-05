using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Valve.VR;

public class VRPointer : MonoBehaviour
{
    public SteamVR_Input_Sources handType = SteamVR_Input_Sources.RightHand; // Or Any
    public SteamVR_Action_Boolean triggerAction;
    public LayerMask UILayer;
    public float maxRayDistance = 100f;
    
    private LineRenderer laserLineRenderer;
    private GameObject currentButton;

    void Start()
    {
        // Create and configure Line Renderer for visual laser
        laserLineRenderer = gameObject.AddComponent<LineRenderer>();
        laserLineRenderer.startWidth = 0.005f;
        laserLineRenderer.endWidth = 0.001f;
        laserLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        laserLineRenderer.startColor = Color.blue;
        laserLineRenderer.endColor = Color.blue;
        laserLineRenderer.positionCount = 2;
    }

    void Update()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position;
        Vector3 rayDirection = transform.forward;
        
        // Draw laser from controller
        laserLineRenderer.SetPosition(0, rayStart);
        
        bool hitSomething = Physics.Raycast(rayStart, rayDirection, out hit, maxRayDistance, UILayer);
        
        if (hitSomething)
        {
            // Set end position of laser to hit point
            laserLineRenderer.SetPosition(1, hit.point);
            
            Button button = hit.collider.GetComponent<Button>();
            if (button != null)
            {
                // Highlight the button
                currentButton = hit.collider.gameObject;
                button.OnPointerEnter(null);
                
                // Check if trigger is pressed
                if (triggerAction.GetStateDown(handType))
                {
                    button.onClick.Invoke();
                }
            }
        }
        else
        {
            // Extend laser to max distance if nothing hit
            laserLineRenderer.SetPosition(1, rayStart + (rayDirection * maxRayDistance));
            
            // Clear current button reference
            if (currentButton != null)
            {
                currentButton.GetComponent<Button>().OnPointerExit(null);
                currentButton = null;
            }
        }
    }
}
