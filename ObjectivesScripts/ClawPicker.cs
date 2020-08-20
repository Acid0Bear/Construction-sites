using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawPicker : MonoBehaviour
{
    public bool IsMagnet = false;
    public float ClawFrequency = 2;
    [Range(0.005f,0.5f)]
    public float ClawDistanceDiff = 0.2f;
    [Range(0.005f, 0.5f)]
    public float MagnetDistanceDiff = 0.01f;
    public AudioClip PickUpClip, ReleaseClip;

    private AudioSource FxSource = null;
    private Controller ThisMController;
    private GameObject ObjectInBound = null;
    private GameObject PickedObject;
    private SpringJoint2D joint;
    private LineRenderer Rope;
    private List<Transform> Pivots = new List<Transform>();
    //For configuring offset of point
    private float Offset;
    private Vector2 ContactPoint;
    private void Awake()
    {
        FxSource = gameObject.AddComponent<AudioSource>();
        FxSource.loop = false;
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Objective") && collision.gameObject.GetComponentInParent<SpringJoint2D>() != null && ObjectInBound == null)
        {
            ObjectInBound = collision.gameObject;
        }
    }*/

    private void OnCollisionExit2D(Collision2D collision)
    {
            ObjectInBound = null;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Objective") && collision.gameObject.GetComponentInParent<SpringJoint2D>() != null)
        {
            ObjectInBound = collision.gameObject;
            ContactPoint = collision.transform.InverseTransformPoint(collision.GetContact(0).point);
        }
    }

    private void Update()
    {
        if (ThisMController == null) { ThisMController = GetComponentInParent<Controller>(); return; }
        if(Controller.M_Controller == ThisMController) {
            Rope = GetComponent<LineRenderer>();
            if (PickedObject == null) UtilButton._Util = PickupObject;
            else UtilButton._Util = ReleaseObject;
        }
        if(PickedObject != null)
        {
            if (Rope != null) AdjustRope();
            joint.connectedBody = GetComponentInParent<Rigidbody2D>();
            joint.connectedAnchor = this.gameObject.transform.localPosition;
            joint.distance = (IsMagnet)? MagnetDistanceDiff : ClawDistanceDiff;
            joint.frequency = (IsMagnet) ? 0 : ClawFrequency;
        }
    }

    private void AdjustRope()
    {
        Rope.SetPosition(1, new Vector3(this.transform.position.x, this.transform.position.y + 0.15f, 0));
        if(Pivots.Count == 0)
        {
            var childs = joint.GetComponentsInChildren<Transform>();
            foreach(var Obj in childs)
            {
                if (Obj.gameObject.tag == "RopePivot") Pivots.Add(Obj);
            }
        }
        if(Pivots.Count < 2) { Debug.LogError("Place at least 2 pivots to object you want to pickup!"); return; }
        Rope.SetPosition(0, new Vector3(Pivots[0].position.x, Pivots[0].position.y, 0));
        Rope.SetPosition(2, new Vector3(Pivots[1].position.x, Pivots[1].position.y, 0));
    }

    private void PickupObject()
    {
        if (ObjectInBound == null) return;
        if (Rope != null)  Rope.enabled = true;
        if (ObjectInBound != null) PickedObject = ObjectInBound;
        if (joint == null)
        {
            joint = PickedObject.GetComponent<SpringJoint2D>();
            joint.enabled = true;
            joint.anchor = ContactPoint;
        }
        FxSource.clip = PickUpClip;
        FxSource.volume = PlayerPrefs.GetFloat("CarsVolume");
        UtilButton._Util = ReleaseObject;
    }

    private void ReleaseObject()
    {
        if (Rope != null)  Rope.enabled = false;
        if (joint != null)
        {
            joint.connectedBody = null;
            joint.connectedAnchor = joint.anchor;
            joint.enabled = false;
        }
        Pivots.Clear();
        PickedObject = null;
        ObjectInBound = null;
        joint = null;
        FxSource.clip = ReleaseClip;
        FxSource.volume = PlayerPrefs.GetFloat("CarsVolume");
        UtilButton._Util = PickupObject;
    }
}
