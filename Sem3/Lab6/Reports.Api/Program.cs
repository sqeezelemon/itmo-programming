using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Reports.Api.Demo;
using Reports.App.Services;
using Reports.Data;
using Reports.Data.Enums;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IMessageService, MessageService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;

        // Where to redirect browser if there is no active session
        options.LoginPath = "/api/auth/login";

        // Where to redirect browser if there ForbidResult acquired.
        options.AccessDeniedPath = "/api/auth/login";
    });

builder.Services.AddDbContext<AppDbContext>(x => x.UseLazyLoadingProxies().UseSqlite("Data Source=database.db"));
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

// https://stackoverflow.com/questions/59774559/how-do-i-get-a-instance-of-a-service-in-asp-net-core-3-1
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // DEMO
        var service = services.GetRequiredService<IMessageService>();
        service.AddMessageSource("test", new MessageFaker("root"));
    }
    catch { }

    try
    {
        var eService = services.GetService<IEmployeeService>();
        await eService.MakeEmployee("root", "root", "root", null, EmployeeRank.Root);
        
        // DEMO
        var aService = services.GetService<IAccountService>();
        await aService.MakeAccount("root", "test", "root");
    }
    catch { }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();