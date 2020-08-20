using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftJoystick : MonoBehaviour
{
    [Header("Stick")]
    [SerializeField] private GameObject joystick = null;
    [Header("Is controller active?")]
    [SerializeField] private bool IsReady = true;
    //Canvas to check current resolution and scale
    private Canvas canvas = null;
    private float ScreenScale = 0;
    public float LeftDivider = 3.5f;
    public float HeightDivider = 2.1f;

    void Start()
    {
        //Setting up initial values
        joystick.transform.position = this.transform.position;
        canvas = GetComponentInParent<Canvas>();
        ScreenScale = canvas.transform.localScale.x * 100;
    }

    void Update()
    {
        if (!Controller.M_Controller) { Debug.Log("M_Controller not found!"); return; }
        //Checking current device and checking button states
        #region JoystickController
        if (Application.isMobilePlatform && Input.touchCount > 0 && IsReady)
        {
            var Touches = Input.touches;
            for (int TouchID = 0; TouchID < Input.touchCount; TouchID++)
            {
                Vector3 touch_pos = Touches[TouchID].position;
                if (CheckBounds(touch_pos))
                {
                    StickHandler(touch_pos);
                    break;
                }
                else
                {
                    joystick.transform.position = transform.position;
                }
            }
        }
        else if (!Application.isMobilePlatform && Input.GetMouseButton(0) && IsReady)
        {
            Vector3 touch_pos = Input.mousePosition;
            if (CheckBounds(touch_pos))
            {
                StickHandler(touch_pos);
            }
            else
            {
                joystick.transform.position = transform.position;
                Controller.M_Controller.YDirectional = 0;
                Controller.M_Controller.XDirectional = 0;
            }
        }
        else
        {
            joystick.transform.position = transform.position;
            Controller.M_Controller.YDirectional = 0;
            Controller.M_Controller.XDirectional = 0;
        }
        #endregion
    }
    //Function to get values accordng to stick postiion
    private void StickHandler(Vector3 touch_pos)
    {
        Vector3 target_pos = touch_pos - this.transform.position;
        //If current mouse position inside a joystick holder
        if (target_pos.magnitude < ScreenScale && CheckBounds(touch_pos))
        {
            joystick.transform.position = touch_pos;
            Vector3 cur = joystick.transform.position;
            target_pos = (cur - transform.position);
            //Rounding values to scale from 0 to 1
            Controller.M_Controller.YDirectional = -target_pos.y / 100;
            Controller.M_Controller.XDirectional = -target_pos.x / 100;
        }
        //If current mouse position outside a joystick holder
        else if (CheckBounds(touch_pos))
        {
            target_pos.Normalize();
            target_pos = this.transform.position + target_pos * ScreenScale;
            joystick.transform.position = target_pos;
            Vector3 cur = joystick.transform.position;
            target_pos = (cur - transform.position);
            //Rounding values to scale from 0 to 1
            Controller.M_Controller.YDirectional = -target_pos.y / 100;
            Controller.M_Controller.XDirectional = -target_pos.x / 100;
        }
    }

    //Bounds for Joystick
    private bool CheckBounds(Vector3 touch_pos)
    {
        return touch_pos.x < Screen.width / LeftDivider && touch_pos.y < Screen.height / HeightDivider;
    }
}