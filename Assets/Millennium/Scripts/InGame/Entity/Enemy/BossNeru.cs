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

            SetupHealthGauge(6, destroyToken);
            DamageWhenEnter(destroyToken).Forget();
            EffectManager.I.Play(EffectType.Warning, Vector3.zero);
            SoundManager.I.PlaySe(SeType.Warning).Forget();

            await RandomMove(4, destroyToken);


            //*
            Health = HealthMax = 8000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await PlayerAimshot1(token);
                }
            }, destroyToken);
            await OnEndPhaseShort(destroyToken);


            Health = HealthMax = 12000;
            await RunPhase(async token =>
            {
                await PlaySkillBalloon("ネル", "あぁ? ふざけんな!", token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillFuzakenna(token);
                }
            }, destroyToken);
            await DropUltimateAccelerant(false, destroyToken);
            await OnEndPhase(destroyToken);
            //*/

            Health = HealthMax = 12000;
            await RunPhase(async token =>
            {
                await PlaySkillBalloon("ネル", "げきこう", token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillRage(token);
                }
            }, destroyToken);
            await DropUltimateAccelerant(false, destroyToken);
            await OnEndPhase(destroyToken);


            Health = HealthMax = 8000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5f), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await Punch(token);
                }
            }, destroyToken);
            await OnEndPhaseShort(destroyToken);


            Health = HealthMax = 16000;
            await RunPhase(async token =>
            {
                await PlaySkillBalloon("ネル", "コールサイン ダブルオー", token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillCallsignDoubleO(token);
                }
            }, destroyToken);
            await DropMedkit(false, destroyToken);
            await OnEndPhase(destroyToken);


            Health = HealthMax = 16000;
            await RunPhase(async token =>
            {
                await PlaySkillBalloon("ネル", "あぁ? ぶっころされてぇか?", token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillBukkoro(token);
                }
            }, destroyToken);
            //await OnEndPhase(destroyToken);



            await PlayDeathEffect(destroyToken);

            destroyToken.ThrowIfCancellationRequested();
            Destroy(gameObject);
        }


        private async UniTask PlayerAimshot1(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

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


        private async UniTask SkillCallsignDoubleO(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();


            UniTask ThrowO(float rotateDirection, CancellationToken token)
            {
                token.ThrowIfCancellationRequested();

                float baseRadian = Seiran.Shared.NextRadian();
                Vector3 center = transform.position + BallisticMath.FromPolar(32, baseRadian);
                const int density = 20;
                float aimRadian = BallisticMath.AimToPlayer(center);

                EffectManager.I.Play(EffectType.Caution, center);
                SoundManager.I.PlaySe(SeType.Concentration).Forget();

                return UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                    .Select((_, i) => i)
                    .Take(20)
                    .ForEachAsync(i =>
                    {
                        float r = baseRadian + i * Mathf.PI * 2 / density;
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, center + BallisticMath.FromPolar(32, r), Vector3.zero);

                        static async UniTaskVoid MoveBullet(BulletBase bullet, int index, float aimRadian, float placeRadian, float rotateDirection, CancellationToken token)
                        {
                            await UniTask.Delay(TimeSpan.FromSeconds((density - index) * 0.05), cancellationToken: token);
                            token.ThrowIfCancellationRequested();
                            SoundManager.I.PlaySe(SeType.Fall).Forget();
                            await bullet.DOSpeed(BallisticMath.FromPolar(64, aimRadian) + BallisticMath.FromPolar(48, placeRadian + Mathf.PI * 7 / 8 * rotateDirection), 1);
                        }
                        MoveBullet(bullet, i, aimRadian, r, rotateDirection, bullet.GetCancellationTokenOnDestroy()).Forget();

                        EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                    }, token);
            }

            await (ThrowO(1, token), ThrowO(-1, token));
            await (ThrowO(1, token), ThrowO(-1, token));
            await (ThrowO(1, token), ThrowO(-1, token));

            await RandomMove(2, token);
        }



        private async UniTask SkillRage(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await MoveTo(Vector3.zero, 1, token);


            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();



            float baseRadian = BallisticMath.AimToPlayer(transform.position) - Mathf.PI / 4;
            await UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.1), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(120)
                .ForEachAsync(i =>
                {
                    foreach (var r in BallisticMath.CalculateWayRadians(baseRadian + (Mathf.Abs(i - 60) * 4 + Seiran.Shared.NextSingle(-6, 6)) * Mathf.Deg2Rad, 3))
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(96, r));
                        EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                    }
                    SoundManager.I.PlaySe(i == 60 ? SeType.Explosion : SeType.EnemyShot).Forget();
                }, token);

            token.ThrowIfCancellationRequested();
            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();

            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

            token.ThrowIfCancellationRequested();
            foreach (var r in BallisticMath.CalculateWayRadians(baseRadian, 18))
            {
                var bullet = BulletBase.Instantiate(m_NormalShotPrefab, BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(-64, r));
                EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
            }
            SoundManager.I.PlaySe(SeType.Explosion).Forget();

        }



        private async UniTask SkillBukkoro(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

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

            token.ThrowIfCancellationRequested();
            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            foreach (var r in BallisticMath.CalculateWayRadians(-Mathf.PI / 2, 6))
            {
                EffectManager.I.Play(EffectType.Caution, transform.position + BallisticMath.FromPolar(64, r));
            }
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);


            await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(60)
                .ForEachAsync(i =>
                {
                    foreach (var r in BallisticMath.CalculateWayRadians(playerDirection + DOVirtual.EasedValue(0, Mathf.PI * 6, i / 60f, Ease.InQuad), 3))
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(128, r));
                        bullet.DOSpeed(BallisticMath.FromPolar(32, r), 1f);
                    }

                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                    EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                }, token);
        }

    }
}
