using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Millennium.InGame.Stage;
using Millennium.Sound;
using Millennium.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Millennium.OutGame.Screen
{
    public class PlayerSelect : ScreenBase
    {
        [SerializeField]
        private Button m_MomoiButton;

        [SerializeField]
        private Button m_MidoriButton;

        [SerializeField]
        private Button m_AliceButton;


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            SelectFirstButton();

            ButtonAction(m_MomoiButton, StageManager.PlayerType.Momoi, token);
            ButtonAction(m_MidoriButton, StageManager.PlayerType.Midori, token);
            ButtonAction(m_AliceButton, StageManager.PlayerType.Alice, token);
        }

        private void ButtonAction(Button button, StageManager.PlayerType playerType, CancellationToken token)
        {
            button.OnClickAsAsyncEnumerable(token)
                .Take(1)
                .ForEachAwaitWithCancellationAsync(async (_, token) =>
                {
                    SoundManager.I.PlaySe(SeType.Accept).Forget();

                    var fader = await Fader.CreateFade();
                    DontDestroyOnLoad(fader);
                    fader.SetColor(Color.cyan);
                    await fader.Show();

                    await EntryPoint.StartInGame(new EntryPoint.InGameParams { PlayerType = playerType });

                    await fader.Hide();
                    Destroy(fader.gameObject);

                    // ‚±‚ê©g‚ÍƒV[ƒ“Ø‚è‘Ö‚¦‚ÅÁ‚¦‚é‚Í‚¸
                });
        }
    }
}
