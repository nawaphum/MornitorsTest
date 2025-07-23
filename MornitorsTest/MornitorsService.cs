using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Net;
using IDatabase = StackExchange.Redis.IDatabase;

namespace MornitorsTest
{
    public class MornitorsService
    {
        private readonly AppDbContext context;
        private readonly IDatabase redis;
        private readonly IConnectionMultiplexer iconect;
        public MornitorsService(AppDbContext _context, IConnectionMultiplexer redisConnection)
        {
            context = _context;
            redis = redisConnection.GetDatabase();
            iconect = redisConnection;
        }
        public async Task<ResponseData<string>> AddEvacuationZones(EvacuationZone req)
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
                //context.EvacuationZone.Add(obj);
                //context.SaveChanges();
                string json = JsonConvert.SerializeObject(obj);
                await redis.StringSetAsync($"evacuationzones:{req.ZoneID}", json);
            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public async Task<ResponseData<string>> AddVehicles(Vehicles req)
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
                //context.Vehicles.Add(obj);
                //context.SaveChanges();
                string json = JsonConvert.SerializeObject(obj);
                await redis.StringSetAsync($"vehicles:{req.VehicleID}", json);
            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public async Task<ResponseData<string>> EvacuationPlan(EvacuationPlan req)
        {
            var res = new ResponseData<string>();
            try
            {
                var GuId = Guid.NewGuid().ToString();
                var obj = new EvacuationPlans
                {
                    GUID = GuId,
                    ZoneID = req.ZoneID,
                    VehicleID = req.VehicleID,
                    ETA = req.ETA,
                    NumberOfPeople = req.NumberOfPeople
                };
                //context.EvacuationPlan.Add(obj);
                //context.SaveChanges();
                string json = JsonConvert.SerializeObject(obj);
                await redis.StringSetAsync($"evacuationplans:{GuId}", json);
            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public async Task<ResponseData<List<EvacuationPlan>>> EvacuationStatus()
        {
            var res = new ResponseData<List<EvacuationPlan>>();
            try
            {

                //res.Data = context.EvacuationPlan.Select(x => new EvacuationPlan
                //{
                //    GUID = x.GUID,
                //    ZoneID = x.ZoneID,
                //    VehicleID = x.VehicleID,
                //    ETA = x.ETA,
                //    NumberOfPeople = x.NumberOfPeople
                //}).ToList();
                var endpoints = iconect.GetEndPoints();
                var server = iconect.GetServer(endpoints.First());

                var keys = server.Keys(pattern: "evacuationplans:*").ToArray();

                var list = new List<EvacuationPlan>();
                foreach (var key in keys)
                {
                    var json = await redis.StringGetAsync(key);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var plan = JsonConvert.DeserializeObject<EvacuationPlan>(json);
                        if (plan != null)
                        {
                            list.Add(plan);
                        }
                    }
                }

                res.Data = list;

            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public async Task<ResponseData<string>> EvacuationUpdate(EvacuationPlan req)
        {
            var res = new ResponseData<string>();
            try
            {

                //EvacuationPlans evacuationPlans = context.EvacuationPlan.FirstOrDefault(e => e.GUID == req.GUID);
                var redisData = await redis.StringGetAsync($"evacuationplans:{req.GUID}");
                if (redisData.IsNullOrEmpty) throw new Exception("EvacuationPlans not found.");

                var evacuationPlans = JsonConvert.DeserializeObject<EvacuationPlans>(redisData!);

                if (!string.IsNullOrWhiteSpace(req.ZoneID)) evacuationPlans.ZoneID = req.ZoneID;
                if (!string.IsNullOrWhiteSpace(req.VehicleID)) evacuationPlans.VehicleID = req.VehicleID;
                if (req.ETA != null) evacuationPlans.ETA = req.ETA;
                if (req.NumberOfPeople != null) evacuationPlans.NumberOfPeople = req.NumberOfPeople;

                string json = JsonConvert.SerializeObject(evacuationPlans);
                await redis.StringSetAsync($"evacuationplans:{req.GUID}", json);

                //context.SaveChanges();

            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public async Task<ResponseData<string>> EvacuationClear()
        {
            var res = new ResponseData<string>();
            try
            {
                //var allPlans = context.EvacuationPlan.ToList();
                //context.EvacuationPlan.RemoveRange(allPlans);
                //context.SaveChanges();ฃ
                var endpoints = iconect.GetEndPoints();
                var server = iconect.GetServer(endpoints.First());
                var keys = server.Keys(pattern: "evacuationplans:*").ToArray();

                if (keys.Length > 0)
                {
                    await redis.KeyDeleteAsync(keys);
                }
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
