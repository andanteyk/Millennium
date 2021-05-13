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

            SetupHealthGauge(4, destroyToken);
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

            await RandomMove(1, token);

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();


            await UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                .Select((_, i) => i)
                .Take(30)
                .ForEachAsync(i =>
                {
                    var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(32, BallisticMath.AimToPlayer(transform.position)));

                    async UniTaskVoid BulletMove(BulletBase bullet, int index)
                    {
                        var token = bullet.GetCancellationTokenOnDestroy();

                        float baseAngle = BallisticMath.AimToPlayer(bullet.transform.position);
                        await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);
                        await bullet.DOSpeedAngle(64, baseAngle, baseAngle + ((index & 1) * 2 - 1) * Mathf.PI * 2, 3).ToUniTask(cancellationToken: token);

                        bullet.Speed += Seiran.Shared.InsideUnitCircle() * 32;
                    }

                    BulletMove(bullet, i).Forget();
                }, token);
        }

    }
}
