using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using Millennium.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{
    public class BossYuuka : BossBase
    {
        [SerializeField]
        private GameObject m_NormalShotPrefab;

        [SerializeField]
        private GameObject m_BulletRemoverPrefab;


        private CancellationTokenSource m_PhaseToken;


        private async void Start()
        {
            // TODO
            Health = HealthMax = 5000;

            var destroyToken = this.GetCancellationTokenOnDestroy();


            // TODO
            InGameUI.I.BossHealthGauge.SetSubGauge(2);
            InGameUI.I.BossHealthGauge.Show();
            UniTaskAsyncEnumerable.EveryUpdate()
                .ForEachAsync(_ =>
                {
                    InGameUI.I.BossHealthGauge.SetGauge(Health, HealthMax);
                }, destroyToken)
                .Forget();
            destroyToken.Register(() => InGameUI.I.BossHealthGauge.Hide());


            m_PhaseToken = new CancellationTokenSource();
            using (var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_PhaseToken.Token, destroyToken))
            {
                var token = combinedTokenSource.Token;

                await UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                    .ForEachAwaitAsync(async _ =>
                    {
                        await RandomMove(1, token);
                        await Baramaki(token);
                        await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);
                    }, token).SuppressCancellationThrow();
            }

            // test
            Instantiate(m_BulletRemoverPrefab).transform.position = transform.position;
            InGameUI.I.BossHealthGauge.SetSubGauge(1);

            EffectManager.I.Play(EffectType.Explosion, transform.position);
            SoundManager.I.PlaySe(SeType.Explosion).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(3), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: destroyToken);

            EffectManager.I.Play(EffectType.Concentration, transform.position);
            SoundManager.I.PlaySe(SeType.Concentration).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(2), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: destroyToken);
            Health = HealthMax = 5000;


            m_PhaseToken = new CancellationTokenSource();
            using (var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_PhaseToken.Token, destroyToken))
            {
                var token = combinedTokenSource.Token;

                await UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                    .ForEachAwaitAsync(async _ =>
                    {
                        await RandomMove(1, token);
                        await PlayerAimShot1(token);
                        await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);
                    }, token).SuppressCancellationThrow();
            }


            // Œã•Ð•t‚¯
            if (destroyToken.IsCancellationRequested)
                return;

            EffectManager.I.Play(EffectType.Explosion, transform.position);
            SoundManager.I.PlaySe(SeType.Explosion).Forget();
            Destroy(gameObject);
        }

        private async UniTask PlayerAimShot1(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            // TODO: cache
            var playerTransform = GameObject.FindGameObjectWithTag(InGameConstants.PlayerTag).transform;

            EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);

            float direction = Mathf.Atan2(playerTransform.position.y - transform.position.y, playerTransform.position.x - transform.position.x);


            for (int i = 0; i < 4 && !token.IsCancellationRequested; i++)
            {
                var bullet = Instantiate(m_NormalShotPrefab).GetComponent<EnemyBullet>();
                bullet.transform.position = transform.position;

                //float direction = Mathf.Atan2(playerTransform.position.y - transform.position.y, playerTransform.position.x - transform.position.x);
                float speed = bullet.Speed.y;
                bullet.Speed = new Vector3(
                    speed * Mathf.Cos(direction),
                    speed * Mathf.Sin(direction)
                    );

                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();

                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);
            }
        }


        private async UniTask Baramaki(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            for (int loop = 0; loop < 32 && !token.IsCancellationRequested; loop++)
            {

                EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();

                float direction = Seiran.Shared.NextSingle(0, Mathf.PI * 2);
                int way = 12;
                for (int i = 0; i < way; i++)
                {
                    var bullet = Instantiate(m_NormalShotPrefab).GetComponent<EnemyBullet>();
                    bullet.transform.position = transform.position;

                    float speed = 64;
                    bullet.Speed = new Vector3(
                        speed * Mathf.Cos(direction + (i * Mathf.PI * 2 / way)),
                        speed * Mathf.Sin(direction + (i * Mathf.PI * 2 / way))
                        );
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);
            }
        }




        public override void DealDamage(DamageSource damage)
        {
            if (m_PhaseToken == null)
                return;


            Health -= damage.Damage;

            if (Health / Math.Max(HealthMax, 0.0) <= 0.1)
            {
                SoundManager.I.PlaySe(SeType.PlayerBulletHitCritical).Forget();
            }
            else
            {
                SoundManager.I.PlaySe(SeType.PlayerBulletHit).Forget();
            }

            if (Health <= 0)
            {
                m_PhaseToken?.Cancel();
                m_PhaseToken?.Dispose();
                m_PhaseToken = null;
            }
        }
    }
}
