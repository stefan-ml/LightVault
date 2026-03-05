# 🔐 LightVault — Secure Secret Management System

LightVault je siguran, API-first sistem za upravljanje tajnama sa fokusom na secure software design. Sistem omogućava enkriptovano skladištenje tajni, versioning i rotation tajni, role-based access control, kao i tamper-evident audit logging.

---

## 🎯 Ciljevi projekta

- Centralizovano upravljanje osetljivim konfiguracionim vrednostima (passwords, API keys, tokens)
- Enkripcija tajni (bez čuvanja plaintext vrednosti)
- Versioning tajni sa kontrolisanom rotation logikom
- Role-Based Access Control (RBAC) za korisnike (Admin, Developer i Auditor)
- Tamper-evident audit logging radi obezbeđivanja redosleda i integriteta sigurnosnih događaja
- API-first dizajn sa Web UI i CLI klijentima

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
  - Audit zapisi se kreiraju za sve sigurnosno kritične akcije (npr. create/update/rotate)
  - Implementiran **tamper-evident audit log** korišćenjem hash-chain pristupa (svaki zapis je povezan sa prethodnim)

---

## 🖥️ Frontend (Web Application)

### Tehnologije
- **.NET 9 Blazor Web App (Interactive Server)**
- **Bootstrap 5**
- **Blazored.LocalStorage**

### Ključne funkcionalnosti
- Autentifikacija korisnika
- Prikaz liste tajni i detalja pojedinačne tajne
- Kreiranje tajni i rotation tokovi
- Administracija korisnika (kreiranje, izmena, upravljanje rolama)
- Prikaz audit logova
- Token-based session management 
- Kreiranje i birsanje novih korisnika

---

## 🔐 Security Highlights

- **Encryption at Rest:** Tajne se nikada ne čuvaju u plaintext obliku.
- **RBAC:** Pristup i operacije su ograničeni na osnovu korisničkih rola.
- **Versioning & Rotation:** Omogućeno bezbedno ažuriranje tajni tokom vremena.
- **Tamper-Evident Audit:** Integritet audit logova je zaštićen korišćenjem hash-chain mehanizma.

