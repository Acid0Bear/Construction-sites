using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ParticleSystem))]
public class FillExtension : MonoBehaviour
{
    private ParticleSystem part;
    [Range(0.001f, 0.01f)]
    [SerializeField] private float ImpactPerTick = 0.003f;
    [SerializeField] private Controller VehicleController = null;
    [SerializeField] private List<Sprite> SpritesForContainer = new List<Sprite>();
    [SerializeField] private SpriteRenderer Container = null;
    [Range(0.1f, 1f)]
    [SerializeField] private float DelayBetweenSwaping = 1;
    void Awake()
    {
        part = GetComponent<ParticleSystem>();
        UtilButton._Util = ConcreteFlowState;
        StartCoroutine(RotateContainer());
    }

    IEnumerator RotateContainer()
    {
        int CurSprite = 0;
        float timer = 0;
        while (true)
        {
            timer += Time.deltaTime;
            if(timer >= DelayBetweenSwaping)
            {
                CurSprite = (CurSprite + 1 != SpritesForContainer.Count) ? CurSprite + 1 : 0;
                Container.sprite = SpritesForContainer[CurSprite];
                timer = 0;
            }
            yield return null;
        }
    }

    private void Update()
    {
        if (VehicleController == null) { Debug.LogError("Setup vehicle controller in objective parameters!"); Destroy(this); return; }
        if(Controller.M_Controller == VehicleController && UtilButton._Util != ConcreteFlowState) UtilButton._Util = ConcreteFlowState;
    }

    void ConcreteFlowState()
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
        if (other.tag == "Fill")
        {
            other.transform.localScale += (other.transform.localScale.y < 1) ? new Vector3(0,ImpactPerTick,0): Vector3.zero;
        }
    }
}
