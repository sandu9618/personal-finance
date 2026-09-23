FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PersonalFinance.Web/PersonalFinance.Web.csproj PersonalFinance.Web/
RUN dotnet restore PersonalFinance.Web/PersonalFinance.Web.csproj

COPY PersonalFinance.Web/ PersonalFinance.Web/
RUN dotnet publish PersonalFinance.Web/PersonalFinance.Web.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PersonalFinance.Web.dll"]
