using Microsoft.AspNetCore.Hosting.Server;
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
                    RemainingPeople = req.NumberOfPeople,
                };
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
        public async Task<ResponseData<List<EvacuationPlan>>> EvacuationPlan()
        {
            var res = new ResponseData<List<EvacuationPlan>>();
            try
            {
                var server = iconect.GetServer(iconect.GetEndPoints().First());
                var zoneKeys = server.Keys(pattern: "evacuationzones:*").ToArray();
                var vehicleKeys = server.Keys(pattern: "vehicles:*").ToArray();

                var zones = new List<EvacuationZones>();
                var vehicles = new List<Vehicless>();

                foreach (var key in zoneKeys)
                {
                    var json = await redis.StringGetAsync(key);
                    if (!string.IsNullOrWhiteSpace(json))
                        zones.Add(JsonConvert.DeserializeObject<EvacuationZones>(json)!);
                }

                foreach (var key in vehicleKeys)
                {
                    var json = await redis.StringGetAsync(key);
                    if (!string.IsNullOrWhiteSpace(json))
                        vehicles.Add(JsonConvert.DeserializeObject<Vehicless>(json)!);
                }

                zones = zones.OrderByDescending(z => z.UrgencyLevel).ToList();

                var assignedVehicles = new HashSet<string>();
                var plans = new List<EvacuationPlan>();

                foreach (var zone in zones)
                {
                    var candidates = vehicles
                      .Where(v => !assignedVehicles.Contains(v.VehicleID))
                        .Select(v => new
                        {
                            Vehicle = v,
                            eta = TimeSpan.FromHours(GetHaversineDistance(zone.Latitude ?? 0, zone.Longitude ?? 0, v.Latitude ?? 0, v.Longitude ?? 0) / (v.Speed ?? 0))
                        })
                        .OrderBy(x => x.eta)
                        .ToList();

                    int currentPepel = zone.NumberOfPeople ?? 0;
                    int oder = 0;

                    foreach (var candidate in candidates)
                    {
                        while (currentPepel > 0)
                        {
                            oder = oder + 1;

                            var plan = new EvacuationPlan
                            {
                                Oders = oder,
                                ZoneID = zone.ZoneID,
                                VehicleID = candidate.Vehicle.VehicleID,
                                NumberOfPeople = currentPepel >= candidate.Vehicle.Capacity ? candidate.Vehicle.Capacity : currentPepel,
                                ETA = candidate.eta.ToString(),
                            };


                            plans.Add(plan);
                            assignedVehicles.Add(candidate.Vehicle.VehicleID);
                            currentPepel = currentPepel - candidate.Vehicle.Capacity ?? 0;
                        }

                    }
                }

                res.Data = plans.OrderBy(x => x.Oders).ToList();
            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public async Task<ResponseData<List<EvacuationStatus>>> EvacuationStatus()
        {
            var res = new ResponseData<List<EvacuationStatus>>();
            try
            {

                var server = iconect.GetServer(iconect.GetEndPoints().First());
                var zoneKeys = server.Keys(pattern: "evacuationzones:*").ToArray();

                var zones = new List<EvacuationZones>();

                foreach (var key in zoneKeys)
                {
                    var json = await redis.StringGetAsync(key);
                    if (!string.IsNullOrWhiteSpace(json))
                        zones.Add(JsonConvert.DeserializeObject<EvacuationZones>(json)!);
                }

                var status = new List<EvacuationStatus>();

                foreach (var zone in zones)
                {
                   
                        
                        var planStatus = new EvacuationStatus
                        {
                            ZoneID = zone.ZoneID,
                            TotalEvacuated = zone.NumberOfPeople - zone.RemainingPeople,
                            RemainingPeople = zone.RemainingPeople,
                            VehicleID = zone.VehicleID
                        };

                        status.Add(planStatus);
                }

                res.Data = status;

            }
            catch (Exception ex)
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.Message = ex.InnerException?.Message ?? ex.Message;
                res.IsOK = false;
            }
            return res;
        }
        public async Task<ResponseData<string>> EvacuationUpdate(EvacuationZoneUpdate req)
        {
            var res = new ResponseData<string>();
            try
            {
                var server = iconect.GetServer(iconect.GetEndPoints().First());
                var zoneKeys = server.Keys(pattern: "evacuationzones:*").ToArray();

                var zones = new List<EvacuationZones>();

                foreach (var key in zoneKeys)
                {
                    var jsonkey = await redis.StringGetAsync(key);
                    if (!string.IsNullOrWhiteSpace(jsonkey))
                        zones.Add(JsonConvert.DeserializeObject<EvacuationZones>(jsonkey)!);
                }

                zones = zones.ToList();
                var zonesData = zones.FirstOrDefault(e => e.ZoneID == req.ZoneID);
                var obj = new EvacuationZones
                {
                    ZoneID = req.ZoneID,
                    Latitude = zonesData.Latitude ?? 0,
                    Longitude = zonesData.Longitude ?? 0,
                    NumberOfPeople = zonesData.NumberOfPeople,
                    UrgencyLevel = zonesData.UrgencyLevel,
                    RemainingPeople = (zonesData?.RemainingPeople ?? 0) - req.TotalEvacuated,
                };
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
                
                var zoneKeys = server.Keys(pattern: "evacuationzones:*").ToArray();
                var vehicleKeys = server.Keys(pattern: "vehicles:*").ToArray();

                if (zoneKeys.Length > 0)
                {
                    await redis.KeyDeleteAsync(zoneKeys);
                }
                if (vehicleKeys.Length > 0)
                {
                    await redis.KeyDeleteAsync(vehicleKeys);
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
        private double GetHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        private double ToRadians(double angle) => angle * (Math.PI / 180.0);
    }
}
