#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 9015

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Forto4kiParser/Forto4kiParser.csproj", "Forto4kiParser/"]
RUN dotnet restore "Forto4kiParser/Forto4kiParser.csproj"
COPY . .
WORKDIR "/src/Forto4kiParser"
RUN dotnet build "Forto4kiParser.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Forto4kiParser.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:9015
ENTRYPOINT ["dotnet", "Forto4kiParser.dll"]