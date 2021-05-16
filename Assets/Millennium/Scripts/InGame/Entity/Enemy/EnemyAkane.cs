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

namespace Millennium.InGame.Entity.Enemy
{
    public class EnemyAkane : BossBase
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
                    await ReflectionShot(token);
                }
            }, destroyToken);
            await OnEndPhaseShort(destroyToken);


            Health = HealthMax = 16000;
            await RunPhase(async token =>
            {
                await PlaySkillBalloon("アカネ", "じょうひんに つらぬきます", token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillElegantPenetration(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);


            Health = HealthMax = 8000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await ReflectionShot2(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);


            Health = HealthMax = 16000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await ThrowBomb(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);
            //*/


            Health = HealthMax = 16000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await Choudan(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);


            await PlayDeathEffect(destroyToken);

            if (destroyToken.IsCancellationRequested)
                return;
            Destroy(gameObject);
        }




        private async UniTask ReflectionShot(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);


            async UniTask Phase(float direction, CancellationToken token)
            {
                float baseRadian = Seiran.Shared.NextRadian();
                int ways = 24;
                await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                    .Select((_, i) => i)
                    .Take(ways)
                    .ForEachAsync(i =>
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(32, baseRadian + i * Mathf.PI * 2 / ways * direction));
                        ReflectionBullet(bullet, bullet.GetCancellationTokenOnDestroy()).Forget();

                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                        EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);

                    }, token);


                await RandomMove(2, token);
            }

            await Phase(1, token);
            await Phase(-1, token);
        }





        private async UniTask SkillElegantPenetration(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);



            UniTask AllRange(CancellationToken token)
            {
                float baseRadian = Seiran.Shared.NextRadian();
                int ways = 15;
                return UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.25), PlayerLoopTiming.FixedUpdate)
                    .Select((_, i) => i)
                    .ForEachAsync(i =>
                    {
                        foreach (var r in BallisticMath.CalculateWayRadians(baseRadian + i * Mathf.PI * 2 / ways, 2))
                        {
                            var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(32, r));
                            ReflectionBullet(bullet, bullet.GetCancellationTokenOnDestroy()).Forget();
                        }
                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                        EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);

                    }, token);
            }

            UniTask Ray(float radian, CancellationToken token)
            {
                return UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.1), PlayerLoopTiming.FixedUpdate)
                    .Take(8)
                    .ForEachAsync(_ =>
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(64, radian));
                        ReflectionBullet(bullet, bullet.GetCancellationTokenOnDestroy()).Forget();

                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                        EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                    }, token);
            }

            async UniTask Lined(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    await RandomMove(2, token);
                    foreach (var r in BallisticMath.CalculateWayRadians(BallisticMath.AimToPlayer(transform.position), 5, 60 * Mathf.Deg2Rad))
                        Ray(r, token).Forget();
                    await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
                }
            }


            await (AllRange(token), Lined(token));
        }



        private async UniTask ReflectionShot2(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            async UniTask Phase(float direction, float leftRight, CancellationToken token)
            {
                int ways = 25;
                float wayRadian = 6 * Mathf.Deg2Rad;
                await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                    .Select((_, i) => i)
                    .Take(ways)
                    .ForEachAsync(i =>
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar((i & 1) != 0 ? 48 : 32, direction + i * wayRadian * leftRight));
                        ReflectionBullet(bullet, bullet.GetCancellationTokenOnDestroy()).Forget();

                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                        EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);

                    }, token);


                await RandomMove(2, token);
            }

            await Phase(-Mathf.PI / 2, -1, token);
            await Phase(-Mathf.PI / 2, +1, token);
        }



        // あまりおもしろくない
        private async UniTask Botsu(CancellationToken token)
        {
            void Binder(CancellationToken token)
            {
                UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.1), PlayerLoopTiming.FixedUpdate)
                    .Select((_, i) => i)
                    .Subscribe(async i =>
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, BallisticMath.PlayerPosition + BallisticMath.FromPolar(48, i * 49 * Mathf.Deg2Rad), BallisticMath.FromPolar(4, Seiran.Shared.NextRadian()));
                        var bulletToken = bullet.GetCancellationTokenOnDestroy();

                        await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: bulletToken);

                        if (bulletToken.IsCancellationRequested)
                            return;

                        EffectManager.I.Play(bullet.EffectOnDestroy, bullet.transform.position);
                        Destroy(bullet.gameObject);
                    }, token);
            }

            UniTask Ray(float radian, CancellationToken token)
            {
                return UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.1), PlayerLoopTiming.FixedUpdate)
                    .Take(4)
                    .ForEachAsync(_ =>
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(64, radian));
                        ReflectionBullet(bullet, bullet.GetCancellationTokenOnDestroy()).Forget();

                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                        EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                    }, token);
            }

            UniTask Walk(CancellationToken token)
            {
                return UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                   .ForEachAwaitWithCancellationAsync(async (_, token) =>
                   {
                       await RandomMove(3, token);
                   }, token);
            }

            Binder(token);
            Walk(token).Forget();

            await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .ForEachAwaitWithCancellationAsync(async (i, token) =>
                {
                    await Ray(Seiran.Shared.NextSingle(-Mathf.PI, 0), token);
                }, token);
        }



        private async UniTask Choudan(CancellationToken token)
        {
            UniTask Ray(float radian, CancellationToken token)
            {
                return UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.1), PlayerLoopTiming.FixedUpdate)
                    .Take(8)
                    .ForEachAsync(_ =>
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(64, radian));
                        ReflectionBullet(bullet, bullet.GetCancellationTokenOnDestroy()).Forget();

                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                        EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                    }, token);
            }

            await (
                UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5), PlayerLoopTiming.FixedUpdate)
                .ForEachAsync(_ =>
                {
                    foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 8))
                    {
                        BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(64, r));
                    }

                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                    EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                }, token),

                UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    var field = InGameConstants.FieldArea;
                    var player = BallisticMath.PlayerPosition;

                    var leftPoint = new Vector3(-(player.x - field.xMin) + field.xMin, player.y);
                    var rightPoint = new Vector3(-(player.x - field.xMax) + field.xMax, player.y);

                    await (
                        Ray(Mathf.Atan2(leftPoint.y - transform.position.y, leftPoint.x - transform.position.x), token),
                        Ray(Mathf.Atan2(rightPoint.y - transform.position.y, rightPoint.x - transform.position.x), token));

                    await RandomMove(2, token);
                }, token)
                );
        }



        private async UniTask ThrowBomb(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            async UniTaskVoid Bomb(BulletBase bullet, CancellationToken token)
            {
                await UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                    .TakeWhile(_ => bullet.transform.position.y >= InGameConstants.FieldArea.yMin + 8)
                    .ForEachAsync(_ =>
                    {
                        bullet.Speed += Vector3.down * (32 * Time.deltaTime);
                    }, token);

                if (token.IsCancellationRequested)
                    return;

                bullet.Speed = Vector3.zero;
                EffectManager.I.Play(EffectType.Caution, bullet.transform.position);

                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
                EffectManager.I.Play(EffectType.Explosion, bullet.transform.position);
                SoundManager.I.PlaySe(SeType.Explosion).Forget();

                await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.125), PlayerLoopTiming.FixedUpdate)
                    .Take(4)
                    .ForEachAsync(_ =>
                    {
                        foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 4))
                        {
                            BulletBase.Instantiate(m_NormalShotPrefab, bullet.transform.position, BallisticMath.FromPolar(Seiran.Shared.NextSingle(16, 32), r));
                        }

                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                    }, token);

                if (token.IsCancellationRequested)
                    return;

                EffectManager.I.Play(bullet.EffectOnDestroy, bullet.transform.position);
                Destroy(bullet.gameObject);
            }


            await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.25), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(3)
                .ForEachAsync(i =>
                {
                    var bullet = BulletBase.Instantiate(m_HugeShotPrefab, transform.position, BallisticMath.FromPolar(24, Mathf.PI / 2 + (i - 1) * 45 * Mathf.Deg2Rad));
                    Bomb(bullet, bullet.GetCancellationTokenOnDestroy()).Forget();

                    EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                    SoundManager.I.PlaySe(SeType.Explosion).Forget();
                }, token);

            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
            await RandomMove(2, token);
        }





        private async UniTaskVoid ReflectionBullet(BulletBase bullet, CancellationToken token)
        {
            await UniTask.WaitUntil(() => bullet.transform.position.x < InGameConstants.FieldArea.xMin || bullet.transform.position.x > InGameConstants.FieldArea.xMax,
                PlayerLoopTiming.FixedUpdate, cancellationToken: token);

            if (token.IsCancellationRequested)
                return;
            bullet.Speed = new Vector3(-bullet.Speed.x, bullet.Speed.y);
            SoundManager.I.PlaySe(SeType.PlayerBulletImmune).Forget();
        }

    }
}
