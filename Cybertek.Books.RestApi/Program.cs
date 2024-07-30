using System.Text.Json.Serialization;
using AutoMapper;
using Cybertek.Apis.Common.Filters;
using Cybertek.Apis.Common.Mappers;
using Cybertek.Books.DataLayer;
using Cybertek.Books.DataLayer.Repositories;
using Cybertek.Books.Domains;
using Cybertek.Books.Services;
using Cybertek.Logging;
using Cybertek.Monitoring;
using Cybertek.Monitoring.Extensions;
using Cybertek.Paging;
using Cybertek.Repository;
using Cybertek.Settings;
using Cybertek.Settings.Extensions;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseLogging();
builder.Configuration.AddConfigurationFiles(builder.Environment);

// Add services to the container.

builder.Services
    .AddControllers(options =>
        {
            options.Filters.Add(typeof(GlobalExceptionFilter));
        })
    .AddJsonOptions(
        options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<AppSettingsBase>(builder.Configuration.GetConfigurationsSection());
builder.Services.AddBasicHealthChecks();
builder.Services.AddScoped<IMapper, CustomMapper>();
builder.Services.AddScoped<IDomainContractConverter, DomainContractConverter>();
builder.Services.AddScoped<IJsonSerializerSettingsProvider, JsonSerializerSettingsProvider>();
builder.Services.AddAdditionalStartupHealthChecks<MsSqlStartupHealthCheck>();
builder.Services.AddDbContext<DbContext, BooksDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["connectionString"]);
});
builder.Services.AddScoped<IRepository<Book>, BookRepository>();
builder.Services.AddScoped<IBooksService, BooksService>();
builder.Services.AddScoped<IPaginator, Paginator>();

WebApplication app = builder.Build();

using IServiceScope scope = app.Services.CreateScope();
IServiceProvider services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<BooksDbContext>();
    DbSet<Book> allBooks = context.Books;
    if (!allBooks.Any())
    {
        context.RemoveRange(allBooks);
        context.SaveChanges();
        //Run the Migration and create a database if we do not already have it.
        await context.Database.MigrateAsync();
        //Adding data to the database is an async action, therefore, we need to await for the result here.
        await SeedSampleData.SeedData(context);
    }
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseKubernetesHealthCheck();

app.UseAuthorization();

app.MapControllers();

app.Run();
