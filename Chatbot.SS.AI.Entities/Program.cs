using Chatbot.SS.AI.Entities.Database;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
string mongoConnectionString = configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017/";
string mongoDatabaseName = configuration["MongoDB:DatabaseName"] ?? "ChatbotDB";

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddSingleton<AppDbContext>(provider =>
    new AppDbContext(mongoConnectionString, mongoDatabaseName)
);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
