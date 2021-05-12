using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
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

            for (int way = 2; way <= 5 && !token.IsCancellationRequested; way++)
            {
                await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.2), PlayerLoopTiming.FixedUpdate)
                    .Zip(BallisticMath.CalculateWayRadians(Mathf.PI / 2, way, ((way & 1) * 2 - 1) * 60 / way * Mathf.Deg2Rad).ToUniTaskAsyncEnumerable(), (_, r) => r)
                    .ForEachAsync(r =>
                    {
                        var bullet = BulletBase.Instantiate(m_BombPrefab, transform.position + BallisticMath.FromPolar(32 * way, r));
                        bullet.Owner = this;

                        SoundManager.I.PlaySe(SeType.Explosion).Forget();
                    }, token);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

            MoveSpeedModifier = 1;
        }
    }
}
