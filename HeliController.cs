using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliController : Controller
{
    [SerializeField] private GameObject Heli = null;
    [SerializeField] private GameObject backRotor = null;
    [SerializeField] private float RotorSpeed = 0;
    [SerializeField] private float DistanceCap = 50;
    [SerializeField] private float HeliSpeedUpCoef = 0.1f;

    public int LockedDir {  get; set; }

    private float LastXDirection = 0;
    private new float VehicleMaxSpeed = 0;
    public float HeliSpeed { get; private set; }
    private bool isGrounded = false;

    private bool isReturning = false;
    private Vector3 InitialPos;
    private void Awake()
    {
        //Reseting object rotation
        gameObject.transform.rotation = Quaternion.Euler(0, TargetRotation, 0);
        VehicleMaxSpeed = MovSpeed;
    }
    void Start()
    {
        //Getting rigidbody from GameObject
        rb = GetComponent<Rigidbody2D>();
        //Assigning center of mass
        rb.centerOfMass = CenterOfMass.localPosition;
        //Assign Wheels Pivot
        SetupPivotsAndAngles();
        InitialPos = this.transform.localPosition;
    }

    private void Update()
    {
        RotateRotor();
        for (int i = 0; i < Parts.Count; i++)
        {
            ControlVisualRopes(i);
        }
        if(Vector3.Distance(this.transform.localPosition,InitialPos) > DistanceCap || isReturning)
        {
            isReturning = true;
            this.transform.localPosition = Vector3.MoveTowards(transform.localPosition, InitialPos, Time.deltaTime * (MovSpeed));
            if (Vector3.Distance(this.transform.localPosition, InitialPos) <= 1)
                isReturning = false;
        }
    }

    public override void M_Manage()
    {
        for (int i = 0; i < Parts.Count; i++)
        {
            //Avoiding invertion of YAxis due to initial rotation
            if (Parts[i].YAxis && Parts[i].Part is HingeJoint2D && TargetRotation == 0) { MoveParts(i, YParam * -1); continue; }
            else if (Parts[i].YAxis) { MoveParts(i, YParam); continue; }
            else if (Parts[i].XAxis) { MoveParts(i, XParam); continue; }
            else if (Parts[i].AdditionalParam) { MoveParts(i, AdditionalParam); continue; }
        }
        if(!isReturning)
        Move();
    }

    private bool LastDirectionState()
    {
        if (LastXDirection > 0 && XDirectional > 0)
            return false;
        else if (LastXDirection < 0 && XDirectional < 0)
            return false;
        else
            return true;
    }

    private void Move()
    {
        if (LastDirectionState() || XDirectional == 0)
            MovSpeed = VehicleMaxSpeed / 4;
        LastXDirection = XDirectional;
        Vector3 Dir = BuildDir(-XDirectional, -YDirectional);
        Debug.Log(-XDirectional);
        foreach (WheelJoint2D wheel in Wheels)
        {
            var Collider = wheel.gameObject.GetComponentInChildren<CircleCollider2D>();
            if (Collider.IsTouchingLayers()) { if (Dir.y < 0) { Dir.y = 0; Dir.x = 0; } rb.bodyType = RigidbodyType2D.Dynamic; isGrounded = true; break; }
            else { rb.bodyType = RigidbodyType2D.Kinematic; rb.Sleep(); isGrounded = false; }
        }
        if (XDirectional != 0 && !isGrounded)
        {
            float CurRot = Convert360to180(transform.rotation.eulerAngles.z);
            if ((XDirectional < 0 ||CurRot <= 9) && (CurRot >= -9 || XDirectional > 0))
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, CurRot + XDirectional), Time.deltaTime * MovSpeed * 10);
        }
        else if (XDirectional == 0 && !isGrounded)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 15);
        MovSpeed = Mathf.Lerp(MovSpeed, VehicleMaxSpeed, HeliSpeedUpCoef);
        Vector3 NewPos = Vector3.MoveTowards(transform.position, transform.position + Dir, Time.deltaTime * MovSpeed);
        HeliSpeed = Vector3.Magnitude(NewPos - transform.position) * 3333;
        transform.position = NewPos;
    }

    private Vector3 BuildDir(float XDir, float YDir)
    {
        if( XDir < 0)
        {
            if (LockedDir >= 8)
                XDir = 0;
        }
        else
        {
            if (LockedDir == 2 || LockedDir == 6 || LockedDir == 10 || LockedDir == 14)
                XDir = 0;
        }
        if(YDir > 0)
        {
            if (LockedDir == 4 || LockedDir == 6 || LockedDir == 12 || LockedDir == 14)
                XDir = 0;
        }
        return new Vector3(XDir, YDir, 0);
    }

    //Creating visual rotation of helices
    private void RotateRotor()
    {
        float CurRot = Heli.transform.localRotation.eulerAngles.y;
        Heli.transform.localRotation = Quaternion.RotateTowards(Heli.transform.localRotation, Quaternion.Euler(0, CurRot + 50, 0), Time.deltaTime * RotorSpeed);
        CurRot = backRotor.transform.localRotation.eulerAngles.z;
        backRotor.transform.localRotation = Quaternion.RotateTowards(backRotor.transform.localRotation, Quaternion.Euler(0, 0, CurRot + 50), Time.deltaTime * RotorSpeed);
    }

    private float Convert360to180(float value)
    {
        if (value < 180) return value;
        else return value - 360;
    }
}
