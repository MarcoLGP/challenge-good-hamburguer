FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["GoodHamburger.Api/GoodHamburger.Api.csproj", "GoodHamburger.Api/"]
COPY ["GoodHamburger.Application/GoodHamburger.Application.csproj", "GoodHamburger.Application/"]
COPY ["GoodHamburger.Domain/GoodHamburger.Domain.csproj", "GoodHamburger.Domain/"]
COPY ["GoodHamburger.Infrastructure/GoodHamburger.Infrastructure.csproj", "GoodHamburger.Infrastructure/"]
RUN dotnet restore "GoodHamburger.Api/GoodHamburger.Api.csproj"

COPY . .
WORKDIR "/src/GoodHamburger.Api"

RUN dotnet publish "GoodHamburger.Api.csproj" \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GoodHamburger.Api.dll"]