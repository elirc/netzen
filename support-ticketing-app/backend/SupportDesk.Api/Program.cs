using SupportDesk.Api.Application;
using SupportDesk.Api.Application.Commands;
using SupportDesk.Api.Application.Queries;
using SupportDesk.Api.Auth;
using SupportDesk.Api.Endpoints;
using SupportDesk.Api.Infrastructure;
using SupportDesk.Api.Realtime;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HeaderUserContext>();
builder.Services.AddSingleton<ITicketStore, InMemoryTicketStore>();
builder.Services.AddSingleton<ICannedReplyStore, InMemoryCannedReplyStore>();
builder.Services.AddScoped<IRealtimeTicketNotifier, SignalRTicketNotifier>();

builder.Services.AddScoped<CreateTicketHandler>();
builder.Services.AddScoped<AddCommentHandler>();
builder.Services.AddScoped<AssignTicketHandler>();
builder.Services.AddScoped<ChangeTicketStatusHandler>();
builder.Services.AddScoped<ReplaceTicketTagsHandler>();
builder.Services.AddScoped<SearchTicketsHandler>();
builder.Services.AddScoped<GetTicketHandler>();
builder.Services.AddScoped<GetCannedRepliesHandler>();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();

app.MapGet("/", () => Results.Redirect("/api/tickets"));
app.MapTicketEndpoints();
app.MapCannedReplyEndpoints();
app.MapHub<TicketHub>("/hubs/tickets");

SeedData.Load(app.Services);

app.Run();
