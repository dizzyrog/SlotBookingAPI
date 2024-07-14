using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlotBooking.Data.Entities;

namespace SlotBooking.Data;

/// <inheritdoc />
public class SlotRepository(HttpClient httpClient, IOptions<SlotServiceOptions> slotServiceOptions)
    : ISlotRepository
{
    private readonly SlotServiceOptions _slotServiceOptions = slotServiceOptions.Value;

    /// <inheritdoc />
    public async Task<BusySlotsSchedule> GetBusySlotsScheduleAsync(DateTimeOffset date)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, 
            $"{_slotServiceOptions.BaseUrl}/GetWeeklyAvailability/{date:yyyyMMdd}");

        AddAuthorizationHeader(httpClient);
        
        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode} and message {response.Content}");
        }
        
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var schedule = JsonConvert.DeserializeObject<BusySlotsSchedule>(jsonResponse);
        PopulateDaySchedules(schedule, jsonResponse);
        
        return schedule;
    }
    
    /// <inheritdoc />
    public async Task<HttpStatusCode> BookSlotAsync(AvailableSlot availableSlot)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, 
            $"{_slotServiceOptions.BaseUrl}/TakeSlot");

        request.Content = new StringContent(JsonConvert.SerializeObject(availableSlot), Encoding.UTF8, "application/json");
        AddAuthorizationHeader(httpClient);
        
        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode} and message {response.Content}");
        }
        
        return response.StatusCode;
    }

    private  void PopulateDaySchedules(BusySlotsSchedule busySlotsSchedule, string json)
    {
        var additionalData = JObject.Parse(json);
        foreach (var day in additionalData)
        {
            if (Enum.TryParse<DayOfWeek>(day.Key, true, out var dayOfWeek))
            {
                var daySchedule = day.Value?.ToObject<DaySchedule>();
                if (daySchedule?.WorkPeriod != null)
                {
                    busySlotsSchedule.DaySchedules[dayOfWeek] = daySchedule;
                }
            }
        }
    }
    
    private void AddAuthorizationHeader(HttpClient client)
    {
        var credentials = Encoding.UTF8.GetBytes(
            $"{_slotServiceOptions.Username}:{_slotServiceOptions.Password}");
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(credentials));
    }
}

