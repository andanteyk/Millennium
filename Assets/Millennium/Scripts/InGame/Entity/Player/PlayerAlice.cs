using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Sound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Player
{
    public class PlayerAlice : Player
    {
        protected override UniTask SubShot()
        {
            for (int i = 0; i < 2; i++)
            {
                var relative = new Vector3(32 * (i - 0.5f), 120);

                var bullet = BulletBase.Instantiate(m_SubShotPrefab, transform.position + relative);
                bullet.Owner = this;

                var laser = bullet.GetComponent<PlayerLaser>();
                laser.OwnerTransform = transform;
                laser.RelativeDistance = relative;
            }

            return UniTask.CompletedTask;
        }


        protected override async UniTask FireBomb(CancellationToken token)
        {
            MoveSpeedModifier = 0.25f;

            EffectManager.I.Play(EffectType.Concentration, transform.position).SetParent(transform);
            SoundManager.I.PlaySe(SeType.Ultimate).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);

            var bullet = BulletBase.Instantiate(m_BombPrefab, transform.position, new Vector3(0, 64));
            bullet.Owner = this;
            SoundManager.I.PlaySe(SeType.AliceBomb).Forget();

            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

            MoveSpeedModifier = 1;
        }
    }
}