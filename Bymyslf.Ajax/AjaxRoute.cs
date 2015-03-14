﻿using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bymyslf.Ajax
{
    public class AjaxRoute : Route
    {
        public AjaxRoute(string url)
            : base(url, new MvcRouteHandler())
        {
            if (url.IndexOf("{action}", StringComparison.OrdinalIgnoreCase) > -1)
            {
                throw new ArgumentException("url may not contain the {action} parameter", "url");
            }
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return null;
        }
    }
}