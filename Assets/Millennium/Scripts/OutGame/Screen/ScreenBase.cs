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
        /// フォーカスを合わせる：この UI が操作可能になった瞬間に呼ぶ必要がある
        /// （もともと非選択状態である・以前の UI が消えてカーソルがなくなっている状態だと矢印キーで操作できないため）
        /// </summary>
        /// <remarks>
        /// 不自然な気もするのでできれば自動でやってほしい
        /// </remarks>
        protected void SelectFirstButton()
        {
            m_FirstSelectedButton.Select();
        }
    }
}
