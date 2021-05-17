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
    public class MidBossKarin : BossBase
    {
        [SerializeField]
        private GameObject m_NormalShotPrefab;

        [SerializeField]
        private GameObject m_HugeShotPrefab;

        [SerializeField]
        private GameObject m_RewardItemPrefab;



        private void Start()
        {
            OnStart().Forget();
        }


        private async UniTaskVoid OnStart()
        {
            var destroyToken = this.GetCancellationTokenOnDestroy();

            SetupHealthGauge(1, destroyToken);
            DamageWhenEnter(destroyToken).Forget();
            SuppressEnemySpawn(destroyToken);

            await RandomMove(2, destroyToken);


            Health = HealthMax = 8000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .Take(4)
                    .WithCancellation(token))
                {
                    await PlayerAimShot1(token);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(4), cancellationToken: token);
            }, destroyToken);
            await OnEndPhaseShort(destroyToken);


            await EscapeEvent(m_RewardItemPrefab, destroyToken);
            destroyToken.ThrowIfCancellationRequested();
            Destroy(gameObject);
        }




        private async UniTask PlayerAimShot1(CancellationToken token)
        {
            await RandomMove(1, token);

            {
                token.ThrowIfCancellationRequested();

                float playerDirection = BallisticMath.AimToPlayer(transform.position);
                await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.25), PlayerLoopTiming.FixedUpdate)
                    .Take(4)
                    .ForEachAsync(_ =>
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(Seiran.Shared.NextSingle(16, 32), playerDirection + Seiran.Shared.NextSingle(-45, 45) * Mathf.Deg2Rad));
                        }

                        var bullet = BulletBase.Instantiate(m_HugeShotPrefab, transform.position + BallisticMath.FromPolar(16, playerDirection), BallisticMath.FromPolar(64, playerDirection));

                        EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                        SoundManager.I.PlaySe(SeType.Explosion).Forget();
                    }, token);
            }

            token.ThrowIfCancellationRequested();
            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

            {
                token.ThrowIfCancellationRequested();

                float playerDirection = BallisticMath.AimToPlayer(transform.position);
                await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.25), PlayerLoopTiming.FixedUpdate)
                    .Select((_, i) => i)
                    .Take(4)
                    .ForEachAsync(i =>
                    {
                        foreach (var r in BallisticMath.CalculateWayRadians(playerDirection, i + 1, 10 * Mathf.Deg2Rad))
                        {
                            var bullet = BulletBase.Instantiate(m_HugeShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(64, r));

                            EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                        }
                        SoundManager.I.PlaySe(SeType.Explosion).Forget();
                    }, token);
            }
        }
    }
}
