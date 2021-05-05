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


        public static float AimToPlayer(Vector3 position)
            => Mathf.Atan2(PlayerTransform.position.y - position.y, PlayerTransform.position.x - position.x);

        public static IEnumerable<float> CalculateWayRadians(float baseRadian, int ways, float wayRadian)
        {
            for (int i = 0; i < ways; i++)
                yield return baseRadian + (i - (ways - 1) / 2f) * wayRadian;
        }

        public static Vector3 FromPolar(float length, float radian)
            => new Vector3(
                length * Mathf.Cos(radian),
                length * Mathf.Sin(radian));
    }
}