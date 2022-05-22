#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV M_MMAP_THRESHOLD_=65536
ENV M_TRIM_THRESHOLD_=65536
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["example/ScriptKidExample/ScriptKidExample.csproj", "example/ScriptKidExample/"]
RUN dotnet restore "example/ScriptKidExample/ScriptKidExample.csproj"
COPY . .
WORKDIR "/src/example/ScriptKidExample"
RUN dotnet build "ScriptKidExample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ScriptKidExample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScriptKidExample.dll"]