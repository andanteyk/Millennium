using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Threading;
using UnityEngine;
using DG.Tweening;

namespace Millennium.InGame.Entity.Enemy
{
    public class BossKarin : BossBase
    {
        [SerializeField]
        private GameObject m_NormalShotPrefab;

        [SerializeField]
        private GameObject m_HugeShotPrefab;

        private void Start()
        {
            OnStart().Forget();
        }


        private async UniTaskVoid OnStart()
        {
            var destroyToken = this.GetCancellationTokenOnDestroy();

            SetupHealthGauge(5, destroyToken);
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
                await PlaySkillBalloon("カリン", "かりょくしえん、かいし", token);
                await MoveTo(new Vector3(64 * (Seiran.Shared.Next(0, 2) * 2 - 1), 64), 1, token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillFireSupport(token);
                }
            }, destroyToken);
            await DropUltimateAccelerant(false, destroyToken);
            await OnEndPhase(destroyToken);



            Health = HealthMax = 8000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await PlayerAimshot2(token);
                }
            }, destroyToken);
            await OnEndPhaseShort(destroyToken);


            Health = HealthMax = 16000;
            await RunPhase(async token =>
            {
                await PlaySkillBalloon("カリン", "ぶそうきょうか、かんりょう", token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillEnhanceArmament(token);
                }
            }, destroyToken);
            await DropUltimateAccelerant(false, destroyToken);
            await OnEndPhaseShort(destroyToken);


            Health = HealthMax = 16000;
            await RunPhase(async token =>
            {
                await PlaySkillBalloon("カリン", "ターゲット、はいじょする", token);
                await MoveTo(new Vector3(64 * (Seiran.Shared.Next(0, 2) * 2 - 1), 64), 1, token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(2), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillEliminateTarget(token);
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

            await RandomMove(1.5f, token);

            EffectManager.I.Play(EffectType.Caution, BallisticMath.PlayerPosition + Vector3.up * 32);
            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);


            float playerDirection = BallisticMath.AimToPlayer(transform.position);
            {
                var bullet = BulletBase.Instantiate(m_HugeShotPrefab, transform.position, BallisticMath.FromPolar(192, playerDirection));
                _ = bullet.DOSpeed(BallisticMath.FromPolar(64, playerDirection), 1f);
            }

            for (int i = 0; i < 16; i++)
            {
                float r = playerDirection + Seiran.Shared.NextSingle(-60, 60) * Mathf.Deg2Rad;
                var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(Seiran.Shared.NextSingle(64, 128), r));
                _ = bullet.DOSpeed(BallisticMath.FromPolar(24, r), 1f);
            }
            SoundManager.I.PlaySe(SeType.Explosion).Forget();

        }


        private async UniTask PlayerAimshot2(CancellationToken token)
        {
            await RandomMove(1, token);

            token.ThrowIfCancellationRequested();

            float playerDirection = BallisticMath.AimToPlayer(transform.position);
            var bullet = BulletBase.Instantiate(m_HugeShotPrefab, transform.position, BallisticMath.FromPolar(128, playerDirection));
            async UniTaskVoid explode(BulletBase bullet, CancellationToken token)
            {
                token.ThrowIfCancellationRequested();
                await bullet.DOSpeed(Vector3.zero, 1f).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, token);

                foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 24))
                {
                    BulletBase.Instantiate(m_NormalShotPrefab, bullet.transform.position, BallisticMath.FromPolar(Seiran.Shared.NextSingle(16, 64), r));
                }

                EffectManager.I.Play(EffectType.Explosion, bullet.transform.position);
                SoundManager.I.PlaySe(SeType.Explosion).Forget();
                Destroy(bullet.gameObject);
            }
            explode(bullet, bullet.GetCancellationTokenOnDestroy()).Forget();

            SoundManager.I.PlaySe(SeType.Explosion).Forget();

        }



        private async UniTask SkillFireSupport(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

            await (
                MoveTo(new Vector3(-64 * Mathf.Sign(transform.position.x), 64) + Seiran.Shared.InsideUnitCircle() * 16, 4, token),
                UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .Take(4)
                    .ForEachAsync(_ =>
                    {
                        var bullet = BulletBase.Instantiate(m_HugeShotPrefab, transform.position, BallisticMath.FromPolar(32, Mathf.PI / 2));
                        bullet.DOSpeed(BallisticMath.FromPolar(96, -Mathf.PI / 2), 1f);
                        UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5), PlayerLoopTiming.FixedUpdate)
                            .Take(8)
                            .ForEachAsync(_ =>
                            {
                                foreach (var r in BallisticMath.CalculateWayRadians(-Mathf.PI / 2, 2, 30 * Mathf.Deg2Rad))
                                {
                                    BulletBase.Instantiate(m_NormalShotPrefab, bullet.transform.position, BallisticMath.FromPolar(32, r));
                                }

                                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                            }, bullet.GetCancellationTokenOnDestroy());

                        EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                        SoundManager.I.PlaySe(SeType.Explosion).Forget();
                    }, token)
                );

        }


        private async UniTask SkillEnhanceArmament(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            const int ways = 40;

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();


            float playerDirection = BallisticMath.AimToPlayer(transform.position);
            await UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(ways)
                .ForEachAsync(i =>
                {
                    foreach (var r in BallisticMath.CalculateWayRadians(playerDirection, 2, (20 + Math.Abs(i - ways / 2) * 15) * Mathf.Deg2Rad))
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(32, r));
                        EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                    }
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }, token);


            token.ThrowIfCancellationRequested();
            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            EffectManager.I.Play(EffectType.Caution, transform.position + BallisticMath.FromPolar(128, playerDirection));

            await UniTask.Delay(TimeSpan.FromSeconds(2), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);


            token.ThrowIfCancellationRequested();
            foreach (var r in BallisticMath.CalculateWayRadians(playerDirection, 3, 10 * Mathf.Deg2Rad))
            {
                var bullet = BulletBase.Instantiate(m_HugeShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(64, r));
                EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
            }
            for (int i = 1; i < 4; i++)
            {
                foreach (var r in BallisticMath.CalculateWayRadians(playerDirection, 3, 10 * Mathf.Deg2Rad))
                {
                    var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(64 * i / 4, r));
                }
            }
            SoundManager.I.PlaySe(SeType.Explosion).Forget();


            await RandomMove(2, token);
        }


        private async UniTask SkillEliminateTarget(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();


            await UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.5), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(8)
                .ForEachAsync(i =>
                {
                    RandomMove(0.5f, token).Forget();

                    float playerDirection = BallisticMath.AimToPlayer(transform.position);
                    var bullet = BulletBase.Instantiate(m_HugeShotPrefab, transform.position, BallisticMath.FromPolar(32, playerDirection));
                    bullet.DOSpeed(BallisticMath.FromPolar(256, playerDirection), 1f);

                    UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.1), PlayerLoopTiming.FixedUpdate)
                        .Take(16)
                        .ForEachAsync(_ =>
                        {
                            var child = BulletBase.Instantiate(m_NormalShotPrefab, bullet.transform.position, Vector3.zero);
                            async UniTaskVoid destroy(CancellationToken token)
                            {
                                await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: token);
                                EffectManager.I.Play(child.EffectOnDestroy, child.transform.position);
                                Destroy(child.gameObject);
                            }
                            destroy(child.GetCancellationTokenOnDestroy()).Forget();

                            SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                        }, bullet.GetCancellationTokenOnDestroy());

                    EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                    SoundManager.I.PlaySe(SeType.Explosion).Forget();
                }, token);


            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

            token.ThrowIfCancellationRequested();

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();

            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

            {
                float playerDirection = BallisticMath.AimToPlayer(transform.position);
                foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 4))
                {
                    var bullet = BulletBase.Instantiate(m_HugeShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(16, playerDirection));
                    _ = bullet.DOSpeed(BallisticMath.FromPolar(192, playerDirection), 1).SetDelay(0.5f);
                }

                foreach (var r in BallisticMath.CalculateWayRadians(playerDirection, 12, 5 * Mathf.Deg2Rad))
                {
                    BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(Seiran.Shared.NextSingle(16, 64), r));
                }

                EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                SoundManager.I.PlaySe(SeType.Explosion).Forget();
            }
        }

    }
}
