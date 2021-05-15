using Cysharp.Threading.Tasks;
using Millennium.InGame.Effect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Bullet
{
    public class DrumBlast : BulletBase
    {
        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            Move(token);
            DestroyWhenExpired(token);
            DamageWhenEnter(token);

            EffectManager.I.Play(EffectType.Explosion, transform.position);
        }
    }
}
