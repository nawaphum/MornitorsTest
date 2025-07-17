using System.Net;

namespace MornitorsTest
{
    public class ResponseData<T> : TableData
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string? Message { get; set; }
        public T? Data { get; set; }

        public int? TotalCount { get; set; }
        public bool IsOK { get; set; } = true;

    }
    public class TableData
    {
        public int Page { get; set; }
        public int Total { get; set; }
        public int PageCount { get; set; }
    }
    public class EvacuationZone
    {
        public string? ZoneID { get; set; }
        public LocationCoordinates? LocationCoordinates { get; set; }
        public int? NumberOfPeople { get; set; }
        public int? UrgencyLevel { get; set; }

    }
    public class LocationCoordinates
    {
        public double? latitude { get; set; }
        public double? longitude { get; set; }
    }
    public class Vehicles
    {
        public string? VehicleID { get; set; }
        public int? Capacity { get; set; }
        public string? Type { get; set; }
        public LocationCoordinates? LocationCoordinates { get; set; }
        public int? Speed { get; set; }
    }
}
