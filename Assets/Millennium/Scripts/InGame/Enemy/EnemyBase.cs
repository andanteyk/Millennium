using Cysharp.Threading.Tasks;
using Millennium.InGame.Bullet;
using Millennium.InGame.Effect;
using Millennium.Sound;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Millennium.InGame.Enemy
{
    public abstract class EnemyBase : MonoBehaviour
    {
        public int Health = 1000;


        async void Start()
        {
            async UniTask OnStart(CancellationToken token)
            {
                var bulletPrefab = await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Bullet/EnemyBullet.prefab");

                while (!token.IsCancellationRequested)
                {
                    var bullet = Instantiate(bulletPrefab);
                    bullet.transform.position = transform.position;

                    await UniTask.Delay(1000);
                }
            }

            await OnStart(this.GetCancellationTokenOnDestroy());
        }

        // TEST
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var bullet = collision.GetComponent<BulletBase>();
            if (bullet != null)
            {
                Health -= bullet.Power;

                if (Health <= 0)
                {
                    EffectManager.I.Play(EffectType.Explosion, transform.position);
                    Destroy(gameObject);
                }
            }
        }
    }
}
