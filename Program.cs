using TestM9iddlewareAuthApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// daniel
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
var appSettings = appSettingsSection.Get<AppSettings>();

builder.Services.Configure<AppSettings>(appSettingsSection);
builder.Services.AddScoped<IAppSettings>(_appSettings => appSettings);
builder.Services.AddScoped<IGisApiHelper, GisApiHelper>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// daniel
app.UseMiddleware<PermissionsMiddleware>();
app.UseMiddleware<BaseGisProxyMiddleware>();

// daniel
app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();



app.Run();
