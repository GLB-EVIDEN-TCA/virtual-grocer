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
            var authority = _config["AzureAd:Authority"];
            
            if (string.IsNullOrEmpty(authority))
            {
                throw new ApplicationException("Unable to load AzureAD authority endpoint from configuration.");
            }

            var clientId = _config["AzureAd:ClientId"];

            if (string.IsNullOrEmpty(clientId))
            {
                throw new ApplicationException("Unable to load AzureAD Client Id from configuration.");
            }

            return new ClientSettings { AzureAdAuthority = authority, AzureAdClientId = clientId };
        }
    }
}