namespace Bymyslf.Ajax
{
    using System;
    using System.Threading;
    using System.Web.Mvc;

    public abstract class AjaxController : Controller
    {
        protected override IActionInvoker CreateActionInvoker()
        {
            return new AjaxActionInvoker();
        }

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            if (RouteData.Values.ContainsKey("action"))
            {
                return base.BeginExecuteCore(callback, state);
            }

            ActionInvoker.InvokeAction(ControllerContext, AjaxActionInvoker.AjaxProxyAction);
            return new CompletedAsyncResult() { AsyncState = state };
        }

        protected override void EndExecuteCore(IAsyncResult asyncResult)
        {
            if (asyncResult is CompletedAsyncResult)
            {
                return;
            }

            base.EndExecuteCore(asyncResult);
        }

        protected override void ExecuteCore()
        {
            EndExecuteCore(BeginExecuteCore(new AsyncCallback((IAsyncResult x) => { }), null));
        }

        private class CompletedAsyncResult : IAsyncResult
        {
            public object AsyncState { get; set; }

            public WaitHandle AsyncWaitHandle
            {
                get { throw new NotSupportedException("CompletedAsyncResult.AsyncWaitHandle"); }
            }

            public bool CompletedSynchronously
            {
                get { return true; }
            }

            public bool IsCompleted
            {
                get { return true; }
            }
        }
    }
}