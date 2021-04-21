using Cysharp.Threading.Tasks;
using Millennium.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Millennium.InGame.Player
{
    public class Player : MonoBehaviour
    {



        // Start is called before the first frame update
        async void Start()
        {
            async UniTask OnStart(CancellationToken token)
            {
                var input = new InputControls();
                input.Enable();

                var bulletPrefab = await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Bullet/PlayerBullet.prefab");

                while (!token.IsCancellationRequested)
                {
                    transform.position += (Vector3)input.Player.Direction.ReadValue<Vector2>() * 64 * Time.deltaTime;

                    if (input.Player.Submit.triggered)
                    {
                        var bullet = Instantiate(bulletPrefab);
                        bullet.transform.position = transform.position;
                    }

                    await UniTask.Yield();
                }
            }

            await OnStart(this.GetCancellationTokenOnDestroy());
        }
    }
}
