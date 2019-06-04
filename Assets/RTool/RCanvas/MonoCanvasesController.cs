using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using REditor;
using UnityEditor;
using UnityEditorInternal;
#endif

namespace RTool.EzCanvas
{
    public class MonoCanvasesController : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private MonoCanvas _currentActiveCanvas;
        public virtual MonoCanvas CurrentActiveCanvas
        {
            internal set
            {
                if (_currentActiveCanvas == value)
                    return;
                CurrentActiveCanvasChanged(value);
                _currentActiveCanvas = value;
            }
            get
            {
                return _currentActiveCanvas;
            }
        }
        protected virtual void CurrentActiveCanvasChanged(MonoCanvas _newCanvas) { }

        [SerializeField, HideInInspector]
        private List<MonoCanvas> AllMono = new List<MonoCanvas>();

        public bool IsBusy { private set; get; } 

        #region stack simulate
        private List<MonoCanvas> CanvasStack = new List<MonoCanvas>();
        private void StackPush(MonoCanvas monoCanvas, MonoCanvas _newMono)
        {
            int index = CanvasStack.IndexOf(_newMono);
            if (index > 0)
            {
                CanvasStack.RemoveRange(index, CanvasStack.Count - index);
            }
            else
                CanvasStack.Add(monoCanvas);
        }
        private MonoCanvas StackPop()
        {
            if (CanvasStack.Count == 0)
                return null;
            int index = CanvasStack.Count - 1;
            MonoCanvas result = CanvasStack[index];
            CanvasStack.RemoveAt(index);
            return result;
        }
        #endregion

        public void RegistMono(MonoCanvas _mono)
        {
            if (AllMono.Contains(_mono))
                return;

            AllMono.Add(_mono);
        }
        public void UnRegistMono(MonoCanvas _mono)
        {
            if (!AllMono.Contains(_mono))
                return;

            AllMono.Remove(_mono);
            if (CurrentActiveCanvas == _mono)
            {
                if (AllMono.Count > 0)
                    CurrentActiveCanvas = AllMono[0];
                else
                    CurrentActiveCanvas = null;
            }
        }
        
        internal void PagingNewMono (MonoCanvas _newMono)
        {
            IsBusy = true;
            //_newMono.onShowFinished += ResetBusyShow;
            if (CurrentActiveCanvas != _newMono)
            {
                if (CurrentActiveCanvas != null)
                {
                    if (CurrentActiveCanvas.IgnoreStack == false)
                    {
                        //Debug.Log("Stack push for " + CurrentActiveCanvas.gameObject.name, CurrentActiveCanvas.gameObject);
                        StackPush(CurrentActiveCanvas, _newMono);
                    }
                    CurrentActiveCanvas.Hide();
                }
                CurrentActiveCanvas = _newMono;
            }
            _newMono.BaseShow(true);
            IsBusy = false;
        }
        internal void PoppinOldMono()
        {
            if (CurrentActiveCanvas == null)
            {
                Debug.Log("There is no canvas to return");
                return;
            }

            IsBusy = true;
            CurrentActiveCanvas.BaseShow(false);
            if (CanvasStack.Count > 0)
            {
                MonoCanvas popCanvas = StackPop();
                popCanvas.Show();
                //popCanvas.onShowFinished += ResetBusyShow;
                CurrentActiveCanvas = popCanvas;
            }
            else
            {
                //CurrentActiveCanvas.BaseShow(false);
                //CurrentActiveCanvas.onHideFinished += ResetBusyHide;
                CurrentActiveCanvas = null;
                Debug.Log("null canvas");
            }
            IsBusy = false;
        }

        private void ResetBusyShow(BaseCanvas canvas)
        {
            StartCoroutine(_ResetBusyShow(canvas));
        }
        IEnumerator _ResetBusyShow(BaseCanvas canvas)
        {
            yield return null;
            IsBusy = false;
            canvas.onShowFinished -= ResetBusyShow;
        }

