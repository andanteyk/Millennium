using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{
    public class BossNeru : BossBase
    {
        [SerializeField]
        private GameObject m_NormalShotPrefab;


        private void Start()
        {
            OnStart().Forget();
        }

        private async UniTaskVoid OnStart()
        {
            var destroyToken = this.GetCancellationTokenOnDestroy();

            SetupHealthGauge(4, destroyToken);

            await RandomMove(1, destroyToken);

            /*
            Health = HealthMax = 5000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await PlayerAimshot1(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);


            Health = HealthMax = 10000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillFuzakenna(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);

            Health = HealthMax = 5000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5f), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await Punch(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);
            //*/


            Health = HealthMax = 10000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillBukkoro(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);



            await PlayDeathEffect(destroyToken);

            if (destroyToken.IsCancellationRequested)
                return;
            Destroy(gameObject);
        }


        private async UniTask PlayerAimshot1(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            RandomMove(4, token).Forget();
            await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.125f), updateTiming: PlayerLoopTiming.FixedUpdate)
                .Take(16)
                .WithCancellation(token))
            {
                float playerDirection = BallisticMath.AimToPlayer(transform.position);
                foreach (var r in BallisticMath.CalculateWayRadians(playerDirection, 3, 18 * Mathf.Deg2Rad))
                {
                    for (int k = 0; k < 3; k++)
                    {
                        BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(64 + 16 * k, r));
                    }
                }

                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
            };


            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();


            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);


            for (int direction = 0; direction < 2; direction++)
            {
                float playerDirection = BallisticMath.AimToPlayer(transform.position);
                int ways = 16;

                await foreach (var i in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05f), updateTiming: PlayerLoopTiming.FixedUpdate)
                   .Select((_, i) => i)
                   .Take(ways)
                   .WithCancellation(token))
                {
                    for (int k = 0; k < 3; k++)
                    {
                        BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(64 + 16 * k, playerDirection + ((float)i / ways - 0.5f) * (direction * 2 - 1) * 30 * Mathf.Deg2Rad));
                    }

                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                    EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                }
            }

            await RandomMove(2, token);
        }


        private async UniTask SkillFuzakenna(CancellationToken token)
        {
            const float wayInterval = 30 * Mathf.Deg2Rad;

            await (
                RandomMove(1, token),
                UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.25f), PlayerLoopTiming.FixedUpdate)
                    .Take(4)
                    .ForEachAsync(_ =>
                    {
                        foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 12))
                        {
                            BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(32, r));
                        }

                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                        EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                    }, token));


            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();

            for (int i = -2; i <= 2; i++)
                EffectManager.I.Play(EffectType.Caution, transform.position + BallisticMath.FromPolar(128, -Mathf.PI / 2 + i * wayInterval));

            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);


            await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05f), updateTiming: PlayerLoopTiming.FixedUpdate)
                .Take(32)
                .WithCancellation(token))
            {
                for (int k = -2; k <= 2; k++)
                {
                    BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(Seiran.Shared.Next(128, 256), -Mathf.PI / 2 + k * wayInterval + Seiran.Shared.NextSingle(-5, 5) * Mathf.Deg2Rad));
                }

                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
            }
        }


        private async UniTask Punch(CancellationToken token)
        {
            var position = transform.position + BallisticMath.FromPolar(32, Seiran.Shared.NextRadian());
            var speed = BallisticMath.FromPolar(128, BallisticMath.AimToPlayer(transform.position) + Seiran.Shared.NextSingle(-15, 15) * Mathf.Deg2Rad);

            await (RandomMove(1, token),
            UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05f), updateTiming: PlayerLoopTiming.FixedUpdate)
                 .Select((_, i) => i)
                 .Take(8)
                 .ForEachAsync(i =>
             {
                 for (int k = 0; k < 4; k++)
                 {
                     var bullet = BulletBase.Instantiate(m_NormalShotPrefab, position + Seiran.Shared.InsideUnitCircle() * 32, Vector3.zero);
                     _ = bullet.DOSpeed(speed, 0.5f)
                         .SetDelay(1 - i * 0.05f);
                 }

                 SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                 EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
             }, token));

            SoundManager.I.PlaySe(SeType.Explosion).Forget();
        }


        // TODO: ‚¿‚å‚Á‚Æ“ïˆÕ“x‚‚¢‚©‚à‚µ‚ê‚È‚¢
        private async UniTask SkillBukkoro(CancellationToken token)
        {
            float playerDirection = BallisticMath.AimToPlayer(transform.position);
            var targetPosition = BallisticMath.FromPolar(64, playerDirection);

            EffectManager.I.Play(EffectType.Caution, targetPosition);
            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

            await (
                transform.DOMove(targetPosition, 4)
                    .SetUpdate(UpdateType.Fixed)
                    .SetLink(gameObject)
                    .WithCancellation(token),

                UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                    .Take(80)
                    .ForEachAsync(_ =>
                    {
                        foreach (var r in BallisticMath.CalculateWayRadians(playerDirection + Seiran.Shared.NextSingle(-35, 35) * Mathf.Deg2Rad, 3))
                        {
                            BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(64, r));
                        }

                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                        EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                    }, token));

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);


            await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(60)
                .ForEachAsync(i =>
                {
                    foreach (var r in BallisticMath.CalculateWayRadians(playerDirection + DOVirtual.EasedValue(0, Mathf.PI * 6, i / 60f, Ease.InQuad), 3))
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(16, r));
                        bullet.DOSpeed(BallisticMath.FromPolar(128, r), 1f)
                            .SetDelay(0.5f);
                    }

                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                    EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                }, token);
        }

    }
}
