using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Entity.Bullet;
using Millennium.InGame.Entity.Enemy;
using Millennium.InGame.Entity.Player;
using Millennium.Sound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

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
            Transform playerTransform = null;

            var token = this.GetCancellationTokenOnDestroy();


            await UniTask.Delay(TimeSpan.FromSeconds(m_StartTime), cancellationToken: token);

            await UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .Where(_ => owner != null && owner.CanMove)
                .ForEachAwaitAsync(async _ =>
                {
                    if (m_Repeat == 0)
                        return;
                    m_Repeat--;

                    playerTransform = playerTransform != null ? playerTransform : GameObject.FindGameObjectWithTag(InGameConstants.PlayerTag)?.transform;

                    if (playerTransform == null)
                        return;

                    // もっと簡単に計算する方法があった気もする…
                    float direction = Mathf.Atan2(playerTransform.position.y - transform.position.y, playerTransform.position.x - transform.position.x);
                    float unitRad = m_WayDegree * Mathf.Deg2Rad;

                    for (int i = 0; i < m_Way; i++)
                    {
                        var instance = Instantiate(m_BulletPrefab);
                        instance.transform.position = transform.position;

                        var bullet = instance.GetComponent<BulletBase>();
                        bullet.Speed = new Vector3(
                            m_Speed * Mathf.Cos(direction + (i - (m_Way - 1) / 2) * unitRad),
                            m_Speed * Mathf.Sin(direction + (i - (m_Way - 1) / 2) * unitRad));
                    }

                    SoundManager.I.PlaySe(SeType.EnemyShot).Forget();

                    await UniTask.Delay(TimeSpan.FromSeconds(m_Interval), cancellationToken: token);
                }, token);
        }
    }
}