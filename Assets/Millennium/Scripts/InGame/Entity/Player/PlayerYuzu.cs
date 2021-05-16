using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Player
{
    public class PlayerYuzu : Player
    {

        protected override UniTask SubShot()
        {
            for (int i = 0; i < 2; i++)
            {
                var position = transform.position + new Vector3(32 * (i - 0.5f), 0);

                var bullet = BulletBase.Instantiate(m_SubShotPrefab, position);
                bullet.Speed = new Vector3(bullet.Speed.x * (i * 2 - 1), bullet.Speed.y);
                bullet.Owner = this;
            }
            return UniTask.CompletedTask;
        }

        protected override async UniTask FireBomb(CancellationToken token)
        {
            MoveSpeedModifier = 0.25f;

            EffectManager.I.Play(EffectType.Concentration, transform.position).SetParent(transform);
            SoundManager.I.PlaySe(SeType.Ultimate).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);


            await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.25), PlayerLoopTiming.FixedUpdate)
                .Take(12)
                .ForEachAsync(_ =>
                {
                    var initialDirection = Seiran.Shared.NextSingle(-Mathf.PI, 0);
                    var bullet = BulletBase.Instantiate(m_BombPrefab, transform.position, BallisticMath.FromPolar(64, initialDirection));
                    bullet.Owner = this;
                    bullet.DOSpeed(new Vector3(Mathf.Cos(initialDirection) * 16, 256 - Mathf.Sin(initialDirection) * 64), 1).SetEase(Ease.InQuart);

                    SoundManager.I.PlaySe(SeType.Fall).Forget();
                }, token);


            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

            MoveSpeedModifier = 1;
        }
    }
}
