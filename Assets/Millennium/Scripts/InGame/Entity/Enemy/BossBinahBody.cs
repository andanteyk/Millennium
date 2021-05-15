using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Effect;
using Millennium.InGame.Entity.Bullet;
using Millennium.Mathematics;
using Millennium.Sound;
using System;
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


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

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
    }
}
