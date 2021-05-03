using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Millennium.InGame
{
    public static class InGameConstants
    {
        /// <summary> ��ʑS�̂̃T�C�Y </summary>
        public static Vector2 ScreenSize => new Vector2(256, 224);

        /// <summary> ���C���t�B�[���h�̍��W�ƃT�C�Y </summary>
        public static Rect FieldArea => new Rect(176 / -2, 224 / -2, 176, 224);

        /// <summary> ���C���t�B�[���h�O���̍��W�ƃT�C�Y(�͈͊O�ɍs�����I�u�W�F�N�g�͍폜���Ă悢) </summary>
        public static Rect ExtendedFieldArea => new Rect(FieldArea.x - 32, FieldArea.y - 32, FieldArea.width + 64, FieldArea.height + 64);

        /// <summary> �v���C���[���ړ��\�Ȕ͈� </summary>
        public static Rect PlayerFieldArea => new Rect(FieldArea.x + 8, FieldArea.y + 8, FieldArea.width - 16, FieldArea.height - 16);


        public static string PlayerTag => "Player";
        public static string EnemyTag => "Enemy";
    }
}