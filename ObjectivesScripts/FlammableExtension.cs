using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlammableExtension : MonoBehaviour
{
    private Rigidbody2D rb;
    private float cooldown = 0;

    [Header("Holder of HitPoints")]
    [SerializeField] GameObject HitPoints = null;
    [Header("Object that indicates damage")]
    [SerializeField] GameObject Damage = null;
    [Header("Limit for collision with objects")]
    [SerializeField] int ColBorder = 5;
    [Header("Limit for barrel velocity with objects")]
    [SerializeField] int VelocityBorder = 3;
    [Header("Base damage")]
    [SerializeField] float BaseDamage = 0.1f;
    [Header("Cooldown to receive velocity damage")]
    [SerializeField] float BaseCoolDown = 0.3f;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.relativeVelocity.magnitude > ColBorder)
        {
            HitPoints.SetActive(true);
            Damage.transform.localScale -= new Vector3( 0, BaseDamage * collision.relativeVelocity.magnitude, 0);
            if(Damage.transform.localScale.y <= 0.01f)
            {
                UtilButton._Util?.Invoke();
                Debug.LogWarning("Barrel is destroyed!");
                Destroy(rb.gameObject);
            }
        }
    }

    private void Update()
    {
        cooldown -= (cooldown > 0) ? Time.deltaTime : 0;
        if(rb.velocity.magnitude > VelocityBorder && cooldown <= 0)
        {
            HitPoints.SetActive(true);
            Damage.transform.localScale -= new Vector3(0, BaseDamage * rb.velocity.magnitude, 0);
            if (Damage.transform.localScale.y <= 0.01f)
            {
                UtilButton._Util?.Invoke();
                Debug.LogWarning("Barrel is destroyed!");
                Destroy(rb.gameObject);
                MainObserver.Observer.LevelFailed("Exploded!");
            }
            cooldown = BaseCoolDown;
        }
    }
}
