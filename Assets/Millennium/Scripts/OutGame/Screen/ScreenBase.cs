using Cysharp.Threading.Tasks;
using Millennium.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
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


        protected async UniTask Transit(string addressablePath, CancellationToken token)
        {
            // TODO: cancellable
            var fader = await Fader.CreateFade();
            fader.SetColor(Color.cyan);
            await fader.Show();



            var instance = Instantiate(await Addressables.LoadAssetAsync<GameObject>(addressablePath));
            instance.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
            instance.transform.SetAsLastSibling();


            await fader.Hide();
            Destroy(fader.gameObject);
            Destroy(gameObject);
        }

        public virtual void ReceiveOutGameParameter(EntryPoint.OutGameParams param)
        {
        }
    }
}
