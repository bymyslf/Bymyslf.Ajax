using Bymyslf.Ajax;
using System.Web.Mvc;

namespace Bymyslf.Ajax.Demo.Areas.BymyslfAjaxDemo
{
    public class BymyslfAjaxDemoAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "BymyslfAjaxDemo";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.Routes.Add(new AjaxRoute("ajax/{controller}"));

            context.MapRoute(
                "BymyslfAjaxDemo_default",
                "ajax/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}