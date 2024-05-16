using Fake.Detection.Auth;
using Fake.Detection.Auth.Extensions;

var builder = Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(x => x.UseStartup<Startup>());

var app = builder.Build();

app.MigrateUp();
app.Run();