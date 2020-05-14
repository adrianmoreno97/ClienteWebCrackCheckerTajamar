using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClienteWebCrackChecker.Filters
{
    public class UsuariosAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity.IsAuthenticated == false)
            {
                RouteValueDictionary ruta = new RouteValueDictionary(new
                {
                    controller = "Manage",
                    action = "Login"
                });
                RedirectToRouteResult result = new RedirectToRouteResult(ruta);
                context.Result = result;
            }            
        }
    }
}
