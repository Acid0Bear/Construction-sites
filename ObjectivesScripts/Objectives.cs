using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Objectives
{
    public enum ObjectiveType { MoveTo, Load, Fire, Roll, Demolish, Fill, Balance }; //Balance ext mods

    [Tooltip("Sprites for info")]
    [SerializeField] private Sprite ExecutorSprite = null, TargetSprite = null;

    [Tooltip("Type of the action")]
    public ObjectiveType Objective_Type = ObjectiveType.Load;

    [Tooltip("Who are performing this action? Also used for hint system.")]
    [SerializeField] private Controller Executor = null;

    [Header("Transforms holder. Hover over 'ObjectiveData' for more info.")]
    [Tooltip("If IsPreSpawned not set, drop here spawn point for objects, otherwise drop here prespawned objects transforms. " +
            "NOTE: If you are setting SpawnPoints here, they should be unique for every objective in scene! " +
            "NOTE: This is NOT used in MoveTo objective.")]
    public List<Transform> ObjectiveData = new List<Transform>();

    [DrawIf("Objective_Type", ObjectiveType.MoveTo, DrawIfAttribute.DisablingType.Draw)]
    [Tooltip("Is objective already on scene?")]
    public bool IsPreSpawned = false;

    [DrawIf("Objective_Type", ObjectiveType.MoveTo, DrawIfAttribute.DisablingType.Draw)]
    [Tooltip("What objects are being spawned?")]
    [SerializeField] private GameObject ObjectPrefab = null;

    [DrawIf("Objective_Type", ObjectiveType.MoveTo, DrawIfAttribute.DisablingType.Draw)]
    [Tooltip("How many objects are being spawned?")]
    [SerializeField] private int Quantity = 0;

    #region Load
    [DrawIf("Objective_Type", ObjectiveType.Load)]
    [Tooltip("Container script of the target object")]
    [SerializeField] private Container LoadTo = null;
    [DrawIf("Objective_Type", ObjectiveType.Load)]
    [Tooltip("Should container display shadow?")]
    [SerializeField] private bool EnableShadow = false;
    [DrawIf("Objective_Type", ObjectiveType.Load)]          //Checking after completion
    [Tooltip("Should checking persist after completing?")]  //Checking after completion
    [SerializeField] private bool KeepChecking = false;     //Checking after completion
    #endregion

    #region Demolish
    [DrawIf("Objective_Type", ObjectiveType.Demolish)]
    [Tooltip("Check if damage to block is depends on strength of impact.")]
    [SerializeField] private bool IsVelocityBased = false;
    #endregion

    //Balance ext mods \/
    #region Balance
    [DrawIf("Objective_Type", ObjectiveType.Balance)]
    [Tooltip("Destination of balance objective.")]
    [SerializeField] private Transform Destination = null;
    [DrawIf("Objective_Type", ObjectiveType.Balance)]
    [Tooltip("Balance object prefab")]
    [SerializeField] private BalanceExtension BalanceObj = null;
    [DrawIf("Objective_Type", ObjectiveType.Balance)]
    [Tooltip("Distance difference between destination and object")]
    [SerializeField] private float Distance = 0.5f;
    #endregion

    #region Move
    [DrawIf("Objective_Type", ObjectiveType.MoveTo)]
    [Tooltip("Target location?")]
    [SerializeField] private Transform Target = null;
    #endregion

    private int GetSpawnPos(int CountInList, int iterator)
    {
        return (CountInList - iterator >= 0) ? CountInList - iterator : GetSpawnPos(CountInList, Mathf.Abs(CountInList - iterator));
    }

    public int DisplayArrow(int MaxDistance)
    {
        if (!Executor) { Debug.LogError("Setup Executor in MainObserver objective!"); return 0; }
        switch (this.Objective_Type)
        {
            case ObjectiveType.MoveTo:
                float Diff = Executor.transform.position.x - Target.position.x;
                if (Mathf.Abs(Diff) > MaxDistance)
                    return (Diff > 0) ? 1 : -1;
                else return 0;
            case ObjectiveType.Load:
                Diff = Executor.transform.position.x - LoadTo.transform.position.x;
                if (Mathf.Abs(Diff) > MaxDistance)
                    return (Diff > 0) ? 1 : -1;
                else return 0;
            case ObjectiveType.Fire: case ObjectiveType.Roll: case ObjectiveType.Demolish: case ObjectiveType.Fill:
                foreach (var @obj in ObjectiveData)
                {
                    if (obj == null) continue;
                    Diff = Executor.transform.position.x - obj.position.x;
                    if (Mathf.Abs(Diff) > MaxDistance)
                        return (Diff > 0) ? 1 : -1;
                    else return 0;
                }
                return 0;
            case ObjectiveType.Balance: //Balance ext mods
                Diff = Executor.transform.position.x - Destination.position.x;
                if (Mathf.Abs(Diff) > MaxDistance)
                    return (Diff > 0) ? 1 : -1;
                else return 0;
        }

        Debug.LogError("Unexpeted objective type!(Should not be catched)");
        return 0;
    }

    public bool Check()
    {
        switch (this.Objective_Type)
        {
            case ObjectiveType.MoveTo: 
				if(Executor.transform.position.x > Target.position.x - 0.3f && Executor.transform.position.x < Target.position.x + 0.3)
				{
					Target.gameObject.SetActive(false);
					return true;
				}
				return false;
            case ObjectiveType.Load: return LoadTo.CheckCompletion();

            case ObjectiveType.Fire: case ObjectiveType.Roll: case ObjectiveType.Demolish:
                foreach (Transform @object in ObjectiveData)
                {
                    if (@object != null) return false;
                }
                return true;

            case ObjectiveType.Fill:
                foreach (Transform @object in ObjectiveData)
                {
                    if (@object.localScale.y < 1) return false;
                }
                return true;
            case ObjectiveType.Balance: //Balance ext mods
                if (Vector3.Distance(BalanceObj.transform.position, Destination.position) < Distance && BalanceObj.IsLoaded)
                {
                    if (BalanceObj.Countdown == null)
                        BalanceObj.InitiateCoutDown();
                }
                else
                {
                    if (BalanceObj.Countdown != null)
                        BalanceObj.StopCountDown();
                }
                Destination.gameObject.SetActive(!BalanceObj.CountDownResult);
                return BalanceObj.CountDownResult;
        }

        Debug.LogError("Unexpeted objective type!(Should not be catched)");
        return false;
    }

    public void ActivateObjective()
    {
        switch (this.Objective_Type)
        {
            case ObjectiveType.MoveTo:
                Target.gameObject.SetActive(true);
                return;
            case ObjectiveType.Fire: case ObjectiveType.Roll: case ObjectiveType.Demolish: case ObjectiveType.Load: case ObjectiveType.Balance: //Balance ext mods
                if (!IsPreSpawned)
                {
                    List<Transform> temp = new List<Transform>();
                    for (int i = 1; i <= Quantity; i++)
                    {
                        GameObject obj = GameObject.Instantiate(ObjectPrefab, ObjectiveData[GetSpawnPos(ObjectiveData.Count, i)].position + new Vector3(Random.Range(-0.05f, 0.05f), 0, 0),
                                                Quaternion.identity, ObjectiveData[GetSpawnPos(ObjectiveData.Count, i)].parent);
                        temp.Add(obj.transform);
                    }
                    foreach (var @object in ObjectiveData) GameObject.Destroy(@object.gameObject);
                    ObjectiveData.Clear();
                    ObjectiveData = temp;
                }
                if(Objective_Type == ObjectiveType.Balance)  //Balance ext mods
                {
                    BalanceObj.SetBalanceProps(this, (ExecutorSprite) ? ExecutorSprite : Executor.MachineSprite);
                }
                if (Objective_Type == ObjectiveType.Load)
                {
                    LoadTo.SetContainer(Quantity,ObjectPrefab, this, EnableShadow, KeepChecking);
                }
                if(Objective_Type == ObjectiveType.Demolish)
                {
                    foreach (var @object in ObjectiveData) @object.GetComponent<DemolishExtension>().VelocityBasedDamage = IsVelocityBased;
                }
                return;
            case ObjectiveType.Fill:
                if (!IsPreSpawned)
                {
                    List<Transform> temp = new List<Transform>();
                    for (int i = 1; i <= Quantity; i++)
                    {
                        GameObject obj = GameObject.Instantiate(ObjectPrefab, ObjectiveData[GetSpawnPos(ObjectiveData.Count, i)].position,
                                                Quaternion.identity, ObjectiveData[GetSpawnPos(ObjectiveData.Count, i)].parent);
                        obj.transform.localScale = new Vector3(obj.transform.localScale.x, 0.01f, 1);
                        temp.Add(obj.transform);
                    }
                    foreach (var @object in ObjectiveData) GameObject.Destroy(@object.gameObject);
                    ObjectiveData.Clear();
                    ObjectiveData = temp;
                }
                return;
        }
        Debug.LogError("Unexpeted objective type!(Should not be catched)");
    }

    public void GetInfo(GameObject InfoPanel)
    {
        var Obs = MainObserver.Observer;
        Obs.ClearInfoPanel();
        switch (this.Objective_Type)
        {
            case ObjectiveType.MoveTo:
                Obs.AddTextToInfoPanel("Move");
                Obs.AddImageToInfoPanel((ExecutorSprite) ? ExecutorSprite : Executor.MachineSprite); //Auto setting sprites \/
                Obs.AddTextToInfoPanel("to highlighted area!");
                return;
            case ObjectiveType.Load:
                Obs.AddTextToInfoPanel("Load");
                Obs.AddImageToInfoPanel((ExecutorSprite) ? ExecutorSprite : Executor.MachineSprite);
                Obs.AddTextToInfoPanel("to the");
                Obs.AddImageToInfoPanel((TargetSprite) ? TargetSprite : LoadTo.TargetSprite); //Auto getting sprite from container
                return;
            case ObjectiveType.Fire:
                Obs.AddTextToInfoPanel("Extinguish fire in the area with");
                Obs.AddImageToInfoPanel((ExecutorSprite) ? ExecutorSprite : Executor.MachineSprite);
                return;
            case ObjectiveType.Roll:
                Obs.AddTextToInfoPanel("Roll bolders on scene with");
                Obs.AddImageToInfoPanel((ExecutorSprite) ? ExecutorSprite : Executor.MachineSprite);
                return;
            case ObjectiveType.Demolish:
                Obs.AddTextToInfoPanel("Demolish structure with");
                Obs.AddImageToInfoPanel((ExecutorSprite) ? ExecutorSprite : Executor.MachineSprite);
                return;
            case ObjectiveType.Fill:
                Obs.AddTextToInfoPanel("Fill the area with concrete!");
                Obs.AddImageToInfoPanel((ExecutorSprite) ? ExecutorSprite : Executor.MachineSprite);
                return;
            case ObjectiveType.Balance: //Balance ext mods
                BalanceObj.SetInfoPanel();
                return;
        }
        Debug.LogError("Unexpeted objective type!(Should not be catched)");
        return;
    }

    public IEnumerator HintRountine()
    {
        float BlinkDelay = 0.5f, timer = BlinkDelay;
        float HintDuration = 5f, Switchtimer = HintDuration;
        bool HalfHintDuration = false;
        if(Executor == null)
        {
            Debug.LogError("Select Executor in MainObserver in order to use hints!");
            MainObserver.Observer.IsHintActive = false;
            yield break;
        }
        if (Objective_Type == ObjectiveType.MoveTo) //Balance ext mods
            ProCamera2D.Instance.CameraTargets[0].TargetTransform = Executor.transform;
        else if (Objective_Type == ObjectiveType.Balance && BalanceObj.IsLoaded)
            ProCamera2D.Instance.CameraTargets[0].TargetTransform = Destination;
        else if (Objective_Type == ObjectiveType.Balance && !BalanceObj.IsLoaded)
            ProCamera2D.Instance.CameraTargets[0].TargetTransform = BalanceObj.ObjectsList[0];
        else if (ObjectiveData.Count != 0)
            ProCamera2D.Instance.CameraTargets[0].TargetTransform = ObjectiveData[0];
        while (true){
            timer -= Time.deltaTime;
            Switchtimer -= Time.deltaTime;
            if(timer <= 0)
            {
                if(this.Objective_Type != ObjectiveType.MoveTo)
                {
                    foreach(var @obj in (this.Objective_Type != ObjectiveType.Balance) ? ObjectiveData : BalanceObj.ObjectsList) //Balance ext mods
                    {
                        if (obj == null) continue;
                        SpriteRenderer ObjSprite;
                        if((ObjSprite = obj.GetComponentInChildren<SpriteRenderer>()) == null) continue;
                        if(ObjSprite.color.a == 1) {
                            ObjSprite.color = new Color(ObjSprite.color.r, ObjSprite.color.g, ObjSprite.color.b, 0.5f);
                        }
                        else
                        {
                            ObjSprite.color = new Color(ObjSprite.color.r, ObjSprite.color.g, ObjSprite.color.b, 1f);
                        }
                    }
                }
                var ExecutorBodySprite = Executor.GetComponentInChildren<SpriteRenderer>();
                if (ExecutorBodySprite.color.a == 1)
                {
                    ExecutorBodySprite.color = new Color(ExecutorBodySprite.color.r, ExecutorBodySprite.color.g, ExecutorBodySprite.color.b, 0.5f);
                }
                else
                {
                    ExecutorBodySprite.color = new Color(ExecutorBodySprite.color.r, ExecutorBodySprite.color.g, ExecutorBodySprite.color.b, 1f);
                }
                if(Objective_Type == ObjectiveType.Load && LoadTo.TargetGraphic)
                {
                    if (LoadTo.TargetGraphic.color.a == 1)
                        LoadTo.TargetGraphic.color = new Color(LoadTo.TargetGraphic.color.r,
                                                            LoadTo.TargetGraphic.color.g,
                                                            LoadTo.TargetGraphic.color.b, 0.5f);
                    else
                        LoadTo.TargetGraphic.color = new Color(LoadTo.TargetGraphic.color.r,
                                                            LoadTo.TargetGraphic.color.g,
                                                            LoadTo.TargetGraphic.color.b, 1f);
                }
                if(Objective_Type == ObjectiveType.Balance && BalanceObj.TargetGraphic && !BalanceObj.IsLoaded) //Balance ext mods
                {
                    if (BalanceObj.TargetGraphic.color.a == 1)
                        BalanceObj.TargetGraphic.color = new Color(BalanceObj.TargetGraphic.color.r,
                                                            BalanceObj.TargetGraphic.color.g,
                                                            BalanceObj.TargetGraphic.color.b, 0.5f);
                    else
                        BalanceObj.TargetGraphic.color = new Color(BalanceObj.TargetGraphic.color.r,
                                                            BalanceObj.TargetGraphic.color.g,
                                                            BalanceObj.TargetGraphic.color.b, 1f);
                }
                timer = BlinkDelay;
            }

            if(!HalfHintDuration && Switchtimer <= HintDuration / 2)
            {
                if (Objective_Type == ObjectiveType.Load)
                    ProCamera2D.Instance.CameraTargets[0].TargetTransform = LoadTo.transform;
                else if(Objective_Type == ObjectiveType.Balance && !BalanceObj.IsLoaded)
                    ProCamera2D.Instance.CameraTargets[0].TargetTransform = BalanceObj.transform;
                else if(Objective_Type == ObjectiveType.MoveTo)
                    ProCamera2D.Instance.CameraTargets[0].TargetTransform = Target;
                else
                    ProCamera2D.Instance.CameraTargets[0].TargetTransform = Executor.transform;
            }

            if (Switchtimer <= 0)
            {
                MainObserver.Observer.SetCameraTargetOnCurrentVehicle();
                MainObserver.Observer.IsHintActive = false;
                if (this.Objective_Type != ObjectiveType.MoveTo)
                    foreach (var @obj in (this.Objective_Type != ObjectiveType.Balance) ? ObjectiveData : BalanceObj.ObjectsList)
                    {
                        var Sprite = obj.GetComponentInChildren<SpriteRenderer>();
                            Sprite.color = new Color(Sprite.color.r, Sprite.color.g, Sprite.color.b, 1f);
                    }
                if (Objective_Type == ObjectiveType.Load && LoadTo.TargetGraphic)
                    LoadTo.TargetGraphic.color = new Color(LoadTo.TargetGraphic.color.r,
                                                            LoadTo.TargetGraphic.color.g,
                                                            LoadTo.TargetGraphic.color.b, 1f);
                var ExecutorBodySprite = Executor.GetComponentInChildren<SpriteRenderer>();
                    ExecutorBodySprite.color = new Color(ExecutorBodySprite.color.r, ExecutorBodySprite.color.g, ExecutorBodySprite.color.b, 1f);
                yield break;
            }
            else
                yield return null;
        }
    }
}