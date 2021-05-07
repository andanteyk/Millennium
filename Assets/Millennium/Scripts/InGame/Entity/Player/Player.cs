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
using UnityEngine.Serialization;

namespace Millennium.InGame.Entity.Player
{
    public abstract class Player : EntityLiving
    {
        [SerializeField]
        protected GameObject m_MainShotPrefab;
        [SerializeField]
        protected GameObject m_SubShotPrefab;
        [SerializeField]
        protected GameObject m_BombPrefab;

        [SerializeField]
        protected float m_MainShotInterval = 0.25f;
        [SerializeField]
        protected float m_SubShotInterval = 0.5f;

        [SerializeField]
        private float m_MoveSpeed = 64;


        protected bool IsInvincible => Time.fixedTime < m_InvincibleUntil;
        private float m_InvincibleUntil = 0;

        private int m_BombCount = 3;

        protected bool IsControllable => Health > 0;



        // Start is called before the first frame update
        private void Start()
        {
            HealthMax = 300;       // TODO
            Health = HealthMax;

            var input = new InputControls();
            input.Enable();


            var rigidbody = GetComponent<Rigidbody2D>();
            var renderer = GetComponent<SpriteRenderer>();


            var token = this.GetCancellationTokenOnDestroy();

            // input loop - main shot
            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAwaitAsync(async _ =>
                {
                    if (!IsControllable)
                        return;
                    if (!input.Player.Fire.IsPressed())
                        return;

                    await MainShot();

                    await UniTask.Delay(TimeSpan.FromSeconds(m_MainShotInterval));
                }, token);

            // input loop - sub shot
            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAwaitAsync(async _ =>
                {
                    if (!IsControllable)
                        return;
                    if (!input.Player.Fire.IsPressed())
                        return;

                    await SubShot();

                    await UniTask.Delay(TimeSpan.FromSeconds(m_SubShotInterval));
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
                .ForEachAsync(_ =>
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
                .ForEachAsync(_ =>
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


        protected virtual UniTask MainShot()
        {
            var bullet = Instantiate(m_MainShotPrefab);
            bullet.transform.position = transform.position;
            return UniTask.CompletedTask;
        }
        protected virtual UniTask SubShot() => UniTask.CompletedTask;


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
                GameOver().Forget();
            }
        }

        private void SetInvincible(float seconds)
        {
            m_InvincibleUntil = Time.fixedTime + seconds;
        }



        // TODO: ここに置くべき?
        private async UniTask GameOver()
        {
            // TODO: game over
            EffectManager.I.Play(EffectType.Explosion, transform.position);

            await UniTask.Delay(1000);

            Time.timeScale = 0;

            var fade = await UI.Fader.CreateFade();
            await fade.Show();
        }
    }
}
