using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.UI
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasCameraSetter : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Canvas>().worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }
    }
}
