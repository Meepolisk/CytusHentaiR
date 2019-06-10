using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RTool.UTask
{
    public sealed class UTask<T>
    {
        private Task<T> task { get; set; }
        private Coroutine coroutine { get; set; }
        public MonoBehaviour Handler => UTaskHandler.Instance;

        public T result { get; private set; }
        public Exception exception { get; private set; }
        public bool isFinished => (result != null || exception != null);

        private List<Action<UTask<T>>> onFinishedActionList { get; set; }
        private List<Action> onSucceededActionList { get; set; }
        private List<Action<T>> onSucceededActionListR { get; set; }
        private List<Action<Exception>> onFailedActionList { get; set; }

        private UTask(Exception _exception)
        {
            exception = _exception;
            coroutine = Handler.StartCoroutine(_Coroutine());
        }
        private UTask(T _result)
        {
            result = _result;
            coroutine = Handler.StartCoroutine(_Coroutine());
        }
        public UTask(Task<T> _task, float _timeOut = 5f)
        {
            try
            {
                task = _task;
                task.ContinueWith(task => 
                {
                    if (task.IsCanceled)
                        exception = new Exception("Task is canceled");
                    else if (task.IsFaulted)
                        exception = task.Exception;
                    else
                        result = task.Result;
                });
            }
            catch (Exception _exception)
            {
                exception = _exception;
            }
            finally
            {
                coroutine = Handler.StartCoroutine(_Coroutine(_timeOut));
            }
        }
        /// <summary>
        /// Create a Failed UTask;
        /// </summary>
        public static UTask<T> ForceFail(Exception exception) => new UTask<T>(exception);
        /// <summary>
        /// Create a Succeed UTask with result;
        /// </summary>
        public static UTask<T> ForceSuccess(T result) => new UTask<T>(result);

        private IEnumerator _Coroutine(float timeOut = 1f)
        {
            float timeTick = 0;
            while (isFinished != true || timeTick < timeOut)
            {
                yield return null;
                timeTick += Time.deltaTime;
            }
            OnCoroutineComplete();
            coroutine = null;
        }
        private void OnCoroutineComplete()
        {
            if (result != null)
            {
                CallBack_Succeeded();
            }
            else
            {
                if (exception == null)
                    exception = new Exception("OH NO");
                CallBack_Failed(exception);
            }
            CallBack_Finished();
        }
        #region CallBack
        private void CallBack_Finished()
        {
            if (onFinishedActionList != null)
                foreach (var function in onFinishedActionList)
                    function(this);
        }
        private void CallBack_Succeeded()
        {
            if (onSucceededActionListR != null)
                foreach (var function in onSucceededActionListR)
                    function(result);
            if (onSucceededActionList != null)
                foreach (var function in onSucceededActionList)
                    function();
        }
        private void CallBack_Failed(Exception exception)
        {
            if (onFailedActionList != null)
                foreach (var function in onFailedActionList)
                    function(exception);
        }
        #endregion

        #region public call
        public UTask<T> onFinished(Action<UTask<T>> _onFinishedAction)
        {
            if (onFinishedActionList == null)
                onFinishedActionList = new List<Action<UTask<T>>>();
            onFinishedActionList.Add(_onFinishedAction);
            return this;
        }
        public UTask<T> onSucceeded(Action _onSucceededAction)
        {
            if (onSucceededActionList == null)
                onSucceededActionList = new List<Action>();
            onSucceededActionList.Add(_onSucceededAction);
            return this;
        }
        public UTask<T> onSucceededResult(Action<T> _onSucceededRAction)
        {
            if (onSucceededActionListR == null)
                onSucceededActionListR = new List<Action<T>>();
            onSucceededActionListR.Add(_onSucceededRAction);
            return this;
        }
        public UTask<T> onFailed(Action<Exception> _onFailedAction)
        {
            if (onFailedActionList == null)
                onFailedActionList = new List<Action<Exception>>();
            onFailedActionList.Add(_onFailedAction);
            return this;
        }
        #endregion
    }
}
