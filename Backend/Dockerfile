FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["BadmintonBooking.API.csproj", "./"]
RUN dotnet restore "BadmintonBooking.API.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "BadmintonBooking.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BadmintonBooking.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

ENTRYPOINT ["dotnet", "BadmintonBooking.API.dll"] 