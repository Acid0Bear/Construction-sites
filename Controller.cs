using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class Controller : MonoBehaviour
{
    public enum Orientation { Left, Right};
    public delegate void _SwitchSides();
    public static _SwitchSides SwitchSides { get; set; }

    [HideInInspector]
    public static Controller M_Controller;
    public Sprite MachineSprite = null; //Added sprite
    [Header("Machine Parameters")]
    [SerializeField] protected float MovSpeed = 200;
    [SerializeField] protected float PartsMovementSpeed = 10;
    [SerializeField] protected Orientation InitialFacing = Orientation.Right;
    protected float TargetRotation { get => this.transform.rotation.eulerAngles.y; }
    [SerializeField] protected List<WheelJoint2D> Wheels = new List<WheelJoint2D>();
    [Header("Leave field below empty if vehicle one-sided")]
    [SerializeField] public List<Controller> Sides = new List<Controller>();
    [Header("MovingParts")]
    [SerializeField] protected List<M_Parts> Parts = new List<M_Parts>();
    [Header("Rigidbody config")]
    [SerializeField] protected Transform CenterOfMass = null;
    [HideInInspector] protected Rigidbody2D rb = null;
    [HideInInspector] public float VehicleMaxSpeed => MovSpeed;

    //Inputs
    [HideInInspector]
    public float YParam, XParam, AdditionalParam, Directional, YDirectional, XDirectional;

    private void Awake()
    {
        //Reseting object rotation
        gameObject.transform.rotation = Quaternion.Euler(0, TargetRotation, 0);
    }

    void Start()
    {
        //Getting rigidbody from GameObject
        rb = GetComponent<Rigidbody2D>();
        //Assigning center of mass
        rb.centerOfMass = CenterOfMass.localPosition;
        //Assign Wheels Pivot
        SetupPivotsAndAngles();
        //Set sides delegate
        SwitchSides = SwitchSidesProc;
    }

    public virtual bool HasAdditionalDependance()
    {
        foreach(var Part in Parts)
        {
            if (Part.AdditionalParam) return true;
        }
        return false;
    }

    private void Update()
    {
        for (int i = 0; i < Parts.Count; i++)
        {
            ControlVisualRopes(i);
        }
    }

    public int GetOrientation()
    {
        return (TargetRotation == 0) ? 1:-1;
    }

    public int GetFacingDir()
    {
        return (InitialFacing == Orientation.Right) ? GetOrientation() : GetOrientation() * -1;
    }

    private void SwitchSidesProc()
    {
        if (Sides.Count <= 1) return;
        else
        {
            if(Sides[0] == M_Controller) { M_Controller = Sides[1]; return; }
            else { M_Controller = Sides[0]; return; }
        }
    }

    public void SetupPivotsAndAngles()
    {
        //1 means original (Prefab state), -1 means reversed state
        int State = GetOrientation();
        foreach (WheelJoint2D Wheel in Wheels)
        {
            Wheel.connectedAnchor = new Vector2(Wheel.transform.localPosition.x*State, Wheel.transform.localPosition.y);
        }
        foreach(M_Parts Part in Parts)
        {
            if(Part.Part is HingeJoint2D)
            {
                var Limits = ((HingeJoint2D)Part.Part).limits;
                if(State == -1)
                {
                    var _Max = Limits.max;
                    Limits.max = Limits.min * State;
                    Limits.min = _Max * State;
                }
                ((HingeJoint2D)Part.Part).limits = Limits;
            }
            if(Part.Part is SliderJoint2D && ((SliderJoint2D)Part.Part).angle != 90)
            {
                ((SliderJoint2D)Part.Part).angle *= State;
                var Limits = ((SliderJoint2D)Part.Part).limits;
                if (State == -1)
                {
                    var _Max = Limits.max;
                    Limits.max = Limits.min;
                    Limits.min = _Max * State;
                }
                ((SliderJoint2D)Part.Part).limits = Limits;
            }
            if (Part.Part is SpringJoint2D)
            {
                var anchor = ((SpringJoint2D)Part.Part).connectedAnchor;
                if (State == -1)
                {
                    anchor.x *= State;
                    ((SpringJoint2D)Part.Part).connectedAnchor = anchor;
                }
            }
        }
    }
    
    //Function which operates with parts if (Parts[PartID].Part is HingeJoint2D && TargetRotation == 0)
    public virtual void M_Manage()
    {
        for(int i = 0;i<Parts.Count;i++)
        {
            //Avoiding invertion of YAxis due to initial rotation
            if (Parts[i].YAxis && Parts[i].Part is HingeJoint2D && TargetRotation == 0) { MoveParts(i, YParam*-1); continue; }
            else if(Parts[i].YAxis) { MoveParts(i, YParam); continue; }
            else if(Parts[i].XAxis) { MoveParts(i, XParam); continue; }
            else if (Parts[i].AdditionalParam) { MoveParts(i, AdditionalParam); continue; }
        }
        Move(Directional);
    }

    //If vehicles is not focused, movement is canceled
    public virtual void StopVehicle()
    {
        for (int i = 0; i < Parts.Count; i++)
        {
            //Avoiding invertion of YAxis due to initial rotation
            if (Parts[i].YAxis && Parts[i].Part is HingeJoint2D && TargetRotation == 0) { MoveParts(i, 0 * -1); continue; }
            else if (Parts[i].YAxis) { MoveParts(i, 0); continue; }
            else if (Parts[i].XAxis) { MoveParts(i, 0); continue; }
            else if (Parts[i].AdditionalParam) { MoveParts(i, 0); continue; }
        }
        Move(0);
    }

    //Main function to rotate moving parts
    public void MoveParts(int PartID, float value)
    {
        //Setting up motor speed according to parts movement speed
        if (Parts[PartID].Part is SpringJoint2D)
        {
            if (value == 0 || !Parts[PartID].Part.isActiveAndEnabled) return;
            var collider = Parts[PartID].Part.GetComponentInChildren<PolygonCollider2D>();
            if (!collider.IsTouchingLayers(LayerMask.GetMask("Default", "M_Parts&Body", "M_Wheels", "M_Misc")))
                ((SpringJoint2D)Parts[PartID].Part).distance += (value * Parts[PartID].AdditionalPartSpeed);
            else if(value < 0)
                ((SpringJoint2D)Parts[PartID].Part).distance += (value * Parts[PartID].AdditionalPartSpeed);
            if (((SpringJoint2D)Parts[PartID].Part).distance < 0.25f) ((SpringJoint2D)Parts[PartID].Part).distance = 0.25f;
        }
        else
        {
            if (Parts[PartID].state == JointLimitState2D.UpperLimit || Parts[PartID].state == JointLimitState2D.LowerLimit)
            {
                if (!Parts[PartID].IsHittedLimit)
                {
                    Parts[PartID].IsHittedLimit = true;
                    try { GetComponentInChildren<EngineSoundManager>().PartIsAtCapSound(); }
                    catch { Debug.LogWarning("No EngineSoundManager found!"); }
                } 
            }
            else
                Parts[PartID].IsHittedLimit = false;
            var motor = Parts[PartID].motor;
            motor.motorSpeed = (value * PartsMovementSpeed * Parts[PartID].AdditionalPartSpeed);
            Parts[PartID].motor = motor;
        }
    }

    //Ropes are has to be controlled even if vehicle is not focused
    protected void ControlVisualRopes(int PartID)
    {
        if (Parts[PartID].Part is SpringJoint2D)
            Parts[PartID].LineRendererForClaw.SetPosition(Parts[PartID].PointID, new Vector3(Parts[PartID].Part.transform.localPosition.x, Parts[PartID].Part.transform.localPosition.y, 0));
    }

    //Moving the machine
    private void Move(float Direction)
    {
        //Iterating through wheels and setting motor speed according to machine speed and Direction
        foreach(WheelJoint2D Wheel in Wheels)
        {
            var motor = Wheel.motor;
            motor.motorSpeed = Direction * MovSpeed;
            Wheel.motor = motor;
        }
    }
}
//Holder class for parts
[System.Serializable]
public class M_Parts
{
    public Joint2D Part;
    [Range(-10,10)]
    public float AdditionalPartSpeed = 1;
    [DrawIf("Part", typeof(SpringJoint2D))]
    public LineRenderer LineRendererForClaw;
    [DrawIf("Part", typeof(SpringJoint2D))]
    public int PointID;
    [HideInInspector]
    public bool IsHittedLimit;
    [HideInInspector]
    public JointLimitState2D state { get => GetLimitState(); }
    [HideInInspector]
    public JointMotor2D motor { get => GetMotor(); set => SetMotor(value); }
    private JointMotor2D GetMotor()
    {
        if (Part is HingeJoint2D)
            return ((HingeJoint2D)Part).motor;
        else if (Part is SliderJoint2D)
            return ((SliderJoint2D)Part).motor;
        else
        {
            Debug.LogError("Unexpected joint!");
            return new JointMotor2D();
        }
    }
    private void SetMotor (JointMotor2D motor)
    {
        if (Part is HingeJoint2D)
            ((HingeJoint2D)Part).motor = motor;
        else if (Part is SliderJoint2D)
            ((SliderJoint2D)Part).motor = motor;
        else
        {
            Debug.LogError("Unexpected joint!");
        }
    }
    private JointLimitState2D GetLimitState()
    {
        if (Part is HingeJoint2D)
            return ((HingeJoint2D)Part).limitState;
        else if (Part is SliderJoint2D)
            return ((SliderJoint2D)Part).limitState;
        else
        {
            Debug.LogError("Unexpected joint!");
            return new JointLimitState2D();
        }
    }
    [Header("Driven by:")]
    public bool YAxis;
    public bool XAxis, AdditionalParam;
}
