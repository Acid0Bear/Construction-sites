using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Collider2D))]
public class BalanceExtension : MonoBehaviour
{
    [Header("Displayed in objective message")]
    public string BalanceObjName;
    [Header("SpriteRenderer for hint blinking")]
    public SpriteRenderer TargetGraphic;
    [Header("Sprite to display on top left")]
    public Sprite TargetSprite;

    [Header("Amount of objects, setup outside of MainObserver!")]
    [SerializeField] private int Amount = 1;
    [Header("ObjectPrefab, setup outside of MainObserver!")]
    [SerializeField] private GameObject Object = null;
    [Header("List of all objects, setup outside of MainObserver!")]
    public List<Transform> ObjectsList;

    [HideInInspector]
    public bool Shadow;
    [HideInInspector]
    public Objectives ConnectedObjective;
    public Coroutine Countdown;

    private int LoadedNumber = 0;
    public bool IsLoaded { get; private set; }
    public bool CountDownResult { get; private set; }
    private List<GameObject> CountedObjects = new List<GameObject>();
    private GameObject ShadowObj;
    private Sprite ExecutorSprite;

    public void SetShadow()
    {
        ShadowObj = new GameObject("Shadow");
        ShadowObj.transform.localScale = Vector3.Scale(Object.transform.localScale, this.transform.localScale);
        var renderer = ShadowObj.AddComponent<SpriteRenderer>();
        renderer.sprite = Object.GetComponentInChildren<SpriteRenderer>().sprite;
        renderer.color = new Color(255, 255, 255, 0.3f);
        ShadowObj.transform.SetParent(this.transform);
        ShadowObj.transform.localPosition = Object.transform.localPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (MainObserver.Observer.ActiveObjective == ConnectedObjective)
            if (Regex.IsMatch(collision.gameObject.name, @"\b" + Object.name + @"\b"))
            {
                if (collision.gameObject.GetComponentInParent<SpringJoint2D>())
                    if (collision.gameObject.GetComponentInParent<SpringJoint2D>().enabled) return;
                    else
                        CountedObjects.Add(collision.gameObject);
                else
                    CountedObjects.Add(collision.gameObject.gameObject);
                LoadedNumber++;
                ObjectsList.Remove(collision.gameObject.transform);
                IsLoaded = LoadedNumber == Amount;
                ShadowObj.SetActive(!IsLoaded);
            }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (MainObserver.Observer.ActiveObjective == ConnectedObjective)
            if (Regex.IsMatch(collision.gameObject.name, @"\b" + Object.name + @"\b"))
            {
                float Dist = Vector2.Distance(collision.transform.position, this.transform.position);

                if (collision.gameObject.GetComponentInParent<SpringJoint2D>().enabled || collision.IsTouchingLayers(LayerMask.GetMask("M_Claw")))
                {
                    if (CountedObjects.Contains(collision.gameObject))
                    {
                        CountedObjects.Remove(collision.gameObject);
                        ObjectsList.Add(collision.gameObject.transform);
                        LoadedNumber--;
                    }
                }
                else if (!CountedObjects.Contains(collision.gameObject))
                {
                    ObjectsList.Remove(collision.gameObject.transform);
                    CountedObjects.Add(collision.gameObject);
                    LoadedNumber++;
                    IsLoaded = LoadedNumber == Amount;
                }
                ShadowObj.SetActive(!IsLoaded);
            }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (MainObserver.Observer.ActiveObjective == ConnectedObjective)
            if (Regex.IsMatch(collision.gameObject.name, @"\b" + Object.name + @"\b"))
            {
                if (CountedObjects.Contains(collision.gameObject))
                {
                    CountedObjects.Remove(collision.gameObject);
                    ObjectsList.Add(collision.gameObject.transform);
                    LoadedNumber--;
                    IsLoaded = LoadedNumber == Amount;
                }
                ShadowObj.SetActive(!IsLoaded);
            }
    }

    private IEnumerator CountDownToCompletion()
    {
        if (MainObserver.Observer.CountDown == null) { Debug.LogError("Assign CountDown to the MainObserver in the inspector!"); yield break; }
        MainObserver.Observer.CountDown.SetActive(true);
        float timer = 0;
        while (timer < 3)
        {
            MainObserver.Observer.CountDown.GetComponent<Image>().fillAmount = timer / 3;
            MainObserver.Observer.CountDown.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Mathf.RoundToInt(timer).ToString();
            if (!IsLoaded) { MainObserver.Observer.CountDown.SetActive(false); Countdown = null; yield break; }
            timer += Time.deltaTime;
            yield return null;
        }
        MainObserver.Observer.CountDown.SetActive(false);
        this.CountDownResult = true;
    }

    public void InitiateCoutDown()
    {
        Countdown = StartCoroutine(CountDownToCompletion());
    }

    public void StopCountDown()
    {
        StopCoroutine(Countdown);
        Countdown = null;
    }

    public void SetBalanceProps(Objectives ConnectedObjective, Sprite ExecutorSprite)
    {
        LoadedNumber = 0;
        IsLoaded = false;
        this.ConnectedObjective = ConnectedObjective;
        this.CountDownResult = false;
        this.ExecutorSprite = ExecutorSprite;
        this.SetShadow();
        foreach (var @obj in ObjectsList)
            obj.transform.SetParent(null);
    }

    public void SetInfoPanel()
    {
        var Obs = MainObserver.Observer;
        Obs.ClearInfoPanel();
        if (this.IsLoaded)
        {
            Obs.AddTextToInfoPanel("Move ");
            Obs.AddImageToInfoPanel(this.ExecutorSprite);
            Obs.AddTextToInfoPanel(" to highlighted area!");
        }
        else
        {
            Obs.AddTextToInfoPanel("Load " + Object.name);
            Obs.AddTextToInfoPanel(" to ");
            Obs.AddImageToInfoPanel((TargetSprite) ? TargetSprite : this.TargetSprite);
        }
    }
}
