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

            UniTaskAsyncEnumerable.EveryUpdate()
                .ForEachAsync(_ => { spriteRenderer.flipX = Time.time % 0.5f < 0.25f; }, token);

            transform.DOScale(4, 0.5f).WithCancellation(token);

            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate)
                .ForEachAwaitAsync(async _ =>
                {
                    Array.ForEach(colliders, c => c.enabled = true);
                    await UniTask.Yield(token);
                    Array.ForEach(colliders, c => c.enabled = false);
                    await UniTask.Delay(TimeSpan.FromSeconds(m_HitInterval), cancellationToken: token);
                }, token);
        }


    }
}
