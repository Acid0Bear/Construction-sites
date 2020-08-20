using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadRollerExtension : MonoBehaviour
{

    [SerializeField] private Rigidbody2D Rb = null;
    [Range(0.005f,0.1f)]
    [SerializeField] private float ImpactPerTick = 0.05f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rollable") && Rb.velocity.x > 0.15f)
        {
            collision.gameObject.transform.localScale -= new Vector3(ImpactPerTick, ImpactPerTick, 0);
            if (collision.gameObject.transform.localScale.x < 0.4f)
                Destroy(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rollable") && Rb.velocity.x > 0.15f)
        {
            collision.gameObject.transform.localScale -= new Vector3(ImpactPerTick, ImpactPerTick, 0);
            if (collision.gameObject.transform.localScale.x < 0.4f)
                Destroy(collision.gameObject);
        }
    }
}
