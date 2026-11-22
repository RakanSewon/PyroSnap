-- Login dan daftar mandiri lewat fungsi database, supaya user aplikasi tidak perlu
-- (dan tidak bisa) membaca kolom password_hash atau membuat akun selain Publik.
-- Penting bila database online: kredensial aplikasi ikut terkemas di .exe.
BEGIN;

CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Mengembalikan satu baris bila username + password benar; kosong bila salah.
CREATE OR REPLACE FUNCTION public.masuk_akun(p_username text, p_password text)
RETURNS TABLE (user_id integer, username varchar, role varchar)
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public, extensions, pg_temp
AS $$
#variable_conflict use_column
DECLARE
    akun public.users%ROWTYPE;
    hash text;
BEGIN
    IF p_username IS NULL OR p_password IS NULL THEN
        RETURN;
    END IF;

    SELECT u.* INTO akun FROM public.users u WHERE u.username = p_username;

    -- Username tak dikenal tetap menjalankan BCrypt agar waktunya tidak membocorkan akun.
    hash := COALESCE(akun.password_hash, gen_salt('bf', 11));
    IF crypt(p_password, hash) = hash AND akun.user_id IS NOT NULL THEN
        RETURN QUERY SELECT akun.user_id, akun.username, akun.role;
    END IF;
END $$;

-- Daftar mandiri warga: peran selalu Publik, tidak bisa dipilih pemanggil.
CREATE OR REPLACE FUNCTION public.daftar_publik(p_username text, p_password text)
RETURNS integer
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = pg_catalog, public, extensions, pg_temp
AS $$
DECLARE
    baru integer;
BEGIN
    IF p_username IS NULL OR p_username !~ '^[A-Za-z0-9_.]{3,30}$' THEN
        RAISE EXCEPTION 'Username 3-30 karakter: huruf, angka, titik, atau garis bawah.' USING ERRCODE = '22023';
    END IF;
    IF p_password IS NULL OR char_length(p_password) < 8 OR octet_length(p_password) > 72 THEN
        RAISE EXCEPTION 'Password harus 8 karakter atau lebih (maksimal 72 byte).' USING ERRCODE = '22023';
    END IF;

    INSERT INTO public.users (username, password_hash, role)
    VALUES (p_username, crypt(p_password, gen_salt('bf', 11)), 'Publik')
    RETURNING users.user_id INTO baru;
    RETURN baru;
END $$;

REVOKE ALL ON FUNCTION public.masuk_akun(text, text) FROM PUBLIC;
REVOKE ALL ON FUNCTION public.daftar_publik(text, text) FROM PUBLIC;

-- Database lokal yang sudah punya user aplikasi.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'pyrosnap_app') THEN
        GRANT EXECUTE ON FUNCTION public.masuk_akun(text, text) TO pyrosnap_app;
        GRANT EXECUTE ON FUNCTION public.daftar_publik(text, text) TO pyrosnap_app;
    END IF;
END $$;

COMMIT;
