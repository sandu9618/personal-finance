FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PersonalFinance.Domain/PersonalFinance.Domain.csproj PersonalFinance.Domain/
COPY PersonalFinance.Application/PersonalFinance.Application.csproj PersonalFinance.Application/
COPY PersonalFinance.Infrastructure/PersonalFinance.Infrastructure.csproj PersonalFinance.Infrastructure/
COPY PersonalFinance.Api/PersonalFinance.Api.csproj PersonalFinance.Api/

RUN dotnet restore PersonalFinance.Api/PersonalFinance.Api.csproj

COPY PersonalFinance.Domain/ PersonalFinance.Domain/
COPY PersonalFinance.Application/ PersonalFinance.Application/
COPY PersonalFinance.Infrastructure/ PersonalFinance.Infrastructure/
COPY PersonalFinance.Api/ PersonalFinance.Api/

RUN dotnet publish PersonalFinance.Api/PersonalFinance.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PersonalFinance.Api.dll"]
