using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UtilButton : MonoBehaviour
{
    private Button Click = null;
    public delegate void _UtilButton();
    public static _UtilButton _Util { get; set; }
    private void Start()
    {
        Click = GetComponent<Button>();
        Click.onClick.AddListener(delegate { _Util?.Invoke(); });
    }
}
