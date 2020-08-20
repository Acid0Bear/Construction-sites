using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemolishExtension : MonoBehaviour
{
    //[SerializeField] private bool IsMultiblock = false;
    [Header("If set to true, DamagePerTick is multiplied by relative velocity")]
    public bool VelocityBasedDamage = false;
    [SerializeField] private float MaxHitPoints = 100;
    [SerializeField] private float DamagePerTick = 1;
    [Header("Sprites from 0(Destroyed) to N(Healthy)")]
    [SerializeField] private List<Sprite> DamageLevelSprites = new List<Sprite>(); 


    private float CurHitPoints = 0;
    private float HpStep;
    private void Start()
    {
        CurHitPoints = MaxHitPoints;
        if(DamageLevelSprites.Count == 0) { Debug.LogError("Assign break sprites to object you want to demolish!"); Destroy(this); return; }
        HpStep = MaxHitPoints / DamageLevelSprites.Count;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.tag == "Demolisher")
        {
            
            if (VelocityBasedDamage)
            {
                CurHitPoints -= DamagePerTick * collision.relativeVelocity.magnitude;
                int curDamageLevel = DamageLevelSprites.Count - Mathf.RoundToInt(CurHitPoints / HpStep);
                if (curDamageLevel == DamageLevelSprites.Count) Destroy(gameObject);
                else if (DamageLevelSprites.Count != 0)
                {
                    GetComponent<SpriteRenderer>().sprite = DamageLevelSprites[curDamageLevel];
                }
            }
            else
            {
                CurHitPoints -= DamagePerTick;
                int curDamageLevel = DamageLevelSprites.Count - Mathf.RoundToInt(CurHitPoints/HpStep);
                if (curDamageLevel == DamageLevelSprites.Count) Destroy(gameObject);
                else if(DamageLevelSprites.Count != 0)
                {
                    GetComponent<SpriteRenderer>().sprite = DamageLevelSprites[curDamageLevel];
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Demolisher")
        {
            Debug.Log(CurHitPoints);
            if (!VelocityBasedDamage)
            {
                CurHitPoints -= DamagePerTick;
                int curDamageLevel = DamageLevelSprites.Count - Mathf.RoundToInt(CurHitPoints / HpStep);
                if (curDamageLevel == DamageLevelSprites.Count) Destroy(gameObject);
                else if (DamageLevelSprites.Count != 0)
                {
                    GetComponent<SpriteRenderer>().sprite = DamageLevelSprites[curDamageLevel];
                }
            }
        }
    }
}
