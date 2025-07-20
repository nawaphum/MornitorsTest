using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MornitorsTest
{
    public class MornitorsService
    {
        private readonly AppDbContext context;
        public MornitorsService(AppDbContext _context)
        {
            context = _context;
        }
        public ResponseData<string> AddEvacuationZones(EvacuationZone req)
        {
            var res = new ResponseData<string>();
            try
            {
                var obj = new EvacuationZones
                {
                    ZoneID = req.ZoneID,
                    Latitude = req.LocationCoordinates.latitude ?? 0,
                    Longitude = req.LocationCoordinates.longitude ?? 0,
                    NumberOfPeople = req.NumberOfPeople,
                    UrgencyLevel = req.UrgencyLevel,
                };
                context.EvacuationZone.Add(obj);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public ResponseData<string> AddVehicles(Vehicles req)
        {
            var res = new ResponseData<string>();
            try
            {
                var obj = new Vehicless
                {
                    VehicleID = req.VehicleID,
                    Capacity = req.Capacity,
                    Type = req.Type,
                    Latitude = req.LocationCoordinates.latitude ?? 0,
                    Longitude = req.LocationCoordinates.longitude ?? 0,
                    Speed = req.Speed
                };
                context.Vehicles.Add(obj);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public ResponseData<string> EvacuationPlan(EvacuationPlan req)
        {
            var res = new ResponseData<string>();
            try
            {
                var obj = new EvacuationPlans
                {
                    GUID = Guid.NewGuid().ToString(),
                    ZoneID = req.ZoneID,
                    VehicleID = req.VehicleID,
                    ETA = req.ETA,
                    NumberOfPeople = req.NumberOfPeople
                };
                context.EvacuationPlan.Add(obj);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public ResponseData<List<EvacuationPlan>> EvacuationStatus()
        {
            var res = new ResponseData<List<EvacuationPlan>>();
            try
            {

                res.Data = context.EvacuationPlan.Select(x => new EvacuationPlan
                {
                    GUID = x.GUID,
                    ZoneID = x.ZoneID,
                    VehicleID = x.VehicleID,
                    ETA = x.ETA,
                    NumberOfPeople = x.NumberOfPeople
                }).ToList();

            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public ResponseData<string> EvacuationUpdate(EvacuationPlan req)
        {
            var res = new ResponseData<string>();
            try
            {

                EvacuationPlans evacuationPlans = context.EvacuationPlan.FirstOrDefault(e => e.GUID == req.GUID);
                if(evacuationPlans == null) throw new ArgumentNullException("EvacuationPlans not found.");

                if (!string.IsNullOrWhiteSpace(req.ZoneID)) evacuationPlans.ZoneID = req.ZoneID;
                if (!string.IsNullOrWhiteSpace(req.ZoneID)) evacuationPlans.VehicleID = req.VehicleID;
                if (!string.IsNullOrWhiteSpace(req.ZoneID)) evacuationPlans.ETA = req.ETA;
                if (!string.IsNullOrWhiteSpace(req.ZoneID)) evacuationPlans.NumberOfPeople = req.NumberOfPeople;

                context.SaveChanges();

            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public ResponseData<string> EvacuationClear()
        {
            var res = new ResponseData<string>();
            try
            {
                var allPlans = context.EvacuationPlan.ToList();
                context.EvacuationPlan.RemoveRange(allPlans);
                context.SaveChanges();
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
