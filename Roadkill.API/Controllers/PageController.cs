using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Roadkill.API.Controllers
{
    [Route("page")]
    [ApiController]
    public class PageController : Controller
    {
        #region ****************** DECLARATIONS *********************

        private readonly IConfiguration _configuration;

        #endregion ****************** DECLARATIONS *********************

        #region ****************** CONSTRUCTORS *********************

        /// <summary>
        /// Main Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public PageController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion ****************** CONSTRUCTORS *********************

        #region ******************** METHODS ************************
        /// <summary>
        /// Returns a url for creating a new wiki page, along with prepopulated values in the query string
        /// </summary>
        /// <param name="data">Data coming from submitted form</param>
        /// <returns>Json suitable for loading and prepopulating a new wiki page form</returns>
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        [Route("new")]
        public JsonResult New([FromForm] IFormCollection data)
        {
            JsonResult response;

            if (data["token"] == _configuration["Integration:token"])
            {
                response = Json(new {
                    text = "Creating new wiki post from data: " + data["text"] + " from Channel: " + data["channel_name"],
                    goto_location = _configuration["Integration:baseURL"] + "/pages/new?title=" + data["text"] + "&tags=" + data["channel_name"]
                });
            }
            else
            {
                response = Json(new {
                    text = "Authentication error."
                });
            }

            return response;
        }
    }
        #endregion ******************** METHODS ************************
}