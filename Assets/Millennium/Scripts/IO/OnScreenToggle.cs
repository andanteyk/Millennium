using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using UnityEngine.InputSystem.Layouts;

namespace Millennium.IO
{
    [RequireComponent(typeof(Toggle))]
    public class OnScreenToggle : OnScreenControl
    {
        private Toggle m_Toggle;

        [InputControl(layout = "Button")]
        [SerializeField]
        private string m_ControlPath;
        protected override string controlPathInternal { get => m_ControlPath; set => m_ControlPath = value; }

        private void Start()
        {
            m_Toggle = GetComponent<Toggle>();

            m_Toggle.onValueChanged.AddListener(value =>
            {
                SendValueToControl(value ? 1f : 0f);
            });

            SendValueToControl(m_Toggle.isOn ? 1f : 0f);
        }
    }
}
