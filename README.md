# API de Gestión de Ventas

API REST desarrollada con C# y .NET para la gestión de ventas, clientes y productos. Implementa arquitectura por capas, autenticación con JWT y manejo de base de datos con SQL Server.

## 🚀 Tecnologías

- C# / .NET
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication

## 📦 Funcionalidades

- Gestión de ventas
- Control de stock
- Registro de clientes
- Autenticación de usuarios

## 🔐 Autenticación

La API utiliza JWT para la autenticación de usuarios.

## 📌 Endpoints principales

### Ventas
- GET /api/ventas
- GET /api/ventas/{id}
- POST /api/ventas
- PUT /api/ventas/{id}
- DELETE /api/ventas/{id}

### Productos
- GET /api/productos
- POST /api/productos

## ⚙️ Cómo ejecutar el proyecto

1. Clonar el repositorio
2. Configurar la cadena de conexión en `appsettings.json`
3. Ejecutar migraciones
4. Ejecutar el proyecto

## 🧠 Arquitectura

El proyecto está estructurado en capas:

- Domain
- Application
- Infrastructure
- API

## 📈 Estado del proyecto

En desarrollo – mejoras continuas en funcionalidad y rendimiento.
