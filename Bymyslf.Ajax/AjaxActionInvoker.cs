﻿using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Script.Serialization;

namespace Bymyslf.Ajax
{
    public class AjaxActionInvoker : ControllerActionInvoker
    {
        public override bool InvokeAction(ControllerContext controllerContext, string actionName)
        {
            if (actionName == "Internal::Proxy")
            {
                return RenderJavaScriptProxyScript(controllerContext);
            }

            return base.InvokeAction(controllerContext, actionName);
        }

        private bool RenderJavaScriptProxyScript(ControllerContext controllerContext)
        {
            var controllerDescriptor = GetControllerDescriptor(controllerContext);
            var actions = from action in controllerDescriptor.GetCanonicalActions()
                          select new
                          {
                              Name = action.ActionName,
                              Method = GetActionHttpMethod(action)
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

        private string GetActionHttpMethod(ActionDescriptor action)
        {
            var reflectedActionDescriptor = action as ReflectedActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                if (reflectedActionDescriptor.IsDefined(typeof(ActionMethodSelectorAttribute), true))
                {
                    var httpMethod = (ActionMethodSelectorAttribute)reflectedActionDescriptor.MethodInfo.GetCustomAttributes(typeof(ActionMethodSelectorAttribute), inherit: true).FirstOrDefault();
                    return httpMethod.GetType().Name.Replace("Http", "").Replace("Attribute", "").ToUpper();
                }
            }

            return "GET";
        }
    }
}