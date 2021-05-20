using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    [SerializeField] private Vector3 velocity;
    private PlayerCharacter playerCharacter;
    private Camera cam;
    private float camRotation;
    private float playerRotation;

    [SerializeField] private Vector3 forward;
    [SerializeField] private Vector3 right;

    [SerializeField] private bool cameraOpen = false;

    [SerializeField] private Canvas pictureCanvas;
    [SerializeField] private Slider cameraCooldownSlider;
    [SerializeField] private Canvas maskCanvas;

    [SerializeField] private PictureCamera pictureCamera;

    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private float cameraTimeElapsed;
    [SerializeField] private float cameraCooldown;
    [SerializeField] private bool cameraOnCooldown;
    [SerializeField] private float cameraFOV;
    [SerializeField] private float cameraDefaultFOV;
    [SerializeField] private float cameraMaxFOV;
    [SerializeField] private float cameraZoomSpeed;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        playerCharacter = GetComponent<PlayerCharacter>();
        cam = Camera.main;
        pictureCanvas.gameObject.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        cameraDefaultFOV = cam.fieldOfView;
    }
    
    void Update()
    {
        RotatePlayer();
        MovePlayer();
        Actions();
    }

    //We want the player body to rotate based on the Mouse X axis movement and the camera to rotate based on the Mouse Y axis movement
    private void RotatePlayer()
    {
        camRotation += input.inpMousePos.y * playerCharacter.RotationSpeed;
        playerRotation += input.inpMousePos.x * playerCharacter.RotationSpeed;

        camRotation = Mathf.Clamp(camRotation, -70.0f, 70.0f);

        Quaternion targetCamRotation = Quaternion.Euler(-camRotation, 0.0f, 0.0f);
        Quaternion targetPlayerRotation = Quaternion.Euler(0.0f, playerRotation, 0.0f);

        cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, targetCamRotation, playerCharacter.RotationSpeed * Time.deltaTime);
        transform.rotation = targetPlayerRotation;
    }

    private void MovePlayer()
    {
        //Movement is based on where the camera is looking
        forward = cam.gameObject.transform.forward * input.inpMove.y;
        right = cam.gameObject.transform.right * input.inpMove.x;

        velocity = (forward + right) * playerCharacter.MoveSpeed;

        if (input.inpShiftHeld)
        {
            velocity *= 1.5f;
        }
        else if (input.inpCtrlHeld)
        {
            velocity.y -= playerCharacter.MoveSpeed * 1.25f;
        }
        else if (input.inpSpaceHeld)
        {
            velocity.y += playerCharacter.MoveSpeed * 1.25f;
        }

        transform.position += velocity * Time.deltaTime;
    }

    private void CameraCooldown()
    {
        if (cameraOnCooldown)
        {
            cameraTimeElapsed += Time.deltaTime;
            cameraCooldownSlider.value = cameraTimeElapsed / cameraCooldown;
        }
        if (cameraTimeElapsed >= cameraCooldown)
        {
            cameraTimeElapsed = 0.0f;
            cameraOnCooldown = false;
        }
    }

    private void Actions()
    {
        StartCoroutine(ToggleCamera());

        if (cameraOpen)
        {
            CameraZoom();
            TakePicture();
        }
        CameraCooldown();
    }

    private void CameraZoom()
    {
        cam.fieldOfView -= input.inpMouseScoll * cameraZoomSpeed;

        if (cam.fieldOfView > cameraDefaultFOV)
        {
            cam.fieldOfView = cameraDefaultFOV;
        }
        else if (cam.fieldOfView < cameraMaxFOV)
        {
            cam.fieldOfView = cameraMaxFOV;
        }
    }

    private void TakePicture()
    {
        if (input.inpLeftClick && cameraOpen && !cameraOnCooldown)
        {
            cameraAnimator.Play("CameraShutter");

            pictureCamera.TakePicture();

            cameraOnCooldown = true;
        }
    }

    private IEnumerator ToggleCamera()
    {
        if (input.inpRightClick)
        {
            cameraOpen = !cameraOpen;
            if (!cameraOpen)
            {
                cameraAnimator.Play("CameraClose");

                yield return new WaitForSeconds(0.3f);
            }

            cam.fieldOfView = cameraDefaultFOV;
            pictureCanvas.gameObject.SetActive(cameraOpen);
            maskCanvas.gameObject.SetActive(!cameraOpen);
        }
    }
}