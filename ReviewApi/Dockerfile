FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ReviewApi/ReviewApi.csproj", "ReviewApi/"]
RUN dotnet restore "ReviewApi/ReviewApi.csproj"
COPY . .
WORKDIR "/src/ReviewApi"
RUN dotnet build "ReviewApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReviewApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReviewApi.dll"]
