namespace Bymyslf.Ajax
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public static class ActionDescriptorExtensions
    {
        private static readonly Dictionary<Type, string> ajaxDataTypes;
        private static readonly Dictionary<Type, string> actionHttpMethods;

        static ActionDescriptorExtensions()
        {
            ajaxDataTypes = new Dictionary<Type, string>()
                {
                    { typeof(ActionResult), "html" },
                    { typeof(ViewResult), "html" },
                    { typeof(PartialViewResult), "html" },
                    { typeof(JsonResult), "json" },
                    { typeof(JavaScriptResult), "script" }
                };

            actionHttpMethods = new Dictionary<Type, string>()
                {
                    { typeof(HttpGetAttribute), "GET" },
                    { typeof(HttpPostAttribute), "POST" },
                    { typeof(HttpPutAttribute), "PUT" },
                    { typeof(HttpDeleteAttribute), "DELETE" }
                };
        }

        public static string GetActionHttpMethod(this ActionDescriptor action)
        {
            var actionMethod = "GET";
            var reflectedActionDescriptor = action as ReflectedActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                if (reflectedActionDescriptor.IsDefined(typeof(ActionMethodSelectorAttribute), true))
                {
                    var httpMethod = (ActionMethodSelectorAttribute)reflectedActionDescriptor.MethodInfo.GetCustomAttributes(typeof(ActionMethodSelectorAttribute), inherit: true).FirstOrDefault();
                    actionHttpMethods.TryGetValue(httpMethod.GetType(), out actionMethod);
                }
            }

            return actionMethod;
        }

        public static string GetAjaxDataType(this ActionDescriptor action)
        {
            var dataType = "text";
            var reflectedActionDescriptor = action as ReflectedActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                var actionReturnType = reflectedActionDescriptor.MethodInfo.ReturnType;
                if (actionReturnType != null && typeof(Task).IsAssignableFrom(actionReturnType))
                {
                    actionReturnType = reflectedActionDescriptor.MethodInfo.ReturnType.GenericTypeArguments[0];
                }

                ajaxDataTypes.TryGetValue(actionReturnType, out dataType);
            }

            return dataType;
        }
    }
}