using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Bullet
{
    public class BulletRemover : BulletBase
    {
        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            Move(token);
            DestroyWhenExpired(token);
            DamageWhenEnter(token);

            transform.DOScale(10, 1f).SetEase(Ease.Linear).WithCancellation(token);
        }
    }
}
