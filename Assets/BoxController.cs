using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    private RectTransform rt;
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void OnMouseDrag()
    {
        print("drag");
        rt.anchoredPosition = Input.mousePosition;  
    }
}
