using Cysharp.Threading.Tasks;
using DG.Tweening;
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
            DamageWhenEnter(token);

            // to activate colliders
            transform.DOShakePosition(0.5f, strength: 0.125f).SetLink(gameObject).WithCancellation(token);
        }
    }

}
