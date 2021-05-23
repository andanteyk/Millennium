using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{
    public class BossAsuna : BossBase
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
                await PlaySkillBalloon("アスナ", "いっくよー!", token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillLetsGo(token);
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
                    await Spiral(token);
                }
            }, destroyToken);
            await OnEndPhaseShort(destroyToken);


            Health = HealthMax = 16000;
            await RunPhase(async token =>
            {
                await PlaySkillBalloon("アスナ", "これは いたいよ?", token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillItHurts(token);
                }
            }, destroyToken);
            await DropUltimateAccelerant(false, destroyToken);
            await OnEndPhaseShort(destroyToken);


            Health = HealthMax = 16000;
            await RunPhase(async token =>
            {
                await PlaySkillBalloon("アスナ", "スピード あげるね?", token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillSpeedUp(token);
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


            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();


            await UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(30)
                .ForEachAsync(i =>
                {
                    var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(64, BallisticMath.AimToPlayer(transform.position)));

                    static async UniTaskVoid BulletMove(BulletBase bullet, int index)
                    {
                        var token = bullet.GetCancellationTokenOnDestroy();

                        token.ThrowIfCancellationRequested();
                        float baseAngle = BallisticMath.AimToPlayer(bullet.transform.position);
                        await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);

                        token.ThrowIfCancellationRequested();
                        await bullet.DOSpeedAngle(64, baseAngle, baseAngle + ((index & 1) * 2 - 1) * Mathf.PI * 2, 1).ToUniTask(cancellationToken: token);

                        bullet.Speed += Seiran.Shared.InsideUnitCircle() * 32;
                    }

                    BulletMove(bullet, i).Forget();

                    EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }, token);


            await RandomMove(1, token);
        }




        private async UniTask Spiral(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await RandomMove(1, token);


            async UniTask SpiralInner(float loopDirection, CancellationToken token)
            {

                EffectManager.I.Play(EffectType.Concentration, transform.position);
                SoundManager.I.PlaySe(SeType.Concentration).Forget();

                await UniTask.Delay(TimeSpan.FromSeconds(2), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);

                await Enumerable.Repeat(Seiran.Shared.NextRadian(), 80).Select((start, i) => start + loopDirection * i * 360f / 20 * Mathf.Deg2Rad).ToUniTaskAsyncEnumerable()
                    .ForEachAwaitAsync(async baseRadian =>
                    {
                        foreach (var r in BallisticMath.CalculateWayRadians(baseRadian, 2))
                        {
                            var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(64, r));

                            static async UniTaskVoid BulletMove(BulletBase bullet, float baseRadian)
                            {
                                var token = bullet.GetCancellationTokenOnDestroy();

                                token.ThrowIfCancellationRequested();
                                await bullet.DOSpeed(Vector3.zero, 1).WithCancellation(token);
                                token.ThrowIfCancellationRequested();
                                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                                await bullet.DOSpeed(BallisticMath.FromPolar(96, baseRadian - Mathf.PI * 3 / 4), 1).WithCancellation(token);
                            }

                            BulletMove(bullet, r).Forget();

                            EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                        }


                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();

                        await UniTask.Delay(TimeSpan.FromSeconds(0.05), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                    }, token);
            }


            await SpiralInner(1, token);
            await SpiralInner(-1, token);

        }




        private async UniTask SkillLetsGo(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();


            async UniTask Shot(CancellationToken token)
            {
                await Enumerable.Repeat(Seiran.Shared.NextRadian(), 20).Select((start, i) => start + i * 360f / 20 * Mathf.Deg2Rad).ToUniTaskAsyncEnumerable()
                    .ForEachAwaitAsync(async baseRadian =>
                    {
                        foreach (var r in BallisticMath.CalculateWayRadians(baseRadian, 2))
                        {
                            var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(64, r));
                            EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                        }

                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();

                        await UniTask.Delay(TimeSpan.FromSeconds(0.05), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                    }, token);

                await UniTask.Delay(TimeSpan.FromSeconds(0.5), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);

                switch (Seiran.Shared.Next(3))
                {
                    case 0:
                        foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 12))
                        {
                            var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(32, r), BallisticMath.FromPolar(64, r + Mathf.PI * 5 / 4));
                            EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                        }
                        SoundManager.I.PlaySe(SeType.Explosion).Forget();
                        break;

                    case 1:
                        for (int i = 0; i < 12; i++)
                        {
                            var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(Seiran.Shared.NextSingle(32, 96), BallisticMath.AimToPlayer(transform.position) + Seiran.Shared.NextSingle(-20, 20) * Mathf.Deg2Rad));
                        }
                        EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                        SoundManager.I.PlaySe(SeType.Explosion).Forget();
                        break;

                    case 2:
                        {
                            var direction = BallisticMath.AimToPlayer(transform.position);
                            foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 12))
                            {
                                var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(32, r), BallisticMath.FromPolar(64, direction));
                                EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                            }
                        }
                        SoundManager.I.PlaySe(SeType.Explosion).Forget();
                        break;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.5), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);

            }


            async UniTask Move(CancellationToken token)
            {
                for (int i = 0; i < 4; i++)
                    await RandomMove(0.5f, token);
            }


            await (Shot(token), Move(token));
        }



        private async UniTask SkillItHurts(CancellationToken token)
        {
            await (
                UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                    .Select((_, i) => i)
                    .ForEachAsync(i =>
                    {
                        float baseRadian = i * 18 * Mathf.Deg2Rad;
                        foreach (var r in BallisticMath.CalculateWayRadians(baseRadian, 2))
                        {
                            var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(64, r));
                            bullet.DOSpeedAngle(64, r, r + Mathf.PingPong(i / 20f, Mathf.PI) - Mathf.PI / 2, 1.5f);
                            EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                        }
                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                    }, token),

                UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(2), PlayerLoopTiming.FixedUpdate)
                    .ForEachAwaitWithCancellationAsync((_, token) => RandomMove(2, token), token));
        }



        private async UniTask SkillSpeedUp(CancellationToken token)
        {
            await Enumerable.Range(0, 5).Select(i => (5 - i) / 2.5f).ToUniTaskAsyncEnumerable()
                .ForEachAwaitWithCancellationAsync(async (moveSeconds, token) =>
                {
                    await MoveTo(new Vector3(-48 * Mathf.Sign(transform.position.x), 64) + Seiran.Shared.InsideUnitCircle() * 16, moveSeconds, token);

                    float baseRadian = Seiran.Shared.NextRadian();

                    await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                        .Select((_, i) => i)
                        .Take(30)
                        .ForEachAsync(i =>
                        {
                            foreach (var r in BallisticMath.CalculateWayRadians(baseRadian + i * 27 / moveSeconds * Mathf.Deg2Rad, 2))
                            {
                                var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(48 / moveSeconds, r - Mathf.PI * 5 / 8));
                                EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                            }
                            SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                        }, token);
                }, token);

        }

    }
}