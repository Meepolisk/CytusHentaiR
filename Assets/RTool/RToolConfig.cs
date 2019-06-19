#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
namespace RTool
{
    internal static class RTool
    {
        internal const string rootNameSpace = "RTool";

        //todo: chỗ này làm List<Action> để reg / unreg action như kiểu SetHideFlag cho PopupCanvasMesh

        internal static bool IsDebug = false;

        private const string menuNameOn = rootNameSpace + " / " + "Debug ON";
        [MenuItem(menuNameOn)]
        private static void DebugOn()
        {
            Debug.Log("RTool debug set to [ON]");
            IsDebug = true;
        }
        [MenuItem(menuNameOn, true)]
        private static bool CanDebugOn() => IsDebug == false;

        private const string menuNameOff = rootNameSpace + " / " + "Debug OFF";
        [MenuItem(menuNameOff)]
        private static void DebugOff()
        {
            Debug.Log("RTool debug set to [OFF]");
            IsDebug = false;
        }
        [MenuItem(menuNameOff, true)]
        private static bool CanDebugOFF() => IsDebug == true;

        [MenuItem(rootNameSpace + " / Clear PlayerPrefs")]
        private static void NewMenuOption()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("All PlayerPrefs cleared");
        }

        [MenuItem("Assets/Load Additive Scene")]
        private static void LoadAdditiveScene()
        {
            var selected = Selection.activeObject;
            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(selected));
        }
    }
}
public class UnityObjectEditor<T> : Editor where T : UnityEngine.Object
{
    protected T handler { private set; get; }

    protected virtual void OnEnable()
    {
        handler = (T)target;
    }
}
#endif