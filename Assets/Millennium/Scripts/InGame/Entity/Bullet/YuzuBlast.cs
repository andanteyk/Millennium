using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Millennium.InGame.Effect;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Millennium.InGame.Entity.Bullet
{

    public class YuzuBlast : BulletBase
    {
        private async UniTaskVoid Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            Move(token).Forget();
            DestroyWhenExpired(token).Forget();
            DamageWhenStay(token).Forget();

            Array.ForEach(GetComponentsInChildren<Collider2D>(), c => c.enabled = false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.125), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);
            CollisionSwitcher(0.5f, 2, token).Forget();
        }
    }

}
