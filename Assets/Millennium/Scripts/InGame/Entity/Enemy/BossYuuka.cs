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





        private void Start()
        {
            OnStart().Forget();
        }

        private async UniTask OnStart()
        {
            var destroyToken = this.GetCancellationTokenOnDestroy();


            SetupHealthGauge(2, destroyToken);

            // TODO
            Health = HealthMax = 5000;
            await RunPhase(async (_, token) =>
            {
                await RandomMove(1, token);
                await Baramaki(token);
                await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);
            }, destroyToken);
            await OnEndPhase(destroyToken);

            Health = HealthMax = 5000;          // TODO
            await RunPhase(async (_, token) =>
            {
                await RandomMove(1, token);
                await PlayerAimShot1(token);
                await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);
            }, destroyToken);

            await PlayDeathEffect(destroyToken);

            if (destroyToken.IsCancellationRequested)
                return;
            Destroy(gameObject);
        }



        private async UniTask PlayerAimShot1(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            // TODO: cache
            var playerTransform = GameObject.FindGameObjectWithTag(InGameConstants.PlayerTag).transform;

            EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);

            float direction = BallisticMath.AimToPlayer(transform.position);


            for (int i = 0; i < 4 && !token.IsCancellationRequested; i++)
            {
                var bullet = BulletBase.Instantiate(m_NormalShotPrefab, transform.position);
                bullet.Speed = BallisticMath.FromPolar(bullet.Speed.y, direction);

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

                foreach (var radian in BallisticMath.CalculateWayRadians(Seiran.Shared.NextSingle(0, Mathf.PI * 2), 12, 2 * Mathf.PI / 12))
                {
                    BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(64, radian));
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

        private void OnDestroy()
        {
            m_PhaseToken?.Cancel();
            m_PhaseToken?.Dispose();
            m_PhaseToken = null;
        }
    }
}
