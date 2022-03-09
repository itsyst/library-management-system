<div align="center">
  <a href="https://github.com/itsyst/khaled_elhamzi_portfolio">
    <img alt="logo" src="https://res.cloudinary.com/dzltxlm9l/image/upload/v1601971370/logo_fd60ee4493.png" width="90"  />
  </a>
  <p> Library-Management-System </p>
  <p>This Project is an open-source project updated from Asp.Net 3.1 to .NET Core 6.</p>
</div>
 
## Give a Star! :star:
If you Like the project, please give a star ;)
## How to use:
- You will need the latest Visual Studio 2022 and the latest .NET Core 6.
- The latest SDK and tools can be downloaded from https://dot.net/core.
Also you can run this Project in Visual Studio Code (Windows, Linux or MacOS).
To know more about how to setup your enviroment visit the [Microsoft .NET Download Guide](https://www.microsoft.com/net/download)
Build a complete e-commerce application ASP.NET Core  MVC  .NET 6

## Layout
 
<a href="https://github.com/itsyst/Library-Management-System">
  <img alt="logo" src="https://res.cloudinary.com/dzltxlm9l/image/upload/v1602522172/small_projects_4_uqjpyz_b0f0e9df8f.png" width="500"  />
</a>

## Here is a quick check list you could follow to migrate your application from .Net 3.1 to .Net 6
- Upgrade the reference in csproj files
```
// Old
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
</Project>

// New
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
	  <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
	  <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
````
- Upgrade the packages

````
//Old
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="3.1.2" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="3.1.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="3.1.2" />
    <PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="3.1.1" />
  </ItemGroup>
  
// New
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    ---
  </PropertyGroup>
 
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.2">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="6.0.0" />
    <PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="6.0.2" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Library.Application\Library.Application.csproj" />
    <ProjectReference Include="..\Library.Infrastructure\Library.Infrastructure.csproj" />
  </ItemGroup>
</Project>
  ````
  
- Upgrade the namespaces
```
//Old
namespace Library.Domain
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public IList<BookDetails>? Books { get; set; }
    }
}

// New
namespace Library.Domain;
public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IList<BookDetails>? Books { get; set; }
}
````

- Migrate startup class to Program.cs
```
//Old StartUp class
using Library.Application.Interfaces;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Library.MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            //  Here we inject our services into the the DI container
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<IBookCopyService, BookCopyService>();
            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IBookCopyLoanService, BookCopyLoanService>();

            // This call injects our applicationDbContext (our implementation of Entity Framework Core)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("LibrarySystem"),
                x => x.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}


//New Programm class
using Library.Application.Interfaces;
using Library.Infrastructure.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//DbContext configuration
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("LibrarySystem"),
    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
);

//Services configuration
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookCopyService, BookCopyService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IBookCopyLoanService, BookCopyLoanService>();

builder.Services.AddRazorPages();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{ 
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
```

- Delete the old bin and obj folders
- Build your application

🚀 Ready to go
