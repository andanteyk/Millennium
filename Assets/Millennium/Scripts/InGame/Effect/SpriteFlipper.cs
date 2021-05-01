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


        private void Start()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            bool flipX = true, flipY = true; ;


            UniTaskAsyncEnumerable.EveryUpdate()
                .ForEachAwaitAsync(async _ =>
                {
                    flipX = !flipX & m_FlipX;
                    flipY = !flipY & m_FlipY;

                    renderer.flipX = flipX;
                    renderer.flipY = flipY;

                    await UniTask.Delay(TimeSpan.FromSeconds(m_Interval));
                }, this.GetCancellationTokenOnDestroy());
        }
    }
}
