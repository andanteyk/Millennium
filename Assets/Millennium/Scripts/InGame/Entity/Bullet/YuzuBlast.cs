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
        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            Move(token);
            DestroyWhenExpired(token);
            DamageWhenStay(token);
            CollisionSwitcher(0.2f, token);
        }
    }

}
