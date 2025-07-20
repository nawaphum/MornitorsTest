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
        public IActionResult VerifyThaiID(Vehicles req)
        {
            var res = mornitorsService.AddVehicles(req);
            return StatusCode((int)res.StatusCode, res);
        }
        [HttpPost("evacuations/plan")]
        public IActionResult EvacuationsPlan(EvacuationPlan req)
        {
            var res = mornitorsService.EvacuationPlan(req);
            return StatusCode((int)res.StatusCode, res);
        }
        [HttpGet("evacuations/status")]
        public IActionResult EvacuationsStatus()
        {
            var res = mornitorsService.EvacuationStatus();
            return StatusCode((int)res.StatusCode, res);
        }
        [HttpPut("evacuations/update")]
        public IActionResult EvacuationsUpdate(EvacuationPlan req)
        {
            var res = mornitorsService.EvacuationUpdate(req);
            return StatusCode((int)res.StatusCode, res);
        }
        [HttpDelete("evacuations/clear")]
        public IActionResult EvacuationsClear()
        {
            var res = mornitorsService.EvacuationClear();
            return StatusCode((int)res.StatusCode, res);
        }
    }
}