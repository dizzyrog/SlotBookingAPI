using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Moq;
using SlotBooking.Data;
using SlotBooking.Data.Entities;
using SlotBooking.Domain.DTOs;
using SlotBooking.Domain.Exceptions;
using SlotBooking.Domain.Validators;

namespace SlotBooking.Domain.UnitTests;

[TestFixture]
public class SlotServiceTests
{
    private Mock<ISlotRepository> _mockRepository;
    private SlotService _slotService;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<ISlotRepository>();
        _slotService = new SlotService(_mockRepository.Object, new GetAvailableSlotsForADateValidator(), 
            new Mock<ILogger<SlotService>>().Object );
    }

    [Test]
    public async Task GetAvailableSlotsAsync_ReturnsAvailableSlotsSchedule_WhenCorrectInput()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 7, 29, 0, 0, 0,DateTimeOffset.Now.Offset);
        var busySlotsSchedule = CreateSimpleTestBusySlotsSchedule(date);

        _mockRepository.Setup(r => r.GetBusySlotsScheduleAsync(date)).ReturnsAsync(busySlotsSchedule);

        // Act
        var availableSlotsAsync = await _slotService.GetAvailableSlotsAsync(date);
        
        // Assert
        Assert.That(availableSlotsAsync.FacilityDto.FacilityId, Is.EqualTo(busySlotsSchedule.Facility.FacilityId));
        Assert.That(availableSlotsAsync.FacilityDto.Name, Is.EqualTo(busySlotsSchedule.Facility.Name));
        Assert.That(availableSlotsAsync.FacilityDto.Address, Is.EqualTo(busySlotsSchedule.Facility.Address));
        Assert.That(availableSlotsAsync.WeekStartDate, Is.EqualTo(date));
        Assert.That(availableSlotsAsync.DaySchedules.Count, Is.EqualTo(2));

        var mondaySchedule = availableSlotsAsync.DaySchedules.First();
        
        Assert.That(mondaySchedule.DayOfWeek, Is.EqualTo(DayOfWeek.Monday.ToString()));
        Assert.That(mondaySchedule.AvailableSlots.Count, Is.EqualTo(4));
        Assert.That(mondaySchedule.AvailableSlots[0].Start, Is.EqualTo(date.AddHours(9).AddMinutes(20)));
        
        var fridaySchedule = availableSlotsAsync.DaySchedules.Find(x => x.DayOfWeek == DayOfWeek.Friday.ToString());
        
        Assert.That(fridaySchedule.DayOfWeek, Is.EqualTo(DayOfWeek.Friday.ToString()));
        Assert.That(fridaySchedule.AvailableSlots.Count, Is.EqualTo(4));
        Assert.That(fridaySchedule.AvailableSlots[0].Start, Is.EqualTo(date.AddDays(4).AddHours(12)));
    }
    
    [Test]
    public async Task GetAvailableSlotsAsync_ReturnsAvailableSlotsSchedule_WhenAllSlotsBusy()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 7, 29, 0, 0, 0, DateTimeOffset.Now.Offset);
        var busySlotsSchedule = CreateSimpleTestAllSlotsAreBusySchedule(date);

        _mockRepository.Setup(r => r.GetBusySlotsScheduleAsync(date)).ReturnsAsync(busySlotsSchedule);

        // Act
        var availableSlotsAsync = await _slotService.GetAvailableSlotsAsync(date);
        
        // Assert
        Assert.That(availableSlotsAsync.FacilityDto.FacilityId, Is.EqualTo(busySlotsSchedule.Facility.FacilityId));
        Assert.That(availableSlotsAsync.FacilityDto.Name, Is.EqualTo(busySlotsSchedule.Facility.Name));
        Assert.That(availableSlotsAsync.FacilityDto.Address, Is.EqualTo(busySlotsSchedule.Facility.Address));
        Assert.That(availableSlotsAsync.WeekStartDate, Is.EqualTo(date));
        Assert.That(availableSlotsAsync.DaySchedules.Count, Is.EqualTo(1));

        var tuesdaySchedule = availableSlotsAsync.DaySchedules.First();
        
        Assert.That(tuesdaySchedule.DayOfWeek, Is.EqualTo(DayOfWeek.Tuesday.ToString()));
        Assert.That(tuesdaySchedule.AvailableSlots.Count, Is.EqualTo(0));
    }
    
    [Test]
    public async Task GetAvailableSlotsAsync_ReturnsAvailableSlotsSchedule_WhenNoBusySlots()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 7, 29, 0, 0, 0,DateTimeOffset.Now.Offset);
        var busySlotsSchedule = CreateSimpleTestNoBusySlotsSchedule();

        _mockRepository.Setup(r => r.GetBusySlotsScheduleAsync(date)).ReturnsAsync(busySlotsSchedule);

        // Act
        var availableSlotsAsync = await _slotService.GetAvailableSlotsAsync(date);
        
        // Assert
        Assert.That(availableSlotsAsync.FacilityDto.FacilityId, Is.EqualTo(busySlotsSchedule.Facility.FacilityId));
        Assert.That(availableSlotsAsync.FacilityDto.Name, Is.EqualTo(busySlotsSchedule.Facility.Name));
        Assert.That(availableSlotsAsync.FacilityDto.Address, Is.EqualTo(busySlotsSchedule.Facility.Address));
        Assert.That(availableSlotsAsync.WeekStartDate, Is.EqualTo(date));
        Assert.That(availableSlotsAsync.DaySchedules.Count, Is.EqualTo(1));

        var thursdaySchedule = availableSlotsAsync.DaySchedules.First();
        
        Assert.That(thursdaySchedule.DayOfWeek, Is.EqualTo(DayOfWeek.Thursday.ToString()));
        Assert.That(thursdaySchedule.AvailableSlots.Count, Is.EqualTo(6));
    }
    
    [Test]
    public async Task GetAvailableSlotsAsync_ThrowsArgumentNullException_WhenBusySlotsScheduleNull()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 7, 29, 0, 0, 0,DateTimeOffset.Now.Offset);

        _mockRepository.Setup(r => r.GetBusySlotsScheduleAsync(date)).ReturnsAsync((BusySlotsSchedule) null);
        
        // Act&Assert
        var ex = Assert.ThrowsAsync<ArgumentNullException>(() =>  _slotService.GetAvailableSlotsAsync(date));
        Assert.That(ex.ParamName, Is.EqualTo("busySlotsSchedule"));
    }
    
    [Test]
    public async Task GetAvailableSlotsAsync_ThrowsArgumentNullException_WhenFacilityNull()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 7, 29, 0, 0, 0,DateTimeOffset.Now.Offset);
        var busySlotsSchedule = new BusySlotsSchedule();

        _mockRepository.Setup(r => r.GetBusySlotsScheduleAsync(date)).ReturnsAsync(busySlotsSchedule);
        
        // Act&Assert
        var ex= Assert.ThrowsAsync<ArgumentNullException>(() =>  _slotService.GetAvailableSlotsAsync(date));
        Assert.That(ex.ParamName, Is.EqualTo("Facility"));
    }
    
    [Test]
    public async Task GetAvailableSlotsAsync_ThrowsArgumentNullException_WhenWorkPeriodNull()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 7, 29, 0, 0, 0,DateTimeOffset.Now.Offset);
        var busySlotsSchedule = new BusySlotsSchedule
        {
            Facility = new Facility
            {
                FacilityId = new Guid().ToString(),
                Name = "Facility 123",
                Address = "345 Test St"
            },
            DaySchedules = new Dictionary<DayOfWeek, DaySchedule>
            {
                {
                    DayOfWeek.Monday, new DaySchedule()
                }
            },
            SlotDurationMinutes = 10
        };

        _mockRepository.Setup(r => r.GetBusySlotsScheduleAsync(date)).ReturnsAsync(busySlotsSchedule);
        
        // Act&Assert
        var ex= Assert.ThrowsAsync<ArgumentNullException>(() =>  _slotService.GetAvailableSlotsAsync(date));
        Assert.That(ex.ParamName, Is.EqualTo("WorkPeriod"));
    }
    
    [Test]
    public async Task BookSlotAsync_DoesNotThrowException_WhenCorrectInput()
    {
        // Arrange
        var bookSlotDto = CreateTestBookAvailableSlotDto();
        var date = new DateTimeOffset(2024, 7, 22, 0, 0, 0,DateTimeOffset.Now.Offset);
        var busySlotsSchedule = CreateSimpleTestNoBusySlotsSchedule();
        
        _mockRepository.Setup(r => r.GetBusySlotsScheduleAsync(date)).ReturnsAsync(busySlotsSchedule);
        _mockRepository.Setup(r => r.BookSlotAsync(It.IsAny<AvailableSlot>()));
        
        try
        {
            // Act
            await _slotService.BookSlotAsync(bookSlotDto);
        }
        catch (FailedToBookAvailableSlotException)
        {
            // Assert
            Assert.Fail("Expected no BookAvailableSlotException to be thrown, but one was thrown.");
        }
    }

    [Test]
    public async Task BookSlotAsync_ThrowsFailedToBookAvailableSlotException_WhenExternalServiceFails()
    {
        // Arrange
        var bookSlotDto = CreateTestBookAvailableSlotDto();
        var date = new DateTimeOffset(2024, 7, 22, 0, 0, 0,DateTimeOffset.Now.Offset);
        var busySlotsSchedule = CreateSimpleTestNoBusySlotsSchedule();
        
        _mockRepository.Setup(r => r.GetBusySlotsScheduleAsync(date)).ReturnsAsync(busySlotsSchedule);
        _mockRepository.Setup(r => r.BookSlotAsync(It.IsAny<AvailableSlot>()))
            .ThrowsAsync(new HttpRequestException());
        
        // Act&Assert
        Assert.ThrowsAsync<FailedToBookAvailableSlotException>(() =>  _slotService.BookSlotAsync(bookSlotDto));
    }
    
    [Test]
    public async Task BookSlotAsync_ThrowsSlotNotAvailableException_WhenSlotIsNotAvailable()
    {
        // Arrange
        var bookSlotDto = new BookAvailableSlotDto
        {
            FacilityId = new Guid().ToString(),
            Start = new DateTimeOffset(2024, 7, 23, 9, 0, 0,DateTimeOffset.Now.Offset),
            End = new DateTimeOffset(2024, 7, 23, 9, 10, 0,DateTimeOffset.Now.Offset),
            Comments = "Test Booking 123",
            Patient = new PatientDto()
            {
                Email = "testTest@example.com",
                Name = "Amelia",
                Phone = "0987654321",
                SecondName = "Smith"
            }
        };
        var date = new DateTimeOffset(2024, 7, 22, 0, 0, 0,DateTimeOffset.Now.Offset);
        var busySlotsSchedule = CreateSimpleTestAllSlotsAreBusySchedule(date);
        
        _mockRepository.Setup(r => r.GetBusySlotsScheduleAsync(date)).ReturnsAsync(busySlotsSchedule);
        _mockRepository.Setup(r => r.BookSlotAsync(It.IsAny<AvailableSlot>()))
            .ThrowsAsync(new HttpRequestException());

        // Act&Assert
        Assert.ThrowsAsync<SlotNotAvailableException>(() =>  _slotService.BookSlotAsync(bookSlotDto));
    }

    private BusySlotsSchedule CreateSimpleTestBusySlotsSchedule(DateTimeOffset date)
    {
        var schedule = new BusySlotsSchedule
        {
            Facility = new Facility
            {
                FacilityId = new Guid().ToString(),
                Name = "Simple Test Facility",
                Address = "123 Test St"
            },
            DaySchedules = new Dictionary<DayOfWeek, DaySchedule>
            {
                {
                    DayOfWeek.Monday, new DaySchedule
                    {
                        WorkPeriod = new WorkPeriod
                        {
                            StartHour = 9,
                            EndHour = 10,
                        },
                        BusySlots =
                        [
                            new BusySlot
                            {
                                Start = date.AddHours(9),
                                End = date.AddHours(9).AddMinutes(10)
                            },

                            new BusySlot
                            {
                                Start = date.AddHours(9).AddMinutes(10),
                                End = date.AddHours(9).AddMinutes(20)
                            },

                            new BusySlot
                            {
                                Start = date.AddHours(10).AddMinutes(20),
                                End = date.AddHours(10).AddMinutes(30)
                            }

                        ]
                    }
                }
            },
            SlotDurationMinutes = 10
        };
        
        schedule.DaySchedules.Add(DayOfWeek.Friday, new DaySchedule()
        {
            WorkPeriod = new WorkPeriod
            {
                StartHour = 12,
                EndHour = 14,
                LunchStartHour = 13,
                LunchEndHour = 14
            },
            BusySlots =
            [
                new BusySlot
                {
                    Start = date.AddDays(4).AddHours(12).AddMinutes(50),
                    End = date.AddDays(4).AddHours(13)
                },

                new BusySlot
                {
                    Start = date.AddDays(4).AddHours(12).AddMinutes(10),
                    End = date.AddDays(4).AddHours(12).AddMinutes(20)
                }
            ]
        });
        return schedule;
    }
    
    private BusySlotsSchedule CreateSimpleTestAllSlotsAreBusySchedule(DateTimeOffset date)
    {
        var schedule = new BusySlotsSchedule
        {
            Facility = new Facility
            {
                FacilityId = new Guid().ToString(),
                Name = "All Slots Busy Facility",
                Address = "All Slots Busy Test St"
            },
            DaySchedules = new Dictionary<DayOfWeek, DaySchedule>
            {
                {
                    DayOfWeek.Tuesday, new DaySchedule
                    {
                        WorkPeriod = new WorkPeriod
                        {
                            StartHour = 9,
                            EndHour = 10,
                        },
                        BusySlots =
                        [
                            new BusySlot
                            {
                                Start = date.AddDays(1).AddHours(9),
                                End = date.AddDays(1).AddHours(9).AddMinutes(10)
                            },

                            new BusySlot
                            {
                                Start = date.AddDays(1).AddHours(9).AddMinutes(10),
                                End = date.AddDays(1).AddHours(9).AddMinutes(20)
                            },

                            new BusySlot
                            {
                                Start = date.AddDays(1).AddHours(9).AddMinutes(20),
                                End = date.AddDays(1).AddHours(9).AddMinutes(30)
                            },

                            new BusySlot
                            {
                                Start = date.AddDays(1).AddHours(9).AddMinutes(30),
                                End = date.AddDays(1).AddHours(9).AddMinutes(40)
                            },

                            new BusySlot
                            {
                                Start = date.AddDays(1).AddHours(9).AddMinutes(40),
                                End = date.AddDays(1).AddHours(9).AddMinutes(50)
                            },

                            new BusySlot
                            {
                                Start = date.AddDays(1).AddHours(9).AddMinutes(50),
                                End = date.AddDays(1).AddHours(10)
                            }

                        ]
                    }
                }
            },
            SlotDurationMinutes = 10
        };
        
        return schedule;
    }
    
    private BusySlotsSchedule CreateSimpleTestNoBusySlotsSchedule()
    {
        var schedule = new BusySlotsSchedule
        {
            Facility = new Facility
            {
                FacilityId = new Guid().ToString(),
                Name = "No Busy Slots Facility",
                Address = "No Busy Slots Test St"
            },
            DaySchedules = new Dictionary<DayOfWeek, DaySchedule>
            {
                {
                    DayOfWeek.Thursday, new DaySchedule
                    {
                        WorkPeriod = new WorkPeriod
                        {
                            StartHour = 9,
                            EndHour = 10,
                        },
                        BusySlots = new List<BusySlot>()
                    }
                }
            },
            SlotDurationMinutes = 10
        };
        
        return schedule;
    }
    
    private  BookAvailableSlotDto CreateTestBookAvailableSlotDto()
    {
        var bookSlotDto = new BookAvailableSlotDto
        {
            FacilityId = new Guid().ToString(),
            Start = new DateTimeOffset(2024, 7, 25, 9, 0, 0,DateTimeOffset.Now.Offset),
            End = new DateTimeOffset(2024, 7, 25, 9, 10, 0,DateTimeOffset.Now.Offset),
            Comments = "Test Booking",
            Patient = new PatientDto()
            {
                Email = "test@example.com",
                Name = "John",
                Phone = "1234567890",
                SecondName = "Doe"
            }
        };
        return bookSlotDto;
    }
}

        