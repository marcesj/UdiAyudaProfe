# ==========================================
# ETAPA 1: Compilación y Publicación (Build)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivo de proyecto y restaurar dependencias
# (se copia primero solo el .csproj para aprovechar la caché de Docker)
COPY *.csproj ./
RUN dotnet restore

# Copiar todo el código fuente (bin/obj quedan fuera por .dockerignore)
# y compilar en modo Release
COPY . ./
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false --no-restore

# ==========================================
# ETAPA 2: Entorno de Ejecución (Runtime)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copiar los artefactos compilados desde la etapa 'build'
COPY --from=build /app/publish .

# Kestrel escucha en todas las interfaces, puerto 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5000

# Correr con el usuario sin privilegios que trae la imagen oficial
USER app

# Comando de inicio del contenedor (el .dll se llama igual que el .csproj)
ENTRYPOINT ["dotnet", "UdiAyudaProfe.dll"]
