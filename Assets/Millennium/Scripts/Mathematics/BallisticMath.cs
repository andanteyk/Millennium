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


        // TODO: ‚ ‚Ü‚è‚æ‚­‚È‚¢‹C‚à‚·‚éc
        public static Vector3 PlayerPosition => PlayerTransform.position;

        public static float AimToPlayer(Vector3 position)
            => Mathf.Atan2(PlayerTransform.position.y - position.y, PlayerTransform.position.x - position.x);

        public static float AimTo(Vector3 targetPosition, Vector3 selfPosition)
            => Mathf.Atan2(targetPosition.y - selfPosition.y, targetPosition.x - selfPosition.x);

        public static IEnumerable<float> CalculateWayRadians(float baseRadian, int ways, float intervalRadian)
        {
            for (int i = 0; i < ways; i++)
                yield return baseRadian + (i - (ways - 1) / 2f) * intervalRadian;
        }

        public static IEnumerable<float> CalculateWayRadians(float baseRadian, int ways)
            => CalculateWayRadians(baseRadian, ways, Mathf.PI * 2 / ways);

        public static Vector3 FromPolar(float length, float radian)
            => new Vector3(
                length * Mathf.Cos(radian),
                length * Mathf.Sin(radian));
    }
}