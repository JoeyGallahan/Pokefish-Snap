using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector2 inpMove { get; private set; }
    public Vector2 inpMousePos { get; private set; }
    public float inpMouseScoll { get; private set; }
    public bool inpRightClick { get; private set; }
    public bool inpLeftClick { get; private set; }

    public bool inpShiftHeld { get; private set; }
    public bool inpCtrlHeld { get; private set; }
    public bool inpSpaceHeld { get; private set; }
    
    // Update is called once per frame
    void Update()
    {
        CheckInputMovement();
        CheckInputMouse();
    }

    private void CheckInputMovement()
    {
        inpMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        inpShiftHeld = Input.GetKey(KeyCode.LeftShift);
        inpSpaceHeld = Input.GetKey(KeyCode.Space);
        inpCtrlHeld = Input.GetKey(KeyCode.LeftControl);
    }

    private void CheckInputMouse()
    {
        inpMousePos = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        inpLeftClick = Input.GetKeyDown(KeyCode.Mouse0);
        inpRightClick = Input.GetKeyDown(KeyCode.Mouse1);
        inpMouseScoll = Input.GetAxisRaw("Mouse ScrollWheel");
    }
}