        private void ResetBusyHide(BaseCanvas canvas)
        {
            StartCoroutine(_ResetBusyHide(canvas));
        }
        IEnumerator _ResetBusyHide(BaseCanvas canvas)
        {
            yield return null;
            IsBusy = false;
            canvas.onHideFinished -= ResetBusyHide;
        }
        protected virtual void Start()
        {
            if (CurrentActiveCanvas != null)
            {
                var initMono = _currentActiveCanvas;
                _currentActiveCanvas = null;
                initMono.Show();
            }
        }
        
        #region public call
        public virtual void ReturnToPreviousMenu()
        {
            if (CurrentActiveCanvas != null)
            {
                CurrentActiveCanvas.Hide();
            }
        }
        #endregion
#if UNITY_EDITOR
        internal void EditorSetCurrentActiveCanvas (MonoCanvas _monoCanvas)
        {
            Undo.RecordObjects(AllMono.ToArray(), "Default MonoCanvas");
            CurrentActiveCanvas = _monoCanvas;
            foreach (var item in AllMono)
            {
                item.IsPreview = false;
                if (item == CurrentActiveCanvas)
                    item.Enable = true;
                else
                    item.Enable = false;
            }
        }
        [SerializeField, HideInInspector]
        private bool showDetail = false;

        internal void ForceVisible(MonoCanvas mono)
        {
            foreach (var item in AllMono)
            {
                item.EditorVisible = ((item == mono) ? true : false);
            }
        }
        internal void ResetAllPreview()
        {
            foreach (var item in AllMono)
            {
                item.IsPreview = false;
            }
        }
        [CustomEditor(typeof(MonoCanvasesController), true, isFallback = true)]
        private class CustomInspector : UnityObjectEditor<MonoCanvasesController>
        {
            private ReorderableList visualCanvasList;
            private ReorderableList visualStackList;
            const float space = 30f;
            const float defaultBoxWidth = 30f;

