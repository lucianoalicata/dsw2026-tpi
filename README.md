# Trabajo Práctico Integrador
## Desarrollo de Software 2026

Acceso al [documento](https://frtutneduar-my.sharepoint.com/:b:/g/personal/franciscovicente_doc_frt_utn_edu_ar/IQD-5kaAARqnT5eL7EnPMCPgAfmPoH09HkfmQQYH8pMgqdA?e=i6C3fT)

Instrucciones:
* Realizar una bifurcación por grupo
* Crear una rama de larga duración `development`
* Completar `README` con los integrantes en cada bifurcación
* Todos los integrantes deben participar con confirmaciones en el repositorio bifurcado
* Organizar el trabajo en equipo y crear ramas temporales
* Actualizar la rama de larga duración mediante **pull-requests**
* No eliminar las ramas temporales
* Tener en cuenta que ya se realizaron las migraciones de Identity, crear nuevas de ser necesario
* Para más detalles, revisar la grabación de la última clase
* El endpoint de registración de usuarios administradores está disponible para crear usuarios y poder hacer pruebas, a futuro se eliminará

# Sistema de Gestión de Turnos Médicos

> Trabajo Práctico Integrador - Desarrollo de Software 2026  
> UTN - Facultad Regional Tucumán

---

## Integrantes del Grupo

| Nombre y Apellido                 | Legajo | Email                      |
| --------------------------------- | ------ | -------------------------- |
| Alarcón Trevisani, Lucas Leonardo | 62478  | lucasalarcon1995@gmail.com |
| Alicata, Luciano Antonio			| 60253	 | lucianoaalicata@gmail.com  |
| Tenreyro, Juan Eduardo            | 60850  | tenreyrojuan1@gmail.com    |
| Valdecantos, Joaquín              | 60326  | joacovaldecantos@gmail.com |

---

## Requisitos Previos

Antes de comenzar, se debe de tener instalado:

- [.NET 10 SDK](https://dotnet.microsoft.com/download) o superior
- [SQL Server](https://www.microsoft.com/es-es/sql-server/sql-server-downloads) (LocalDB, Express o superior)
- [Git](https://git-scm.com/)
### Recomendado
- Sistema operativo Windows 10 o superior
- Visual Studio 2026 (y ejecutarlo desde el mismo)

---

## Configuración y Ejecución Local

### 1. Clonar el repositorio y situarse en la rama development

- Se recomienda crear un directorio en el que desee tener el proyecto, luego abrir el Bash de Git desde dicho directorio, para pegar y ejecutar el siguiente comando:

```bash
git clone https://github.com/tenreyrojuan/dsw2026-tpi
cd dsw2026-tpi
git checkout development
```

### 2. Configurar la cadena de conexión

Abrir el archivo `Dsw2026Tpi.Api/appsettings.Development.json` y verificar la cadena de conexión:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Database=Dsw2026Tpi;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True"
}
```

> Si usa SQLite o una instancia diferente, modifique el `Data Source` acorde a su entorno.

### 3. Aplicar migraciones y crear la base de datos

#### Usando Visual Studio
1. Abrir la solución `Dsw2026Tpi.slnx` con Visual Studio 2026
2. En `Tools` en la barra de herramientas buscar `NuGet Package Manager` y seleccionar `Package Manager Console`
3. Ejecutar los siguientes comandos, secuencialmente
``` PacketManagerConsole
Update-Database -Context Dsw2026TpiDbContext
```

``` PacketManagerConsole
Update-Database -Context AuthenticationDbContext
```
4. Revisar desde `SQL Server Object Explorer` que la base de datos `Dsw2026Tpi` esté creada en `(localdb)\\MSSQLLocalDB` dentro de la carpeta `Databases` con todas sus tablas presentes.

#### Sin Visual Studio
Desde la raíz del proyecto abrimos PowerShell o la Terminal:

> Asegurarse de tener instalada la herramienta global de EF Core:

``` PowerShell
dotnet tool install --global dotnet-ef
```

Luego ejecutar uno por uno los siguientes comandos:
```Powershell
dotnet restore
cd Dsw2026Tpi.Api
dotnet ef database update --context Dsw2026TpiDbContext
dotnet ef database update --context AuthenticationDbContext
```

### 4. Verificar archivos de semilla

El proyecto utiliza dos archivos JSON de semilla ubicados en `Dsw2026Tpi.Api/Sources/`:

- **`roles.json`**: Roles del sistema (`Administrador`, `Paciente`). Se cargan automáticamente al iniciar.
- **`holidays.json`**: Días feriados para la generación de disponibilidades. Pueden ser editados para agregar o quitar fechas en formato `"YYYY-MM-DD"`.

> **Nota:** El archivo `holidays.json` debe copiarse al directorio de salida (`bin`). Esto se configura automáticamente en el `.csproj` con `CopyToOutputDirectory`.

### 5. Ejecutar la aplicación

#### Con Visual Studio

- Ejecutar presionando F5 o el botón de play verde que dice https.

#### Sin Visual Studio

```Powershell
dotnet run --launch-profile https
```

La API se levantará en:
- **HTTP**: `http://localhost:5278`
- **HTTPS**: `https://localhost:7075` (si ejecutas el segundo comando)

### 6. Acceder a la documentación Swagger

Una vez en ejecución, abrir en el navegador:
```
https://localhost:7075/swagger
```
o, abrir:
```
http://localhost:5278/swagger
```

Allí se encuentran todos los endpoints documentados con sus request/response y la opción de autenticar con JWT (botón **Authorize**).

---

## Arquitectura del Proyecto

La solución sigue una arquitectura **N-Capas** orientada a dominio:

```
┌───────────────────────────────────┐
│           Dsw2026Tpi.Api          │  ← Controllers, Middleware, Configuración
│         (Capa de Presentación)    │
├───────────────────────────────────┤
│      Dsw2026Tpi.Application       │  ← Servicios, DTOs, Interfaces de aplicación
│         (Capa de Aplicación)      │
├───────────────────────────────────┤
│        Dsw2026Tpi.Domain          │  ← Entidades, Enums, Interfaces de dominio
│         (Capa de Dominio)         │
├───────────────────────────────────┤
│         Dsw2026Tpi.Data           │  ← EF Core, Configuraciones Fluent API
│      (Capa de Infraestructura)    │     Repositorios, Identity, Seed
├───────────────────────────────────┤
│     Dsw2026Tpi.CrossCutting       │  ← Excepciones, Helpers, Recursos
│         (Capa Transversal)        │
└───────────────────────────────────┘
```

### Tecnologías principales

- **Backend**: ASP.NET Core Web API (.NET 10)
- **ORM**: Entity Framework Core 10 + SQL Server
- **Autenticación**: ASP.NET Core Identity + JWT Bearer Tokens
- **Logging**: Serilog (consola + archivo rotativo)
- **Rate Limiting**: ASP.NET Core Rate Limiter (configurable desde `appsettings.json`)
- **Serialización**: System.Text.Json
- **Documentación**: Swagger / OpenAPI

---

## Autenticación y Autorización

El sistema utiliza **JWT** con dos roles:

| Rol | Descripción |
|-----|-------------|
| `Administrador` | Gestiona médicos, especialidades, disponibilidades y consulta turnos |
| `Paciente` | Consulta disponibilidad, reserva y cancela turnos |

### Obtener un token

#### Login de Administrador
```http
POST /api/auth/admin/login
Content-Type: application/json

{
  "email": "admin@system.com",
  "password": "string"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "role": "ADMINISTRADOR"
}
```

#### Login de Paciente
```http
POST /api/auth/patient/login
Content-Type: application/json

{
  "email": "paciente@email.com",
  "dni": 40123456
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "role": "PACIENTE"
}
```

> **Regla de negocio (RN06):** Si el paciente inicia sesión por primera vez, el sistema lo registra automáticamente y le asigna el rol `PACIENTE`.

### Usar el token

Incluir el header en todas las peticiones protegidas:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

En Swagger, hacer clic en **Authorize** e ingresar: `Bearer [token devuelto]`.

---

## Endpoints Implementados

### Módulo de Autenticación (`/api/auth`)

| Método | Endpoint                   | Descripción                                    | Acceso  |
| ------ | -------------------------- | ---------------------------------------------- | ------- |
| `POST` | `/api/auth/admin/login`    | Login de administrador                         | Público |
| `POST` | `/api/auth/patient/login`  | Login de paciente (auto-registro si no existe) | Público |
| `POST` | `/api/auth/admin/register` | Registro transitorio de administrador          | Público |

> **Nota:** El endpoint de registro de admin es transitorio debido a que no hay un seed del mismo. Se recomienda registrar el primer administrador y luego comentar/ocultar este endpoint.

---

### Módulo de Especialidades (`/api/specialties`)

| Método   | Endpoint                                                        | Descripción                                                  | Acceso |
| -------- | --------------------------------------------------------------- | ------------------------------------------------------------ | ------ |
| `GET`    | `/api/specialties?pageSize=number&pageIndex=number&name=string` | Listar especialidades paginadas (filtro por nombre opcional) | Admin  |
| `POST`   | `/api/specialties`                                              | Crear especialidad                                           | Admin  |
| `PUT`    | `/api/specialties/{id}`                                         | Actualizar especialidad                                      | Admin  |
| `DELETE` | `/api/specialties/{id}`                                         | Eliminación lógica (soft delete)                             | Admin  |

**Request POST/PUT:**
```json
{
  "name": "Cardiologia",
  "description": "Estudio y tratamiento de trastornos del corazón."
}
```

**Response GET:**
```json
{
  "pageSize": 10,
  "pageIndex": 1,
  "data": [
    { "id": "...", "name": "Cardiología", "description": "..." }
  ],
  "total": 1
}
```

---

### Módulo de Médicos (`/api/doctors`)

| Método   | Endpoint                                                    | Descripción                                           | Acceso |
| -------- | ----------------------------------------------------------- | ----------------------------------------------------- | ------ |
| `GET`    | `/api/doctors?pageSize=number&pageIndex=number&name=string` | Listar médicos paginados (filtro por nombre opcional) | Admin  |
| `GET`    | `/api/doctors/{id}/availabilities`                          | Obtener disponibilidad mensual del médico             | Admin  |
| `POST`   | `/api/doctors`                                              | Registrar médico                                      | Admin  |
| `PUT`    | `/api/doctors/{id}`                                         | Actualizar médico                                     | Admin  |
| `DELETE` | `/api/doctors/{id}`                                         | Eliminación lógica (soft delete)                      | Admin  |

**Request POST/PUT:**
```json
{
  "name": "Dr. Joaquin Valdecantos",
  "licenseNumber": "MED-4920",
  "specialtyId": "insertar-guid-especialidad"
}
```

**Response GET `/api/doctors?pageSize=number&pageIndex=number&name=string`:
```json
{
	"pageSize": number,
	"pageIndex": number,
	"data": [ 
		{
			"id": "Guid",
			"name": "string",
			"licenseNumber": "string",
			"specialty": { 
				"id": "Guid",
				"name": "string"
				}
		} , 
	], 
	"total": number }
}
```
**Response GET `/api/doctors/{id}/availabilities`:**
```json
[
  {
    "id": "...",
    "day": "LUNES",
    "startTime": "08:00",
    "endTime": "16:00"
  }
]
```


---

### Módulo de Disponibilidades (`/api/availabilities`)

| Método | Endpoint | Descripción | Acceso |
|--------|----------|-------------|--------|
| `POST` | `/api/availabilities` | Crear disponibilidades mensuales para un médico | Admin |
| `PUT` | `/api/availabilities` | Actualizar disponibilidades (sobrescribe solo slots no reservados) | Admin |

**Request:**
```json
{
  "doctorId": "insertar-guid-doctor",
  "days": [
    {
      "day": "Lunes",
      "startTime": "08:00",
      "endTime": "16:00"
    },
    {
      "day": "Miércoles",
      "startTime": "08:00",
      "endTime": "12:00"
    }
  ]
}
```

**Comportamiento:**
- Se generan slots de **30 minutos** automáticamente para cada día indicado, por el resto del mes.
- **No se generan turnos en feriados** (configurables en `Sources/holidays.json`).
- Se valida que no haya **solapamientos** con disponibilidades existentes del mismo médico.
- En `PUT`, se preservan los slots que ya están **reservados** (`BOOKED`).

---

### Módulo de Citas (`/api/appointments`)

| Método   | Endpoint                                                                                                                    | Descripción                                         | Acceso   |
| -------- | --------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------- | -------- |
| `POST`   | `/api/appointments`                                                                                                         | Reservar un turno                                   | Paciente |
| `GET`    | `/api/appointments/patient?dni=number`                                                                                      | Ver turnos activos del paciente                     | Paciente |
| `DELETE` | `/api/appointments/{id}`                                                                                                    | Cancelar turno (soft delete vía estado `CANCELLED`) | Paciente |
| `GET`    | `/api/appointments?date=YYYY-MM-DD`                                                                                         | Turnos del día (admin)                              | Admin    |
| `GET`    | `/api/appointments/search?pageSize=number&pageIndex=number& specialtyId=number&doctorId=number&dni=number&date=YYYY-MM -DD` | Búsqueda combinada de turnos                        | Admin    |

#### Reservar turno (`POST`)
```json
{
  "doctorId": "[GUID_MEDICO]",
  "availabilitySlotId": "[GUID_SLOT]",
  "patient": {
    "dni": 47353765
  },
  "reason": "Le duele la panza"
}
```

**Validaciones:**
- El slot debe estar en estado `AVAILABLE`.
- No se permiten turnos en fechas pasadas.
- El DNI debe tener entre 7 y 8 dígitos.
- El motivo debe tener al menos 5 caracteres.

#### Cancelar turno (`DELETE`)
Cambia el estado de la cita a `CANCELLED`, libera el slot (vuelve a `AVAILABLE`) y registra la fecha de cancelación.

#### Búsqueda combinada admin (`GET /api/appointments/search`)
```http
GET /api/appointments/search?pageSize=10&pageIndex=1&specialtyId=[GUID]&doctorId=[GUID]&dni=40123456&date=2026-05-22
```

**Response:**
```json
{
  "pageSize": 10,
  "pageIndex": 1,
  "data": [
    {
      "appointmentsId": "...",
      "appointmentsStatus": "BOOKED",
      "patient": {
        "dni": 47353765,
        "fullName": "Juan Tenreyro"
      },
      "doctor": {
        "doctorId": "...",
        "name": "Dr. Joaquin Valdecantos",
        "specialty": {
          "specialtyId": "...",
          "name": "Cardiología"
        }
      }
    }
  ],
  "total": 1
}
```

---

## Configuraciones Importantes

### Rate Limiting (`appsettings.json`)

Las políticas de limitación de solicitudes se configuran externamente:

```json
"RateLimiting": {
  "AdminLogin": { "PermitLimit": 5, "WindowInMinutes": 1 },
  "PatientLogin": { "PermitLimit": 10, "WindowInMinutes": 1 },
  "Appointments": { "PermitLimit": 5, "WindowInMinutes": 1 },
  "General": { "PermitLimit": 100, "WindowInMinutes": 1 }
}
```

| Endpoint | Límite |
|----------|--------|
| `/api/auth/admin/login` | 5 req/min por IP |
| `/api/auth/patient/login` | 10 req/min por IP |
| `POST /api/appointments` | 5 req/min por paciente autenticado |
| Resto de endpoints | 100 req/min por usuario/IP |

### JWT (`appsettings.Development.json`)

```json
"Jwt": {
  "Key": "[...]",
  "Issuer": "Dsw2026Tpi.Api",
  "Audience": "Dsw2026Tpi.ApiUsers",
  "ExpiresInMinutes": 60
}
```

> **Advertencia de seguridad:** Cambiar la clave por una de al menos 32 caracteres en producción.

---
## Pruebas Rápidas

Primero registrarse como admin, luego loguearse, obtener el token, pegarlo en Authorize como se indicó anteriormente, y probar los siguientes endpoints

### 1. Registrar una especialidad
```json
{
  "name": "Cardiologia",
  "description": "Arregla corazones!"
}
```

Luego copiar el `specialtyId` que retorna la entidad para usarlo en el siguiente:
### 2. Registrar un médico
```json
{
  "name": "Lucas Alarcón",
  "licenseNumber": "MP 39292",
  "specialtyId": "pegar-aqui"
}
```

Copiar el `doctorId` que retorna la entidad para usarlo en el siguiente endpoint:
### 3. Crear disponibilidad
```json
{
  "doctorId": "pegar-aqui",
  "days": [
    {
      "day": "LUNES",
      "startTime": "10:00",
      "endTime": "14:00"
    },
	{
      "day": "MARTES",
      "startTime": "08:00",
      "endTime": "14:00"
    },
    {
      "day": "MIERCOLES",
      "startTime": "19:00",
      "endTime": "22:00"
    }
  ]
}
```

### 4. Reservar turno (como paciente)
Implica desloguearse como admin y loguearse como paciente, el registro es automático, el token 

```json
{
  "doctorId": "pegar-aqui-el-mismo-doctor-id",
  "availabilitySlotId": "buscarlo-en-la-base-de-datos",
  "patient": {
    "dni": "dni-con-el-que-te-registraste"
  },
  "reason": "Le duele la panza"
}
```


---

## Consideraciones Finales

- **Soft Delete:** Ningún registro se elimina físicamente. Se marca `Deleted = true` y se excluye de las queries mediante `HasQueryFilter`.
- **Consistencia:** Los estados de cita (`BOOKED`, `ATTENDED`, `CANCELLED`, `NO_SHOW`) y de turno (`AVAILABLE`, `BOOKED`, `BLOCKED`) se persisten como `varchar(20)`.
- **Concurrencia:** La reserva de turnos utiliza un índice `UNIQUE` a nivel de base de datos sobre `Appointments.AvailabilitySlotId` para prevenir dobles reservas.
- **Feriados:** Editar `Sources/holidays.json` y reiniciar la aplicación para aplicar cambios en la generación de disponibilidades.

---

> **UTN - FRT | Desarrollo de Software 2026**
