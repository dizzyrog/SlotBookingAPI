using FluentValidation;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using SlotBooking.API;
using SlotBooking.Data;
using SlotBooking.Domain;
using SlotBooking.Domain.DTOs;
using SlotBooking.Domain.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddTransient<ISlotRepository, SlotRepository>();
builder.Services.AddTransient<ISlotService, SlotService>();
builder.Services.AddRouting();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IValidator<DateTimeOffset>, GetAvailableSlotsForADateValidator>();
builder.Services.AddScoped<IValidator<BookAvailableSlotDto>, BookAvailableSlotDtoValidator>();

builder.Services.AddFluentValidationAutoValidation();

//builder.Services.AddFluentValidationRulesToSwagger();

builder.Services.Configure<SlotServiceOptions>(
    builder.Configuration.GetSection(SlotServiceOptions.SlotService));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();
app.MapControllers();
app.UseAuthorization();

app.Run();