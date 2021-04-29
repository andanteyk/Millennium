using Cysharp.Threading.Tasks;
using Millennium.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Millennium.InGame.Entity.Player
{
    public class Player : EntityLiving
    {
        // Start is called before the first frame update
        async void Start()
        {
            Health = HealthMax;

            async UniTask OnStart(CancellationToken token)
            {
                var input = new InputControls();
                input.Enable();

                var bulletPrefab = await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Bullet/PlayerBullet.prefab");
                var bombPrefab = await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Bullet/AliceBomb Variant.prefab");

                var rigidbody = GetComponent<Rigidbody2D>();

                var coolTime = 0f;
                var coolTimeMax = 0.25f;


                input.Player.Bomb.started += context =>
                {
                    var bullet = Instantiate(bombPrefab);
                    bullet.transform.position = transform.position;
                };

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

                while (!token.IsCancellationRequested)
                {
                    // note: 物理挙動周りは FixedUpdate でやる
                    // 入力が絡むもの (特に down/up イベントが必要なもの) は Update でやる

                    // move
                    MoveByInput(rigidbody, input, 1);

                    // shot
                    if (coolTime <= 0)
                    {
                        var bullet = Instantiate(bulletPrefab);
                        bullet.transform.position = transform.position;

                        coolTime += coolTimeMax;
                    }
                    coolTime -= Time.deltaTime;

                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
                }
            }

            await OnStart(this.GetCancellationTokenOnDestroy());
        }


        protected void MoveByInput(Rigidbody2D rigidbody, InputControls input, float scale)
        {
            var movedPosition = transform.position + (Vector3)input.Player.Direction.ReadValue<Vector2>() * 64 * Time.deltaTime;
            movedPosition = new Vector3(
                Mathf.Clamp(movedPosition.x, InGameConstants.PlayerFieldArea.xMin, InGameConstants.PlayerFieldArea.xMax),
                Mathf.Clamp(movedPosition.y, InGameConstants.PlayerFieldArea.yMin, InGameConstants.PlayerFieldArea.yMax));
            rigidbody.MovePosition(movedPosition);
        }

        public override void DealDamage(DamageSource damage)
        {
            Health -= damage.Damage;

            // hit register(i.e. invincibl-ize)
            // TODO: death
        }
    }
}
