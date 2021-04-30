using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System;
using UnityEngine;

namespace Millennium.InGame.Effect
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSwitcher : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] m_Sprites;

        [SerializeField]
        private float m_Interval = 0.5f;


        private void Start()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            int spriteIndex = -1;

            UniTaskAsyncEnumerable.EveryUpdate()
                .ForEachAwaitAsync(async _ =>
                {
                    if (++spriteIndex >= m_Sprites.Length)
                        spriteIndex = 0;

                    renderer.sprite = m_Sprites[spriteIndex];

                    await UniTask.Delay(TimeSpan.FromSeconds(m_Interval));
                }, this.GetCancellationTokenOnDestroy());
        }
    }
}
