# 🔐 LightVault — Secure Secret Management System

LightVault je siguran, API-first sistem za upravljanje tajnama sa fokusom na secure software design. Sistem omogućava enkriptovano skladištenje tajni, versioning i rotation tajni, role-based access control, kao i tamper-evident audit logging.

---

## 🎯 Ciljevi projekta

- Centralizovano upravljanje osetljivim konfiguracionim vrednostima
- Enkripcija tajni 
- Versioning tajni sa kontrolisanom rotation logikom
- Role-Based Access Control (RBAC) za korisnike (Admin, Developer i Auditor)
- Tamper-evident audit logging radi obezbeđivanja redosleda i integriteta sigurnosnih događaja

---

## 🧱 Pregled rešenja

Rešenje se sastoji od:

- **Backend (API + data access)**
- **Web Application (UI)**
- **CLI Client**
- **Domain i Infrastructure slojevi**

---

## 🧠 Backend

### Tehnologije
- **.NET 9**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **Microsoft SQL Server**
- **JWT Bearer Authentication**
- **Cryptography (AES + HMAC/SHA-256)**
- **Clean Architecture**

### Ključne funkcionalnosti
- **Authentication & Authorization**
  - JWT-based login
  - Role-based authorization (RBAC)

- **Secrets Management**
  - Kreiranje i čitanje tajni kroz autentifikovani API
  - Tajne se skladište isključivo u **enkriptovanom** obliku (ciphertext + nonce + tag)
  - Podrška za secret **versioning** (istorija verzija se čuva)
  - Podrška za **rotation** (nova verzija postaje aktivna)

- **Audit Logging**
  - Audit zapisi se kreiraju za sve operacije nad tajnama
  - Implementiran **tamper-evident audit log** korišćenjem hash-chain pristupa

---

## 🖥️ Frontend (Web Application)

### Tehnologije
- **.NET 9 Blazor Web App (Interactive Server)**
- **Bootstrap 5**
- **Blazored.LocalStorage**

### Ključne funkcionalnosti
- Autentifikacija korisnika
- Prikaz liste tajni i detalja pojedinačne tajne
- Kreiranje tajni i rotation
- Administracija korisnika 
- Prikaz audit logova
---

## 🚀 Pokretanje projekta

- Pokretanje MSSQL servera lokalno u docker-u: 

```
docker run -e "ACCEPT_EULA=Y" \
-e "SA_PASSWORD=YourStrong!Passw0rd" \
-p 1433:1433 \
--name lightvault-sql \
-d mcr.microsoft.com/mssql/server:2022-latest
```

- Pokrenuti migracije:

```dotnet ef database update```

- I pokretanje projekta: 

```dotnet run```
