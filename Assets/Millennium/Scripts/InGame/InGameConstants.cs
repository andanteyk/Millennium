using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame
{
    public static class InGameConstants
    {
        /// <summary> 画面全体のサイズ </summary>
        public static Vector2 ScreenSize => new Vector2(256, 224);

        /// <summary> メインフィールドの座標とサイズ </summary>
        public static Rect FieldArea => new Rect(176 / -2, 224 / -2, 176, 224);

        /// <summary> メインフィールド外周の座標とサイズ(範囲外に行ったオブジェクトは削除してよい) </summary>
        public static Rect ExtendedFieldArea => new Rect(FieldArea.x - 32, FieldArea.y - 32, FieldArea.width + 64, FieldArea.height + 64);

        /// <summary> プレイヤーが移動可能な範囲 </summary>
        public static Rect PlayerFieldArea => new Rect(FieldArea.x + 8, FieldArea.y + 8, FieldArea.width - 16, FieldArea.height - 16);


        public static string PlayerTag => "Player";
        public static string EnemyTag => "Enemy";
    }
}