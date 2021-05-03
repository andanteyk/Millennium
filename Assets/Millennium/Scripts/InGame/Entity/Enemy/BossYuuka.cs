using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
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
            // TODO
            Health = HealthMax = 1000;

            var token = this.GetCancellationTokenOnDestroy();

            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAwaitAsync(async _ =>
                {
                    await RandomMove(1, token);
                    await PlayerAimShot1(token);
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);
                }, token);
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
    }
}
