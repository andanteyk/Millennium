using Cysharp.Threading.Tasks;
using Millennium.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium
{

    public class EntryPoint : MonoBehaviour
    {
        async void Start()
        {
            Sound.SoundManager.I.PlayBgm(Sound.BgmType.Test).Forget();
        }
    }

}
