### MVC
This layer is a Presentation layer 
This is a MVC Application using ASP.NET Core 3. 
This layer depends on both the Application and Infrastructure layers, however, 
the dependency on Infrastructure is only to support dependency injection. 
Therefore only *Startup.cs* should reference Infrastructure.