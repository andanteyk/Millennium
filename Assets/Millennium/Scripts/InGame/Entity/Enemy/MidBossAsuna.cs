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

    public class MidBossAsuna : BossBase
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


            Health = HealthMax = 12000;
            await RunPhase(async token =>
            {
                await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), PlayerLoopTiming.FixedUpdate)
                    .Take(4)
                    .WithCancellation(token))
                {
                    await CurveShot(token);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(4), cancellationToken: token);
            }, destroyToken);
            await OnEndPhase(destroyToken);




            await EscapeEvent(m_RewardItemPrefab, destroyToken);

            if (destroyToken.IsCancellationRequested)
                return;

            Destroy(gameObject);
        }



        private async UniTask CurveShot(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            await RandomMove(1, token);

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();


            UniTask Spiral(float directionMultiplier, CancellationToken token)
            {
                int density = 20;
                return UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.05), PlayerLoopTiming.FixedUpdate)
                    .Select((_, i) => i)
                    .Take(density * 2)
                    .ForEachAsync(i =>
                    {
                        var r = Mathf.PI / 2 + i * 360f / density * Mathf.Deg2Rad * directionMultiplier;
                        var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position + BallisticMath.FromPolar(16, r), BallisticMath.FromPolar(32, r));

                        bullet.DOSpeedAngle(48, r, r + Mathf.PI * directionMultiplier, 2);

                        EffectManager.I.Play(EffectType.MuzzleFlash, bullet.transform.position);
                        SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                    }, token);
            }

            await (Spiral(1, token), Spiral(-1, token));
        }



        // TODO: ‹¤’Ê‰»‚Æ‚©
        private async UniTask EscapeEvent(GameObject rewardItem, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            m_DamageRatio = 0;

            if (Health <= 0)
            {
                var item = Instantiate(rewardItem);
                item.transform.position = transform.position;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.5), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);

            await MoveTo(new Vector3(128, 128), 2, token);
        }

        private void SuppressEnemySpawn(CancellationToken destroyToken)
        {
            var stageManager = FindObjectOfType<StageManager>();
            stageManager.SuppressEnemySpawn();
            destroyToken.Register(() => stageManager.ResumeEnemySpawn());
        }
    }
}
