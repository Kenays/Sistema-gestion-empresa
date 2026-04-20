-- =========================================
-- Manejo de error: eliminar BD si existe
-- =========================================
IF DB_ID('EmpresaDB') IS NOT NULL
BEGIN
    ALTER DATABASE EmpresaDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE EmpresaDB;
END
GO

-- =========================================
-- Crear base de datos
-- =========================================
CREATE DATABASE EmpresaDB;
GO

USE EmpresaDB;
GO

-- =========================================
-- ROLES
-- =========================================
CREATE TABLE Rol (
    IdRol INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

INSERT INTO Rol (Nombre) VALUES
('Administrador'),
('Operador');
GO

-- =========================================
-- USUARIOS
-- =========================================
CREATE TABLE Usuario (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

CREATE TABLE UsuarioRol (
    IdUsuario INT NOT NULL,
    IdRol INT NOT NULL,
    CONSTRAINT PK_UsuarioRol PRIMARY KEY (IdUsuario, IdRol),
    CONSTRAINT FK_UsuarioRol_Usuario FOREIGN KEY (IdUsuario)
        REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_UsuarioRol_Rol FOREIGN KEY (IdRol)
        REFERENCES Rol(IdRol)
);
GO

-- Usuario inicial
INSERT INTO Usuario (Nombre, Email, PasswordHash)
VALUES ('Admin Sistema', 'admin@empresa.com', '$2a$11$PUI1T2oXyeBvYVp2nJZEruo9CchynqkWdO6/8XnrghEwQ.Tju9MY6');
GO

INSERT INTO UsuarioRol(IdUsuario, IdRol)
VALUES (1, 1);
GO

-- =========================================
-- PROCEDURE
-- =========================================
CREATE PROCEDURE sp_CrearUsuario
    @Nombre NVARCHAR(100),
    @Email NVARCHAR(150),
    @PasswordHash NVARCHAR(255),
    @IdRol INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NuevoId INT;

    INSERT INTO Usuario (Nombre, Email, PasswordHash)
    VALUES (@Nombre, @Email, @PasswordHash);

    SET @NuevoId = SCOPE_IDENTITY();

    INSERT INTO UsuarioRol (IdUsuario, IdRol)
    VALUES (@NuevoId, @IdRol);
END;
GO

-- =========================================
-- VISTA USUARIOS
-- =========================================
CREATE VIEW vw_Usuarios AS
SELECT 
    u.IdUsuario,
    u.Nombre,
    u.Email,
    u.Activo,
    r.Nombre AS Rol
FROM Usuario u
JOIN UsuarioRol ur ON ur.IdUsuario = u.IdUsuario
JOIN Rol r ON r.IdRol = ur.IdRol;
GO

CREATE INDEX IX_Usuario_Email ON Usuario(Email);
CREATE INDEX IX_UsuarioRol_IdRol ON UsuarioRol(IdRol);
GO

-- =========================================
-- CLIENTE
-- =========================================
CREATE TABLE Cliente (
    IdCliente INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    Telefono NVARCHAR(20),
    Activo BIT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

CREATE INDEX IX_Cliente_Email ON Cliente(Email);
GO

-- =========================================
-- PRODUCTO
-- =========================================
CREATE TABLE Producto(
	IdProducto INT IDENTITY(1,1) PRIMARY KEY,
	Nombre NVARCHAR(150) NOT NULL,
	Descripcion NVARCHAR(300),
	Precio DECIMAL(10, 2) NOT NULL,
	Costo DECIMAL(10, 2) NOT NULL,
	STOCK INT NOT NULL,
	Codigo NVARCHAR(50) NOT NULL,
	Impuesto DECIMAL(5,2),
	Activo BIT NOT NULL DEFAULT 1,
	FechaCreacion DATETIME2 DEFAULT SYSDATETIME()
);
GO

ALTER TABLE Producto ADD CONSTRAINT CK_Precio CHECK (Precio >= 0);
ALTER TABLE Producto ADD CONSTRAINT CK_Stock CHECK (Stock >= 0);
ALTER TABLE Producto ADD CONSTRAINT CK_Costo CHECK (Costo >= 0);
GO

-- =========================================
-- VENTA
-- =========================================
CREATE TABLE Venta(
	IdVenta INT IDENTITY(1,1) PRIMARY KEY,
	IdCliente INT NOT NULL,
	Fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
	Subtotal DECIMAL(10,2) NOT NULL,
	Impuesto DECIMAL(10,2) NOT NULL,
	Total DECIMAL(10,2) NOT NULL,
	Activo BIT NOT NULL DEFAULT 1,
	CONSTRAINT FK_Venta_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente(IdCliente)
);
GO

-- =========================================
-- DETALLE VENTA
-- =========================================
CREATE TABLE DetalleVenta(
	IdDetalle INT IDENTITY(1,1) PRIMARY KEY,
	IdVenta INT NOT NULL,
	IdProducto INT NOT NULL,
	Cantidad INT NOT NULL,
	PrecioUnitario DECIMAL(10,2) NOT NULL,
	Impuesto DECIMAL(10,2) NOT NULL,
	Subtotal DECIMAL(10,2) NOT NULL,
	CONSTRAINT FK_DetalleVenta_Venta FOREIGN KEY (IdVenta) REFERENCES Venta(IdVenta),
	CONSTRAINT FK_DetalleVenta_Producto FOREIGN KEY (IdProducto) REFERENCES Producto(IdProducto)
);
GO

ALTER TABLE DetalleVenta ADD CONSTRAINT CK_Detalle_Cantidad CHECK (Cantidad>0);
ALTER TABLE DetalleVenta ADD CONSTRAINT CK_Detalle_Precio CHECK (PrecioUnitario>=0);
ALTER TABLE Venta ADD CONSTRAINT CK_Detalle_Total CHECK (Total >= 0);
GO

CREATE INDEX IX_Venta_IdCliente ON Venta(IdCliente);
CREATE INDEX IX_Venta_Fecha ON Venta(Fecha);
CREATE INDEX IX_DetalleVenta_IdVenta ON DetalleVenta(IdVenta);
CREATE INDEX IX_DetalleVenta_IdProducto ON DetalleVenta(IdProducto);
GO

-- =========================================
-- VISTA VENTAS
-- =========================================
CREATE VIEW vw_VentasDetalle AS
SELECT 
    v.IdVenta,
    v.Fecha,
    c.Nombre AS Cliente,
    p.Nombre AS Producto,
    d.Cantidad,
    d.PrecioUnitario,
    d.Subtotal
FROM Venta v
JOIN Cliente c ON v.IdCliente = c.IdCliente
JOIN DetalleVenta d ON v.IdVenta = d.IdVenta
JOIN Producto p ON d.IdProducto = p.IdProducto;
GO

-- =========================================
-- DATOS DE PRUEBA
-- =========================================
INSERT INTO Cliente (Nombre, Email, Telefono) VALUES
('Juan Pérez', 'juan.perez@gmail.com', '55512345'),
('María López', 'maria.lopez@gmail.com', '55523456'),
('Carlos Gómez', 'carlos.gomez@gmail.com', '55534567'),
('Ana Martínez', 'ana.martinez@gmail.com', '55545678'),
('Luis Rodríguez', 'luis.rodriguez@gmail.com', '55556789');
GO

INSERT INTO Producto (Nombre, Descripcion, Precio, Costo, Stock, Codigo, Impuesto) VALUES
('Laptop', 'Laptop básica', 5000.00, 4000.00, 10, 'LAP001', 0.12),
('Mouse', 'Mouse inalámbrico', 150.00, 80.00, 50, 'MOU001', 0.12),
('Teclado', 'Teclado mecánico', 300.00, 200.00, 30, 'TEC001', 0.12),
('Monitor', 'Monitor 24 pulgadas', 1200.00, 900.00, 15, 'MON001', 0.12),
('USB', 'Memoria USB 32GB', 80.00, 40.00, 100, 'USB001', 0.12);
GO

INSERT INTO Venta (IdCliente, Subtotal, Impuesto, Total) VALUES
(1, 5300.00, 636.00, 5936.00),
(2, 540.00, 64.80, 604.80),
(3, 2400.00, 288.00, 2688.00),
(4, 310.00, 37.20, 347.20),
(5, 5300.00, 636.00, 5936.00);
GO

INSERT INTO DetalleVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario, Impuesto, Subtotal) VALUES
(1, 1, 1, 5000.00, 600.00, 5000.00),
(1, 2, 2, 150.00, 36.00, 300.00),
(2, 3, 1, 300.00, 36.00, 300.00),
(2, 5, 3, 80.00, 28.80, 240.00),
(3, 4, 2, 1200.00, 288.00, 2400.00),
(4, 2, 1, 150.00, 18.00, 150.00),
(4, 5, 2, 80.00, 19.20, 160.00),
(5, 1, 1, 5000.00, 600.00, 5000.00),
(5, 3, 1, 300.00, 36.00, 300.00);
GO
