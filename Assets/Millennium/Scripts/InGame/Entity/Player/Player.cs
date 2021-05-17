using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.IO;
using Millennium.Sound;
using Millennium.UI;
using System;
using System.Threading;
using UnityEngine;

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
        protected GameObject m_BulletRemoverPrefab;

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
        private int m_SkillPointMax = 100000;

        protected bool IsControllable => Health > 0;

        public long Score { get; protected set; } = 0;

        protected bool m_IsDead = false;
        protected DamageSource m_DelayedDamage = null;

        public bool IsDebugMode { get; set; } = false;


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

                    // WasPressedThisFrame だと低 FPS 時に絶望的に反応が悪くなるので;
                    // 押しっぱなしで連続発動してしまうが、あまり問題にならないと思うので IsPressed で
                    if (!input.Player.Bomb.IsPressed())
                        return;

                    if (IsInvincible)
                        return;
                    if (BombCount <= 0)
                    {
                        await SoundManager.I.PlaySe(SeType.Disabled);
                        return;
                    }

                    if (!IsDebugMode)
                        BombCount--;

                    SetInvincible(6);
                    m_DelayedDamage = null;

                    await FireBomb(token);
                }, token);

            // input loop - move
            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAsync(_ =>
                {
                    if (!IsControllable)
                        return;

                    float nonShotModifier = input.Player.Fire.IsPressed() ? 1 : 1.5f;

                    var movedPosition = transform.position + (Vector3)input.Player.Direction.ReadValue<Vector2>() * (m_MoveSpeed * nonShotModifier * MoveSpeedModifier * Time.deltaTime);
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


            if (IsDebugMode)
                Score = -99999999;
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
            if (IsInvincible || m_IsDead || m_DelayedDamage != null)
            {
                return;
            }

            m_DelayedDamage = damage;
            SoundManager.I.PlaySe(SeType.PlayerDamaged).Forget();


            UniTask.Create(async () =>
            {
                var token = this.GetCancellationTokenOnDestroy();

                await UniTask.Delay(TimeSpan.FromSeconds(0.25), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);

                token.ThrowIfCancellationRequested();
                if (m_DelayedDamage != null)
                {
                    // hit
                    if (!IsDebugMode)
                    {
                        Health -= m_DelayedDamage.Damage;
                        BombCount = Math.Max(BombCount, 2);
                    }

                    if (Health > 0)
                    {
                        SetInvincible(5);

                        var remover = Instantiate(m_BulletRemoverPrefab);
                        remover.transform.position = transform.position;
                    }
                    else if (!m_IsDead)
                    {
                        m_IsDead = true;
                        GameOver().Forget();
                    }

                    m_DelayedDamage = null;
                }
            });
        }

        private void SetInvincible(float seconds)
        {
            m_InvincibleUntil = Time.fixedTime + seconds;
        }


        public void AddScore(long score)
        {
            Score += score;

            AddSkillPoint((int)score);
        }

        public void Extend()
        {
            Health = Math.Min(Health + 100, HealthMax);

            EffectManager.I.Play(EffectType.PlusDecayRed, transform.position);
            SoundManager.I.PlaySe(SeType.Accept).Forget();
        }

        public void AddSkillPoint(int value)
        {
            m_SkillPoint += (int)value;
            if (m_SkillPoint >= m_SkillPointMax)
            {
                BombCount += m_SkillPoint / m_SkillPointMax;
                m_SkillPoint %= m_SkillPointMax;

                EffectManager.I.Play(EffectType.PlusDecayBlue, transform.position);
                SoundManager.I.PlaySe(SeType.Accept).Forget();
            }
        }

        public void AddStageClearReward()
        {
            Score += 1000000;
        }


        private async UniTaskVoid GameOver()
        {
            EffectManager.I.Play(EffectType.Explosion, transform.position);

            await UniTask.Delay(2000);

            FindObjectOfType<Stage.StageManager>().GameOver().Forget();
        }
    }
}
