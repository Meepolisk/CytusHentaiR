using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace RTool.EzCanvas
{
    internal class DefaultConfirmPopup : ConfirmPopup
    {
        [SerializeField]
        private Text txtTitle = null;
        [SerializeField]
        private Text txtDescription = null;

        internal void RefreshText(string _title, string _desc)
        {
            txtTitle.text = _title;
            txtDescription.text = _desc;
        }

        protected override void OnHideFinished()
        {
            base.OnHideFinished();
            Destroy(gameObject);
        }
    }
}