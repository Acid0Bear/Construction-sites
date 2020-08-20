using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwtichSides : MonoBehaviour
{
    private Button Click = null;
    private void Start()
    {
        Click = GetComponent<Button>();
        Click.onClick.AddListener(delegate { Controller.SwitchSides?.Invoke(); });
    }

}
