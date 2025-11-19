using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Avant.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Só isso: faz o Program usar UseInMemoryDatabase("AvantDbTests")
        builder.UseEnvironment("Testing");
    }
}
