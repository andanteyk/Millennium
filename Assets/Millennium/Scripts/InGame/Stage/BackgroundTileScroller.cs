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
        void Start()
        {
            // TODO
            transform.DOMoveY(GetComponent<Tilemap>().size.y * -16 + InGameConstants.FieldArea.height, 60, true).SetEase(Ease.Linear);
        }

    }
}