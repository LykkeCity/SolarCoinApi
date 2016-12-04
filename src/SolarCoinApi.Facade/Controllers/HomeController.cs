﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SolarCoinApi.Facade.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Version()
        {
            return Json(new { Version = typeof(HomeController).GetTypeInfo().Assembly.GetName().Version.ToString() });
        }
    }
}
