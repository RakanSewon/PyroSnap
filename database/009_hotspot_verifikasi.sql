-- OPS-01 (ADD-10, 2026-11-22): verifikasi lapangan oleh Operator/Admin.
-- Satu hotspot bisa diverifikasi berkali-kali; yang ditampilkan adalah yang terbaru.
BEGIN;

CREATE TABLE hotspot_verifikasi (
    verifikasi_id   SERIAL PRIMARY KEY,
    hotspot_id      BIGINT NOT NULL
                        REFERENCES hotspot (hotspot_id)
                        ON DELETE CASCADE,   -- verifikasi ikut hilang bila hotspot dihapus
    user_id         INTEGER
                        REFERENCES users (user_id)
                        ON DELETE SET NULL,  -- riwayat tetap ada walau akun pemeriksa dihapus
    status          VARCHAR(20) NOT NULL
                        CHECK (status IN ('api_aktif', 'sudah_padam', 'bukan_kebakaran')),
    catatan         VARCHAR(500),
    waktu           TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_hotspot_verifikasi_hotspot ON hotspot_verifikasi (hotspot_id, waktu DESC);

COMMENT ON TABLE hotspot_verifikasi IS 'Hasil cek lapangan atas hotspot (api aktif / sudah padam / bukan kebakaran)';

-- Beri hak yang sama dengan tabel users kepada user aplikasi yang sudah ada
-- (nama user aplikasi bisa berbeda di tiap mesin, jadi tidak ditulis langsung).
DO $$
DECLARE
    pengguna text;
BEGIN
    FOR pengguna IN
        SELECT DISTINCT grantee FROM information_schema.role_table_grants
        WHERE table_name = 'users' AND privilege_type = 'INSERT' AND grantee <> current_user
    LOOP
        EXECUTE format('GRANT SELECT, INSERT, UPDATE, DELETE ON hotspot_verifikasi TO %I', pengguna);
        EXECUTE format('GRANT USAGE ON SEQUENCE hotspot_verifikasi_verifikasi_id_seq TO %I', pengguna);
    END LOOP;
END $$;

COMMIT;
