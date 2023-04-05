using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedLibrary.Responses
{
    public class AuthenticationFailedResponse
    {
        public string[] Errors { get; set; }

        public AuthenticationFailedResponse(IEnumerable<string> errors)
        {
            Errors = errors.ToArray();
        }
    }
}
