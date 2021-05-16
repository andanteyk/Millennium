using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Millennium.InGame.Stage
{
    [RequireComponent(typeof(Tilemap))]
    public class BackgroundTileScroller : MonoBehaviour
    {
        [SerializeField]
        private float m_Speed = 4;

        private void Start()
        {
            transform.DOMoveY(GetComponent<Tilemap>().size.y * -16 + InGameConstants.FieldArea.height + 24, m_Speed, true)
                .SetSpeedBased(true)
                .SetEase(Ease.Linear)
                .SetLink(gameObject);
        }

    }
}