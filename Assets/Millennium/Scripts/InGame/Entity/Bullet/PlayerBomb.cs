using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Millennium.InGame.Effect;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame.Entity.Bullet
{
    /// <summary>
    /// �A���X�̃X�L�� (�挩�̖����Ȃ��̂Ŗ��O���Ђǂ�)
    /// </summary>
    public class PlayerBomb : BulletBase
    {
        [SerializeField]
        private float m_HitInterval = 0.25f;

        private void Start()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            var colliders = GetComponents<Collider2D>();

            var token = this.GetCancellationTokenOnDestroy();

            Move(token);
            DestroyWhenExpired(token);
            DamageWhenEnter(token);
            CollisionSwitcher(m_HitInterval, 2, token);

            UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(0.25))
                .Select((_, i) => (i & 1) != 0)
                .ForEachAsync(flip => spriteRenderer.flipX = flip, token);

            transform.DOScale(4, 0.5f)
                .SetLink(gameObject)
                .WithCancellation(token);
        }
    }
}
