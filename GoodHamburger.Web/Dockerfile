FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["GoodHamburger.Web/GoodHamburger.Web.csproj", "GoodHamburger.Web/"]
COPY ["GoodHamburger.Application/GoodHamburger.Application.csproj", "GoodHamburger.Application/"]
COPY ["GoodHamburger.Domain/GoodHamburger.Domain.csproj", "GoodHamburger.Domain/"]
RUN dotnet restore "GoodHamburger.Web/GoodHamburger.Web.csproj"

COPY . .
WORKDIR "/src/GoodHamburger.Web"
RUN dotnet publish "GoodHamburger.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GoodHamburger.Web.dll"]