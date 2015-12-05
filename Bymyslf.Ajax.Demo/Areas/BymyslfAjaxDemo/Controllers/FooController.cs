using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Bymyslf.Ajax.Demo.Areas.BymyslfAjaxDemo.Models;

namespace Bymyslf.Ajax.Demo.Areas.BymyslfAjaxDemo.Controllers
{
    public class FooController : AjaxController
    {
        public ActionResult List()
        {
            return new JsonResult()
            {
                Data = new Foo[] {
                      new Foo { Id = 1, Title = "Groo" },
                      new Foo { Id = 2, Title = "Batman" },
                      new Foo { Id = 3, Title = "Spiderman" }
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public Task<ActionResult> ListAsync()
        {
            return Task.FromResult(List());
        }

        [HttpPost]
        public ActionResult Save(Foo fooObj)
        {
            return new JsonResult { Data = new { message = "Saved!", Object = fooObj } };
        }

        [HttpPost]
        [ValidateAjaxAntiForgeryToken]
        public ActionResult SaveWithAntiForgeryToken(Foo fooObj)
        {
            return new JsonResult { Data = new { message = "Saved With AntiForgeryToken!", Object = fooObj } };
        }
    }
}