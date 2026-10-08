# Product Specification Lookup

A fast internal tool for QC staff to look up product specifications.
The workflow is **Open → Search → Check → Done**.

- **No login to search or view.** The first screen is the search box.
- **Login (user ID + password) is required to change anything**: add or edit products, edit specifications, upload images or drawings, import Excel/CSV, archive or restore products, and manage users. This is enforced by the server, not just by hiding buttons.
- Works on PC, laptop and phone, and can be installed to the phone home screen (PWA).

This is a standalone application that lives in the Berex Tech QMS repository. It does not need the QMS to run. See [Future integration with Berex Tech QMS](#future-integration-with-berex-tech-qms).

---

## Features

| Area | What it does |
|---|---|
| Search | Exact product code is ranked first. Also matches name, description, size, variant, model, material, group, supplier, notes and specification values. Ignores case, extra spaces and punctuation (`2mlt 3101a` finds `2MLT3101A`; `1880 x 1810` finds `1,810mm x 1,880mm`). Falls back to similar matches for typos. |
| Voice search | Microphone button next to the search box. Spoken text is turned into a normal search, and command words such as "show me" are removed. If the browser has no speech recognition, it shows *Voice search is not supported on this browser.* |
| Product page | Product information, specification (name / value / unit / tolerance), inspection requirements, packaging requirements, supplier information, images and drawings, notes, and revision history. Missing values show **Not specified**. On phones, tables become stacked cards. A print layout is included. |
| Editing | One form for all fields. Specification, inspection, packaging and supplier rows can be added, edited, deleted and reordered. |
| Revision control | Every saved change creates a new revision (Rev. 01, Rev. 02, …) that records who, when, a summary, and each changed field with its previous and new value. If two people edit at once, the second save is refused instead of silently overwriting. Admins can restore an earlier revision; this is saved as a new revision. |
| Duplicate codes | Product codes are unique, ignoring case. A duplicate is never overwritten: *Product code already exists.* with **View Product / Edit Existing Product / Cancel**. |
| Images and drawings | JPEG, PNG, WebP, GIF and PDF, up to 10 MB each (5 MB on the free setup). Large product photos are shrunk in the browser before upload (longest side 2000 px); drawings are uploaded unchanged. The file type is checked from the file content, not the file name. Files are stored in the database, so nothing is lost on redeploy. |
| Excel / CSV import | Upload, review a preview (total rows, new products, existing codes, missing codes, invalid rows, merged rows, column mapping), then choose **Skip** or **Update existing** for existing codes, or **Cancel**. Nothing is written before you confirm. |
| Archive | Products are archived, not deleted. Archived products leave normal search, and logged-in users can view and restore them. |
| Audit log | Every change is logged, for example `QC01 updated 2MLT3101A: Width from 1800 mm to 1810 mm. New revision Rev. 03.` |
| Users | Roles: **Admin** and **QC User**. Import permission is a per-user option for QC users. Admins manage users and settings, including whether the public can see revision history. |
| Recent searches | Kept on each device (browser storage). This is only a convenience; all product data is in PostgreSQL. |

## Data rules

- Values are stored **exactly as entered or imported**. The only change is trimming spaces at the start and end. Codes, units, decimals, tolerances, supplier names and terminology are never changed.
- Nothing is invented. Empty values display as **Not specified**.
- Search uses a separate hidden index that is normalized (case, digit commas, punctuation). The displayed data is not touched.

### Importing the "Incoming Inspection 2026" material list

The importer reads that workbook's **Material List** sheet directly. The **Forecast** and **AQL** sheets are not imported.

| Excel column | Stored as |
|---|---|
| Item Code | Product Code |
| Material Name | Product Name |
| Description | Description |
| Group | Group |
| Applicated | Model / Application |
| Unit | Unit |
| Supplier1 / Supplier2 / Supplie3 | Suppliers (each distinct name kept, exact spelling) |
| No. | not imported |

Result for the current file: **211 rows → 169 products.**

- **34 rows repeat a product code that appears earlier.** They are combined into one product, and every supplier is kept (for example, `2MLT3101A` has suppliers *Maus* and *LSK*). Where a repeated row has a different value (another Applicated, Unit, name or description), that row is copied word-for-word into the product's **Notes** as `Source row N — …`, so no information is lost. Some repeated codes have conflicting descriptions in the source file (for example, `2MTH0010B` appears as both *Thread Penguin (White)* and *40/2 Cotton Thread (White)*). Please check these and correct them in the app.
- **8 rows are not imported because they contain only a code:**
  - Six codes have no product name anywhere in the file: `3GCM0001A`, `2MLB0201A/B`, `2MLB0202A/B` (two rows), `2VBF0008B`, `2VBF0008A`.
  - Two rows repeat a code that has full data on another row (`2MTH0007A`, `2MTH0009A`) and add nothing.

  Add the missing ones with **+ Add Product**, or fill in the names in Excel and import again.
- Columns with any other heading are imported as specification fields, using the heading as the specification name.

## Using it

1. Open the site and type a code, name or keyword, or tap the microphone. Press Enter on an exact code to go straight to the product.
2. To change something, click **Edit Product** or **+ Add Product** and log in.
3. Save. A new revision is created and the change is searchable straight away.

Install on a phone: in Chrome (Android) use *menu → Add to Home screen / Install app*; in Safari (iPhone) use *Share → Add to Home Screen*.

---

## Technology

| Part | Choice |
|---|---|
| Server | Node.js 22, Fastify 5, TypeScript |
| Database | PostgreSQL 13+ (16 recommended) with `pg_trgm` for fast keyword search, or the built-in PGlite database (PostgreSQL in WebAssembly) for single-PC installs |
| Web app | React 18, TypeScript, Vite, TanStack Query, plain CSS |
| Auth | User ID + password (scrypt hashing), server-side sessions in an HttpOnly cookie, role checks on every write endpoint |

One container serves both the API and the web app, so there is one URL to deploy.

```
spec-lookup/
  server/           API (src/routes, src/products.ts, src/search.ts, src/importer.ts, src/migrations.ts)
  server/test/      API integration tests (needs PostgreSQL)
  web/              React PWA (src/pages, src/components, public/sw.js, public/manifest.webmanifest)
  Dockerfile        Production image (API + web)
  docker-compose.yml  Local / on-premise run with PostgreSQL
  render.yaml       Render.com blueprint (free plan, uses a Neon database)
  .env.example      All environment variables
```

## Environment variables

See [`.env.example`](.env.example). The main ones are:

| Variable | Purpose |
|---|---|
| `DATABASE_URL` | PostgreSQL connection string. If not set, the built-in database in `DATA_DIR` (default `spec-lookup/data`) is used. For Neon, use the direct (non-pooled) connection string. |
| `DB_SCHEMA` | Schema for all tables (default `spec_lookup`). Lets the app share a database with the QMS. |
| `DATABASE_SSL` | `false`, `true` (verified TLS) or `no-verify`. |
| `ADMIN_USER_ID` / `ADMIN_PASSWORD` | First administrator, created only when there are no users. If the password is empty, a one-time password is printed in the server log. |
| `COOKIE_SECURE` | Defaults to `true` in production (HTTPS). Set `false` only for plain-http use on a local network. |
| `SESSION_TTL_HOURS`, `MAX_FILE_MB`, `MAX_IMPORT_MB` | Session length and upload limits. |

Tables are created automatically on startup (migrations run in order and are safe to re-run).

## Local development

Requirements: Node.js 22 and a PostgreSQL database.

```bash
# 1. API (http://localhost:8080)
cd spec-lookup/server
npm ci
DATABASE_URL=postgres://user:pass@localhost:5432/spec_lookup ADMIN_PASSWORD='choose-a-password' npm run dev

# 2. Web app with hot reload (http://localhost:5174, proxies /api to :8080)
cd spec-lookup/web
npm ci
npm run dev
```

Tests and checks:

```bash
cd spec-lookup/server
npm run typecheck
TEST_DATABASE_URL=postgres://user:pass@localhost:5432/spec_test npm test   # uses a temporary schema, then drops it
TEST_EMBEDDED=1 npm test                                                   # same tests on the built-in database

cd ../web
npm run build        # type check + production build
```

Create or reset a user from the command line (for example, if the admin password is lost):

```bash
cd spec-lookup/server && npm run build
NEW_PASSWORD='new-password' DATABASE_URL=... npm run create-user -- admin admin
NEW_PASSWORD='qc-password'  DATABASE_URL=... npm run create-user -- QC01 qc --import
```

## Deployment

### Option A — Free online website (Render + Neon, no cost)

This gives a public `https://…onrender.com` address that works on office PCs, laptops and phones, with no credit card needed. It uses two free services:

- **Neon** (neon.tech) — free PostgreSQL database. It does not expire and has 0.5 GB of storage.
- **Render** (render.com) — free web hosting for the app.

**Step 1 — Create the free database (Neon)**
1. Sign up at <https://neon.tech> (you can use your GitHub account) and create a project, for example `spec-lookup`. Choose the region closest to you, such as *AWS Asia Pacific (Singapore)*.
2. On the project dashboard, click **Connect**. Turn **Connection pooling off**, so the host name does *not* contain `-pooler`.
3. Copy the connection string. It looks like `postgresql://neondb_owner:xxxx@ep-xxxx.ap-southeast-1.aws.neon.tech/neondb?sslmode=require`.

**Step 2 — Create the free website (Render)**
1. Sign up at <https://render.com> with your GitHub account and allow it to access the `berex-tech-qms` repository.
2. Click **New → Blueprint**, select the repository, set **Blueprint file path** to `spec-lookup/render.yaml`, and pick the branch that contains `spec-lookup/` (`main` once it is merged).
3. When Render asks for `DATABASE_URL`, paste the Neon connection string from Step 1. Click **Apply**.
4. Wait for the first build to finish (about 5–10 minutes). The address appears at the top of the service page, for example `https://spec-lookup.onrender.com`.

**Step 3 — First login and data**
1. On the Render service, open **Environment** and copy the value of `ADMIN_PASSWORD`.
2. Open the website, click **Log in**, and use user ID `admin` with that password. Then click your user name (top right) and change the password.
3. Go to **Import**, upload `1. Incoming Inspection 2026.xlsx`, check the preview, and click **Confirm Import**.
4. Under **Admin → Users**, create a login for each QC user.

**What "free" means here**

| Limit | Effect | What to do |
|---|---|---|
| Render free websites sleep after 15 minutes without visitors | The first visit after a quiet period takes about 30–60 seconds; after that it is fast. | Optional: a free monitor such as UptimeRobot can open `https://<your-site>/health` every 10 minutes during working hours to keep it awake. Render's free plan includes 750 hours a month, enough for one site running all month. |
| Neon free storage is 0.5 GB | Product data is small (thousands of products fit easily). Images and drawings use most of the space. | Product photos are shrunk automatically before upload (longest side 2000 px). Drawings are kept as uploaded, up to 5 MB each. Check usage on the Neon dashboard. |
| Free plans have no paid support or uptime guarantee | Fine for an internal lookup tool. | Back up the data now and then: in Neon, use **Branches** or run `pg_dump` with the connection string. |

You can move to paid plans or your own server later without changing the app; only `DATABASE_URL` changes.

### Option B — One office PC, no database to install (simplest)

The app has a built-in database. When `DATABASE_URL` is not set, it keeps all data in the `spec-lookup/data` folder, so there is nothing else to install.

1. On the PC that will host the app, install **Node.js LTS** from <https://nodejs.org> (a normal installer: click Next until it finishes).
2. Download this repository (on GitHub: **Code → Download ZIP**) and unzip it, for example to `C:\SpecLookup`.
3. Double-click `spec-lookup\start-windows.bat`. The first start installs and builds the app, which takes a few minutes and needs internet. If Windows Firewall asks, allow access on **Private networks**.
4. The window shows the addresses, for example `From other PCs/phones: http://192.168.1.20:8080`. Open that address on other PCs and phones connected to the same office network.
5. The first login (user `admin` and a generated password) is in `spec-lookup\data\FIRST-LOGIN.txt`. Log in, change the password, then delete that file.

Notes:

- Keep the window open, and the PC switched on, while people use the app. To start it automatically, put a shortcut to `start-windows.bat` in the Startup folder (press Win+R and type `shell:startup`).
- **Backup:** close the window, then copy the `spec-lookup\data` folder somewhere safe.
- Only one copy of the app may use the `data` folder at a time.
- Over plain `http://`, browsers do not allow the microphone, so voice search does not work on phones. Typed search works normally. Voice search works on the host PC itself at `http://localhost:8080`, and anywhere once the app is served over HTTPS (Option A).
- To use a PostgreSQL server instead, set `DATABASE_URL`.

### Option C — Your own server or office PC (Docker)

```bash
cd spec-lookup
POSTGRES_PASSWORD='strong-db-password' ADMIN_PASSWORD='strong-admin-password' docker compose up -d --build
```

Open `http://<server-ip>:8080` from any PC or phone on the network. The database is kept in the `spec-db` Docker volume; back it up with `docker compose exec db pg_dump -U spec spec_lookup > backup.sql`.

For access from outside the office, put the app behind HTTPS (for example Caddy, nginx or Cloudflare Tunnel) and remove `COOKIE_SECURE=false`. Phone home-screen install (PWA) and voice search require HTTPS, except on `localhost`.

### Option D — Any Docker host or Node.js host

Build the image with `docker build -t spec-lookup spec-lookup/`. Run it with `DATABASE_URL` (and `ADMIN_PASSWORD` for the first start) and expose port 8080. Health check: `GET /health`.

Without Docker: run `npm ci && npm run build` in `web/` and in `server/`, then run `node server/dist/index.js` from `spec-lookup/`.

## Security summary

- Public endpoints are read-only: search, product view, files, and revision history (the history can be turned off for the public in Settings).
- Every write endpoint checks the session and role on the server. Writes from other websites are rejected (Origin check plus SameSite cookies).
- Passwords are hashed with scrypt and a per-user salt. After 5 failed logins in 15 minutes, login is locked for 15 minutes per IP and user ID. Disabling a user or resetting a password ends that user's sessions.
- All SQL is parameterized. Input is validated on the server (lengths, required fields). React escapes all output, and a strict Content-Security-Policy is sent.
- Uploads are checked by their content (magic bytes), limited in size, and SVG and HTML are never accepted. Files are served with `nosniff`, and images with a sandboxing CSP.

## API (for integration)

Public (no login): `GET /api/search?q=`, `GET /api/products`, `GET /api/products/:id`, `GET /api/products/by-code/:code`, `GET /api/products/:id/revisions`, `GET /api/files/:id`.

Login required: `POST /api/products`, `PUT /api/products/:id` (with `expectedRevision`), `POST /api/products/:id/archive` and `/restore`, `POST /api/products/:id/files`, `DELETE /api/products/:id/files/:fileId`, `POST /api/import/preview`, `POST /api/import/:batchId/commit`. Admin only: `/api/users`, `/api/settings`, `POST /api/products/:id/revisions/:revisionId/restore`.

## Future integration with Berex Tech QMS

- All tables live in their own PostgreSQL schema (`DB_SCHEMA`), so the app can share the QMS database without name clashes.
- Products have stable UUIDs and unique product codes, and `GET /api/products/by-code/:code` gives a direct lookup. QMS modules (incoming / WIP / outgoing inspection, NCR, CAPA, supplier quality) can reference products by ID or code.
- Suppliers are a separate table linked to products, ready to map to QMS supplier management.
- Inspection requirements per product are stored as structured check points (specification, tolerance, method) that a future inspection module can use as its checklist.
