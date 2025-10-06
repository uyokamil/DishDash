// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    /// <summary>
    /// Aligns the canvas to always face the camera.
    /// </summary>

    private void Update()
    {
        // Get the camera's position
        Vector3 cameraPosition = Camera.main.transform.position;

        // Calculate the direction from the transform to the camera
        Vector3 directionToCamera = cameraPosition - transform.position;

        // Set the Y rotation of the transform to face the camera
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(directionToCamera, Vector3.up), Vector3.up);
        targetRotation *= Quaternion.Euler(0, 180, 0); // Rotate by 180 degrees on the Y axis

        // Apply the rotation only on the Y axis
        transform.rotation = Quaternion.Euler(45, targetRotation.eulerAngles.y, 0);
    }
}
