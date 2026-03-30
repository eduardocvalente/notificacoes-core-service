FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY NotificacoesService.Domain/NotificacoesService.Domain.csproj           NotificacoesService.Domain/
COPY NotificacoesService.Application/NotificacoesService.Application.csproj NotificacoesService.Application/
COPY NotificacoesService.Infrastructure/NotificacoesService.Infrastructure.csproj NotificacoesService.Infrastructure/
COPY NotificacoesService.API/NotificacoesService.API.csproj                 NotificacoesService.API/

RUN dotnet restore NotificacoesService.API/NotificacoesService.API.csproj

COPY . .

RUN dotnet publish NotificacoesService.API/NotificacoesService.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd --system --gid 1001 appgroup && \
    useradd  --system --uid 1001 --gid appgroup appuser

COPY --from=build /app/publish .

RUN chown -R appuser:appgroup /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NotificacoesService.API.dll"]
