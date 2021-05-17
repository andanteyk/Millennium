using Cysharp.Threading.Tasks;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{
    public class KarinTurret : EnemyBase
    {
        [SerializeField]
        private GameObject m_NormalShotPrefab;

        [SerializeField]
        private GameObject m_HugeShotPrefab;



        [SerializeField]
        private float m_ShotDegree = -90;

        [SerializeField]
        private bool m_AimToPlayer = false;

        [SerializeField]
        private float m_BaseSpeed = 96;


        private void Start()
        {
            OnStart().Forget();
        }

        private async UniTaskVoid OnStart()
        {
            var token = this.GetCancellationTokenOnDestroy();

            token.ThrowIfCancellationRequested();
            float direction = m_AimToPlayer ? BallisticMath.AimToPlayer(transform.position) : (m_ShotDegree * Mathf.Deg2Rad);

            EffectManager.I.Play(EffectType.Caution, transform.position + BallisticMath.FromPolar(64, direction));
            EffectManager.I.Play(EffectType.Caution, transform.position + BallisticMath.FromPolar(128, direction));
            EffectManager.I.Play(EffectType.Caution, transform.position + BallisticMath.FromPolar(192, direction));

            await UniTask.Delay(TimeSpan.FromSeconds(1), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);


            token.ThrowIfCancellationRequested();

            BulletBase.Instantiate(m_HugeShotPrefab, transform.position, BallisticMath.FromPolar(m_BaseSpeed, direction));

            for (int i = 2; i < 8; i++)
            {
                BulletBase.Instantiate(m_NormalShotPrefab, transform.position, BallisticMath.FromPolar(m_BaseSpeed * i / 8, direction));
            }

            SoundManager.I.PlaySe(SeType.Explosion).Forget();


            Destroy(gameObject);
        }
    }
}

