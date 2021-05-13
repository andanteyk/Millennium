using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
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
        }

    }
}
