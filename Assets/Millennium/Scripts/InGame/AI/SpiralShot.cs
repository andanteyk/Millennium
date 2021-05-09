using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.InGame.Entity.Enemy;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.AI
{
    [RequireComponent(typeof(EnemyBase))]
    public class SpiralShot : MonoBehaviour
    {
        [SerializeField]
        private float m_Speed = 32;

        [SerializeField]
        private float m_Interval = 0.1f;

        [SerializeField]
        private float m_StartTime = 1;

        [SerializeField]
        private float m_InitialDegree = -90;

        [SerializeField]
        private float m_InitialDegreeRandomness = 0;

        [SerializeField]
        private int m_Way = 1;

        [SerializeField]
        private float m_WayDegree = 30;

        [SerializeField]
        private int m_ShotCount = 30;


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


            float baseDegree = m_InitialDegree + Seiran.Shared.NextSingle(-m_InitialDegreeRandomness, m_InitialDegreeRandomness);

            await UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(m_StartTime), TimeSpan.FromSeconds(m_Interval), updateTiming: PlayerLoopTiming.FixedUpdate)
                .Where(_ => owner != null && owner.CanMove)
                .Take(m_ShotCount)
                .Select((_, i) => i)
                .ForEachAsync(i =>
                {
                    float direction = (baseDegree + i * m_WayDegree) * Mathf.Deg2Rad;

                    foreach (var way in BallisticMath.CalculateWayRadians(direction, m_Way))
                    {
                        BulletBase.Instantiate(m_BulletPrefab, transform.position, BallisticMath.FromPolar(m_Speed, way));
                    }

                    EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
                }, token);
        }

    }
}