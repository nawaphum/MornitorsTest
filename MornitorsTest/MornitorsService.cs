using Microsoft.AspNetCore.Http;
using System.Net;

namespace MornitorsTest
{
    public class MornitorsService
    {
        public ResponseData<string> AddEvacuationZones(EvacuationZone req)
        {
            var res = new ResponseData<string>();
            try
            {
                
                res.Data = "test";

            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
    }
}