            protected override void OnEnable()
            {
                base.OnEnable();
                SetupVisualCanvasList();
                SetupStack();
                Shuffle();
            }
            private void SetupVisualCanvasList()
            {
                visualCanvasList = new ReorderableList(handler.AllMono, typeof(MonoCanvas), false, true, true, true);
                visualCanvasList.drawHeaderCallback = (Rect _rect) =>
                {
                    GUI.Label(_rect, new GUIContent("List MonoCanvas", "Setup all MonoCanvas that can be controlled by this Component here"));
                };
                visualCanvasList.drawElementCallback = (Rect _rect, int _index, bool _active, bool _focus) =>
                {
                    var mono = handler.AllMono[_index];
                    Rect firstRect = new Rect(_rect.x, _rect.y, space - 10f, EditorGUIUtility.singleLineHeight);
                    Rect valueRect = new Rect(firstRect.xMax + 10f, _rect.y, _rect.width - space - defaultBoxWidth, EditorGUIUtility.singleLineHeight);
                    Rect defaultRect = new Rect(valueRect.xMax, _rect.y, defaultBoxWidth, EditorGUIUtility.singleLineHeight);

                    if (mono != null)
                    {
                        //draw the preview section
                        var activeContent = new GUIContent("≡", "Click to preview the layout");
                        var view = GUI.Toggle(firstRect, mono.EditorVisible, activeContent, EditorStyles.miniButton);
                        if (view != mono.EditorVisible)
                        {
                            if (view == true)
                            {
                                handler.ForceVisible(mono);
                            }
                            else
                            {
                                handler.ResetAllPreview();
                            }
                        }

                        //draw the default section
                        bool isDefault = (handler.CurrentActiveCanvas != null && mono == handler.CurrentActiveCanvas);
                        bool newBool = GUI.Toggle(defaultRect, isDefault, "", EditorStyles.radioButton);
                        if (newBool != isDefault)
                        {
                            if (newBool == true)
                            {
                                //handler.ResetAllPreview();
                                handler.EditorSetCurrentActiveCanvas(mono);
                            }
                            else
                            {
                                //handler.ResetAllPreview();
                                handler.EditorSetCurrentActiveCanvas(null);
                            }
                        }
                    }

                    MonoCanvas value = EditorGUI.ObjectField(valueRect, mono, typeof(MonoCanvas), true) as MonoCanvas;
                    if (value != mono)
                    {
                        if (!handler.AllMono.Contains(value))
                        {
                            if (mono != null)
                                mono.controller = null;
                            if (value != null)
                            {
                                value.controller = handler;
                                handler.AllMono[_index] = value;
                            }
                        }
                        else
                        {
                            Debug.LogError(value.gameObject.name + " is already contained in the List", value.gameObject);
                        }
                    }
                };
                visualCanvasList.onAddCallback = (ReorderableList _rList) =>
                {
                    handler.AllMono.Add(null);
                };
                visualCanvasList.onCanAddCallback = (ReorderableList _rList) =>
                {
                    if (handler.AllMono.Count == 0)
                        return true;
                    if (handler.AllMono[handler.AllMono.Count - 1] == null)
                        return false;
                    return true;
                };
                visualCanvasList.onRemoveCallback = (ReorderableList _rList) =>
                {
                    int index = _rList.index;
                    if (handler.AllMono[index] != null)
                        handler.AllMono[index].Controller = null;
                };
            }
            private void SetupStack()
            {
                visualStackList = new ReorderableList(handler.CanvasStack, typeof(MonoCanvas), false, false, false, false);
                visualStackList.drawHeaderCallback = (Rect _rect) =>
                {
                    GUI.Label(_rect, new GUIContent("Menu Stack", "Current paging stack of all MonoCanvas)"));
                };
                visualStackList.drawElementCallback = (Rect _rect, int _index, bool _active, bool _focus) =>
                {
                    var mono = handler.CanvasStack[_index];
                    Rect firstRect = new Rect(_rect.x, _rect.y, space, EditorGUIUtility.singleLineHeight);
                    Rect valueRect = new Rect(firstRect.xMax, _rect.y, _rect.width - space, EditorGUIUtility.singleLineHeight);

                    GUI.Label(firstRect, new GUIContent(_index.ToString(), "Click to preview the layout"));
                    
                    EditorGUI.ObjectField(valueRect, mono, typeof(MonoCanvas), true);
                };
            }
            private void Shuffle()
            {
                List<int> toDel = new List<int>();
                for (int i = 0; i < handler.AllMono.Count; i++)
                {
                    if (handler.AllMono[i] == null)
                        toDel.Add(i);
                }
                for (int i = toDel.Count -1; i >= 0;i--)
                {
                    handler.AllMono.RemoveAt(toDel[i]);
                }
            }

            public override void OnInspectorGUI()
            {
                GUILayout.BeginVertical();
                EditorGUILayout.LabelField(new GUIContent("Mono Canvas Controller",
                    "A small but purfect tool for manage canvas. E-meow me fur more detail at: hoa.nguyenduc1206@gmail.com"),
                    EditorStyles.centeredGreyMiniLabel);
                if (Application.isPlaying)
                    DrawStack();
                else 
                    DrawListMono();
                GUILayout.EndVertical();
                base.OnInspectorGUI();
            }

            private void DrawListMono()
            {
                handler.showDetail = GUILayout.Toggle(handler.showDetail, new GUIContent("Setup"), EditorStyles.miniButton);
                if (handler.showDetail)
                {
                    visualCanvasList.DoLayoutList();
                }
            }
            private void DrawStack()
            {
                handler.showDetail = GUILayout.Toggle(handler.showDetail, new GUIContent("Menu Stack"), EditorStyles.miniButton);
                if (handler.showDetail)
                {
                    visualStackList.DoLayoutList();
                }
            }
        }
#endif
    }
}

