using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;   
    public float smoothSpeed = 5f; 
    public Vector3 offset = new Vector3(0, 15, -10);
    
    [Header("Camera Settings")]
    public float rotationX = 45f;

    private void Start()
    {
        // Ensure camera is perspective
        Camera cam = GetComponent<Camera>();
        if (cam != null && cam.orthographic)
        {
            cam.orthographic = false;
            cam.fieldOfView = 60f;
        }
    }

    private void LateUpdate() 
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
        
        // Look at player with fixed angle
        Vector3 lookAtPos = target.position;
        lookAtPos.y = target.position.y + 1f; // Look slightly above player
        transform.LookAt(lookAtPos);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        if (target != null)
        {
            // Set initial position immediately
            transform.position = target.position + offset;
            
            Vector3 lookAtPos = target.position;
            lookAtPos.y = target.position.y + 1f;
            transform.LookAt(lookAtPos);
        }
    }
}
