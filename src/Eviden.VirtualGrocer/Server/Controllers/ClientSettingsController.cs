using Eviden.VirtualGrocer.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Eviden.VirtualGrocer.Web.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientSettingsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ClientSettingsController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Returns the client settings for the current environment.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ClientSettings Get()
        {
            return new ClientSettings { AzureAdAuthority = _config["AzureAd:Authority"], AzureAdClientId = _config["AzureAd:ClientId"] };
        }
    }
}