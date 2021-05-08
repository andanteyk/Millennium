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

            SetupHealthGauge(4, destroyToken);

            await RandomMove(1, destroyToken);

            //*
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
                await MoveTo(new Vector3(64 * (Seiran.Shared.Next(0, 2) * 2 - 1), 64), 1, token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillFireSupport(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);



            Health = HealthMax = 5000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await PlayerAimshot2(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);



            Health = HealthMax = 10000;
            await RunPhase(async token =>
            {
                await MoveTo(new Vector3(64 * (Seiran.Shared.Next(0, 2) * 2 - 1), 64), 1, token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(2), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillEliminateTarget(token);
                }
            }, destroyToken);
            //await OnEndPhase(destroyToken);



            await PlayDeathEffect(destroyToken);

            if (destroyToken.IsCancellationRequested)
                return;
            Destroy(gameObject);
        }




        private async UniTask PlayerAimshot1(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            await RandomMove(1.5f, token);

            EffectManager.I.Play(EffectType.Caution, BallisticMath.PlayerPosition + Vector3.up * 32);
            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);


            float playerDirection = BallisticMath.AimToPlayer(transform.position);
            {
                var bullet = BulletBase.Instantiate(m_HugeShotPrefab, transform.position, BallisticMath.FromPolar(16, playerDirection));
                _ = bullet.DOSpeed(BallisticMath.FromPolar(256, playerDirection), 1f);
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
            if (token.IsCancellationRequested)
                return;

            await RandomMove(1, token);

            float playerDirection = BallisticMath.AimToPlayer(transform.position);
            var bullet = BulletBase.Instantiate(m_HugeShotPrefab, transform.position, BallisticMath.FromPolar(96, playerDirection));
            async UniTaskVoid explode(BulletBase bullet, CancellationToken token)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.75), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);

                if (token.IsCancellationRequested || bullet == null)
                    return;

                foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 24))
                {
                    BulletBase.Instantiate(m_NormalShotPrefab, bullet.transform.position, BallisticMath.FromPolar(Seiran.Shared.NextSingle(16, 64), r));
                }

                EffectManager.I.Play(EffectType.Explosion, bullet.transform.position);
                SoundManager.I.PlaySe(SeType.Explosion).Forget();
                Destroy(bullet.gameObject);
            }
            explode(bullet, token).Forget();

            SoundManager.I.PlaySe(SeType.Explosion).Forget();

        }



        private async UniTask SkillFireSupport(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

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


        private async UniTask SkillEliminateTarget(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

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

            if (token.IsCancellationRequested)
                return;

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
