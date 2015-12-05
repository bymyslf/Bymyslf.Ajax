namespace Bymyslf.Ajax
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Web.Mvc;
    using System.Web.Mvc.Async;

    public class AjaxReflectedControllerDescriptor : ReflectedAsyncControllerDescriptor
    {
        private ActionDescriptor[] canonicalActionsCache;

        public AjaxReflectedControllerDescriptor(Type controllerType)
            : base(controllerType)
        {
        }

        public override ActionDescriptor[] GetCanonicalActions()
        {
            var existingCache = Interlocked.CompareExchange(ref canonicalActionsCache, null, null);
            if (existingCache != null)
            {
                return existingCache;
            }

            var memberInfos = this.GetAllActionMethods();

            var descriptorsList = new List<ActionDescriptor>(memberInfos.Count);
            for (int i = 0; i < memberInfos.Count; i++)
            {
                var methodInfo = memberInfos[i];
                ActionDescriptor descriptor = new ReflectedActionDescriptor(methodInfo, methodInfo.Name, this);
                if (descriptor != null)
                {
                    descriptorsList.Add(descriptor);
                }
            }

            var descriptors = descriptorsList.ToArray();

            ActionDescriptor[] updatedCache = Interlocked.CompareExchange(ref canonicalActionsCache, descriptors, null);
            return updatedCache ?? descriptors;
        }

        private List<MethodInfo> GetAllActionMethods()
        {
            var allMethods = ControllerType.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            var actionMethods = Array.FindAll(allMethods, IsValidActionMethod);

            var aliasedMethods = Array.FindAll(actionMethods, IsMethodDecoratedWithAliasingAttribute);
            var nonAliasedMethods = actionMethods.Except(aliasedMethods).ToLookup(method => method.Name, StringComparer.OrdinalIgnoreCase);

            var allValidMethods = new List<MethodInfo>();
            allValidMethods.AddRange(aliasedMethods);
            allValidMethods.AddRange(nonAliasedMethods.SelectMany(g => g));

            return allValidMethods;
        }

        private static bool IsValidActionMethod(MethodInfo methodInfo)
        {
            return !(methodInfo.IsSpecialName ||
                     methodInfo.GetBaseDefinition().DeclaringType.IsAssignableFrom(typeof(Controller)));
        }

        private static bool IsMethodDecoratedWithAliasingAttribute(MethodInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(ActionNameSelectorAttribute), true);
        }
    }
}