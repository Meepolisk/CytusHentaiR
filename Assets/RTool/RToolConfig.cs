using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace RTool
{
    internal static class RTool
    {
        internal const string rootNameSpace = "RTool";

        //todo: chỗ này làm List<Action> để reg / unreg action như kiểu SetHideFlag cho PopupCanvasMesh

        //internal static bool IsDebug = false;

        //private const string menuNameOn = rootNameSpace + " / " + "Debug ON";
        //[MenuItem(menuNameOn)]
        //private static void DebugOn()
        //{
        //    Debug.Log("RTool debug set to [ON]");
        //    IsDebug = true;
        //}
        //[MenuItem(menuNameOn, true)]
        //private static bool CanDebugOn() => IsDebug == false;

        //private const string menuNameOff = rootNameSpace + " / " + "Debug OFF";
        //[MenuItem(menuNameOff)]
        //private static void DebugOff()
        //{
        //    Debug.Log("RTool debug set to [OFF]");
        //    IsDebug = false;
        //}
        //[MenuItem(menuNameOff, true)]
        //private static bool CanDebugOFF() => IsDebug == true;

    }
}