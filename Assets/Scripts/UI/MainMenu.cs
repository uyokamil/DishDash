// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    [Header("Camera/Object")]
    [SerializeField] private GameObject menuCamera;

    [Header("Camera/Positions")]
    [SerializeField] private Quaternion mainCameraRotation;
    [SerializeField] private Quaternion levelSelectCameraRotation;
    [SerializeField] private Quaternion settingsCameraRotation;

    private bool isRotating = false;

    public void StartLevel(int levelToStart)
    {
        Cursor.visible = false;
        SceneManager.LoadScene(levelToStart);
    }

    public void OnButtonInteract(string interaction)
    {
        if (interaction.Contains("button_level_select"))
        {
            StartCoroutine(RotateCamera(levelSelectCameraRotation));
        }
        else if (interaction.Contains("button_settings"))
        {
            StartCoroutine(RotateCamera(settingsCameraRotation));
        }
        else if (interaction.Contains("button_back"))
        {
            StartCoroutine(RotateCamera(mainCameraRotation));
        }
        else if (interaction.Contains("button_quit")) 
        {
            Application.Quit();
        }
    }

    public IEnumerator RotateCamera(Quaternion targetRotation)
    {
        float duration = .65f;
        yield return new WaitForEndOfFrame();
        isRotating = true;
        Quaternion startRotation = menuCamera.transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (!isRotating)
                break;

            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t); // Ease in and out

            // Apply quadratic smoothing
            t = t * t;

            Quaternion interpolatedRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            interpolatedRotation = Quaternion.Euler(interpolatedRotation.eulerAngles.x, interpolatedRotation.eulerAngles.y, 0f);
            menuCamera.transform.rotation = interpolatedRotation;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (isRotating)
        {
            menuCamera.transform.rotation = targetRotation;
            isRotating = false;
        }
    }
}
