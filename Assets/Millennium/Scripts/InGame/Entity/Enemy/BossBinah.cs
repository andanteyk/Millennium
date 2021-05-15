using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using Millennium.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{
    public class BossBinah : BossBase
    {
        [SerializeField]
        private GameObject m_NormalShotPrefab;

        [SerializeField]
        private GameObject m_LargeShotPrefab;

        [SerializeField]
        private GameObject m_HugeShotPrefab;


        [SerializeField]
        private GameObject[] m_Bodies;


        private Sequence[] m_MoveSequences;


        private void Start()
        {
            OnStart().Forget();
        }


        private async UniTaskVoid OnStart()
        {
            var destroyToken = this.GetCancellationTokenOnDestroy();

            SetupHealthGauge(12, destroyToken);
            EffectManager.I.Play(EffectType.Warning, Vector3.zero);
            SoundManager.I.PlaySe(SeType.Warning).Forget();


            // TODO: 多段ヒットするのでアリス大正義すぎる

            /*

            Health = HealthMax = 50000;
            await RunPhase(async token =>
            {
                await MoveAppeared(token);

                await (
                    MoveEight(token),
                    UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5), PlayerLoopTiming.FixedUpdate)
                        .Select((_, i) => i)
                        .ForEachAsync(i =>
                        {
                            foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 8))
                            {
                                var body = m_Bodies[i % m_Bodies.Length];
                                BulletBase.Instantiate(m_NormalShotPrefab, body.transform.position, BallisticMath.FromPolar(64, r));

                                EffectManager.I.Play(EffectType.MuzzleFlash, body.transform.position);
                                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                            }
                        }, token)
                    );
            }, destroyToken);
            await OnEndPhase(destroyToken);



            Health = HealthMax = 50000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await Tackle(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);



            Health = HealthMax = 30000;
            await RunPhase(async token =>
            {
                await Around(token);
            }, destroyToken);
            await OnEndPhase(destroyToken);


            Health = HealthMax = 50000;
            await RunPhase(async token =>
            {
                await MoveStraight(new Vector3(0, 64), 2, token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await Breath(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);


            Health = HealthMax = 50000;
            await RunPhase(async token =>
            {
                await (
                    MoveEight(token),
                    UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5), PlayerLoopTiming.FixedUpdate)
                        .Select((_, i) => i)
                        .ForEachAsync(i =>
                        {
                            var body = m_Bodies[i * 2 % m_Bodies.Length];
                            foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 6))
                            {
                                BulletBase.Instantiate(m_NormalShotPrefab, body.transform.position, BallisticMath.FromPolar(32, r));
                            }
                            EffectManager.I.Play(EffectType.MuzzleFlash, body.transform.position);
                            SoundManager.I.PlaySe(SeType.EnemyShot).Forget();


                            if (i % 2 == 0)
                            {
                                foreach (var r in BallisticMath.CalculateWayRadians(BallisticMath.AimToPlayer(m_Bodies[0].transform.position), 3, 30 * Mathf.Deg2Rad))
                                {
                                    BulletBase.Instantiate(m_LargeShotPrefab, m_Bodies[0].transform.position, BallisticMath.FromPolar(64, r));
                                }
                                EffectManager.I.Play(EffectType.MuzzleFlash, m_Bodies[0].transform.position);
                                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                            }
                        }, token)
                    );
            }, destroyToken);
            await OnEndPhase(destroyToken);


             Health = HealthMax = 50000;
            await RunPhase(async token =>
            {
                await MoveStraight(new Vector3(0, 64), 2, token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .WithCancellation(token))
                {
                    await Missile(token);
                }
            }, destroyToken);
            await OnEndPhase(destroyToken);

            Health = HealthMax = 50000;
            await RunPhase(async token =>
            {
                await (
                    MoveEight(token),
                    UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.125), PlayerLoopTiming.FixedUpdate)
                        .Select((_, i) => i)
                        .ForEachAsync(i =>
                        {
                            foreach (var r in BallisticMath.CalculateWayRadians(i * 18 * Mathf.Deg2Rad, 3))
                            {
                                BulletBase.Instantiate(m_NormalShotPrefab, m_Bodies[0].transform.position, BallisticMath.FromPolar(64, r));
                            }
                            EffectManager.I.Play(EffectType.MuzzleFlash, m_Bodies[0].transform.position);
                            SoundManager.I.PlaySe(SeType.EnemyShot).Forget();


                            if ((i & 7) == 0)
                            {
                                var tailTransform = m_Bodies[m_Bodies.Length - 1].transform;
                                foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 10))
                                {
                                    BulletBase.Instantiate(m_NormalShotPrefab, tailTransform.position, BallisticMath.FromPolar(64, r));
                                }
                                EffectManager.I.Play(EffectType.MuzzleFlash, tailTransform.position);
                                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                            }
                        }, token)
                    );
            }, destroyToken);
            await OnEndPhase(destroyToken);

            //*/


            Health = HealthMax = 100000;
            await RunPhase(async token =>
            {
                await MoveStraight(new Vector3(0, 64), 2, token);

                await Sandstorm(token);
            }, destroyToken);
            await OnEndPhase(destroyToken);


            Health = HealthMax = 100000;
            await RunPhase(async token =>
            {
                await MoveAppeared(token);

                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                     .WithCancellation(token))
                {
                    await Rage(token);
                }
            }, destroyToken);
            //await OnEndPhase(destroyToken);



            await PlayBinahDeathEffect(destroyToken);

            if (destroyToken.IsCancellationRequested)
                return;
            Destroy(gameObject);
        }



        private async UniTask MoveAppeared(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                m_DamageRatio = 0.1f;

                foreach (var b in m_Bodies)
                    b.transform.position = new Vector3(-32, 128);

                var origin = new Vector3(-32, 32);
                await MoveStraight(origin, 2, token);
                await MoveCircularUpper(origin += new Vector3(32, -32), 1, token);
                await MoveCircularLower(origin += new Vector3(32, 32), 1, token);
                await MoveCircularUpper(origin += new Vector3(-32, 32), 1, token);
                await MoveCircularLower(origin += new Vector3(-32, -32), 1, token);
                await MoveStraight(origin += new Vector3(0, -32), 1, token);
                await PauseMove(token);

                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

                foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 16))
                {
                    BulletBase.Instantiate(m_NormalShotPrefab, m_Bodies[0].transform.position, BallisticMath.FromPolar(16, r));

                    EffectManager.I.Play(EffectType.MuzzleFlash, m_Bodies[0].transform.position);
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }

                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

            }
            finally
            {
                m_DamageRatio = 1;
            }
        }


        private async UniTask Tackle(CancellationToken token)
        {
            await MoveStraight(new Vector3(0, 64), 2, token);
            await MoveCircularLower(new Vector3(32, 32), 1, token);
            await MoveCircularUpper(new Vector3(0, 0), 1, token);
            await MoveCircularLower(new Vector3(-32, 32), 1, token);
            await MoveCircularUpper(new Vector3(0, 64), 1, token);


            token.ThrowIfCancellationRequested();
            EffectManager.I.Play(EffectType.Concentration, m_Bodies[0].transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();

            var playerPosition = BallisticMath.PlayerPosition;
            EffectManager.I.Play(EffectType.Caution, playerPosition + new Vector3(0, 32));

            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

            await TerminateMove(token);

            await Move((sequence, t) =>
            {
                sequence.Append(
                    t.DOMove(playerPosition, 1)
                    .SetEase(Ease.Linear));
            }, 0.1f, true, token: token);

            SoundManager.I.PlaySe(SeType.Explosion).Forget();


            await UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(0.25), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(m_Bodies.Length)
                .ForEachAsync(i =>
                {
                    foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 4))
                    {
                        BulletBase.Instantiate(m_NormalShotPrefab, m_Bodies[i].transform.position, BallisticMath.FromPolar(32, r));
                    }

                    EffectManager.I.Play(EffectType.MuzzleFlash, m_Bodies[i].transform.position);
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }, token);

            await TerminateMove(token);
        }


        private async UniTask Around(CancellationToken token)
        {
            async UniTask MoveAround(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    await MoveStraight(new Vector3(InGameConstants.FieldArea.xMax - 16, InGameConstants.FieldArea.yMax - 16), 3, token);
                    await MoveStraight(new Vector3(InGameConstants.FieldArea.xMax - 16, InGameConstants.FieldArea.yMin + 16), 3, token);
                    await MoveStraight(new Vector3(InGameConstants.FieldArea.xMin + 16, InGameConstants.FieldArea.yMin + 16), 3, token);
                    await MoveStraight(new Vector3(InGameConstants.FieldArea.xMin + 16, InGameConstants.FieldArea.yMax - 16), 3, token);
                }
            }

            await (
                MoveAround(token),
                UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.25), PlayerLoopTiming.FixedUpdate)
                    .Select((_, i) => i % m_Bodies.Length)
                    .ForEachAsync(i =>
                    {
                        foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 3))
                        {
                            BulletBase.Instantiate(m_NormalShotPrefab, m_Bodies[i].transform.position, BallisticMath.FromPolar(32, r));
                        }

                        EffectManager.I.Play(EffectType.MuzzleFlash, m_Bodies[i].transform.position);
                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                    }, token)
                );
        }




        private async UniTask Breath(CancellationToken token)
        {
            async UniTask MoveCircle(CancellationToken token)
            {
                await MoveCircularLower(new Vector3(32, 32), 1, token);
                await MoveCircularUpper(new Vector3(0, 0), 1, token);
                await MoveCircularLower(new Vector3(-32, 32), 1, token);
                await MoveCircularUpper(new Vector3(0, 64), 1, token);
            }

            await (
                MoveCircle(token),

                UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                .Take(4)
                .ForEachAsync(i =>
                {
                    var bodyTransform = m_Bodies[Seiran.Shared.Next(m_Bodies.Length)].transform;
                    foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 10))
                    {
                        BulletBase.Instantiate(m_NormalShotPrefab, bodyTransform.position, BallisticMath.FromPolar(32, r));
                    }

                    EffectManager.I.Play(EffectType.MuzzleFlash, bodyTransform.position);
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }, token));


            token.ThrowIfCancellationRequested();
            EffectManager.I.Play(EffectType.Concentration, m_Bodies[0].transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();

            var playerPosition = BallisticMath.PlayerPosition;
            var playerDirection = BallisticMath.AimToPlayer(m_Bodies[0].transform.position);
            EffectManager.I.Play(EffectType.Caution, playerPosition + new Vector3(0, 32));

            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);





            var huge = UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.125), PlayerLoopTiming.FixedUpdate)
                .Take(12)
                .ForEachAsync(i =>
                {
                    float direction = playerDirection + Seiran.Shared.NextSingle(-10, 10) * Mathf.Deg2Rad;
                    var bullet = BulletBase.Instantiate(m_HugeShotPrefab, m_Bodies[0].transform.position, BallisticMath.FromPolar(32, direction));
                    bullet.DOSpeed(BallisticMath.FromPolar(256, direction), 0.5f);

                    EffectManager.I.Play(EffectType.MuzzleFlash, m_Bodies[0].transform.position);
                    SoundManager.I.PlaySe(SeType.Explosion).Forget();
                }, token);

            var large = UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(3 / 8f), TimeSpan.FromSeconds(0.125), PlayerLoopTiming.FixedUpdate)
                .Take(16)
                .ForEachAsync(i =>
                {
                    BulletBase.Instantiate(m_LargeShotPrefab, m_Bodies[0].transform.position, BallisticMath.FromPolar(128, playerDirection + Seiran.Shared.NextSingle(-20, 20) * Mathf.Deg2Rad));

                    EffectManager.I.Play(EffectType.MuzzleFlash, m_Bodies[0].transform.position);
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }, token);

            var normal = UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(13 / 16f), TimeSpan.FromSeconds(0.25), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(12)
                .ForEachAsync(i =>
                {
                    foreach (var r in BallisticMath.CalculateWayRadians(i * Mathf.PI * 2 / 8 / 5, 5))
                    {
                        BulletBase.Instantiate(m_NormalShotPrefab, m_Bodies[0].transform.position, BallisticMath.FromPolar(32, r));
                    }

                    EffectManager.I.Play(EffectType.MuzzleFlash, m_Bodies[0].transform.position);
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }, token);

            await (huge, large, normal);


            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
        }





        private async UniTask Missile(CancellationToken token)
        {
            async UniTask MoveCircle(CancellationToken token)
            {
                await MoveCircularLower(new Vector3(32, 32), 1, token);
                await MoveCircularUpper(new Vector3(0, 0), 1, token);
                await MoveCircularLower(new Vector3(-32, 32), 1, token);
                await MoveCircularUpper(new Vector3(0, 64), 1, token);
            }

            await (
                MoveCircle(token),

                UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                .Take(4)
                .ForEachAsync(_ =>
                {
                    float baseRadian = Seiran.Shared.NextRadian();

                    foreach (var body in m_Bodies)
                    {
                        foreach (var r in BallisticMath.CalculateWayRadians(baseRadian, 1))
                        {
                            BulletBase.Instantiate(m_NormalShotPrefab, body.transform.position, BallisticMath.FromPolar(16, r));
                        }
                        EffectManager.I.Play(EffectType.MuzzleFlash, body.transform.position);
                    }
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }, token));


            token.ThrowIfCancellationRequested();
            EffectManager.I.Play(EffectType.Concentration, m_Bodies[0].transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();


            await UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.1), PlayerLoopTiming.FixedUpdate)
                 .Take(20)
                 .ForEachAsync(_ =>
                 {
                     var bodyTransform = m_Bodies[Seiran.Shared.Next(m_Bodies.Length)].transform;
                     var bullet = BulletBase.Instantiate(m_LargeShotPrefab, bodyTransform.position, BallisticMath.FromPolar(96, Seiran.Shared.NextSingle(Mathf.PI / 8, Mathf.PI * 7 / 8)));
                     BulletMove(bullet, bullet.GetCancellationTokenOnDestroy()).Forget();

                     async UniTaskVoid BulletMove(BulletBase bullet, CancellationToken token)
                     {
                         token.ThrowIfCancellationRequested();
                         await bullet.DOSpeed(Vector3.zero, 1).WithCancellation(token);
                         SoundManager.I.PlaySe(SeType.Fall).Forget();
                         token.ThrowIfCancellationRequested();
                         await bullet.DOSpeed(BallisticMath.FromPolar(64, Seiran.Shared.NextSingle(-Mathf.PI, 0)), 0.25f).WithCancellation(token);
                         await UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                            .Select((_, i) => i)
                            .Take(60)
                            .ForEachAsync(i =>
                            {
                                bullet.Speed = Vector3.RotateTowards(bullet.Speed, BallisticMath.PlayerPosition - bullet.transform.position, 120 * Mathf.Deg2Rad * Time.deltaTime, 0);
                                bullet.Speed += new Vector3(0, -64) * Time.deltaTime;
                            }, token);
                     }

                     EffectManager.I.Play(EffectType.MuzzleFlash, bodyTransform.position);
                     SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                 }, token);


            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
        }


        private async UniTask Sandstorm(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                m_DamageRatio = 0;
                foreach (var collider in GetComponentsInChildren<Collider2D>())
                    collider.enabled = false;
                Health = HealthMax;

                EffectManager.I.Play(EffectType.Concentration, m_Bodies[0].transform.position);
                SoundManager.I.PlaySe(SeType.Concentration).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);


                async UniTaskVoid MoveAway(CancellationToken token)
                {
                    await MoveStraight(new Vector3(128, 256), 4, token);
                    token.ThrowIfCancellationRequested();
                    foreach (var s in m_MoveSequences)
                        s?.Play();
                }
                token.ThrowIfCancellationRequested();
                MoveAway(token).Forget();
                m_DamageRatio = 1;



                // countdown by HP
                UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .ForEachAsync(_ =>
                    {
                        DealDamage(new DamageSource(this, 1000));
                    }, token).Forget();


                static IEnumerable<float> Linear(float start, float end, int way)
                {
                    for (int i = 0; i < way; i++)
                    {
                        yield return Mathf.Lerp(start, end, i / (way - 1f));
                    }
                }


                Health = 9;

                UniTask TopWall(float startSeconds, float intervalSeconds, CancellationToken token)
                {
                    return UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(startSeconds), TimeSpan.FromSeconds(intervalSeconds), PlayerLoopTiming.FixedUpdate)
                    .Select((_, i) => i)
                    .Take(4)
                    .ForEachAsync(i =>
                    {
                        foreach (var x in Linear(-128, 128, 9))
                        {
                            BulletBase.Instantiate(m_NormalShotPrefab, new Vector3(x + i * 4, 128), new Vector3(8, -32));
                            BulletBase.Instantiate(m_NormalShotPrefab, new Vector3(x + i * 4, 128), new Vector3(-8, -32));
                        }
                        SoundManager.I.PlaySe(SeType.Explosion).Forget();
                    }, token);
                }

                UniTask LeftWall(float startSeconds, float intervalSeconds, CancellationToken token)
                {
                    return UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(startSeconds), TimeSpan.FromSeconds(intervalSeconds), PlayerLoopTiming.FixedUpdate)
                        .Select((_, i) => i)
                        .Take(4)
                        .ForEachAsync(i =>
                        {
                            foreach (var y in Linear(-128, 128, 9))
                            {
                                BulletBase.Instantiate(m_NormalShotPrefab, new Vector3(-96, y + i * 4), new Vector3(32, 8));
                                BulletBase.Instantiate(m_NormalShotPrefab, new Vector3(-96, y + i * 4), new Vector3(32, -8));
                            }
                            SoundManager.I.PlaySe(SeType.Explosion).Forget();
                        }, token);
                }

                UniTask RightWall(float startSeconds, float intervalSeconds, CancellationToken token)
                {
                    return UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(startSeconds), TimeSpan.FromSeconds(intervalSeconds), PlayerLoopTiming.FixedUpdate)
                        .Select((_, i) => i)
                        .Take(4)
                        .ForEachAsync(i =>
                        {
                            foreach (var y in Linear(-128, 128, 9))
                            {
                                BulletBase.Instantiate(m_NormalShotPrefab, new Vector3(96, y + i * 4), new Vector3(-32, 8));
                                BulletBase.Instantiate(m_NormalShotPrefab, new Vector3(96, y + i * 4), new Vector3(-32, -8));
                            }
                            SoundManager.I.PlaySe(SeType.Explosion).Forget();
                        }, token);
                }


                await TopWall(2, 2, token);

                await LeftWall(4, 2, token);
                await RightWall(2, 2, token);

                token.ThrowIfCancellationRequested();
                SoundManager.I.PlaySe(SeType.Concentration).Forget();


                await (TopWall(4, 3, token), LeftWall(5, 3, token), RightWall(6, 3, token));



                {
                    var corner = new[] {
                        new Vector3(InGameConstants.FieldArea.xMax - 16, InGameConstants.FieldArea.yMax - 16),
                        new Vector3(InGameConstants.FieldArea.xMax - 16, InGameConstants.FieldArea.yMin + 16),
                        new Vector3(InGameConstants.FieldArea.xMin + 16, InGameConstants.FieldArea.yMin + 16),
                        new Vector3(InGameConstants.FieldArea.xMin + 16, InGameConstants.FieldArea.yMax - 16),
                    };


                    async UniTask AroundCircle(IEnumerable<Vector3> locations, float intervalSeconds, CancellationToken token)
                    {
                        SoundManager.I.PlaySe(SeType.Concentration).Forget();
                        UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(intervalSeconds), PlayerLoopTiming.FixedUpdate)
                           .Zip(locations.ToUniTaskAsyncEnumerable(), (a, b) => b)
                           .Subscribe(async origin =>
                           {
                               EffectManager.I.Play(EffectType.Caution, origin);
                               await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
                               foreach (var r in BallisticMath.CalculateWayRadians(Seiran.Shared.NextRadian(), 24))
                               {
                                   BulletBase.Instantiate(m_NormalShotPrefab, origin + BallisticMath.FromPolar(-16, r), BallisticMath.FromPolar(32, r));
                               }

                               SoundManager.I.PlaySe(SeType.Explosion).Forget();
                           }, token);
                        await UniTask.Delay(TimeSpan.FromSeconds(6), cancellationToken: token);
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

                    await AroundCircle(corner, 0.25f, token);
                    await AroundCircle(corner.Reverse(), 0.25f, token);

                    await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
                    token.ThrowIfCancellationRequested();
                    SoundManager.I.PlaySe(SeType.Concentration).Forget();

                    await AroundCircle(corner, 1 / 60f, token);
                }





                async UniTask SideStorm(bool isRight, float startSeconds, CancellationToken token)
                {
                    float sign = isRight ? 1 : -1;

                    UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(startSeconds - 2))
                        .ForEachAsync(_ => EffectManager.I.Play(EffectType.Caution, new Vector3((InGameConstants.FieldArea.xMax - 16) * sign, InGameConstants.FieldArea.yMax - 16)), token)
                        .Forget();

                    async UniTask StormUnit(GameObject bullet, float start, float speed, int density)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(start), cancellationToken: token);
                        token.ThrowIfCancellationRequested();
                        SoundManager.I.PlaySe(SeType.Explosion).Forget();

                        await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5 / density), PlayerLoopTiming.FixedUpdate)
                            .Zip(Linear(128, -128, density + 1).ToUniTaskAsyncEnumerable(), (a, b) => b)
                            .ForEachAsync(y =>
                            {
                                BulletBase.Instantiate(bullet, new Vector3(96 * sign, y), new Vector3(-speed * sign + Seiran.Shared.NextSingle(0, speed / 8) * sign, -speed / 4 - Seiran.Shared.NextSingle(0, speed / 8)));
                            }, token);
                    }

                    async UniTask Se()
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(startSeconds - 2), cancellationToken: token);
                        SoundManager.I.PlaySe(SeType.Concentration).Forget();
                    }

                    await (
                        StormUnit(m_NormalShotPrefab, startSeconds, 64, 8),
                        StormUnit(m_NormalShotPrefab, startSeconds + 0.5f, 64, 8),
                        StormUnit(m_NormalShotPrefab, startSeconds + 0.5f, 48, 12),
                        StormUnit(m_NormalShotPrefab, startSeconds + 1, 48, 12),
                        StormUnit(m_NormalShotPrefab, startSeconds + 1, 32, 16),
                        StormUnit(m_NormalShotPrefab, startSeconds + 1.5f, 24, 16),
                        StormUnit(m_NormalShotPrefab, startSeconds + 1.5f, 16, 16),
                        Se());
                }

                await SideStorm(true, 6, token);
                await SideStorm(false, 6, token);



                await UniTask.Delay(TimeSpan.FromSeconds(6), cancellationToken: token);
                token.ThrowIfCancellationRequested();
                SoundManager.I.PlaySe(SeType.Concentration).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);



                async UniTask FrontStorm(GameObject bullet, float start, float speed, int density, CancellationToken token)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(start), cancellationToken: token);
                    token.ThrowIfCancellationRequested();
                    SoundManager.I.PlaySe(SeType.Explosion).Forget();

                    await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5 / density), PlayerLoopTiming.FixedUpdate)
                        .Zip(Linear(-96, 96, density + 1).ToUniTaskAsyncEnumerable(), (a, b) => b)
                        .ForEachAsync(x =>
                        {
                            BulletBase.Instantiate(bullet, new Vector3(x, 128), new Vector3(Seiran.Shared.NextSingle(-speed / 8, speed / 8), -Seiran.Shared.NextSingle(speed * 7 / 8, speed * 9 / 8)));
                        }, token);
                }

                await (
                    FrontStorm(m_NormalShotPrefab, 0, 64, 8, token),
                    FrontStorm(m_NormalShotPrefab, 0.5f, 64, 8, token),
                    FrontStorm(m_NormalShotPrefab, 0.5f, 48, 12, token),
                    FrontStorm(m_NormalShotPrefab, 1f, 48, 12, token),
                    FrontStorm(m_NormalShotPrefab, 1f, 32, 16, token),
                    FrontStorm(m_NormalShotPrefab, 1.25f, 24, 16, token),
                    FrontStorm(m_NormalShotPrefab, 1.25f, 16, 16, token),
                    FrontStorm(m_LargeShotPrefab, 1.5f, 24, 8, token),
                    FrontStorm(m_LargeShotPrefab, 1.5f, 16, 8, token));

                await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: token);
                token.ThrowIfCancellationRequested();
                SoundManager.I.PlaySe(SeType.Concentration).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

                await (
                    FrontStorm(m_NormalShotPrefab, 0, 64, 8, token),
                    FrontStorm(m_NormalShotPrefab, 0.5f, 64, 8, token),
                    FrontStorm(m_NormalShotPrefab, 0.5f, 48, 12, token),
                    FrontStorm(m_NormalShotPrefab, 1f, 48, 12, token),
                    FrontStorm(m_NormalShotPrefab, 1f, 32, 16, token),
                    FrontStorm(m_LargeShotPrefab, 1.25f, 24, 8, token),
                    FrontStorm(m_LargeShotPrefab, 1.25f, 16, 8, token),
                    FrontStorm(m_HugeShotPrefab, 1.5f, 24, 4, token),
                    FrontStorm(m_HugeShotPrefab, 1.5f, 16, 4, token));

                await UniTask.Delay(TimeSpan.FromSeconds(60), cancellationToken: token);
            }
            finally
            {
                m_DamageRatio = 1;
                if (this != null)
                {
                    foreach (var collider in GetComponentsInChildren<Collider2D>())
                        collider.enabled = true;
                }
            }
        }




        private async UniTask Rage(CancellationToken token)
        {
            EffectManager.I.Play(EffectType.Concentration, m_Bodies[0].transform.position);

            // TODO
        }




        /// <summary>
        /// 8 の字
        /// </summary>
        private async UniTask MoveEight(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 手作業つらくないですか_(:3｣∠)_
                await MoveStraight(new Vector3(64, 64), 3, token);
                await MoveCircularLower(new Vector3(80, 48), 1, token);
                await MoveCircularUpper(new Vector3(64, 32), 1, token);
                await MoveStraight(new Vector3(-64, 32), 3, token);
                await MoveCircularLower(new Vector3(-80, 16), 1, token);
                await MoveCircularUpper(new Vector3(-64, 0), 1, token);
                await MoveStraight(new Vector3(64, 0), 3, token);
                await MoveCircularLower(new Vector3(80, 16), 1, token);
                await MoveCircularUpper(new Vector3(64, 32), 1, token);
                await MoveStraight(new Vector3(-64, 32), 3, token);
                await MoveCircularLower(new Vector3(-80, 48), 1, token);
                await MoveCircularUpper(new Vector3(-64, 64), 1, token);
            }
        }



        private UniTask PauseMove(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (m_MoveSequences == null)
                return UniTask.CompletedTask;

            foreach (var s in m_MoveSequences)
                _ = s?.Pause();

            return UniTask.CompletedTask;
        }

        private UniTask TerminateMove(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (m_MoveSequences == null)
                return UniTask.CompletedTask;

            foreach (var s in m_MoveSequences)
                s?.Kill();

            return UniTask.CompletedTask;
        }


        private async UniTask Move(Action<Sequence, Transform> sequenceSetter, float unitDelaySeconds = 0.5f, bool pauseAtEnd = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();


            m_MoveSequences ??= new Sequence[m_Bodies.Length];
            foreach (var s in m_MoveSequences)
                s?.Play();

            for (int i = 0; i < m_Bodies.Length; i++)
            {
                m_MoveSequences[i] = DOTween.Sequence()
                    .AppendInterval(unitDelaySeconds * i);

                sequenceSetter(m_MoveSequences[i], m_Bodies[i].transform);

                _ = m_MoveSequences[i].SetUpdate(UpdateType.Fixed)
                    .SetLink(m_Bodies[i])
                    .WithCancellation(token);
            }

            await m_MoveSequences[0];

            if (pauseAtEnd)
                await PauseMove(token);
        }

        private UniTask MoveStraight(Vector3 destination, float duration, CancellationToken token)
        {
            return Move((sequence, t) =>
            {
                sequence.Append(
                    t.DOMove(destination, duration)
                    .SetEase(Ease.Linear));
            }, token: token);
        }


        /// <summary> 
        /// 　時計回り ◞ ◜
        /// 反時計回り ◝ ◟
        /// </summary>
        private UniTask MoveCircularUpper(Vector3 destination, float duration, CancellationToken token)
        {
            return Move((sequence, t) =>
            {
                sequence.Append(
                    t.DOMoveX(destination.x, duration)
                    .SetEase(Ease.InSine));
                sequence.Join(
                    t.DOMoveY(destination.y, duration)
                    .SetEase(Ease.OutSine));
            }, token: token);
        }

        /// <summary>
        /// 　時計回り ◝ ◟
        /// 反時計回り ◞ ◜
        /// </summary>
        private UniTask MoveCircularLower(Vector3 destination, float duration, CancellationToken token)
        {
            return Move((sequence, t) =>
            {
                sequence.Append(
                    t.DOMoveX(destination.x, duration)
                    .SetEase(Ease.OutSine));
                sequence.Join(
                    t.DOMoveY(destination.y, duration)
                    .SetEase(Ease.InSine));
            }, token: token);
        }




        protected async UniTask PlayBinahDeathEffect(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                await PauseMove(token);

                Time.timeScale = 1 / 3f;

                Instantiate(m_BulletRemoverPrefab).transform.position = transform.position;
                InGameUI.I.BossHealthGauge.DecrementSubGauge();

                SoundManager.I.StopBgm(4f).Forget();

                await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.125))
                    .Take(8)
                    .ForEachAsync(_ =>
                    {
                        EffectManager.I.Play(EffectType.Explosion, new Vector3(0, -32) + Seiran.Shared.InsideUnitCircle() * 64);
                        SoundManager.I.PlaySe(SeType.Explosion).Forget();
                    }, token);

                await UniTask.Delay(TimeSpan.FromSeconds(0.25), cancellationToken: token);

                await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.125))
                    .Select((_, i) => m_Bodies.Length - 1 - i)
                    .Take(m_Bodies.Length - 1)
                    .ForEachAsync(i =>
                    {
                        m_Bodies[i].GetComponent<SpriteRenderer>().enabled = false;
                        EffectManager.I.Play(EffectType.Explosion, m_Bodies[i].transform.position);
                        for (int k = 0; k < 3; k++)
                            EffectManager.I.Play(EffectType.PlusDecay, m_Bodies[i].transform.position + Seiran.Shared.InsideUnitCircle() * 16);
                        SoundManager.I.PlaySe(SeType.Explosion).Forget();
                    }, token);

                token.ThrowIfCancellationRequested();
                EffectManager.I.Play(EffectType.Concentration, m_Bodies[0].transform.position);

                await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);

                token.ThrowIfCancellationRequested();
                EffectManager.I.Play(EffectType.Explosion, m_Bodies[0].transform.position);
                EffectManager.I.Play(EffectType.SpreadExplosion, m_Bodies[0].transform.position);
                SoundManager.I.PlaySe(SeType.SpreadExplosion).Forget();

                transform.position = new Vector3(0, 1024);          // 雑に非表示にする

                await UniTask.Delay(TimeSpan.FromSeconds(0.25), cancellationToken: token);

                Time.timeScale = 1f;

                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

                EffectManager.I.Play(EffectType.StageClear, Vector3.zero);

                await UniTask.Delay(TimeSpan.FromSeconds(6), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                Time.timeScale = 1f;
                GotoNextStage();
            }
        }
    }
}
