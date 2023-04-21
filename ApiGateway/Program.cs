using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot-aggregation.json");

builder.Services.AddOcelot();

var app = builder.Build();

//app.UseHttpsRedirection();

app.UseHttpMetrics();

//app.MapMetrics(); //Doesn't work here for some unknown reason.
// Do it the old way instead:
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();
});

app.UseOcelot().Wait();

app.Run();