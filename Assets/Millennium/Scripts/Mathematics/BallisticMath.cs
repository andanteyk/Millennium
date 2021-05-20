using Millennium.InGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.Mathematics
{
    public static class BallisticMath
    {
        private static Transform m_PlayerTransform;
        private static Transform PlayerTransform => m_PlayerTransform != null ? m_PlayerTransform :
            m_PlayerTransform = GameObject.FindGameObjectWithTag(InGameConstants.PlayerTag).transform;


        // TODO: あまりよくない気もする…
        public static Vector3 PlayerPosition => PlayerTransform.position;

        /// <summary>
        /// <paramref name="position"/>(主に自分)からプレイヤーへの角度 (rad) を取得
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static float AimToPlayer(Vector3 position)
            => Mathf.Atan2(PlayerTransform.position.y - position.y, PlayerTransform.position.x - position.x);

        /// <summary>
        /// <paramref name="selfPosition"/> から <paramref name="targetPosition"/> への角度 (rad) を取得
        /// </summary>
        public static float AimTo(Vector3 targetPosition, Vector3 selfPosition)
            => Mathf.Atan2(targetPosition.y - selfPosition.y, targetPosition.x - selfPosition.x);


        /// <summary>
        /// n way 弾の角度 (rad) をまとめて計算
        /// </summary>
        /// <param name="baseRadian">中心軸となる方向; <see cref="AimToPlayer(Vector3)"/> など</param>
        /// <param name="ways">way 数</param>
        /// <param name="intervalRadian">way の間隔(rad)</param>
        public static IEnumerable<float> CalculateWayRadians(float baseRadian, int ways, float intervalRadian)
        {
            for (int i = 0; i < ways; i++)
                yield return baseRadian + (i - (ways - 1) / 2f) * intervalRadian;
        }

        /// <summary>
        /// 全方位 n way 弾の角度 (rad) をまとめて計算
        /// </summary>
        /// <param name="baseRadian">中心軸となる方向; <see cref="AimToPlayer(Vector3)"/> など</param>
        /// <param name="ways">way 数</param>
        public static IEnumerable<float> CalculateWayRadians(float baseRadian, int ways)
            => CalculateWayRadians(baseRadian, ways, Mathf.PI * 2 / ways);


        /// <summary>
        /// 極座標から xy 平面上の Vector3 を生成
        /// </summary>
        /// <param name="length">距離</param>
        /// <param name="radian">方向(rad)</param>
        public static Vector3 FromPolar(float length, float radian)
            => new Vector3(
                length * Mathf.Cos(radian),
                length * Mathf.Sin(radian));
    }
}
