using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FireTruckExtension : MonoBehaviour
{
    private ParticleSystem part;
    [Range(0.001f, 0.01f)]
    [SerializeField] private float ImpactPerTick = 0.003f;
    private Controller VehicleController = null;
    void Awake()
    {
        part = GetComponent<ParticleSystem>();
        if (!GetComponent<Collider2D>()) UtilButton._Util = WaterThrowerState;
        VehicleController = GetComponentInParent<Controller>();
    }

    private void Update()
    {
        if (this.tag == "Fire") return;
        if (VehicleController == null) { Debug.LogError("Vehicle controller not found! On " + this.gameObject); Destroy(this); return; }
        if (Controller.M_Controller == VehicleController && UtilButton._Util != WaterThrowerState) UtilButton._Util = WaterThrowerState;
    }

    void WaterThrowerState()
    {
        if (part.emission.rateOverTimeMultiplier != 0)
        {
            var module = part.emission;
            module.rateOverTimeMultiplier = 0;
        }
        else
        {
            var module = part.emission;
            module.rateOverTimeMultiplier = 15;
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Fire")
        {
            if (other.GetComponent<FireTruckExtension>() == null) { var script = other.AddComponent<FireTruckExtension>(); script.ImpactPerTick = ImpactPerTick; return; }
            other.GetComponent<FireTruckExtension>().ExtinguishFire();
        }
    }

    void ExtinguishFire()
    {
        part.Emit(1);
        if (this.transform.localScale.x > 0.2f)
            this.transform.localScale -= new Vector3(ImpactPerTick, ImpactPerTick, 0);
        else
        {
            Debug.LogWarning("FireExtinguished!");
            Destroy(this.gameObject);
        }
    }
}
