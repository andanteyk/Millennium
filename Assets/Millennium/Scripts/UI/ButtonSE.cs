using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Millennium.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonSE : MonoBehaviour, ISelectHandler
    {
        public void OnSelect(BaseEventData eventData)
        {
            SoundManager.I.PlaySe(SeType.Ok).Forget();
        }
    }
}
