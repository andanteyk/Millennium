using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Player
{
    public class PlayerMomoi : Player
    {
        protected override UniTask SubShot(CancellationToken token)
        {
            for (int i = -2; i <= 2; i++)
            {
                if (i == 0)
                    continue;

                float angle = 15f * i;
                var quaternion = Quaternion.AngleAxis(angle, Vector3.forward);

                var bullet = BulletBase.Instantiate(m_SubShotPrefab, transform.position);
                bullet.transform.rotation = quaternion;
                bullet.Speed = quaternion * bullet.Speed;       // set by prefab
                bullet.Owner = this;
            }
            return UniTask.CompletedTask;
        }


        protected override async UniTask FireBomb(CancellationToken token)
        {
            MoveSpeedModifier = 0.25f;

            EffectManager.I.Play(EffectType.Concentration, transform.position).SetParent(transform);
            SoundManager.I.PlaySe(SeType.Ultimate).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(0.5), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);


            EffectManager.I.Play(EffectType.MomoiUlt, transform.position + Vector3.up * 24).SetParent(transform);
            await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.1), PlayerLoopTiming.FixedUpdate)
                .Take(30)
                .ForEachAsync(_ =>
                {
                    foreach (var r in BallisticMath.CalculateWayRadians((90 + Seiran.Shared.NextSingle(-30, 30)) * Mathf.Deg2Rad, 3, 45 * Mathf.Deg2Rad))
                    {
                        var bullet = BulletBase.Instantiate(m_BombPrefab, transform.position, BallisticMath.FromPolar(256, r));
                        bullet.transform.rotation = Quaternion.AngleAxis(r * Mathf.Rad2Deg - 90, Vector3.forward);
                        bullet.Owner = this;
                    }

                    EffectManager.I.Play(EffectType.PlusDecayRed, transform.position);
                    SoundManager.I.PlaySe(SeType.MomoiMidoriBomb).Forget();
                }, token);

            MoveSpeedModifier = 1;
        }
    }
}
