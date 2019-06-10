using UnityEngine;
using UnityEngine.UI;

namespace RTool.EzCanvas
{
    public class ConfirmPopup : PopupCanvas
    {
        [Header("Component Ref")]
        [SerializeField]
        private Button btnConfirm = null;
        private bool result { get; set; }

        public delegate void OnExit(bool _isConfirmed);
        private OnExit onExitDelegate { get; set; }

        public ConfirmPopup onExit(OnExit _onExit)
        {
            onExitDelegate = _onExit;
            return this;
        }

        private void Awake()
        {
            btnConfirm.onClick.AddListener(() =>
            {
                result = true;
                Hide();
            });
        }

        protected override void OnHideFinished()
        {
            base.OnHideFinished();
            if (onExitDelegate != null)
                onExitDelegate(result);
            result = false;
        }
    }
}
