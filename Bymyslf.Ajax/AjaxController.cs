﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace Bymyslf.Ajax
{
    public class AjaxController : Controller
    {
        protected override IActionInvoker CreateActionInvoker()
        {
            return new AjaxActionInvoker();
        }

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            if (!RouteData.Values.ContainsKey("action"))
            {
                ActionInvoker.InvokeAction(ControllerContext, "Internal::Proxy");
                return new CompletedAsyncResult() { AsyncState = state };
            }

            return base.BeginExecuteCore(callback, state);
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