using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlotBooking.Data.Entities;

namespace SlotBooking.Data.UnitTests;

[TestFixture]
public class SlotRepositoryTests
{
    private ISlotRepository _slotRepository;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private IOptions<SlotServiceOptions> _slotServiceOptions;

    [SetUp]
    public void SetUp()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _slotServiceOptions = Options.Create(new SlotServiceOptions
        {
            BaseUrl = "https://draliatest.azurewebsites.net/api/availability",
            Username = "techuser",
            Password = "secretpassWord"
        });

        _slotRepository = new SlotRepository(httpClient, _slotServiceOptions);
    }

    [Test]
    public async Task GetScheduleAsync_ReturnsSchedule_WhenCorrectInput()
    {
        // Arrange
        var date = new DateTime(2024, 8, 2);
        var jsonResponseMock = JToken.Parse(
            await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "appsettings.test.json"))
        ).ToString();
        
        var content = new StringContent(jsonResponseMock, Encoding.UTF8, "application/json");
        
        var request = CreateHttpRequestMessage(content, $"GetWeeklyAvailability/{date:yyyyMMdd}", HttpMethod.Get);
        await SetupProtectedSendAsyncMock(request, HttpStatusCode.OK);

        // Act
        var scheduleResponse = await _slotRepository.GetBusySlotsAsync(date);

        // Assert
        Assert.IsNotNull(scheduleResponse);
        Assert.Multiple(() =>
        {
            Assert.That(scheduleResponse.Facility.Name, Is.EqualTo("Facility 1"));
            Assert.That(scheduleResponse.SlotDurationMinutes, Is.EqualTo(30));
            Assert.That(scheduleResponse.DaySchedules.Count, Is.EqualTo(3));
            Assert.That(scheduleResponse.DaySchedules[DayOfWeek.Friday].BusySlots.Count, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task GetScheduleAsync_ThrowsHttpRequestException_WhenResponseIsNotSuccessful()
    {
        // Arrange
        var date = new DateTime(2024, 8, 2);
        var request = CreateHttpRequestMessage(null, $"GetWeeklyAvailability/{date:yyyyMMdd}", HttpMethod.Get);
        await SetupProtectedSendAsyncMock(request, HttpStatusCode.BadGateway);

        // Act&Assert
        Assert.ThrowsAsync<HttpRequestException>(() => _slotRepository.GetBusySlotsAsync(date));
    }

    [Test]
    public async Task PostSlotAsync_ThrowsHttpRequestException_WhenResponseIsNotSuccessful()
    {
        // Arrange
        var request = CreateHttpRequestMessage(null, "TakeSlot", HttpMethod.Post);
        await SetupProtectedSendAsyncMock(request, HttpStatusCode.BadGateway);

        // Act&Assert
        Assert.ThrowsAsync<HttpRequestException>(() => _slotRepository.PostSlotAsync(new AvailableSlot()));
    }
    
    [Test]
    public async Task PostSlotAsync_ReturnsOk_WhenCorrectInput()
    {
        // Arrange
        var availableSlot = CreateAvailableSlot();
        var content = new StringContent(JsonConvert.SerializeObject(availableSlot), Encoding.UTF8, "application/json");
        
        var request = CreateHttpRequestMessage(content, "TakeSlot", HttpMethod.Post);
        await SetupProtectedSendAsyncMock(request, HttpStatusCode.OK);

        // Act
        var response = await _slotRepository.PostSlotAsync(availableSlot);

        // Assert
        Assert.That(response, Is.EqualTo(HttpStatusCode.OK));
    }

    private HttpRequestMessage CreateHttpRequestMessage(HttpContent content, string path, HttpMethod method)
    {
        var request = new HttpRequestMessage(method,
            $"{_slotServiceOptions.Value.BaseUrl}/{path}")
        {
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            $"{_slotServiceOptions.Value.Username}:{_slotServiceOptions.Value.Password}")))
            },
            Content = content
        };
        return request;
    }

    private async Task<IReturnsResult<HttpMessageHandler>> SetupProtectedSendAsyncMock(HttpRequestMessage request, HttpStatusCode responseStatusCode)
    {
        return _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == request.Method &&
                    req.RequestUri == request.RequestUri &&
                    req.Headers.Authorization.Scheme == "Basic" &&
                    req.Headers.Authorization.Parameter == Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            $"{_slotServiceOptions.Value.Username}:{_slotServiceOptions.Value.Password}"))),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = responseStatusCode,
                Content = request.Content
            });
    }

    private AvailableSlot CreateAvailableSlot()
    {
        var slot = new AvailableSlot()
        {
            FacilityId = new Guid().ToString(),
            Start = new DateTime(2023, 7, 10, 8, 0, 0),
            End = new DateTime(2023, 7, 10, 8, 30, 0),
            Comments = "Comments",
            Patient = new Patient()
            {
                Email = "email",
                Name = "Name",
                Phone = "Phone",
                SecondName = "SecondName"
            }
        };
        return slot;
    }
}