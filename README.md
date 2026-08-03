# \#Trabajo Práctico Integrador - DSW 2026



## \##INTEGRANTES

* Alicata, Luciano Antonio, 60253
* Leyva, María Belén, 62013
* Rivero, Juanita Sofía, 60638



## \##Requisitos previos



\- .NET SDK 10

\- SQL Server LocalDB 



## **##Instrucciones de Instalación y Configuración** 

1. Clonar el repositorio y abrir Dsw2026Tpi.slnx.
2. Configurar Base de Datos: La cadena de conexión ya viene configurada en Dsw2026Tpi.Api/appsettings.Development.json, apuntando a LocalDB.
3. Aplicar migraciones: Abrir la Consola del Administrador de Paquetes y ejecutar los comandos para crear la base Dsw2026Tpi en LocalDB con todas las tablas:
* 'Update-Database -Project Dsw2026Tpi.Data -StartupProject Dsw2026Tpi.Api -Context Dsw2026TpiDbContext'
* 'Update-Database -Project Dsw2026Tpi.Data -StartupProject Dsw2026Tpi.Api -Context AuthenticationDbContext'

4\. Compilar y ejecutar el proyecto.

5\. Crear el primer administrador: llamar a \*\*POST\*\* `/api/auth/admin/register` con `{"email": "...", "password": "..."}` (mínimo 8 caracteres, con mayúscula, minúscula y dígito). Solo funciona una vez, si se vuelve a llamar, devuelve 409.

6\. Autenticarse en \*\*POST\*\* `/api/auth/admin/login` con esas mismas credenciales para obtener el token JWT, y usarlo en el candado "Authorize" de Swagger para probar el resto de los endpoints protegidos.

## 

## \##**Descripción de Endpoints** 



#### **##Autenticación**

* **\*\*POST\*\* `/api/auth/admin/register`**: Registra un nuevo usuario con rol de Administrador en el sistema. Solo puede existir un administrador dado de alta por esta vía, si ya existe uno, devuelve 409. 
* **\*\*POST\*\* `/api/auth/admin/login`:** Autentica a un administrador mediante email y contraseña, retornando el token JWT necesario para acceder a las rutas protegidas.
* **\*\*POST\*\* `/api/auth/patient/login`:** Autentica a un paciente mediante email y DNI, registrándolo automáticamente si es su primer acceso, y devuelve un token JWT.

#### 

#### **##Especialidades**

* **\*\*GET\*\* `/api/specialties`:** Obtiene el listado de especialidades activas. Soporta paginado (pageSize, pageIndex) y filtrado opcional por nombre. 
* **\*\*POST\*\* `/api/specialties`:** Registra una nueva especialidad médica. El name debe tener entre 3 y 100 caracteres, description entre 10 y 100. Devuelve la especialidad creada (con su id), o 400/409 si los datos no son válidos o el nombre ya existe. 
* **\*\*PUT\*\* `/api/specialties/{id}`:** Actualiza el nombre o descripción de una especialidad existente. Mismas validaciones que el registro (name: entre 3 y 100 caracteres, description: entre 10 y 100). Devuelve la especialidad actualizada, o 404/400/409 si no existe, los datos no son válidos, o el nuevo nombre ya está en uso.  
* **\*\*DELETE\*\* `/api/specialties/{id}`:** Realiza un borrado lógico (soft delete) de una especialidad cambiando su estado deleted.



