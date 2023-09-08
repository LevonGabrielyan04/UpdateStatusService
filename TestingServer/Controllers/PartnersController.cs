using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UpdateStatusService.Models;

namespace TestingServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PartnersController : ControllerBase
    {
        [HttpGet]
        [Route("GetStatus")]
        public ActionResult<GetStatusModel> GetStatus(int agentId)
        {
            switch (agentId)
            {
                case 4:
                case 5:
                    return new GetStatusModel(70077,1);
                default:
                    return new GetStatusModel(40044,2);
            }
        }
    }
}
