using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class Container : MonoBehaviour
{
    [Header("Displayed in objective message")]
    public string ContainerName;
    [Header("SpriteRenderer for hint blinking")]
    public SpriteRenderer TargetGraphic;
    [Header("Sprite to display on top left")]
    public Sprite TargetSprite;  //For auto setting sprites on short hint

    [Header("Additional check for distance")]
    [SerializeField] private bool DistanceCheck = false;
    [DrawIf("DistanceCheck", true)]
    [SerializeField] private float DistanceToObject = 0.5f;

    [Header("SpawnPrefab on completion")]
    [SerializeField] private bool SpawnPrefab = false;
    [DrawIf("SpawnPrefab", true)]
    [SerializeField] private GameObject Prefab = null;

    [HideInInspector]
    public int Amount;
    [HideInInspector]
    public GameObject Object;
    [HideInInspector]
    public bool Shadow;
    [HideInInspector]
    public Objectives ConnectedObjective;

    private int LoadedNumber = 0;
    private bool Completed = false, ShouldBeRechecked = false; //ShouldBeRechecked added
    private List<GameObject> CountedObjects = new List<GameObject>();
    private GameObject ShadowObj;
    private Coroutine CountDown = null;

    public void SetShadow()
    {
        ShadowObj = new GameObject("Shadow");
        ShadowObj.transform.localScale = Vector3.Scale(Object.GetComponentInChildren<SpriteRenderer>().transform.localScale,Object.transform.localScale);
        var renderer = ShadowObj.AddComponent<SpriteRenderer>();
        renderer.sprite = Object.GetComponentInChildren<SpriteRenderer>().sprite;
        renderer.color = new Color(255, 255, 255, 0.3f);
        ShadowObj.transform.SetParent(this.transform);
        ShadowObj.transform.localPosition = Vector3.zero;
    }

    //NOTE \b means checking word boundaries
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.transform.parent && MainObserver.Observer.ActiveObjective == ConnectedObjective || (ShouldBeRechecked && Completed)) //Mods for rechecking
            if (Regex.IsMatch(collision.gameObject.transform.parent.name, @"\b" + Object.name + @"\b"))
            {
                if (DistanceCheck) return;
                if (collision.gameObject.GetComponentInParent<SpringJoint2D>())
                    if (collision.gameObject.GetComponentInParent<SpringJoint2D>().enabled) return;
                    else
                        CountedObjects.Add(collision.gameObject);
                else
                    CountedObjects.Add(collision.gameObject.transform.parent.gameObject);
                LoadedNumber++;
                ConnectedObjective.ObjectiveData.Remove(collision.gameObject.transform.parent);
                if (LoadedNumber == Amount && !Completed) CountDown = StartCoroutine(CheckPersist()); //Mods for rechecking
            }
    }
    
    //Globally simplified
    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.gameObject.transform.parent && MainObserver.Observer.ActiveObjective == ConnectedObjective) || (ShouldBeRechecked && Completed)) //Mods for rechecking
            if (Regex.IsMatch(collision.gameObject.transform.parent.name, @"\b" + Object.name + @"\b"))
            {
                float Dist = Vector2.Distance(collision.transform.position, this.transform.position);
                bool Dynamic = collision.gameObject.GetComponentInParent<SpringJoint2D>();

                if (collision.gameObject.GetComponentInParent<SpringJoint2D>().enabled || collision.IsTouchingLayers(LayerMask.GetMask("M_Claw")))
                {
                    if (CountDown != null && CountedObjects.Contains((Dynamic) ? collision.gameObject : collision.transform.parent.gameObject))
                    {
                        CountedObjects.Remove((Dynamic) ? collision.gameObject : collision.transform.parent.gameObject);
                        ConnectedObjective.ObjectiveData.Add(collision.gameObject.transform.parent);
                        LoadedNumber--;
                    }
                    if (DistanceCheck && Dynamic)
                        if (DistanceToObject >= Dist)
                        {
                            var spr = collision.GetComponent<SpriteRenderer>();
                            spr.color = new Color(0.3f, 1, 0.3f, 1);
                        }
                        else if (DistanceToObject <= Dist)
                        {
                            var spr = collision.GetComponent<SpriteRenderer>();
                            spr.color = new Color(1, (1 / Dist), (1 / Dist), 1);
                        }
                    return;
                }
                else if (!CountedObjects.Contains((Dynamic) ? collision.gameObject : collision.transform.parent.gameObject) && !DistanceCheck)
                {
                    ConnectedObjective.ObjectiveData.Remove(collision.gameObject.transform.parent);
                    CountedObjects.Add((Dynamic) ? collision.gameObject : collision.transform.parent.gameObject);
                    LoadedNumber++;
                    if (LoadedNumber == Amount && !Completed) CountDown = StartCoroutine(CheckPersist()); //Mods for rechecking
                }
                else if (DistanceCheck)
                {
                    if (!CountedObjects.Contains((Dynamic) ? collision.gameObject : collision.transform.parent.gameObject) && DistanceToObject >= Dist)
                    {
                        var spr = collision.GetComponent<SpriteRenderer>();
                        spr.color = new Color(0.3f, 1, 0.3f, 1);
                        LoadedNumber++;
                        ConnectedObjective.ObjectiveData.Remove(collision.gameObject.transform.parent);
                        CountedObjects.Add((Dynamic) ? collision.gameObject : collision.transform.parent.gameObject);
                        if (LoadedNumber == Amount && !Completed) CountDown = StartCoroutine(CheckPersist()); //Mods for rechecking
                    }
                    else if (DistanceToObject <= Dist)
                    {
                        var spr = collision.GetComponent<SpriteRenderer>();
                        spr.color = new Color(1, (1 / Dist), (1 / Dist), 1);
                    }
                }
            }
    }
    private void OnTriggerExit2D(Collider2D collision) //Has mods for rechecking
    {
        if (collision.gameObject.transform.parent && (MainObserver.Observer.ActiveObjective == ConnectedObjective || ShouldBeRechecked))
            if (Regex.IsMatch(collision.gameObject.transform.parent.name, @"\b" + Object.name + @"\b"))
            {
                bool Dynamic = collision.gameObject.GetComponentInParent<SpringJoint2D>();

                if (CountedObjects.Contains((Dynamic) ? collision.gameObject : collision.transform.parent.gameObject))
                {
                    CountedObjects.Remove((Dynamic) ? collision.gameObject : collision.transform.parent.gameObject);
                    ConnectedObjective.ObjectiveData.Add(collision.gameObject.transform.parent);
                    LoadedNumber--;
                    if(ShouldBeRechecked && Completed && CountDown == null)
                        CountDown = StartCoroutine(FailingCountdown());
                }
            }
    }

    public bool CheckCompletion()
    {
        return Completed;
    }

    //Contains mods for rechecking
    public void SetContainer(int Amount, GameObject ObjectPrefab, Objectives ConnectedObjective, bool Shadow, bool ShouldBeRechecked)
    {
        LoadedNumber = 0;
        Completed = false;
        this.Amount = Amount;
        this.Object = ObjectPrefab;
        this.ConnectedObjective = ConnectedObjective;
        this.ShouldBeRechecked = ShouldBeRechecked;
        if (Shadow) this.SetShadow();
    }

    //Added for rechecking
    IEnumerator FailingCountdown()
    {
        if (MainObserver.Observer.CountDown == null) { Debug.LogError("Assign CountDown to the MainObserver in the inspector!"); yield break; }
        MainObserver.Observer.CountDown.SetActive(true);
        float timer = 3;
        while (timer >= 0)
        {
            MainObserver.Observer.CountDown.GetComponent<Image>().fillAmount = timer / 3;
            MainObserver.Observer.CountDown.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Mathf.RoundToInt(timer).ToString();
            if (LoadedNumber == Amount) { MainObserver.Observer.CountDown.SetActive(false); CountDown = null; yield break; }
            timer -= Time.deltaTime;
            yield return null;
        }
        MainObserver.Observer.CountDown.SetActive(false);
        MainObserver.Observer.LevelFailed(Object.name + " is lost!");
    }

    IEnumerator CheckPersist()
    {
        if (MainObserver.Observer.CountDown == null) { Debug.LogError("Assign CountDown to the MainObserver in the inspector!"); yield break; }
        MainObserver.Observer.CountDown.SetActive(true);
        float timer = 0;
        while(timer < 3)
        {
            MainObserver.Observer.CountDown.GetComponent<Image>().fillAmount = timer / 3;
            MainObserver.Observer.CountDown.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Mathf.RoundToInt(timer).ToString();
            if (LoadedNumber != Amount) { MainObserver.Observer.CountDown.SetActive(false); CountDown = null; yield break; }
            timer += Time.deltaTime;
            yield return null;
        }
        Completed = true;
        Destroy(ShadowObj);
        foreach (GameObject obj in CountedObjects)
        {
            if (obj.GetComponentInParent<SpringJoint2D>() != null)
            {
                var spr = obj.GetComponent<SpriteRenderer>();
                spr.color = new Color(255, 255, 255);
                //Destroy(obj.GetComponentInParent<SpringJoint2D>()); //Useless, should be removed
            }
            else
            {
                var spr = obj.GetComponentInChildren<SpriteRenderer>();
                spr.color = new Color(255, 255, 255);
            }
        }
        MainObserver.Observer.CountDown.SetActive(false);
        if (SpawnPrefab && Prefab != null)
        {
            GameObject.Instantiate(Prefab, this.transform.position, Quaternion.identity, this.transform.parent);
            foreach (GameObject obj in CountedObjects)
                /*if (obj.GetComponentInParent<SpringJoint2D>() != null)
                {
                    Destroy(obj.GetComponentInParent<SpringJoint2D>().transform.gameObject); //Useless, should be removed
                }
                else*/
                    Destroy(obj);
        }
        //CountedObjects.Clear(); //Should not be cleared
        yield break;
    }
}
