using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Millennium.InGame.Entity.Enemy
{

    public class BossBinahBody : EnemyBase
    {
        [SerializeField]
        private BossBinah m_Parent;


        [SerializeField]
        private Sprite m_FrontSprite;

        [SerializeField]
        private Sprite m_SideSprite;


        private LinkedList<Sequence> m_Sequences;



        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            DamageWhenEnter(token).Forget();

            m_Sequences = new LinkedList<Sequence>();

            var previousPosition = transform.position;
            var spriteRenderer = GetComponent<SpriteRenderer>();
            UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.5))
                .ForEachAsync(_ =>
                {
                    var difference = transform.position - previousPosition;

                    spriteRenderer.flipX = difference.x < 0;
                    spriteRenderer.sprite = Mathf.Abs(difference.x) > Mathf.Abs(difference.y) ?
                        m_SideSprite : m_FrontSprite;

                    previousPosition = transform.position;
                }, token);
        }


        public override void DealDamage(DamageSource damage)
        {
            m_Parent.DealDamage(damage);
        }


        public void AddTween(Sequence sequence)
        {
            m_Sequences.AddLast(sequence);

            sequence.OnKill(() => m_Sequences.Remove(sequence));
            sequence.OnComplete(() => m_Sequences.Remove(sequence));
        }

        public void PauseTween()
        {
            foreach (var sequence in m_Sequences)
                sequence.Pause();
        }

        public void ResumeTween()
        {
            foreach (var sequence in m_Sequences)
                sequence.Play();
        }

        public void KillTween()
        {
            // OnKill Ç≈ m_Sequences Ç™ïœçXÇ≥ÇÍÇÈÇÃÇ≈ÉNÉçÅ[Éìè„Ç≈óÒãìÇ∑ÇÈ
            foreach (var sequence in m_Sequences.ToArray())
                sequence.Kill();
        }

    }
}
