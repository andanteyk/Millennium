using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.InGame.Entity.Enemy;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using UnityEngine;

namespace Millennium.InGame.AI
{
    [RequireComponent(typeof(EnemyBase))]
    public class AimShot : MonoBehaviour
    {
        [SerializeField]
        private float m_Speed = 32;

        [SerializeField]
        private float m_Interval = 1;

        [SerializeField]
        private float m_StartTime = 1;

        [SerializeField]
        private float m_Way = 1;

        [SerializeField]
        private float m_WayDegree = 30;

        [SerializeField]
        private int m_Repeat = -1;

        [SerializeField]
        private GameObject m_BulletPrefab;


        private void Start()
        {
            OnStart().Forget();
        }

        private async UniTaskVoid OnStart()
        {
            var owner = GetComponent<EnemyBase>();

            var token = this.GetCancellationTokenOnDestroy();


            await UniTask.Delay(TimeSpan.FromSeconds(m_StartTime), cancellationToken: token);

            await UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .Where(_ => owner != null && owner.CanMove)
                .ForEachAwaitAsync(async _ =>
                {
                    if (m_Repeat == 0)
                        return;
                    m_Repeat--;

                    float direction = BallisticMath.AimToPlayer(transform.position);

                    foreach (var angle in BallisticMath.CalculateWayRadians(direction, (int)m_Way, m_WayDegree * Mathf.Deg2Rad))
                    {
                        BulletBase.Instantiate(m_BulletPrefab, transform.position, BallisticMath.FromPolar(m_Speed, angle));
                    }

                    EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();

                    await UniTask.Delay(TimeSpan.FromSeconds(m_Interval), cancellationToken: token);
                }, token);
        }
    }
}