using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary.Routes
{
    public static class ApiRoutes 
    {
        public static class Authentication
        {
            public const string Register = "register";
            public const string Login = "login";
        }

        public static class Map
        {
            public const string GetMap = "{id:int}";
        }
    }
}
