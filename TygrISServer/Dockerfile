#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TygrISServer/TygrISServer.csproj", "TygrISServer/"]
RUN dotnet restore "TygrISServer/TygrISServer.csproj"
COPY . .
WORKDIR "/src/TygrISServer"
RUN dotnet build "TygrISServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TygrISServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TygrISServer.dll"]