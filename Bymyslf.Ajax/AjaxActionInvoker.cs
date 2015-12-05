namespace Bymyslf.Ajax
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Mvc.Async;
    using System.Web.Script.Serialization;

    public class AjaxActionInvoker : AsyncControllerActionInvoker
    {
        private static readonly ControllerDescriptorCache descriptorCache;
        internal const string AjaxProxyAction = "Internal::Proxy";

        static AjaxActionInvoker()
        {
            descriptorCache = new ControllerDescriptorCache();
        }

        public override bool InvokeAction(ControllerContext controllerContext, string actionName)
        {
            if (actionName == AjaxProxyAction)
            {
                return RenderJavaScriptProxyScript(controllerContext);
            }

            return base.InvokeAction(controllerContext, actionName);
        }

        protected override ControllerDescriptor GetControllerDescriptor(ControllerContext controllerContext)
        {
            Type controllerType = controllerContext.Controller.GetType();
            return descriptorCache.GetDescriptor(controllerType, () => new AjaxReflectedControllerDescriptor(controllerType));
        }

        private bool RenderJavaScriptProxyScript(ControllerContext controllerContext)
        {
            var controllerDescriptor = this.GetControllerDescriptor(controllerContext);
            var actions = from action in controllerDescriptor.GetCanonicalActions()
                          select new
                          {
                              Name = action.ActionName,
                              Method = action.GetActionHttpMethod(),
                              DataType = action.GetAjaxDataType()
                          };

            var serializer = new JavaScriptSerializer();
            var actionMethods = serializer.Serialize(actions);

            string proxyScript = @";(function (mvc, undefined) {{
                    mvc.{0} = [];
                    var actions = {1};
                    for (var i = 0, len = actions.length; i < len; i++) {{
                        var action = actions[i];
                        (function (action, mvc) {{
                            mvc.{0}[action.Name] = function(obj, includeAntiForgeryToken) {{
                                var headers = {{'x-mvc-action': action}};
                                if (includeAntiForgeryToken) {{
                                    headers['__RequestVerificationToken'] = $('input[name=""__RequestVerificationToken""]').val();
                                }}
                                return $.ajax({{
                                    cache: false,
                                    dataType: 'json',
                                    type: action.Method,
                                    headers: headers,
                                    data: JSON.stringify(obj),
                                    contentType: 'application/json; charset=utf-8',
                                    url: '{2}/' + action.Name
                                }});
                            }};
                        }})(action, mvc);
                    }};
                }})(window.mvc = window.mvc || {{}});
                ";

            proxyScript = String.Format(proxyScript,
                controllerDescriptor.ControllerName,
                actionMethods,
                controllerContext.HttpContext.Request.RawUrl);

            controllerContext.HttpContext.Response.ContentType = "text/javascript";
            controllerContext.HttpContext.Response.Write(proxyScript);

            return true;
        }

        private class ControllerDescriptorCache : ReaderWriterCache<Type, ControllerDescriptor>
        {
            public ControllerDescriptorCache()
            {
            }

            public ControllerDescriptor GetDescriptor(Type controllerType, Func<ControllerDescriptor> creator)
            {
                return FetchOrCreateItem(controllerType, creator);
            }
        }
    }
}