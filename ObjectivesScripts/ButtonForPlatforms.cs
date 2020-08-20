using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonForPlatforms : MonoBehaviour
{
    private bool IsPressed = false;
    private Vector3 Initial = Vector3.zero;
    private Vector3 Pressed = Vector3.zero;


    [SerializeField] private GameObject ConnectedPlatform = null; //Mod for compensator
    [SerializeField] private float PlatformSpeed = 5; //Mod for compensator
    [SerializeField] private bool CompensateWhenVehicleIsMoving = false; //Mod for compensator
    private Vector3 NewPosForPlatform = Vector3.zero;
    private Vector3 DisabledPosForPlatform = Vector3.zero;
    private PlatformMovementCompensator PlatformCompensator; //Mod for compensator
    private bool IsMovingDown, IsMovingUp;
    private PolygonCollider2D PlatformCollider;
    private void Awake()
    {
        Initial = this.transform.localPosition;
        Pressed = this.transform.localPosition - new Vector3(0, 1.7f, 0);
        if (!ConnectedPlatform.GetComponentInChildren<Transform>()) { Debug.LogError("Set second position for platform! (Add empty GameObject to platfrom as child)"); Destroy(this); return; }
        NewPosForPlatform = ConnectedPlatform.GetComponentsInChildren<Transform>()[1].position;
        DisabledPosForPlatform = ConnectedPlatform.transform.position;
        PlatformCollider = ConnectedPlatform.GetComponent<PolygonCollider2D>(); //Mod for compensator \/
        PlatformCompensator = ConnectedPlatform.GetComponent<PlatformMovementCompensator>();
        PlatformCompensator.Speed = PlatformSpeed;
        PlatformCompensator.CompensateWhenVehicleIsMoving = CompensateWhenVehicleIsMoving;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        IsPressed = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        IsPressed = false;
    }

    private void Update()
    {
        if (IsPressed || IsMovingDown)
        {
            if (!ConnectedPlatform.GetComponent<Collider2D>()) { Debug.LogError("No collider found on platform! Button is operatable"); return; }
            PlatformCollider.enabled = true; //Mod for compensator \/
            PlatformCompensator.Target = (Vector2)NewPosForPlatform;
            //ConnectedPlatform.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 1);
            if (IsMovingDown = this.transform.localPosition != Pressed)
                this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, Pressed, Time.deltaTime * 5);
            else
                IsMovingDown = false;
                
        }
        else if(!IsPressed || IsMovingUp)
        {
            if (!ConnectedPlatform.GetComponent<Collider2D>()) { Debug.LogError("No collider found on platform! Button is operatable"); return; }
            PlatformCollider.enabled = false; //Mod for compensator \/
            PlatformCompensator.Target = (Vector2)DisabledPosForPlatform;
            //ConnectedPlatform.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 0.5f);
            if (IsMovingUp = this.transform.localPosition != Initial)
                this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, Initial, Time.deltaTime * 5);
            else
                IsMovingUp = false;
        }
    }
}
