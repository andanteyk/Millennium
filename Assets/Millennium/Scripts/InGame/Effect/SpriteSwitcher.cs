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


        private async UniTaskVoid Start()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();

            await foreach (var i in UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(m_Interval))
                .Select((_, i) => i % m_Sprites.Length)
                .WithCancellation(this.GetCancellationTokenOnDestroy()))
            {
                renderer.sprite = m_Sprites[i];
            }
        }
    }
}
