using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Millennium.OutGame.Screen
{
    public abstract class ScreenBase : MonoBehaviour
    {
        [SerializeField]
        protected Button m_FirstSelectedButton;


        /// <summary>
        /// �t�H�[�J�X�����킹��F���� UI ������\�ɂȂ����u�ԂɌĂԕK�v������
        /// �i���Ƃ��Ɣ�I����Ԃł���E�ȑO�� UI �������ăJ�[�\�����Ȃ��Ȃ��Ă����Ԃ��Ɩ��L�[�ő���ł��Ȃ����߁j
        /// </summary>
        /// <remarks>
        /// �s���R�ȋC������̂łł���Ύ����ł���Ăق���
        /// </remarks>
        protected void SelectFirstButton()
        {
            m_FirstSelectedButton.Select();
        }
    }
}
