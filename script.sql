-- Veterinaria (S14): script de base de datos con tabla Usuario para JWT (SQL Server)
IF DB_ID('VeterinariaDB') IS NULL CREATE DATABASE VeterinariaDB;
GO
USE VeterinariaDB;
GO

IF OBJECT_ID('dbo.Usuario') IS NOT NULL DROP TABLE dbo.Usuario;
IF OBJECT_ID('dbo.Cita') IS NOT NULL DROP TABLE dbo.Cita;
IF OBJECT_ID('dbo.Mascota') IS NOT NULL DROP TABLE dbo.Mascota;
GO

CREATE TABLE dbo.Mascota (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          NVARCHAR(60)  NOT NULL,
    Especie         NVARCHAR(30)  NOT NULL,
    Raza            NVARCHAR(60)  NULL,
    FechaNacimiento DATE          NULL,
    NombreDueno     NVARCHAR(100) NOT NULL,
    Telefono        NVARCHAR(20)  NULL,
    Activo          BIT           NOT NULL DEFAULT 1
);

CREATE TABLE dbo.Cita (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    MascotaId INT           NOT NULL,
    FechaHora DATETIME2     NOT NULL,
    Motivo    NVARCHAR(200) NOT NULL,
    Costo     DECIMAL(10,2) NOT NULL DEFAULT 0,
    Estado    NVARCHAR(20)  NOT NULL DEFAULT 'Pendiente',
    CONSTRAINT FK_Cita_Mascota FOREIGN KEY (MascotaId) REFERENCES dbo.Mascota(Id),
    CONSTRAINT CK_Cita_Estado CHECK (Estado IN ('Pendiente','Atendida','Cancelada'))
);
GO

INSERT INTO dbo.Mascota (Nombre, Especie, Raza, FechaNacimiento, NombreDueno, Telefono) VALUES
('Firulais','Perro','Mestizo','2019-03-12','Carlos Ramos','987654321'),
('Michi','Gato','Siames','2021-07-01','Ana Torres','912345678'),
('Rocky','Perro','Bulldog','2020-11-20','Luis Paredes','955111222'),
('Luna','Gato','Persa','2022-02-05','Marta Díaz','944333222'),
('Piolín','Ave','Canario','2023-01-15','Jorge Vega','966777888');

INSERT INTO dbo.Cita (MascotaId, FechaHora, Motivo, Costo, Estado) VALUES
(1,'2026-01-10T09:00:00','Vacuna antirrábica',45.00,'Atendida'),
(2,'2026-01-12T10:30:00','Control general',60.00,'Atendida'),
(3,'2030-05-20T11:00:00','Desparasitación',35.00,'Pendiente'),
(4,'2030-05-21T15:00:00','Esterilización',250.00,'Pendiente'),
(1,'2030-06-01T09:30:00','Control posvacuna',30.00,'Pendiente');
GO

-- ===== S14: usuarios y roles =====
CREATE TABLE dbo.Usuario (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    Correo    NVARCHAR(120) NOT NULL,
    ClaveHash NVARCHAR(100) NOT NULL,   -- hash BCrypt, nunca la clave en texto
    Rol       NVARCHAR(30)  NOT NULL,
    CONSTRAINT UQ_Usuario_Correo UNIQUE (Correo),
    CONSTRAINT CK_Usuario_Rol CHECK (Rol IN ('Cliente','Recepcionista','Veterinario'))
);
GO

-- Un usuario por rol. Clave de prueba de los tres: Clave123*  (hash BCrypt)
INSERT INTO dbo.Usuario (Correo, ClaveHash, Rol) VALUES
('cliente@vet.com',       '$2a$11$u7PXAvWmuJBI9vLoHGe2durgzEkDar5S23GsiE.XS7b8Mqhzsy0X.', 'Cliente'),
('recepcion@vet.com',     '$2a$11$u7PXAvWmuJBI9vLoHGe2durgzEkDar5S23GsiE.XS7b8Mqhzsy0X.', 'Recepcionista'),
('veterinario@vet.com',   '$2a$11$u7PXAvWmuJBI9vLoHGe2durgzEkDar5S23GsiE.XS7b8Mqhzsy0X.', 'Veterinario');
GO

-- Para promover a un usuario registrado por la API (el rol nunca lo elige el cliente):
-- UPDATE dbo.Usuario SET Rol = 'Veterinario' WHERE Correo = 'otro@vet.com';
