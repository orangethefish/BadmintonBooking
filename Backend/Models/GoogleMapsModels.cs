using System.Text.Json.Serialization;

namespace BadmintonBooking.API.Models
{
    public class PlaceDetailsResponse
    {
        public string Status { get; set; }
        public PlaceResult Result { get; set; }
    }

    public class PlaceResult
    {
        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; }
        
        public string Name { get; set; }
        
        [JsonPropertyName("formatted_phone_number")]
        public string FormattedPhoneNumber { get; set; }
        
        public PlaceGeometry Geometry { get; set; }
        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; }
    }

    public class PlaceGeometry
    {
        public PlaceLocation Location { get; set; }
    }

    public class PlaceLocation
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
} 