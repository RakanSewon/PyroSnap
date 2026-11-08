-- =====================================================================
-- PyroSnap Database — PostgreSQL DDL
-- Implementasi dari ERD Modul 4 (8 entitas: 7 tabel data + 1 tabel junction)
-- Tipe data mengikuti Modul 4 nomor 3, dipetakan ke tipe native PostgreSQL:
--   INT (PK)        -> SERIAL / BIGSERIAL
--   VARCHAR(n)       -> VARCHAR(n)
--   DECIMAL(p,s)     -> NUMERIC(p,s)
--   DATETIME         -> TIMESTAMP
--   TINYINT UNSIGNED -> SMALLINT + CHECK (Postgres tidak punya TINYINT)
--   TINYINT(1)       -> BOOLEAN (Postgres punya tipe boolean native)
-- =====================================================================

BEGIN;

-- ---------------------------------------------------------------------
-- 1. USERS — akun Admin/Analis/Operator
-- ---------------------------------------------------------------------
CREATE TABLE users (
    user_id         SERIAL PRIMARY KEY,
    username        VARCHAR(50)  NOT NULL UNIQUE,
    password_hash   VARCHAR(255) NOT NULL,
    role            VARCHAR(20)  NOT NULL
                        CHECK (role IN ('Admin', 'Analis', 'Operator'))
);

COMMENT ON TABLE users IS 'Akun pengguna sistem PyroSnap';
COMMENT ON COLUMN users.password_hash IS 'Hash bcrypt/Argon2, tidak pernah menyimpan password asli';

-- ---------------------------------------------------------------------
-- 2. HOTSPOT_DATA_SOURCE — sumber data eksternal (NASA FIRMS, BMKG, dst.)
-- ---------------------------------------------------------------------
CREATE TABLE hotspot_data_source (
    source_id           SERIAL PRIMARY KEY,
    nama_sumber          VARCHAR(100) NOT NULL,
    endpoint_url         VARCHAR(255) NOT NULL,
    waktu_sinkronisasi   TIMESTAMP
);

-- ---------------------------------------------------------------------
-- 3. LOCATION — titik referensi wilayah (dipakai sebagai MapCenter)
-- ---------------------------------------------------------------------
CREATE TABLE location (
    location_id     SERIAL PRIMARY KEY,
    nama_lokasi     VARCHAR(100) NOT NULL,
    latitude        NUMERIC(9,6) NOT NULL CHECK (latitude BETWEEN -90 AND 90),
    longitude       NUMERIC(9,6) NOT NULL CHECK (longitude BETWEEN -180 AND 180)
);

-- ---------------------------------------------------------------------
-- 4. FILTER_CRITERIA — kriteria pencarian tersimpan milik satu USER
-- ---------------------------------------------------------------------
CREATE TABLE filter_criteria (
    filter_id        SERIAL PRIMARY KEY,
    user_id          INTEGER NOT NULL
                         REFERENCES users (user_id)
                         ON DELETE CASCADE,   -- preferensi personal, ikut hilang kalau user dihapus
    waktu_mulai      TIMESTAMP,
    waktu_akhir      TIMESTAMP,
    wilayah          VARCHAR(100),
    confidence_min   SMALLINT CHECK (confidence_min BETWEEN 0 AND 100),
    frp_min          NUMERIC(6,2)
);

CREATE INDEX idx_filter_criteria_user_id ON filter_criteria (user_id);

-- ---------------------------------------------------------------------
-- 5. HOTSPOT — data inti titik panas hasil deteksi satelit
-- ---------------------------------------------------------------------
CREATE TABLE hotspot (
    hotspot_id       BIGSERIAL PRIMARY KEY,   -- BIGINT: volume data satelit tumbuh cepat
    latitude         NUMERIC(9,6) NOT NULL CHECK (latitude BETWEEN -90 AND 90),
    longitude        NUMERIC(9,6) NOT NULL CHECK (longitude BETWEEN -180 AND 180),
    confidence       SMALLINT     NOT NULL CHECK (confidence BETWEEN 0 AND 100),
    brightness       NUMERIC(6,2),
    frp              NUMERIC(8,2),
    waktu_akuisisi   TIMESTAMP NOT NULL,
    source_id        INTEGER NOT NULL
                         REFERENCES hotspot_data_source (source_id)
                         ON DELETE RESTRICT,  -- jangan sampai source terhapus selagi masih dipakai
    location_id      INTEGER
                         REFERENCES location (location_id)
                         ON DELETE SET NULL   -- kolom nullable, wilayah boleh dihapus tanpa menghapus hotspot
);

CREATE INDEX idx_hotspot_source_id ON hotspot (source_id);
CREATE INDEX idx_hotspot_location_id ON hotspot (location_id);
CREATE INDEX idx_hotspot_waktu_akuisisi ON hotspot (waktu_akuisisi);

-- ---------------------------------------------------------------------
-- 6. HOTSPOT_REPORT — laporan yang dicetak/diekspor oleh satu USER
-- ---------------------------------------------------------------------
CREATE TABLE hotspot_report (
    report_id        SERIAL PRIMARY KEY,
    user_id          INTEGER NOT NULL
                         REFERENCES users (user_id)
                         ON DELETE RESTRICT,  -- laporan tetap harus punya pembuat yang valid
    format           VARCHAR(10) NOT NULL
                         CHECK (format IN ('PDF', 'CSV', 'XLSX')),
    tanggal_dibuat   TIMESTAMP NOT NULL DEFAULT now(),
    file_path        VARCHAR(255)
);

CREATE INDEX idx_hotspot_report_user_id ON hotspot_report (user_id);

-- ---------------------------------------------------------------------
-- 7. ALERT — notifikasi yang dipicu HOTSPOT, ditujukan ke satu USER
-- ---------------------------------------------------------------------
CREATE TABLE alert (
    alert_id          SERIAL PRIMARY KEY,
    hotspot_id        BIGINT NOT NULL
                          REFERENCES hotspot (hotspot_id)
                          ON DELETE CASCADE,  -- alert terikat ke satu hotspot spesifik
    target_user_id    INTEGER NOT NULL
                          REFERENCES users (user_id)
                          ON DELETE CASCADE,  -- alert untuk user yang sudah dihapus jadi tidak relevan
    message           VARCHAR(255) NOT NULL,
    "timestamp"       TIMESTAMP NOT NULL DEFAULT now(),
    is_read           BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_alert_hotspot_id ON alert (hotspot_id);
CREATE INDEX idx_alert_target_user_id ON alert (target_user_id);

-- ---------------------------------------------------------------------
-- 8. REPORT_HOTSPOT — tabel junction, memecah relasi many-to-many
--    HOTSPOT_REPORT <-> HOTSPOT
-- ---------------------------------------------------------------------
CREATE TABLE report_hotspot (
    report_id    INTEGER NOT NULL
                     REFERENCES hotspot_report (report_id)
                     ON DELETE CASCADE,
    hotspot_id   BIGINT NOT NULL
                     REFERENCES hotspot (hotspot_id)
                     ON DELETE CASCADE,
    PRIMARY KEY (report_id, hotspot_id)
);

CREATE INDEX idx_report_hotspot_hotspot_id ON report_hotspot (hotspot_id);

COMMIT;
