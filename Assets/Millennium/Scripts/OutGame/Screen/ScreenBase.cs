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
