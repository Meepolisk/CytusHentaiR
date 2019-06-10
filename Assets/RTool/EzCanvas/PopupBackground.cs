using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTool.Attribute;
using RTool.Extension;

namespace RTool.EzCanvas
{
    public class PopupBackground : MonoBehaviour
    {
        [Header("Prefabs Ref")]
        [SerializeField]
        private DefaultConfirmPopup defaultComfirmPopup = null;

        [Header("Config")]
        [SerializeField, Range(0f, 1f), ReadOnlyWhenPlaying]
        private float fadeInDuration = 0.2f;
        public float FadeInDuration => fadeInDuration;
        [SerializeField, Range(0f, 1f), ReadOnlyWhenPlaying]
        private float fadeOutDuration = 0.2f;
        public float FadeOutDuration => fadeOutDuration;
        [SerializeField, Range(0f, 1f), ReadOnlyWhenPlaying]
        private float fullMaskAlpha = 0.5f;
        public float FullMaskAlpha => fullMaskAlpha;

        private PopupBackgroundMesh bgMesh { set; get; }
        internal static PopupBackground AutoInstance { private set; get; }
        private List<PopupCanvas> popUpList = new List<PopupCanvas>();

        private void Awake()
        {
            AutoInstance = this;
            if (bgMesh == null)
            {
                var newGO = new GameObject("BgMesh");
                newGO.transform.parent = transform;
                bgMesh = newGO.AddComponent<PopupBackgroundMesh>();
                bgMesh.Setup(this);
#if UNITY_EDITOR
                bgMesh.gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
            }
        }

        /// <summary>
        /// Show a temporary comfirm popup, return that popup. Use .onExit(Action<bool>) to handle the event if player confirm or not
        /// </summary>
        public static ConfirmPopup ShowDefaultPopup(string title, string description)
        {
            if (AutoInstance == null && AutoInstance.defaultComfirmPopup)
            {
                Debug.LogError("DefaultPopup or PopupBackground not setup");
                return null;
            }
            DefaultConfirmPopup newGO = Instantiate(AutoInstance.defaultComfirmPopup, AutoInstance.transform);
            newGO.RefreshText(title, description);
            newGO.Show();
            return newGO as ConfirmPopup;
        }

        internal void RegistPopup(PopupCanvas _popupCanvas)
        {
            if (popUpList.Contains(_popupCanvas))
            {
                popUpList.Remove(_popupCanvas);
            }
            else
            {
                _popupCanvas.onHideFinished += HideFinish;
            }
            popUpList.Push(_popupCanvas);
            ActivateMesh(_popupCanvas);
        }
        private void HideFinish(BaseCanvas _popup)
        {
            _popup.onHideFinished -= HideFinish;
            PopupCanvas popup = (PopupCanvas)_popup;
            //if popup is currently active
            if (popup == popUpList.PeekLast())
            {
                popUpList.Pop();
                TurnOffActivePopup();
            }
            else if (popUpList.Contains(popup))
            {
                popUpList.Remove(popup);
                popup.Hide();
            }
        }
        internal void BackgroundMeshClicked()
        {
            PopupCanvas canvas = popUpList.Pop();
            if (canvas != null)
            {
                canvas.Hide();
                TurnOffActivePopup();
            }
        }
        private void TurnOffActivePopup()
        {
            if (popUpList.Count > 0)
            {
                ActivateMesh(popUpList.PeekLast());
            }
            else
                bgMesh.Hide();
        }

        private void ActivateMesh(PopupCanvas _canvas)
        {
            if (_canvas.transform.parent != transform)
                _canvas.transform.parent = transform;
            bgMesh.transform.SetAsLastSibling();
            _canvas.transform.SetAsLastSibling();
            bgMesh.Show();
        }
    }


    [RequireComponent(typeof(Button), typeof(Image), typeof(RectTransform))]
    internal class PopupBackgroundMesh : BaseCanvas
    {
        private readonly Color meshColor = new Color(0, 0, 0, 0);

        private Button fakeButton { get; set; }
        private Image meshImage { get; set; }
        private RectTransform rectTransform { get; set; }
        private PopupBackground handler { get; set; }

        private float FadeInDuration => handler.FadeInDuration;
        private float FadeOutDuration => handler.FadeOutDuration;
        private float FullMaskAlpha => handler.FullMaskAlpha;
        private float imageAlpha
        {
            get => meshImage.color.a;
            set
            {
                var color = meshImage.color;
                color.a = Mathf.Clamp01(value);
                meshImage.color = color;
            }
        }
        public float AlphaValue
        {
            get => Mathf.Clamp01(imageAlpha / FullMaskAlpha);
            private set => imageAlpha = value * FullMaskAlpha;
        }
        //eventually, Awake will happen before Setup called from Handler
        private void Awake()
        {
            meshImage = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;

            fakeButton = GetComponent<Button>();
            fakeButton.transition = Selectable.Transition.None;
            fakeButton.onClick.AddListener(() =>
            {
                handler.BackgroundMeshClicked();
            });
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllCoroutines();
        }
        internal void Setup(PopupBackground _handler)
        {
            handler = _handler;
            meshImage.color = new Color(meshColor.r, meshColor.g, meshColor.b, 0f);

            Enable = false;
        }

        public override void Show(bool _isOn = true)
        {
            if (_isOn == true)
            {
                base.Show(true);
                StartCoroutine(_FadeIn(FadeInDuration));
            }
            else
            {
                StartCoroutine(_FadeOut(FadeOutDuration));
            }
        }

        private IEnumerator _FadeIn(float maxDur)
        {
            float _timeTick = AlphaValue * maxDur;
            while (true)
            {
                //Debug.Log(_timeTick);
                yield return null;
                _timeTick += Time.deltaTime;
                if (_timeTick >= maxDur)
                    break;
                AlphaValue = (_timeTick / maxDur);
            }
            AlphaValue = 1;
        }

        private IEnumerator _FadeOut(float maxDur)
        {
            float _timeTick = AlphaValue * maxDur;
            while (true)
            {
                //Debug.Log(_timeTick);
                yield return null;
                _timeTick -= Time.deltaTime;
                if (_timeTick <= 0)
                    break;
                AlphaValue = (_timeTick / maxDur);
            }
            AlphaValue = 0;
            base.Show(false);
        }
        
    }
}
