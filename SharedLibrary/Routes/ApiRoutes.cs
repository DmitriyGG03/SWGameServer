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
        
        public static class Hero
        {
            public const string GetById = "{id:int}";
            public const string Create = "";
            public const string Update = "{id:int}";
        }
        
        public static class Map
        {
            public const string GetMap = "{id:int}";
        }
    }
}
