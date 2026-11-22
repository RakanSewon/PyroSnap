-- Contoh seed akun uji (ACC-A3). JANGAN isi password di berkas ini.
--
-- Cara pakai:
--   1. cp database/seed-local.example.sql database/seed-local.sql
--      (seed-local.sql sudah di-gitignore, tidak ikut ter-commit)
--   2. Ganti setiap <password-...> di seed-local.sql dengan password uji pilihanmu
--      (minimal 8 karakter). Simpan password itu di pengelola password, bukan di repo.
--   3. Jalankan sebagai postgres (butuh hak membuat ekstensi):
--        cat database/seed-local.sql | sudo -u postgres psql -v ON_ERROR_STOP=1 -d pyrosnap
--   4. Hapus seed-local.sql setelah dipakai.
--
-- Password di-hash oleh PostgreSQL (pgcrypto, BCrypt cost 11) sehingga kolom
-- password_hash hanya berisi hash; aplikasi memverifikasinya dengan BCrypt.Net.
-- Akun yang sudah ada tidak ditimpa. Seed dibatalkan bila masih ada penanda <password-...>.
--
-- Membersihkan akun uji:
--   DELETE FROM users WHERE username LIKE '%\_uji';

BEGIN;

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TEMP TABLE akun_uji (username text, pw text, role text) ON COMMIT DROP;

INSERT INTO akun_uji (username, pw, role) VALUES
    ('admin_uji',    '<password-admin>',    'Admin'),
    ('analis_uji',   '<password-analis>',   'Analis'),
    ('operator_uji', '<password-operator>', 'Operator');

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM akun_uji WHERE pw LIKE '<password-%' OR length(pw) < 8) THEN
        RAISE EXCEPTION 'Isi semua password uji (minimal 8 karakter) sebelum menjalankan seed.';
    END IF;
END $$;

INSERT INTO users (username, password_hash, role)
SELECT username, crypt(pw, gen_salt('bf', 11)), role FROM akun_uji
ON CONFLICT (username) DO NOTHING;

COMMIT;
