# Trabajo Práctico Integrador - DSW 2026


## INTEGRANTES

* Alicata, Luciano Antonio, 60253
* Leyva, María Belén, 62013
* Rivero, Juanita Sofía, 60638


## Requisitos previos


\- .NET SDK 10

\- SQL Server LocalDB 



## **Instrucciones de Instalación y Configuración** 

1. Clonar el repositorio y abrir Dsw2026Tpi.slnx.
2. Configurar Base de Datos: La cadena de conexión ya viene configurada en Dsw2026Tpi.Api/appsettings.Development.json, apuntando a LocalDB.
3. Aplicar migraciones: Abrir la Consola del Administrador de Paquetes y ejecutar los comandos para crear la base Dsw2026Tpi en LocalDB con todas las tablas:
* 'Update-Database -Project Dsw2026Tpi.Data -StartupProject Dsw2026Tpi.Api -Context Dsw2026TpiDbContext'
* 'Update-Database -Project Dsw2026Tpi.Data -StartupProject Dsw2026Tpi.Api -Context AuthenticationDbContext'

4\. Compilar y ejecutar el proyecto.

5\. Crear el primer administrador: Llamar a POST `/api/auth/admin/register` con `{"email": "...", "password": "..."}` (mínimo 8 caracteres, con mayúscula, minúscula y dígito). Solo funciona una vez, si se vuelve a llamar, devuelve 409.

6\. Autenticarse en POST `/api/auth/admin/login` con esas mismas credenciales para obtener el token JWT, y usarlo en el candado "Authorize" de Swagger para probar el resto de los endpoints protegidos.

## 



## **Descripción de Endpoints** 

#### **Autenticación (Authentication)**

* **POST `/api/auth/admin/register`**: Registra un nuevo usuario con rol de Administrador en el sistema. Solo puede existir un administrador dado de alta por esta vía, si ya existe uno, devuelve 409. 
* **POST `/api/auth/admin/login`:** Autentica a un administrador mediante email y contraseña, retornando el token JWT necesario para acceder a las rutas protegidas.
* **POST `/api/auth/patient/login`:** Autentica a un paciente mediante email y DNI, registrándolo automáticamente si es su primer acceso, y devuelve un token JWT.

#### 

#### **Especialidades (Specialties)**

* **GET `/api/specialties`:** Disponible para usuarios con rol **Paciente** y **Administrador**. Obtiene el listado de especialidades activas. Soporta paginado (pageSize, pageIndex) y filtrado opcional por nombre. 
* **POST `/api/specialties`:** Exclusivo para usuarios con rol **Administrador**. Registra una nueva especialidad médica. El name debe tener entre 3 y 100 caracteres, description entre 10 y 100. Devuelve la especialidad creada (con su id), o 400/409 si los datos no son válidos o el nombre ya existe. 
* **PUT `/api/specialties/{id}`:** Exclusivo para usuarios con rol **Administrador**. Actualiza el nombre o descripción de una especialidad existente. Mismas validaciones que el registro (name: entre 3 y 100 caracteres, description: entre 10 y 100). Devuelve la especialidad actualizada, o 404/400/409 si no existe, los datos no son válidos, o el nuevo nombre ya está en uso.  
* **DELETE `/api/specialties/{id}`:** Exclusivo para usuarios con rol **Administrador**. Realiza un borrado lógico (soft delete) de una especialidad cambiando su estado deleted.


#### **Citas (Appointments)**
* **POST `/api/appointments`**: Permite a un usuario con rol **Paciente** realizar la reserva de una cita médica asociada a un bloque de disponibilidad (`AvailabilitySlot`) válido.
* **GET `/api/appointments/patient`**: Permite a usuarios con rol **Paciente** listar sus turnos ingresando su `dni` como parámetro de consulta. Retorna únicamente las citas en estado `Booked` (reservado) con soporte para paginación (`pageSize`, `pageIndex`).
* **DELETE `/api/appointments/{id}`**: Permite a usuarios con rol **Paciente** cancelar una cita reservada. Cambia el estado del turno a `Cancelled` y libera el bloque de disponibilidad correspondiente para que pueda volver a ser reservado.
* **GET `/api/appointments`**: Exclusivo para usuarios con rol **Administrador**. Consulta todos los turnos programados para una fecha específica recibida por parámetro de consulta (`date` en formato `YYYY-MM-DD`), con soporte para paginación.
* **GET `/api/appointments/search`**: Exclusivo para usuarios con rol **Administrador**. Permite realizar búsquedas avanzadas de citas filtrando mediante combinación opcional de `specialtyId`, `doctorId`, `dni` y `date`, incluyendo soporte para paginación (`pageSize`, `pageIndex`).


