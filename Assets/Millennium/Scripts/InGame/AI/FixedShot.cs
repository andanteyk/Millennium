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
    public class FixedShot : MonoBehaviour
    {
        [SerializeField]
        private float m_Speed = 32;

        [SerializeField]
        private float m_Interval = 1;

        [SerializeField]
        private float m_StartTime = 1;

        [SerializeField]
        private float m_InitialDegree = -90;

        [SerializeField]
        private float m_RandomizeDegree = 0;

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


            await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(m_StartTime), TimeSpan.FromSeconds(m_Interval), PlayerLoopTiming.FixedUpdate)
                .Where(_ => owner != null && owner.CanMove)
                .WithCancellation(token))
            {
                if (m_Repeat == 0)
                    return;
                m_Repeat--;


                float direction = (m_InitialDegree + Seiran.Shared.NextSingle(-m_RandomizeDegree, m_RandomizeDegree)) * Mathf.Deg2Rad;

                foreach (var way in BallisticMath.CalculateWayRadians(direction, (int)m_Way, m_WayDegree * Mathf.Deg2Rad))
                {
                    BulletBase.Instantiate(m_BulletPrefab, transform.position, BallisticMath.FromPolar(m_Speed, way));
                }

                EffectManager.I.Play(EffectType.MuzzleFlash, transform.position);
                SoundManager.I.PlaySe(SeType.EnemyShot).Forget();
            }
        }
    }
}