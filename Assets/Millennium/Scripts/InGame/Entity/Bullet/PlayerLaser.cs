using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Millennium.InGame.Effect;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Bullet
{
    public class PlayerLaser : BulletBase
    {
        public Transform OwnerTransform { get; set; }
        public Vector3 RelativeDistance { get; set; }


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            DestroyWhenExpired(token);
            DamageWhenStay(token);
            CollisionSwitcher(0.5f, token);

            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAsync(_ =>
                {
                    if (OwnerTransform != null)
                    {
                        transform.position = OwnerTransform.position + RelativeDistance;
                    }
                }, token);
        }
    }
}
