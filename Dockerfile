FROM mcr.microsoft.com/dotnet/sdk:8.0 as base
WORKDIR /app
EXPOSE 6000
EXPOSE 443
ENV ASPNETCORE_URLS=http://+:6000

# Copy everything
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /src
COPY  Library.MVC/*.csproj  Library.MVC/ 
COPY  Library.Infrastructure/*.csproj  Library.Infrastructure/ 
COPY  Library.Domain/*.csproj  Library.Domain/ 
COPY  Library.Application/*.csproj  Library.Application/ 

# Restore as distinct layers
RUN dotnet restore Library.MVC/Library.MVC.csproj

# Copy everything
COPY . ./ 

# Build and publish a release
WORKDIR "/src/Library.MVC"
RUN dotnet build "Library.MVC.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Library.MVC.csproj" -c Release -o /app

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Library.MVC.dll", "--server.urls", "http://0.0.0.0:6000"]
