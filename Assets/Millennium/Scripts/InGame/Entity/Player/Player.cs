using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
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

        public int BombCount { get; private set; } = 3;
        private int m_SkillPoint = 0;
        private int m_SkillPointMax = 50000;

        protected bool IsControllable => Health > 0;

        public long Score { get; protected set; } = 0;

        protected bool m_IsDead = false;


        // Start is called before the first frame update
        private void Start()
        {
            // 100 HP == 1 残機
            HealthMax = 800;
            Health = 300;

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
                    if (BombCount <= 0)
                    {
                        await SoundManager.I.PlaySe(SeType.Disabled);
                        return;
                    }

                    BombCount--;

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


                    // DEBUG: ブルアカらしくて残しておいてもいいかもしれない
                    if (UnityEngine.InputSystem.Keyboard.current.f8Key.wasPressedThisFrame)
                        Time.timeScale = Time.timeScale != 1 ? 1 : 3;

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
                    ui.SetScore(Score);
                    ui.PlayerHealthGauge.SetGauge(Health, HealthMax);
                    ui.PlayerHealthGauge.SetSubGauge(Health / 100);
                    ui.SkillGauge.SetSubGauge(BombCount);
                    ui.SkillGauge.SetGauge(m_SkillPoint, m_SkillPointMax);
                }, token);
        }


        protected virtual UniTask MainShot()
        {
            for (int i = -1; i <= 1; i += 2)
            {
                var bullet = BulletBase.Instantiate(m_MainShotPrefab, transform.position + 8 * i * Vector3.right);
                bullet.Owner = this;
            }

            return UniTask.CompletedTask;
        }
        protected virtual UniTask SubShot() => UniTask.CompletedTask;
        protected virtual UniTask FireBomb(CancellationToken token) => UniTask.CompletedTask;


        public override void DealDamage(DamageSource damage)
        {
            if (IsInvincible || m_IsDead)
            {
                return;
            }


            Health -= damage.Damage;
            BombCount = Math.Max(BombCount, 2);

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


        public void AddScore(long score)
        {
            Score += score;

            m_SkillPoint += (int)score;
            if (m_SkillPoint >= m_SkillPointMax)
            {
                BombCount += m_SkillPoint / m_SkillPointMax;
                m_SkillPoint %= m_SkillPointMax;

                SoundManager.I.PlaySe(SeType.Accept).Forget();
            }
        }

        public void AddStageClearReward()
        {
            Health = Math.Min(Health + 100, HealthMax);
            Score += 1000000;
        }


        // TODO: ここに置くべき?
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

            await EntryPoint.StartOutGame(new EntryPoint.OutGameParams
            {
                FirstUIAddress = "Assets/Millennium/Assets/Prefabs/OutGame/UI/Result.prefab",
                Score = Score,
                IsCleared = false,
            });

            Time.timeScale = 1;
            await fade.Hide();
            Destroy(fade.gameObject);
        }
    }
}
