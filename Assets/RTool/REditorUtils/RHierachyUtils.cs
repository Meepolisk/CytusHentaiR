//#if UNITY_EDITOR
//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;
//using UnityEngine.SceneManagement;

//namespace REditor.HierachyUtil
//{
//    [InitializeOnLoad]
//    public class CustomHierarchyView
//    {
//        static CustomHierarchyView()
//        {
//            Debug.Log("Oh ho ê hê");
//            Setup();
//            EditorApplication.hi += HierarchyWindowItemOnGUI;
//        }
//        static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
//        {
//            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
//            EditorApplication
//            //Debug.Log("ASSDADASDASDASD", gameObject);
//            try
//            {
//                var asdf = gameObject.GetComponent<RHierachyCallback>();
//                if (asdf == null)
//                    return;

//                if (currentActive.Contains(asdf) == false)
//                {
//                    currentActive.Add(asdf);
//                    asdf.OnEnterScene();
//                }
//                else if (currentActive == null)
//                {
//                    currentActive.Remove(asdf);
//                    asdf.OnLeaveScene();
//                }
//            }
//            catch { }
//        }
//        static List<RHierachyCallback> currentActive = new List<RHierachyCallback>();
//        static void Setup()
//        {
//            currentActive = GetGameObjectLoaded();
//        }

//        public static List<RHierachyCallback> GetGameObjectLoaded()
//        {
//            List<RHierachyCallback> results = new List<RHierachyCallback>();
//            if (!Application.isPlaying)
//            {
//                for (int i = 0; i < SceneManager.sceneCount; i++)
//                {
//                    var s = SceneManager.GetSceneAt(i);
//                    if (s.isLoaded)
//                    {
//                        var allGameObjects = s.GetRootGameObjects();
//                        for (int j = 0; j < allGameObjects.Length; j++)
//                        {
//                            var go = allGameObjects[j];
//                            RHierachyCallback[] toAdd = go.GetComponentsInChildren<RHierachyCallback>(true);
//                            foreach (var item in toAdd)
//                            {
//                                if (item != null)
//                                {
//                                    results.Add(item);
//                                    Debug.Log(item.GetName(), item.GetGameObject());
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            return results;
//        }
//    }
//#endif
//    public interface RHierachyCallback
//    {

//        string GetName();
//        GameObject GetGameObject();

//        void OnEnterScene();
//        void OnLeaveScene();
//    }
//#if UNITY_EDITOR
//}
//#endif