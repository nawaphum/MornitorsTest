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
        public async Task<IActionResult> AddEvacuationZones(EvacuationZone req)
        {
            var res = await mornitorsService.AddEvacuationZones(req);
            return StatusCode((int)res.StatusCode, res);
        }
        [HttpPost("vehicles")]
        public async Task<IActionResult> VerifyThaiID(Vehicles req)
        {
            var res = await mornitorsService.AddVehicles(req);
            return StatusCode((int)res.StatusCode, res);
        }
        [HttpPost("evacuations/plan")]
        public async Task<IActionResult> EvacuationsPlan(EvacuationPlan req)
        {
            var res = await mornitorsService.EvacuationPlan(req);
            return StatusCode((int)res.StatusCode, res);
        }
        [HttpGet("evacuations/status")]
        public async Task<IActionResult> EvacuationsStatus()
        {
            var res = await mornitorsService.EvacuationStatus();
            return StatusCode((int)res.StatusCode, res);
        }
        [HttpPut("evacuations/update")]
        public async Task<IActionResult> EvacuationsUpdate(EvacuationPlan req)
        {
            var res = await mornitorsService.EvacuationUpdate(req);
            return StatusCode((int)res.StatusCode, res);
        }
        [HttpDelete("evacuations/clear")]
        public async Task<IActionResult> EvacuationsClear()
        {
            var res = await mornitorsService.EvacuationClear();
            return StatusCode((int)res.StatusCode, res);
        }
    }
}