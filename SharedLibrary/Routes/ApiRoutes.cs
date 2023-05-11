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
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
        }

        public static class Lobby
        {
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Delete = "{id}";
        }
        
        public static class Session
        {
            public const string GetById = "{id}";
            public const string ResearchColonizePlanet = "researchorcolonize/{sessionId}";
        }
        
        public static class Map
        {
            public const string GetMap = "{id}";
        }
    }
}
