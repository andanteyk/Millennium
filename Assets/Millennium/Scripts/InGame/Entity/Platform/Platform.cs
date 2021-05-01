using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.Sound;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Platform
{
    public class Platform : Entity
    {
        // Start is called before the first frame update
        void Start()
        {
            OnStart(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid OnStart(CancellationToken token)
        {
            var trigger = this.GetAsyncTriggerEnter2DTrigger();

            while (!token.IsCancellationRequested)
            {
                await trigger.OnTriggerEnter2DAsync(token);

                SoundManager.I.PlaySe(SeType.PlayerBulletImmune).Forget();
            }
        }
    }
}