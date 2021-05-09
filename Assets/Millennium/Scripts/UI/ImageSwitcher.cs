using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Millennium.UI
{
    [RequireComponent(typeof(Image))]
    public class ImageSwitcher : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] m_Sprites;

        [SerializeField]
        private float m_Interval = 0.5f;


        private void Start()
        {
            Image image = GetComponent<Image>();
            int spriteIndex = -1;

            UniTaskAsyncEnumerable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(m_Interval))
                .ForEachAsync(_ =>
                {
                    if (++spriteIndex >= m_Sprites.Length)
                        spriteIndex = 0;

                    image.sprite = m_Sprites[spriteIndex];

                }, this.GetCancellationTokenOnDestroy());
        }
    }
}
