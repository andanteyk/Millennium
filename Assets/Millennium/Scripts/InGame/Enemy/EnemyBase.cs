using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Millennium.InGame.Enemy
{
    public abstract class EnemyBase : MonoBehaviour
    {
        // Start is called before the first frame update
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
            Destroy(gameObject);
        }
    }
}
