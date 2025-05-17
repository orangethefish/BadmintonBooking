using BadmintonBooking.API.Models;
using BadmintonBooking.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace BadmintonBooking.API.Services
{
    public class FacilityService : IFacilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public FacilityService(
            ApplicationDbContext context, 
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Facility> CreateFacilityAsync(CreateFacilityRequest request, Guid userId)
        {
            try
            {
                var facility = new Facility
                {
                    Name = request.Name,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    Description = request.Description,
                    OwnerId = userId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    MapsUrl = request.MapsUrl,
                    CourtLatitude = request.CourtLatitude,
                    CourtLongitude = request.CourtLongitude,
                    PlaceId = request.PlaceId
                };

                _context.Facilities.Add(facility);
                await _context.SaveChangesAsync();
                return facility;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error creating facility: {ex.Message}", ex);
            }
        }

        public async Task<Facility> GetFacilityAsync(int id)
        {
            try
            {
                var facility = await _context.Facilities
                    .Include(f => f.Courts)
                    .FirstOrDefaultAsync(f => f.Id == id);
                
                return facility;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error retrieving facility with id {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Facility>> GetUserFacilitiesAsync(Guid userId)
        {
            try
            {
                return await _context.Facilities
                    .Where(f => f.OwnerId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error retrieving facilities for user {userId}: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsFacilityOwnerAsync(int facilityId, Guid userId)
        {
            try
            {
                var facility = await _context.Facilities
                    .FirstOrDefaultAsync(f => f.Id == facilityId && f.OwnerId == userId);
                
                return facility != null;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error checking facility ownership: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateFacilityAsync(int id, UpdateFacilityRequest request)
        {
            try
            {
                var facility = await _context.Facilities.FindAsync(id);
                
                if (facility == null)
                    return false;
                
                facility.Name = request.Name ?? facility.Name;
                facility.Address = request.Address ?? facility.Address;
                facility.PhoneNumber = request.PhoneNumber ?? facility.PhoneNumber;
                facility.Description = request.Description ?? facility.Description;
                // facility.IsActive = request.IsActive ?? facility.IsActive;
                facility.UpdatedAt = DateTime.UtcNow;
                
                _context.Facilities.Update(facility);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error updating facility {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFacilityAsync(int id)
        {
            try
            {
                var facility = await _context.Facilities.FindAsync(id);
                
                if (facility == null)
                    return false;
                
                // Check if courts exist
                var hasCourts = await _context.Courts.AnyAsync(c => c.FacilityId == id);
                
                if (hasCourts)
                {
                    throw new InvalidOperationException("Cannot delete facility with existing courts");
                }
                
                _context.Facilities.Remove(facility);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error deleting facility {id}: {ex.Message}", ex);
            }
        }

        public async Task<ResolveUrlResponse> ResolveUrlAsync(ResolveUrlRequest request)
        {
            if (string.IsNullOrEmpty(request.Url))
            {
                throw new ArgumentException("URL is required");
            }

            var response = new ResolveUrlResponse();
            string finalUrl = await ResolveRedirectedUrlAsync(request.Url);
            response.FinalUrl = finalUrl;

            // Extract location information from URL
            var placeInfo = await ExtractPlaceInfoFromUrlAsync(finalUrl);
            if (placeInfo != null)
            {
                response.FormattedAddress = placeInfo.FormattedAddress;
                response.Name = placeInfo.Name;
                response.Latitude = placeInfo.Latitude;
                response.Longitude = placeInfo.Longitude;
                response.PhoneNumber = placeInfo.PhoneNumber;
                response.PlaceId = placeInfo.PlaceId;
            }

            return response;
        }

        private async Task<string> ResolveRedirectedUrlAsync(string url)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "BadmintonBookingApp");
            
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            
            using var redirectClient = new HttpClient(handler);

            string currentUrl = url;
            int maxRedirects = 10;
            int redirectCount = 0;

            while (redirectCount < maxRedirects)
            {
                HttpResponseMessage response = await redirectClient.GetAsync(currentUrl);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Moved ||
                    response.StatusCode == System.Net.HttpStatusCode.MovedPermanently ||
                    response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                    response.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect ||
                    (int)response.StatusCode == 308) // Permanent Redirect
                {
                    if (response.Headers.Location == null)
                    {
                        break;
                    }

                    string locationUrl = response.Headers.Location.ToString();
                    
                    // If the location URL is relative, combine it with the base URL
                    if (!response.Headers.Location.IsAbsoluteUri)
                    {
                        Uri baseUri = new Uri(currentUrl);
                        locationUrl = new Uri(baseUri, locationUrl).ToString();
                    }

                    currentUrl = locationUrl;
                    redirectCount++;
                }
                else
                {
                    break;
                }
            }

            return currentUrl;
        }

        private async Task<PlaceInfo> ExtractPlaceInfoFromUrlAsync(string url)
        {
            try
            {
                // Extract CID from Google Maps URL - two common patterns
                // Pattern 1: !1s0x....:0x.... 
                var cidPattern1 = new Regex(@"!1s(0x[a-fA-F0-9]+:[a-fA-F0-9x]+)");
                // Pattern 2: cid=... in the URL
                var cidPattern2 = new Regex(@"cid=([0-9]+)");

                string cid = null;
                var match1 = cidPattern1.Match(url);
                if (match1.Success)
                {
                    var fullCid = match1.Groups[1].Value;
                    // Take the part after colon
                    cid = fullCid.Split(':')[1];
                }
                else
                {
                    var match2 = cidPattern2.Match(url);
                    if (match2.Success)
                    {
                        cid = match2.Groups[1].Value;
                    }
                }

                if (string.IsNullOrEmpty(cid))
                {
                    return null;
                }

                // Get the API key from configuration
                string apiKey = _configuration["GoogleMaps:ApiKey"];
                
                // Call Google Places API
                using var httpClient = _httpClientFactory.CreateClient();
                var placeDetailsUrl = $"https://maps.googleapis.com/maps/api/place/details/json?cid={cid}&key={apiKey}";
                var response = await httpClient.GetAsync(placeDetailsUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var placeDetails = JsonSerializer.Deserialize<PlaceDetailsResponse>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (placeDetails?.Status != "OK" || placeDetails.Result == null)
                {
                    return null;
                }

                return new PlaceInfo
                {
                    FormattedAddress = placeDetails.Result.FormattedAddress,
                    Name = placeDetails.Result.Name,
                    Latitude = placeDetails.Result.Geometry?.Location?.Lat,
                    Longitude = placeDetails.Result.Geometry?.Location?.Lng,
                    PhoneNumber = placeDetails.Result.FormattedPhoneNumber,
                    PlaceId = placeDetails.Result.PlaceId
                };
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error extracting place info: {ex.Message}");
                return null;
            }
        }
    }
}
