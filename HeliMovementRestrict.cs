using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliMovementRestrict : MonoBehaviour
{
    [SerializeField] public int LockingValue;
    bool isApplied = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isApplied)
        {
            GetComponentInParent<HeliController>().LockedDir += LockingValue;
            isApplied = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isApplied)
        {
            GetComponentInParent<HeliController>().LockedDir -= LockingValue;
            isApplied = false;
        }
    }
}
