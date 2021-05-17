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
    public class MidBossAkane : BossBase
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
                    .Take(4)
                    .WithCancellation(token))
                {
                    await ReflectionShot(token);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(4), cancellationToken: token);
            }, destroyToken);
            await OnEndPhaseShort(destroyToken);


            await EscapeEvent(m_RewardItemPrefab, destroyToken);
            destroyToken.ThrowIfCancellationRequested();
            Destroy(gameObject);
        }




        private async UniTask ReflectionShot(CancellationToken token)
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
