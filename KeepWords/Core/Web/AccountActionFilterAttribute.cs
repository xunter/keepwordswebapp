using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KeepWords.Core.Web.Security;

namespace KeepWords.Core.Web
{
    public class AccountActionFilterAttribute : ActionFilterAttribute
    {
        public IAuthService AuthService { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Result is ViewResultBase)
            {
                var viewResult = (ViewResultBase)filterContext.Result;
                viewResult.ViewData["account"] = AuthService.CurrentAccount;
            }
            base.OnActionExecuted(filterContext);
        }
    }
}