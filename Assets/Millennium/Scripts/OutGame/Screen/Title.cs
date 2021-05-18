using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Millennium.Mathematics;
using Millennium.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace Millennium.OutGame.Screen
{
    public class Title : ScreenBase
    {
        [SerializeField]
        private Button m_StartButton;

        [SerializeField]
        private Button m_InformationButton;

        [SerializeField]
        private RectTransform[] m_Characters;


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            SelectFirstButton();

            m_StartButton.OnClickAsAsyncEnumerable(token)
                .Take(1)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    SoundManager.I.PlaySe(SeType.Accept).Forget();

                    await Transit("Assets/Millennium/Assets/Prefabs/OutGame/UI/PlayerSelect.prefab", token);
                }, token);

            m_InformationButton.OnClickAsAsyncEnumerable(token)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    SoundManager.I.PlaySe(SeType.Accept).Forget();

                    await Transit("Assets/Millennium/Assets/Prefabs/OutGame/UI/DialogInformation.prefab", token);
                }, token);



            foreach (var character in m_Characters)
                MoveCharacterSprite(character);
        }



        private void MoveCharacterSprite(RectTransform rectTransform)
        {
            int activeIndex = Seiran.Shared.Next(rectTransform.childCount);
            for (int i = 0; i < rectTransform.childCount; i++)
                rectTransform.GetChild(i).gameObject.SetActive(i == activeIndex);

            rectTransform.DOAnchorPosX(240 - 32, (240 - 32) / 16, true).SetEase(Ease.Linear).SetLink(rectTransform.gameObject).SetLoops(-1, LoopType.Yoyo).Goto(Seiran.Shared.NextSingle(0, (240 - 32) / 16 * 2), true);
            rectTransform.DOAnchorPosY(192 - 32, (192 - 32) / 16, true).SetEase(Ease.Linear).SetLink(rectTransform.gameObject).SetLoops(-1, LoopType.Yoyo).Goto(Seiran.Shared.NextSingle(0, (192 - 32) / 16 * 2), true);
        }
    }
}
