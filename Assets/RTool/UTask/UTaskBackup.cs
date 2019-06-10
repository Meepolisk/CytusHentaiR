//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace RTool.UTask
//{
//    public abstract class UTaskBase<T> where T : Task
//    {
//        private Coroutine coroutine { get; set; }
//        private bool forceStopCheck { get; set; }
//        public MonoBehaviour Handler { get; private set; }
//        protected T task { get; private set; }

//        protected UTaskBase(MonoBehaviour _handler, T _task)
//        {
//            try
//            {
//                Handler = _handler;
//                task = _task;
//                coroutine = Handler.StartCoroutine(_Coroutine());
//                task.ContinueWith(task =>
//                {
//                    forceStopCheck = true;
//                });
//            }
//            catch (Exception exception)
//            {
//                Handler.StartCoroutine(_ForstopCoroutine(exception));
//            }
//        }
//        private IEnumerator _Coroutine()
//        {
//            forceStopCheck = false;
//            while (forceStopCheck != true)
//            {
//                yield return null;
//            }
//            OnCoroutineComplete();
//            coroutine = null;
//        }
//        private IEnumerator _ForstopCoroutine(Exception exception)
//        {
//            yield return null;
//            OnCoroutineComplete(exception);
//        }
//        private void OnCoroutineComplete(Exception _forceException = null)
//        {
//            if (task.IsCanceled)
//            {
//                CallBack_Failed(new Exception("Task canceled!"));
//                CallBack_Canceled();
//            }
//            else if (task.IsFaulted)
//            {
//                CallBack_Failed(task.Exception);
//                CallBack_Fault(task.Exception);
//            }
//            else //is succeeded
//            {
//                CallBack_Succeeded();
//            }
//            CallBack_Finished();
//        }
//        protected virtual void CallBack_Failed(Exception exception)
//        {
//            if (onFailedActionList != null)
//                foreach (var function in onFailedActionList)
//                    function(exception);
//        }
//        protected virtual void CallBack_Canceled()
//        {
//            if (onCanceledActionList != null)
//                foreach (var function in onCanceledActionList)
//                    function();
//        }
//        protected virtual void CallBack_Fault(Exception exception)
//        {
//            if (onFaultedActionList != null)
//                foreach (var function in onFaultedActionList)
//                    function(exception);
//        }
//        protected virtual void CallBack_Finished()
//        {
//            if (onFinishedActionList != null)
//                foreach (var function in onFinishedActionList)
//                    function(this);
//        }
//        protected virtual void CallBack_Succeeded()
//        {
//            if (onSucceededActionList != null)
//                foreach (var function in onSucceededActionList)
//                    function();
//        }

//        private List<Action<UTaskBase<T>>> onFinishedActionList { get; set; }
//        private List<Action> onSucceededActionList { get; set; }
//        private List<Action<Exception>> onFailedActionList { get; set; }
//        private List<Action> onCanceledActionList { get; set; }
//        private List<Action<Exception>> onFaultedActionList { get; set; }

//        public UTaskBase<T> onFinished(Action<UTaskBase<T>> _onFinishedAction)
//        {
//            if (onFinishedActionList == null)
//                onFinishedActionList = new List<Action<UTaskBase<T>>>();
//            onFinishedActionList.Add(_onFinishedAction);
//            return this;
//        }
//        public iTask<T> onSucceeded(Action _onSucceededAction)
//        {
//            if (onSucceededActionList == null)
//                onSucceededActionList = new List<Action>();
//            onSucceededActionList.Add(_onSucceededAction);
//            return this;
//        }
//        public iTask<T> onFailed(Action<Exception> _onFailedAction)
//        {
//            if (onFailedActionList == null)
//                onFailedActionList = new List<Action<Exception>>();
//            onFailedActionList.Add(_onFailedAction);
//            return this;
//        }
//        public iTask<T> onCanceled(Action _onCanceledAction)
//        {
//            if (onCanceledActionList == null)
//                onCanceledActionList = new List<Action>();
//            onCanceledActionList.Add(_onCanceledAction);
//            return this;
//        }
//        public iTask<T> onFaulted(Action<Exception> _onFaultedAction)
//        {
//            if (onFaultedActionList == null)
//                onFaultedActionList = new List<Action<Exception>>();
//            onFaultedActionList.Add(_onFaultedAction);
//            return this;
//        }
//    }
//    public class UTask : UTaskBase<Task>
//    {
//        public UTask(MonoBehaviour _handler, Task _task) : base(_handler, _task) { }
//        public new UTask onFinished(Action<UTask> _onFinishedAction) => base.onFinished(_onFinishedAction);
//    }
//    public class UTask<T> : UTaskBase<Task<T>>
//    {
//        public UTask(MonoBehaviour _handler, Task<T> _task) : base(_handler, _task) { }

//        protected override void CallBack_Succeeded()
//        {
//            if (onSucceededActionR != null)
//                foreach (var function in onSucceededActionR)
//                    function(task.Result);
//            base.CallBack_Succeeded();
//        }
//        private List<Action<T>> onSucceededActionR { get; set; }
//        public UTask<T> onSucceededR(Action<T> _onSucceededAction)
//        {
//            if (onSucceededActionR == null)
//                onSucceededActionR = new List<Action<T>>();
//            onSucceededActionR.Add(_onSucceededAction);
//            return this;
//        }
//        public new UTask<T> onFaulted(Action<Exception> _onFaultedAction) => (UTask<T>)base.onFaulted(_onFaultedAction);
//        public new UTask<T> onFinished(Action<UTask<T>> _onFinishedAction) => (UTask<T>)base.onFinished(_onFinishedAction);
//        public new UTask<T> onSucceeded(Action _onSucceededAction)
//        {
//            if (onSucceededActionList == null)
//                onSucceededActionList = new List<Action>();
//            onSucceededActionList.Add(_onSucceededAction);
//            return this;
//        }
//        public new UTask<T> onFailed(Action<Exception> _onFailedAction)
//        {
//            if (onFailedActionList == null)
//                onFailedActionList = new List<Action<Exception>>();
//            onFailedActionList.Add(_onFailedAction);
//            return this;
//        }
//        public new UTask<T> onCanceled(Action _onCanceledAction)
//        {
//            if (onCanceledActionList == null)
//                onCanceledActionList = new List<Action>();
//            onCanceledActionList.Add(_onCanceledAction);
//            return this;
//        }
//    }
//    public static class UTaskExtension
//    {
//        public static UTask<T> UTask<T>(this MonoBehaviour _handler, Task<T> _task)
//        {
//            return new UTask<T>(_handler, _task);
//        }
//        public static UTask UTask(this MonoBehaviour _handler, Task _task)
//        {
//            return new UTask(_handler, _task);
//        }
//    }
//}
