using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.InGame.Stage;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{

    public class MidBossNeru : BossBase
    {

        [SerializeField]
        private GameObject m_NormalShotPrefab;

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
                    .Take(6)
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
            token.ThrowIfCancellationRequested();

            EffectManager.I.Play(EffectType.Concentration, transform.position).SetParent(transform);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();

            await RandomMove(1, token);

            await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                .Take(10)
                .ForEachAsync(_ =>
                {
                    var baseRadian = BallisticMath.AimToPlayer(transform.position) + Seiran.Shared.NextSingle(-10, 10) * Mathf.Deg2Rad;
                    foreach (var r in BallisticMath.CalculateWayRadians(baseRadian, 3, 45))
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(16, r));
                        bullet.DOSpeed(BallisticMath.FromPolar(128, r), 1);

                        EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                    }
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }, token);

            await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);

            await UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.25), PlayerLoopTiming.FixedUpdate)
                .Take(3)
                .ForEachAsync(_ =>
                {
                    var baseRadian = BallisticMath.AimToPlayer(transform.position);
                    foreach (var r in BallisticMath.CalculateWayRadians(baseRadian, 11))
                    {
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(16, r));
                        bullet.DOSpeed(BallisticMath.FromPolar(128, r), 1);

                        EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                    }
                    SoundManager.I.PlaySe(SeType.Explosion).Forget();
                }, token);
        }
    }
}
