# DotNetKafkaRabbitMQExample 🚀

Este es un proyecto de ejemplo desarrollado en **.NET 10** diseñado para demostrar flujos de **CI/CD** (Integración Continua y Despliegue Continuo). La aplicación consiste en una Web API que gestiona un catálogo de productos y categorías, utilizando **PostgreSQL** como base de datos.

## 🛠 Tecnologías Utilizadas

- **Backend**: .NET 10 (C#)
- **Base de Datos**: PostgreSQL
- **ORM**: Entity Framework Core
- **Pruebas**: xUnit, Moq, EF Core InMemory
- **Contenedores**: Docker & Docker Compose
- **Mapeo**: Mapster
- **Documentación**: Swagger/OpenAPI (v1.0)

## 📋 Requisitos Previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- Herramienta de línea de comandos (PowerShell, Bash, etc.)

## 🚀 Pasos para levantar la aplicación

### 1. Clonar el repositorio
```bash
git clone <url-del-repositorio>
cd DotNetKafkaRabbitMQExample
```

### 2. Levantar la base de datos (Docker)
El proyecto incluye un archivo `docker-compose.yaml` configurado para levantar una instancia de PostgreSQL en el puerto `5433`.
```bash
cd DotNetKafkaRabbitMQExample
docker-compose up -d
```

### 3. Configurar la cadena de conexión
Asegúrate de que el archivo `appsettings.json` tenga la cadena de conexión correcta hacia el contenedor de Docker:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=ApiEcommerce;Username=postgres;Password=password"
}
```

### 4. Ejecutar Migraciones
Aplica la estructura de la base de datos:
```bash
dotnet ef database update --project DotNetKafkaRabbitMQExample
```

### 5. Ejecutar la Aplicación
```bash
dotnet run --project DotNetKafkaRabbitMQExample
```
La definición de la API estará disponible en: `http://localhost:5285/openapi/v1.json`

> **Nota**: Este proyecto utiliza **Microsoft.AspNetCore.OpenApi** (nativo de .NET 9/10) para generar el documento de especificación OpenAPI en lugar de Swagger UI. Puedes visualizar este JSON en herramientas como [Scalar](https://scalar.com/), [Postman](https://www.postman.com/) o importarlo en cualquier visor de OpenAPI.

Para validar la integridad del código y asegurar que los cambios no rompan funcionalidades existentes:

```bash
dotnet test DotNetKafkaRabbitMQExample.sln
```

Las pruebas incluyen:
- Pruebas unitarias de Repositorios.
- Lógica de negocio (Stock de productos, creación de categorías).
- Soporte para ejecución In-Memory o contra DB real mediante `appsettings.Test.json`.

## 🐳 Dockerización

Para construir la imagen de Docker de la aplicación completa:
```bash
docker build -t dotnet-api-example-cicd .
```

---
*Este proyecto es parte de un ejemplo educativo de implementación de pipelines de CI/CD.*
