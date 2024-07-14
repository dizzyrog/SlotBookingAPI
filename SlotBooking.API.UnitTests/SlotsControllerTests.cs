using System.Net;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SlotBooking.API;
using SlotBooking.Domain;
using SlotBooking.Domain.DTOs;
using DateTimeOffset = System.DateTimeOffset;

namespace SlotBookingAPI.UnitTests;

[TestFixture]
public class SlotsControllerTests
{
    private Mock<ISlotService> _slotServiceMock;
    private SlotsController _controller;

    [SetUp]
    public void SetUp()
    {
        _slotServiceMock = new Mock<ISlotService>();
        _controller = new SlotsController(_slotServiceMock.Object);
    }

    [Test]
    public async Task GetAvailableSlots_ReturnsOkResult_WhenCorrectInput()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 07, 01, 0, 0, 0, DateTimeOffset.Now.Offset); 

        // Act
        var result = await _controller.GetAvailableSlots(date);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result.Result);
    }
    
    [Test]
    public async Task PostSlot_ReturnsOkResult_WhenCorrectInput()
    {
        // Arrange
        var slotDto = new BookAvailableSlotDto(); 
        _slotServiceMock.Setup(service => service.BookSlotAsync(slotDto))
            .ReturnsAsync(HttpStatusCode.OK);

        // Act
        var result = await _controller.PostSlot(slotDto);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task GetAvailableSlots_ReturnsBadRequest_WhenDateIsNotMonday()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 07, 23, 0, 0, 0, DateTimeOffset.Now.Offset); 

        // Act
        var result = await _controller.GetAvailableSlots(date);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
    }
}