using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services.Abstract;
using SharedLibrary.Models;
using SharedLibrary.Responses;
using SharedLibrary.Routes;


namespace Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }
        [HttpPost, Route(ApiRoutes.Session.Create)]
        public async Task<IActionResult> Create([FromRoute] Guid id) {
            var result = await _sessionService.Create(id);
            if(result.Success)
            return Ok();
            return BadRequest(result);
        }
    }
}
