/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TzIdentityManager.Api.Filters;
using TzIdentityManager.Assets;
using TzIdentityManager.Configuration;

namespace TzIdentityManager.Api.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [SecurityHeaders]
    public class PageController : Controller
    {
        IdentityManagerOptions _idmConfig;
        public PageController(IOptions<IdentityManagerOptions> idmConfig)
        {
            if (idmConfig == null) throw new ArgumentNullException("idmConfig");

            this._idmConfig = idmConfig.Value;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return new EmbeddedHtmlResult(Request, "TzIdentityManager.Assets.Templates.index.html");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            //idmConfig.SecurityConfiguration.SignOut(Request.GetOwinContext());
            return RedirectToRoute(Constants.RouteNames.Home, null);
        }
    }
}
