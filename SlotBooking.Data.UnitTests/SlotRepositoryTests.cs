using System.Net;
using NUnit.Framework;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Moq;
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
            BaseUrl = "https://test.net/api/availability",
            Username = "testuser1234",
            Password = "pass1234"
        });

        _slotRepository = new SlotRepository(httpClient, _slotServiceOptions);
    }

    [Test]
    public async Task GetScheduleAsync_ReturnsSchedule_WhenCorrectInput()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 8, 2, 0, 0, 0, DateTimeOffset.Now.Offset);
        var jsonResponseMock = JToken.Parse(
            await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "appsettings.test.json"))
        ).ToString();
        
        var content = new StringContent(jsonResponseMock, Encoding.UTF8, "application/json");
        
        var request = CreateHttpRequestMessage(content, $"GetWeeklyAvailability/{date:yyyyMMdd}", HttpMethod.Get);
        SetupProtectedSendAsyncMock(request, HttpStatusCode.OK);

        // Act
        var scheduleResponse = await _slotRepository.GetBusySlotsScheduleAsync(date);

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
    public void GetScheduleAsync_ThrowsHttpRequestException_WhenResponseIsNotSuccessful()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 8, 2, 0, 0, 0, DateTimeOffset.Now.Offset);
        var request = CreateHttpRequestMessage(null, $"GetWeeklyAvailability/{date:yyyyMMdd}", HttpMethod.Get);
        SetupProtectedSendAsyncMock(request, HttpStatusCode.BadGateway);

        // Act&Assert
        Assert.ThrowsAsync<HttpRequestException>(() => _slotRepository.GetBusySlotsScheduleAsync(date));
    }

    [Test]
    public void PostSlotAsync_ThrowsHttpRequestException_WhenResponseIsNotSuccessful()
    {
        // Arrange
        var request = CreateHttpRequestMessage(null, "TakeSlot", HttpMethod.Post);
        SetupProtectedSendAsyncMock(request, HttpStatusCode.BadGateway);

        // Act&Assert
        Assert.ThrowsAsync<HttpRequestException>(() => _slotRepository.BookSlotAsync(new AvailableSlot()));
    }
    
    [Test]
    public async Task PostSlotAsync_ReturnsOk_WhenCorrectInput()
    {
        // Arrange
        var availableSlot = CreateAvailableSlot();
        var content = new StringContent(JsonConvert.SerializeObject(availableSlot), Encoding.UTF8, "application/json");
        
        var request = CreateHttpRequestMessage(content, "TakeSlot", HttpMethod.Post);
        SetupProtectedSendAsyncMock(request, HttpStatusCode.OK);

        // Act
        var response = await _slotRepository.BookSlotAsync(availableSlot);

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

    private void SetupProtectedSendAsyncMock(HttpRequestMessage request, HttpStatusCode responseStatusCode)
    { 
        _mockHttpMessageHandler
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
            Start = new DateTimeOffset(2023, 7, 10, 8, 0, 0, DateTimeOffset.Now.Offset),
            End = new DateTimeOffset(2023, 7, 10, 8, 30, 0, DateTimeOffset.Now.Offset),
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