# Deploy en un VPS con Docker

Guía paso a paso para subir la API a un VPS Linux (Ubuntu/Debian) por SSH.
Reemplazá **`USUARIO`** y **`IP_DEL_VPS`** por tus datos.

> **Si tu SSH usa otro puerto** (por ejemplo 2222): en `ssh` se indica con **`-p 2222`** (minúscula)
> y en `scp` con **`-P 2222`** (mayúscula). Ej.: `ssh -p 2222 USUARIO@IP_DEL_VPS` y
> `scp -P 2222 archivo USUARIO@IP_DEL_VPS:~`

```
Tu PC (Windows)                          VPS (Linux)
──────────────                           ─────────────────────────────────────
 empaquetar  ──── scp ──────────────►    ~/udiayudaprofe/
                                           docker compose up -d --build
                                             ├── contenedor "api"  :5000  ◄── internet
                                             └── contenedor "db"   (privado)
                                                   └── volumen pgdata (los datos)
```

---

## Paso 1 · Entrar al VPS (desde PowerShell en tu PC)
```bash
ssh USUARIO@IP_DEL_VPS
```
Ver qué sistema tiene y si Docker ya está instalado:
```bash
cat /etc/os-release | head -2
docker --version
docker compose version
```

## Paso 2 · Instalar Docker (solo si no está)
```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER
exit
```
Volvé a entrar con `ssh USUARIO@IP_DEL_VPS` (para que tome el grupo `docker`) y comprobá:
```bash
docker run --rm hello-world
```

## Paso 3 · Empaquetar el proyecto (en tu PC, PowerShell)
Parado en la carpeta del proyecto. Deja afuera `bin`, `obj` y los archivos con tu contraseña local:
```bash
cd C:\Users\MARCELO\Desktop\UDI2\UdiAyudaProfe
tar --exclude=bin --exclude=obj --exclude=.vscode --exclude=.env --exclude=appsettings.Development.json -czf ..\udiayudaprofe.tar.gz .
```

## Paso 4 · Subirlo al VPS (en tu PC)
```bash
scp ..\udiayudaprofe.tar.gz USUARIO@IP_DEL_VPS:~
```

## Paso 5 · Descomprimir (en el VPS)
```bash
mkdir -p ~/udiayudaprofe
tar -xzf ~/udiayudaprofe.tar.gz -C ~/udiayudaprofe
cd ~/udiayudaprofe
ls
```

## Paso 6 · Crear el `.env` con una contraseña segura (en el VPS)
```bash
cp .env.example .env
sed -i "s/^DB_PASSWORD=.*/DB_PASSWORD=$(openssl rand -hex 24)/" .env
cat .env
```
La contraseña se genera sola y queda guardada en `.env`. No hace falta recordarla.

## Paso 7 · Levantar todo (en el VPS)
```bash
docker compose up -d --build
```
La primera vez tarda unos minutos (descarga .NET y PostgreSQL). Después:
```bash
docker compose ps
docker compose logs api
```
En los logs tiene que aparecer `Applying migration '..._InicialConSeed'` y `Now listening on: http://[::]:5000`.

## Paso 8 · Probar
En el VPS:
```bash
curl http://localhost:5000/health
curl http://localhost:5000/cursos
```
Desde tu PC (navegador): `http://IP_DEL_VPS:5000/estudiantes`

Si desde tu PC no responde, el proveedor del VPS puede tener un firewall en su panel web:
habilitá ahí el puerto **5000/TCP**.

---

## Comandos del día a día (en el VPS, dentro de `~/udiayudaprofe`)

| Para… | Comando |
|---|---|
| Ver estado | `docker compose ps` |
| Ver logs en vivo | `docker compose logs -f api` (salir con Ctrl+C) |
| Reiniciar la API | `docker compose restart api` |
| Apagar todo (los datos se conservan) | `docker compose down` |
| Volver a encender | `docker compose up -d` |
| Entrar a la base con psql | `docker compose exec db psql -U udi -d udiayudaprofe` |
| Backup de la base | `docker compose exec db pg_dump -U udi udiayudaprofe > backup_$(date +%F).sql` |

## Actualizar a una versión nueva
1. En tu PC repetí los **pasos 3 y 4**.
2. En el VPS:
```bash
cd ~/udiayudaprofe
tar -xzf ~/udiayudaprofe.tar.gz -C ~/udiayudaprofe
docker compose up -d --build
```
El `.env` y los datos de la base se conservan. Si agregaste una migración nueva, se aplica sola al arrancar.

---

## ⚠️ Cuidado
- **`docker compose down -v` BORRA la base de datos** (la `-v` elimina el volumen). No la uses salvo que quieras empezar de cero.
- **No compartas el archivo `.env`**: tiene la contraseña de la base.
- **Docker abre el puerto 5000 aunque tengas `ufw`**: Docker escribe sus propias reglas de firewall y `ufw` no las bloquea. Si no querés exponer la API, cambiá en `docker-compose.yml` la línea de `ports` por `"127.0.0.1:5000:5000"`.
- La base de datos **no** tiene puerto publicado a propósito: desde internet nadie puede conectarse a ella.
- La API responde por **HTTP** (sin candado). Para HTTPS con dominio propio el siguiente paso es poner un proxy como **Caddy** o **Nginx** delante.
