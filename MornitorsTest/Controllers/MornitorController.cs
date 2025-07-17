using Microsoft.AspNetCore.Mvc;

namespace MornitorsTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MornitorController : Controller
    {
        private readonly MornitorsService mornitorsService;
        public MornitorController(MornitorsService _mornitorsService)
        {
            mornitorsService = _mornitorsService;
        }

        [HttpPost("evacuation-zones")]
        public IActionResult AddEvacuationZones(EvacuationZone req)
        {
            var res = mornitorsService.AddEvacuationZones(req);
            return StatusCode((int)res.StatusCode, res);
        }
        [HttpPost("vehicles")]
        public IActionResult VerifyThaiID(EvacuationZone req)
        {
            var res = mornitorsService.AddEvacuationZones(req);
            return StatusCode((int)res.StatusCode, res);
        }
    }
}