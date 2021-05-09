using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Millennium.UI
{
    [RequireComponent(typeof(Image))]
    public class ImageScroller : MonoBehaviour
    {
        [SerializeField]
        private Vector2 m_TileSize = new Vector2(16, 16);

        [SerializeField]
        private float m_Duration = 1;

        private void Start()
        {
            var image = GetComponent<Image>();

            DOTween.To(() => image.rectTransform.anchoredPosition, value => image.rectTransform.anchoredPosition = value,
                m_TileSize, m_Duration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetLink(gameObject);
        }
    }
}
