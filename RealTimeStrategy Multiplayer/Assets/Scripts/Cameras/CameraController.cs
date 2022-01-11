using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBorderThickness = 10f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    private Vector2 previousInput;

    private Controls controls;

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);

        controls = new Controls();

        //You don't have to unsubscribe because unity's input system does it automatically
        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;

        controls.Enable();
    }

    [ClientCallback]
    private void Update() 
    {
        //Checks if you have authority and if you are still on the application(eg you esc and did something else on computer then your camera won't go to the edge)
        if(!hasAuthority || !Application.isFocused) { return; }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 pos = playerCameraTransform.position;

        //If we didn't put anything on the keyboard then check mouse
        if(previousInput == Vector2.zero)
        {
            Vector3 cursorMovement = Vector3.zero;

            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            if(cursorPosition.y >= Screen.height - screenBorderThickness)
            {
                cursorMovement.z += 1;
            }
            else if(cursorPosition.y <= screenBorderThickness)
            {
                cursorMovement.z -= 1;
            }
            if(cursorPosition.x >= Screen.width - screenBorderThickness)
            {
                cursorMovement.x += 1;
            }
            else if(cursorPosition.x <= screenBorderThickness)
            {
                cursorMovement.x -= 1;
            }

            pos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else
        {
            pos += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }

        //Makes sure that it doesn't go out of bounds of the screen. These x and y are just the 1 and 2nd values not coords. x is min and y is max
        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenXLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenZLimits.y);

        playerCameraTransform.position = pos;

    }

    private void SetPreviousInput(InputAction.CallbackContext ctx)
    {
        previousInput = ctx.ReadValue<Vector2>();
    }
}
