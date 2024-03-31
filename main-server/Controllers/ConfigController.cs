using BLL.Common.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/config")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        [HttpGet("get-configs")]
        public ActionResult GetConfigs()
        {
            return Ok(new
            {
                StaticUrl = FileConstants.StaticFilesFolder,
                UsersFilesUrl = @$"{FileConstants.StaticFilesFolder}/{FileConstants.UsersFiles}"
            });
        }
    }
}
