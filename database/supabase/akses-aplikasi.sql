-- Khusus Supabase. Jalankan sebagai pemilik (postgres) setelah migrasi 001-010
-- dan setelah role pyrosnap_app dibuat. Aman dijalankan berulang. Tanpa rahasia.
BEGIN;

-- 1. Tutup akses peran publik Supabase (Data API) ke semua objek PyroSnap.
REVOKE ALL ON ALL TABLES    IN SCHEMA public FROM anon, authenticated;
REVOKE ALL ON ALL SEQUENCES IN SCHEMA public FROM anon, authenticated;
REVOKE ALL ON ALL ROUTINES  IN SCHEMA public FROM PUBLIC, anon, authenticated;
REVOKE CREATE ON SCHEMA public FROM PUBLIC, anon, authenticated;

-- Objek baru buatan postgres juga tidak otomatis terbuka.
ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public REVOKE ALL ON TABLES    FROM anon, authenticated;
ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public REVOKE ALL ON SEQUENCES FROM anon, authenticated;
ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public REVOKE ALL ON ROUTINES  FROM PUBLIC, anon, authenticated;

-- 2. Hak minimum user aplikasi (sesuai query di PyroSnap/Services/*Repository.cs).
REVOKE ALL ON ALL TABLES    IN SCHEMA public FROM pyrosnap_app;
REVOKE ALL ON ALL SEQUENCES IN SCHEMA public FROM pyrosnap_app;
GRANT USAGE ON SCHEMA public TO pyrosnap_app;

GRANT SELECT (user_id, username, role) ON users TO pyrosnap_app;   -- tanpa password_hash
GRANT SELECT, INSERT                   ON hotspot_data_source TO pyrosnap_app;
GRANT UPDATE (waktu_sinkronisasi)      ON hotspot_data_source TO pyrosnap_app;
GRANT SELECT, INSERT                   ON hotspot TO pyrosnap_app;
GRANT SELECT, INSERT                   ON alert TO pyrosnap_app;
GRANT UPDATE (is_read)                 ON alert TO pyrosnap_app;
GRANT SELECT, INSERT, DELETE           ON filter_criteria TO pyrosnap_app;
GRANT SELECT, INSERT                   ON hotspot_verifikasi TO pyrosnap_app;
GRANT SELECT, INSERT                   ON hotspot_report TO pyrosnap_app;
GRANT SELECT, INSERT                   ON report_hotspot TO pyrosnap_app;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO pyrosnap_app;

GRANT EXECUTE ON FUNCTION public.masuk_akun(text, text)    TO pyrosnap_app;
GRANT EXECUTE ON FUNCTION public.daftar_publik(text, text) TO pyrosnap_app;

COMMIT;
