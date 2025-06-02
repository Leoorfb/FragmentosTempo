using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileCanvasActivator : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(Application.isMobilePlatform);
    }
}
