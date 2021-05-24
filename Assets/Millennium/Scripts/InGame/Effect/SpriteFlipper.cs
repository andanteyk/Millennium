using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System;
using UnityEngine;

namespace Millennium.InGame.Effect
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipper : MonoBehaviour
    {
        [SerializeField]
        private bool m_FlipX;

        [SerializeField]
        private bool m_FlipY;

        [SerializeField]
        private float m_Interval = 0.5f;


        private async UniTaskVoid Start()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            bool flipX = true, flipY = true;


            await foreach (var _ in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(m_Interval))
                .WithCancellation(this.GetCancellationTokenOnDestroy()))
            {
                flipX = !flipX & m_FlipX;
                flipY = !flipY & m_FlipY;

                renderer.flipX = flipX;
                renderer.flipY = flipY;
            }
        }
    }
}
