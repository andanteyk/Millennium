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


        // TODO: ���܂�悭�Ȃ��C������c
        public static Vector3 PlayerPosition => PlayerTransform.position;

        /// <summary>
        /// <paramref name="position"/>(��Ɏ���)����v���C���[�ւ̊p�x (rad) ���擾
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static float AimToPlayer(Vector3 position)
            => Mathf.Atan2(PlayerTransform.position.y - position.y, PlayerTransform.position.x - position.x);

        /// <summary>
        /// <paramref name="selfPosition"/> ���� <paramref name="targetPosition"/> �ւ̊p�x (rad) ���擾
        /// </summary>
        public static float AimTo(Vector3 targetPosition, Vector3 selfPosition)
            => Mathf.Atan2(targetPosition.y - selfPosition.y, targetPosition.x - selfPosition.x);


        /// <summary>
        /// n way �e�̊p�x (rad) ���܂Ƃ߂Čv�Z
        /// </summary>
        /// <param name="baseRadian">���S���ƂȂ����; <see cref="AimToPlayer(Vector3)"/> �Ȃ�</param>
        /// <param name="ways">way ��</param>
        /// <param name="intervalRadian">way �̊Ԋu(rad)</param>
        public static IEnumerable<float> CalculateWayRadians(float baseRadian, int ways, float intervalRadian)
        {
            for (int i = 0; i < ways; i++)
                yield return baseRadian + (i - (ways - 1) / 2f) * intervalRadian;
        }

        /// <summary>
        /// �S���� n way �e�̊p�x (rad) ���܂Ƃ߂Čv�Z
        /// </summary>
        /// <param name="baseRadian">���S���ƂȂ����; <see cref="AimToPlayer(Vector3)"/> �Ȃ�</param>
        /// <param name="ways">way ��</param>
        public static IEnumerable<float> CalculateWayRadians(float baseRadian, int ways)
            => CalculateWayRadians(baseRadian, ways, Mathf.PI * 2 / ways);


        /// <summary>
        /// �ɍ��W���� xy ���ʏ�� Vector3 �𐶐�
        /// </summary>
        /// <param name="length">����</param>
        /// <param name="radian">����(rad)</param>
        public static Vector3 FromPolar(float length, float radian)
            => new Vector3(
                length * Mathf.Cos(radian),
                length * Mathf.Sin(radian));
    }
}
