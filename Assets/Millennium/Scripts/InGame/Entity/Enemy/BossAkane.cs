using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{
    public class BossAkane : BossBase
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
            await DropUltimateAccelerant(false, destroyToken);
            await OnEndPhase(destroyToken);


            Health = HealthMax = 8000;
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
                await PlaySkillBalloon("アカネ", "てきかくに せいあつします", token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await SkillAccuratelySupremacy(token);
                }
            }, destroyToken);
            await DropUltimateAccelerant(false, destroyToken);
            await OnEndPhaseShort(destroyToken);


            Health = HealthMax = 16000;
            await RunPhase(async token =>
            {
                await PlaySkillBalloon("アカネ", "ゆうがに はいじょします", token);

                await SkillGracefullyTerminate(token);
            }, destroyToken);
            //await OnEndPhase(destroyToken);


            await PlayDeathEffect(destroyToken);

            destroyToken.ThrowIfCancellationRequested();
            Destroy(gameObject);
        }




        private async UniTask ReflectionShot(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

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
            token.ThrowIfCancellationRequested();

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

            UniTask Lined(CancellationToken token)
            {
                return UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(2), PlayerLoopTiming.FixedUpdate)
                    .ForEachAwaitWithCancellationAsync(async (_, token) =>
                    {
                        await RandomMove(2, token);
                        token.ThrowIfCancellationRequested();
                        foreach (var r in BallisticMath.CalculateWayRadians(BallisticMath.AimToPlayer(transform.position), 5, 60 * Mathf.Deg2Rad))
                        {
                            Ray(r, token).Forget();
                        }
                    }, token);
            }


            await (AllRange(token), Lined(token));
        }




        private async UniTask SkillGracefullyTerminate(CancellationToken token)
        {
            UniTask Binder(CancellationToken token)
            {
                return UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.1), PlayerLoopTiming.FixedUpdate)
                    .ForEachAsync(_ =>
                    {
                        var field = InGameConstants.FieldArea;
                        var player = BallisticMath.PlayerPosition + new Vector3(0, 48);

                        var targets = new[] {
                            new Vector3(-(player.x - field.xMin) + field.xMin, player.y) ,
                        new Vector3(-(player.x - field.xMax) + field.xMax, player.y)};

                        foreach (var point in targets)
                        {
                            var r = BallisticMath.AimTo(point, transform.position);

                            var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(256, r));
                            ReflectionBullet(bullet, bullet.GetCancellationTokenOnDestroy()).Forget();

                            SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                            EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                        }
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

            UniTask Shot(CancellationToken token)
            {
                return UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .ForEachAwaitWithCancellationAsync(async (i, token) =>
                {
                    await Ray(4, Seiran.Shared.NextSingle(-Mathf.PI, 0), token);
                }, token);
            }

            await (
                Binder(token),
                Walk(token),
                Shot(token)
                );
        }



        private UniTask Ray(int count, float radian, CancellationToken token)
        {
            return UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.1), PlayerLoopTiming.FixedUpdate)
                .Take(count)
                .ForEachAsync(_ =>
                {
                    var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(16, radian), BallisticMath.FromPolar(64, radian));
                    ReflectionBullet(bullet, bullet.GetCancellationTokenOnDestroy()).Forget();

                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                    EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                }, token);
        }


        private async UniTask SkillAccuratelySupremacy(CancellationToken token)
        {
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
                        Ray(8, BallisticMath.AimTo(leftPoint, transform.position), token),
                        Ray(8, BallisticMath.AimTo(rightPoint, transform.position), token));

                    await RandomMove(2, token);
                }, token)
                );
        }



        private async UniTask ThrowBomb(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            async UniTaskVoid Bomb(BulletBase bullet, CancellationToken token)
            {
                await UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                    .TakeWhile(_ => bullet.transform.position.y >= InGameConstants.FieldArea.yMin + 8)
                    .ForEachAsync(_ =>
                    {
                        bullet.Speed += Vector3.down * (32 * Time.deltaTime);
                    }, token);

                token.ThrowIfCancellationRequested();

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

                token.ThrowIfCancellationRequested();

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

            token.ThrowIfCancellationRequested();

            bullet.Speed = new Vector3(-bullet.Speed.x, bullet.Speed.y);
            SoundManager.I.PlaySe(SeType.PlayerBulletImmune).Forget();
        }

    }
}
