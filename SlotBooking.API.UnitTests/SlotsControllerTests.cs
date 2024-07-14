using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SlotBooking.API;
using SlotBooking.Domain;
using SlotBooking.Domain.DTOs;

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
        var date = new DateTime(2024, 07, 01); 

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
        _slotServiceMock.Setup(service => service.CreateSlotAsync(slotDto))
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
        var date = new DateTime(2024, 07, 23); 

        // Act
        var result = await _controller.GetAvailableSlots(date);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
    }

    [Test]
    public async Task PostSlot_ReturnsBadRequest_WhenNullInput()
    {
        // Arrange

        // Act
        var result = await _controller.PostSlot(null);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }
}