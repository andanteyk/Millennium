using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.IO;
using Millennium.Sound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Millennium.InGame.Entity.Player
{
    public class Player : EntityLiving
    {
        [SerializeField]
        private GameObject m_BulletPrefab;
        [SerializeField]
        private GameObject m_BombPrefab;

        [SerializeField]
        private float m_ShotInterval = 0.25f;

        [SerializeField]
        private float m_MoveSpeed = 64;


        private bool IsInvincible => Time.fixedTime < m_InvincibleUntil;
        private float m_InvincibleUntil = 0;

        private int m_BombCount = 3;

        private bool IsControllable => Health > 0;



        // Start is called before the first frame update
        private void Start()
        {
            HealthMax = 300;       // TODO
            Health = HealthMax;

            var input = new InputControls();
            input.Enable();

            //var bulletPrefab = await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Bullet/PlayerBullet.prefab");
            //var bombPrefab = await Addressables.LoadAssetAsync<GameObject>("Assets/Millennium/Assets/Prefabs/InGame/Bullet/AliceBomb Variant.prefab");

            var rigidbody = GetComponent<Rigidbody2D>();
            var renderer = GetComponent<SpriteRenderer>();


            var token = this.GetCancellationTokenOnDestroy();

            // input loop - shot
            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAwaitAsync(async _ =>
                {
                    if (!IsControllable)
                        return;
                    if (!input.Player.Fire.IsPressed())
                        return;

                    var bullet = Instantiate(m_BulletPrefab);
                    bullet.transform.position = transform.position;

                    await UniTask.Delay(TimeSpan.FromSeconds(m_ShotInterval));
                }, token);

            // input loop - bomb
            UniTaskAsyncEnumerable.EveryUpdate()
                .ForEachAwaitAsync(async _ =>
                {
                    if (!IsControllable)
                        return;
                    if (!input.Player.Bomb.WasPressedThisFrame())
                        return;

                    if (IsInvincible)
                        return;
                    if (m_BombCount <= 0)
                    {
                        await SoundManager.I.PlaySe(SeType.Disabled);
                        return;
                    }

                    m_BombCount--;

                    SetInvincible(5);

                    var bullet = Instantiate(m_BombPrefab);
                    bullet.transform.position = transform.position;
                }, token);

            // input loop - move
            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAwaitAsync(async _ =>
                {
                    if (!IsControllable)
                        return;

                    var movedPosition = transform.position + (Vector3)input.Player.Direction.ReadValue<Vector2>() * m_MoveSpeed * Time.deltaTime;
                    movedPosition = new Vector3(
                        Mathf.Clamp(movedPosition.x, InGameConstants.PlayerFieldArea.xMin, InGameConstants.PlayerFieldArea.xMax),
                        Mathf.Clamp(movedPosition.y, InGameConstants.PlayerFieldArea.yMin, InGameConstants.PlayerFieldArea.yMax));
                    rigidbody.MovePosition(movedPosition);
                }, token);

            // display update
            UniTaskAsyncEnumerable.EveryUpdate()
                .ForEachAwaitAsync(async _ =>
                {
                    if (IsInvincible)
                    {
                        renderer.enabled = Time.time % (1 / 8f) < (1 / 8f / 2);
                    }
                    else
                    {
                        renderer.enabled = true;
                    }
                }, token);
        }


        public override void DealDamage(DamageSource damage)
        {
            if (IsInvincible)
            {
                return;
            }


            Health -= damage.Damage;

            if (Health > 0)
            {
                // damaged - invinsiblize, bomb?
                SetInvincible(5);

                SoundManager.I.PlaySe(SeType.PlayerDamaged).Forget();
            }
            else
            {
                // TODO: game over
                EffectManager.I.Play(EffectType.Explosion, transform.position);
            }
        }

        private void SetInvincible(float seconds)
        {
            m_InvincibleUntil = Time.fixedTime + seconds;
        }
    }
}
