using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlotBooking.Data;

public class SlotRepository : ISlotRepository
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SlotRepository(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Schedule> GetScheduleAsync(DateTime date)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://draliatest.azurewebsites.net/api/availability/GetWeeklyAvailability/{date:yyyyMMdd}");

        AddAuthorizationHeader(client);
        
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }
        
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var schedule = JsonConvert.DeserializeObject<Schedule>(jsonResponse);
        PopulateDaySchedules(schedule, jsonResponse);
        

        return schedule;
    }

    private  void PopulateDaySchedules(Schedule schedule, string json)
    {
        var additionalData = JObject.Parse(json);
        foreach (var day in additionalData)
        {
            if (Enum.TryParse<DayOfWeek>(day.Key, true, out var dayOfWeek))
            {
                var daySchedule = day.Value.ToObject<DaySchedule>();
                if (daySchedule?.WorkPeriod != null)
                {
                    schedule.DaySchedules[dayOfWeek] = daySchedule;
                }
            }
        }
    }
    
    private void AddAuthorizationHeader(HttpClient client)
    {
        var credentials = System.Text.Encoding.UTF8.GetBytes("techuser:secretpassWord");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
    }
}