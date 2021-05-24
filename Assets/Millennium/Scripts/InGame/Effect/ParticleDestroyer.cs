using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Effect
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleDestroyer : MonoBehaviour
    {
        async UniTaskVoid Start()
        {
            var particle = GetComponent<ParticleSystem>();
            var token = this.GetCancellationTokenOnDestroy();

            await UniTask.WaitWhile(() => particle != null && particle.IsAlive(), cancellationToken: token);


            token.ThrowIfCancellationRequested();
            Destroy(gameObject);
        }
    }
}
