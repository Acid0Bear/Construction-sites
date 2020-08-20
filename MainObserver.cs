using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainObserver : MonoBehaviour
{
    public static MainObserver Observer;

    [Header("Misc for objectives")]
    public int FontSizeForInfoPanel = 50;
    public TMP_FontAsset FontForInfoPanel = null;
    public GameObject CountDown;
    [SerializeField] private GameObject AdditionalParamHolder = null;
    [SerializeField] private GameObject DirectionalBase = null;
    [SerializeField] private GameObject DirectionalHeli = null;
    [SerializeField] private GameObject SwitchSidesHolder = null;
    [SerializeField] private GameObject UtilButtonHolder = null;
    [SerializeField] private GameObject InfoPanel = null;
    [SerializeField] private int DistanceToShowArrow = 5;
    [SerializeField] private GameObject LeftArrow = null, RightArrow = null;

    //[SerializeField] private Transform SpawnPoint = null;
    [Header("Handler of all objectives!")]
    [SerializeField] private List<Objectives> Objectives = new List<Objectives>();
    [Header("Vehicles handler!")]
    [SerializeField] private List<Controller> Vehicles = new List<Controller>();
    

    [HideInInspector]
    public bool IsHintActive = false;
    [HideInInspector]
    public Objectives ActiveObjective { get => (!IsLevelFinished)?Objectives[CurObjective]:null; }

    private GameObject M_Cur;
    private List<GameObject> InfoPanelPieces = new List<GameObject>();
    private int CurObjective;
    private int CurVehicle = 0;
    private bool IsLevelFinished = false;
    private void Awake()
    {
        if (Observer == null) { Observer = this; }
        else { Destroy(Observer.gameObject); Observer = this; }

        if (Vehicles.Count == 0) { Debug.LogError("No vehicles assigned!"); return; }
        Controller.M_Controller = Vehicles[0];
        ProCamera2D.Instance.CameraTargets[0].TargetTransform = Vehicles[CurVehicle].transform;

        if (Objectives.Count == 0) return;
        /*foreach (Objectives Objective in Objectives)
            if (Objective.Objective_Type != global::Objectives.ObjectiveType.MoveTo)
                Objective.ActivateObjective();*/
        Objectives[CurObjective].ActivateObjective();
        Objectives[CurObjective].GetInfo(InfoPanel);
        /*if (Objectives[CurObjective].Objective_Type == global::Objectives.ObjectiveType.MoveTo)
            Objectives[CurObjective].ActivateObjective();*/
        CheckDependence();

        //Arrow blinking
        StartCoroutine(BlinkArrow(LeftArrow));
        StartCoroutine(BlinkArrow(RightArrow));
    }
    private void FixedUpdate()
    {
        if (!Controller.M_Controller) { Debug.Log("M_Controller not found!"); return; }
        else
        {
            Controller.M_Controller.M_Manage();
            if (IsLevelFinished || Objectives.Count == 0) return;

            switch (Objectives[CurObjective].DisplayArrow(DistanceToShowArrow))
            {
                case -1:
                    LeftArrow.SetActive(false);
                    RightArrow.SetActive(true);
                    break;
                case 1:
                    LeftArrow.SetActive(true);
                    RightArrow.SetActive(false);
                    break;
                case 0:
                    LeftArrow.SetActive(false);
                    RightArrow.SetActive(false);
                    break;
            }

            if (Objectives[CurObjective].Check()) { Debug.Log("Objective Completed!"); NextObjective(); }

            if (Vehicles[CurVehicle].transform.rotation.eulerAngles.z > 90 && Vehicles[CurVehicle].transform.rotation.eulerAngles.z < 180)
                LevelFailed("Crashed!");
        }
        if (UtilButton._Util != null) UtilButtonHolder.SetActive(true);
        else UtilButtonHolder.SetActive(false);
    }

    //Used for activation or deactivation of UIElements
    private void CheckDependence()
    {
        if (Vehicles[CurVehicle].HasAdditionalDependance()) AdditionalParamHolder.SetActive(true);
        else AdditionalParamHolder.SetActive(false);
        if (Vehicles[CurVehicle].Sides.Count == 0) SwitchSidesHolder.SetActive(false);
        else SwitchSidesHolder.SetActive(true);
        if(Vehicles[CurVehicle] is HeliController) { DirectionalBase.SetActive(false); DirectionalHeli.SetActive(true); }
        else { DirectionalBase.SetActive(true); DirectionalHeli.SetActive(false); }
    }  

    public void SwitchVehicle()
    {
        if (IsHintActive) return;
        CurVehicle = (CurVehicle + 1 < Vehicles.Count) ? CurVehicle + 1 : 0;
        Controller.M_Controller.StopVehicle();
        Controller.M_Controller = Vehicles[CurVehicle];
        CheckDependence();
        UtilButton._Util = null;
        ProCamera2D.Instance.CameraTargets[0].TargetTransform = Vehicles[CurVehicle].transform;
    }

    private IEnumerator BlinkArrow(GameObject Arrow)
    {
        float BlinkDelay = 0.5f, timer = BlinkDelay;
        Image renderer;
        if((renderer = Arrow.GetComponent<Image>()) == null) { Debug.LogError("Arrow does not have Image component on it!"); yield break; }
        while (true)
        {
            timer -= Time.deltaTime;
            if(timer <= 0)
            {
                if (renderer.color.a == 1)
                    renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.5f);
                else
                    renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1f);
                timer = BlinkDelay;
            }
            yield return null;
        }
    }

    /*public void SpawnMachine(GameObject Machine)
    {
        if (M_Cur != null) Destroy(M_Cur);
        M_Cur = Instantiate(Machine, SpawnPoint);
    }*/

    public void SetCameraTargetOnCurrentVehicle()
    {
        ProCamera2D.Instance.CameraTargets[0].TargetTransform = Vehicles[CurVehicle].transform;
    }

    #region LevelManagment
    public void SkipLevel()
    {
        while (!IsLevelFinished)
        {
            NextObjective();
        }
        IsLevelFinished = true;
        Debug.Log("LevelSkipped!");
    }

    public void GetHint()
    {
        if (IsHintActive) return;
        IsHintActive = true;
        StartCoroutine(Objectives[CurObjective].HintRountine());
    }

    private void NextObjective()
    {
        CurObjective++;
        if (IsLevelFinished = Objectives.Count <= CurObjective)
        {
            ClearInfoPanel();
            AddTextToInfoPanel("Level completed!");
            return;
        }
        Objectives[CurObjective].GetInfo(InfoPanel);
        Objectives[CurObjective].ActivateObjective();
        /*if (Objectives[CurObjective].Objective_Type == global::Objectives.ObjectiveType.MoveTo)
            Objectives[CurObjective].ActivateObjective();*/
    }

    public void LevelFailed(string Cause)
    {
        while (!IsLevelFinished)
        {
            NextObjective();
        }
        ClearInfoPanel();
        AddTextToInfoPanel("Level failed! " + Cause);
        IsLevelFinished = true;
        Debug.LogWarning("LevelFailed!");
    }
    #endregion

    #region ElementsForInfoPanel
    public void ClearInfoPanel()
    {
        foreach (var @obj in InfoPanelPieces)
            Destroy(@obj);
        InfoPanelPieces.Clear();
    }
    public void AddTextToInfoPanel(string Text)
    {
        var Info = new GameObject("Info");
        InfoPanelPieces.Add(Info);
        Info.transform.SetParent(InfoPanel.transform);
        Info.transform.localScale = Vector3.one;
        var Txt = Info.AddComponent<TextMeshProUGUI>();
        Txt.text = Text;
        Txt.fontSize = FontSizeForInfoPanel;
        Txt.alignment = TextAlignmentOptions.Midline;
        Txt.raycastTarget = false;
        Txt.color = Color.black;
        Txt.font = FontForInfoPanel;
        Info.AddComponent<LayoutElement>();
    }

    public void AddImageToInfoPanel(Sprite Img)
    {
        var Info = new GameObject("Info");
        InfoPanelPieces.Add(Info);
        Info.transform.SetParent(InfoPanel.transform);
        Info.transform.localScale = Vector3.one;
        var image = Info.AddComponent<Image>();
        image.sprite = Img;
        image.raycastTarget = false;
        var Layout = Info.AddComponent<LayoutElement>();
        Layout.preferredWidth = 100;
    }
    #endregion
}
