using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Effect
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleDestroyer : MonoBehaviour
    {
        async void Start()
        {
            var particle = GetComponent<ParticleSystem>();
            await UniTask.WaitWhile(() => particle != null && particle.IsAlive());

            // エディタ再生を終了したときに null になるので
            if (gameObject != null)
                Destroy(gameObject);
        }
    }
}
