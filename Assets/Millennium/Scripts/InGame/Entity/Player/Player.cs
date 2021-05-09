using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.IO;
using Millennium.Sound;
using Millennium.UI;
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
        private SpriteRenderer m_CollisionRenderer;

        [SerializeField]
        protected float m_MainShotInterval = 0.25f;
        [SerializeField]
        protected float m_SubShotInterval = 0.5f;

        [SerializeField]
        private float m_MoveSpeed = 64;

        protected float MoveSpeedModifier = 1;

        protected bool IsInvincible => Time.fixedTime < m_InvincibleUntil;
        private float m_InvincibleUntil = 0;

        private int m_BombCount = 3;

        protected bool IsControllable => Health > 0;

        protected long m_Score = 0;

        protected bool m_IsDead = false;


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

                    SetInvincible(6);


                    await FireBomb(token);
                }, token);

            // input loop - move
            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAsync(_ =>
                {
                    if (!IsControllable)
                        return;

                    var movedPosition = transform.position + (Vector3)input.Player.Direction.ReadValue<Vector2>() * m_MoveSpeed * MoveSpeedModifier * Time.deltaTime;
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
                        renderer.enabled = !m_IsDead;
                    }

                    m_CollisionRenderer.enabled = input.Player.Fire.IsPressed();
                }, token);


            InGameUI.I.PlayerHealthGauge.SetSubGauge(0);
            InGameUI.I.SkillGauge.SetGaugeColor(Color.cyan);
            InGameUI.I.PlayerHealthGauge.Show();
            InGameUI.I.SkillGauge.Show();

            // UI update
            UniTaskAsyncEnumerable.EveryUpdate()
                .ForEachAsync(_ =>
                {
                    var ui = InGameUI.I;
                    ui.SetScore(m_Score);
                    ui.PlayerHealthGauge.SetGauge(Health, HealthMax);
                    ui.PlayerHealthGauge.SetSubGauge(Health / 100);
                    ui.SkillGauge.SetSubGauge(m_BombCount);
                }, token);
        }


        protected virtual UniTask MainShot()
        {
            var bullet = Instantiate(m_MainShotPrefab);
            bullet.transform.position = transform.position;
            return UniTask.CompletedTask;
        }
        protected virtual UniTask SubShot() => UniTask.CompletedTask;
        protected virtual UniTask FireBomb(CancellationToken token) => UniTask.CompletedTask;


        public override void DealDamage(DamageSource damage)
        {
            if (IsInvincible)
            {
                return;
            }


            Health -= damage.Damage;

            if (Health > 0)
            {
                SetInvincible(5);
                SoundManager.I.PlaySe(SeType.PlayerDamaged).Forget();
            }
            else if (!m_IsDead)
            {
                m_IsDead = true;
                GameOver().Forget();
            }
        }

        private void SetInvincible(float seconds)
        {
            m_InvincibleUntil = Time.fixedTime + seconds;
        }

        public void AddScore(long score) => m_Score += score;


        // TODO: ‚±‚±‚É’u‚­‚×‚«?
        private async UniTaskVoid GameOver()
        {
            // TODO: game over
            EffectManager.I.Play(EffectType.Explosion, transform.position);
            SoundManager.I.PlaySe(SeType.PlayerDamaged).Forget();

            await UniTask.Delay(2000);

            Time.timeScale = 0;

            var fade = await UI.Fader.CreateFade();
            fade.SetColor(Color.cyan);
            DontDestroyOnLoad(fade);
            await fade.Show();

            await EntryPoint.StartOutGame("Assets/Millennium/Assets/Prefabs/OutGame/UI/Result.prefab");

            Time.timeScale = 1;
            await fade.Hide();
            Destroy(fade.gameObject);
        }
    }
}
