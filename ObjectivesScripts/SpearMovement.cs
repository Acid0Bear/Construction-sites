using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearMovement : MonoBehaviour
{
    private Controller ThisMController;
    private static SpearMovement M_SpearMovement;
    private Coroutine SpearRoutine;

    private GameObject ConnectedRigidbody;
    [SerializeField] private float SpearSpeed = 20;
    private void Awake()
    {
        ConnectedRigidbody = GetComponent<Collider2D>().attachedRigidbody.gameObject;
    }
    private void Update()
    {
        if (ThisMController == null) { ThisMController = GetComponentInParent<Controller>(); return; }
        if (Controller.M_Controller == ThisMController)
        {
            UtilButton._Util = SpearActivation;
        }
    }

    private void SpearActivation()
    {
        if(SpearRoutine == null)
        {
            ConnectedRigidbody.tag = "Demolisher";
            SpearRoutine = StartCoroutine(Spear());
        }
        else
        {
            ConnectedRigidbody.tag = "Untagged";
            StopCoroutine(SpearRoutine);
            SpearRoutine = null;
        }
    }

    private IEnumerator Spear()
    {
        bool Forward = false;
        while (true)
        {
            if (Forward)
            {
                this.gameObject.transform.localPosition = Vector3.MoveTowards(this.gameObject.transform.localPosition, new Vector3(0.85f, -6.1f, 0), Time.deltaTime * SpearSpeed);
                if (this.gameObject.transform.localPosition.y <= -6.1f)
                    Forward = false;
            }
            else
            {
                this.gameObject.transform.localPosition = Vector3.MoveTowards(this.gameObject.transform.localPosition, new Vector3(0.85f, -5.1f, 0), Time.deltaTime * SpearSpeed);
                if (this.gameObject.transform.localPosition.y >= -5.1f)
                    Forward = true;
            }
            yield return null;
        }
    }
    
}
