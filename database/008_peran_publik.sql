-- ROLE-01 (keputusan K9/K10, 2026-11-22):
--   * Admin dan Analis digabung menjadi Admin (hak aksesnya sama).
--   * Peran baru Publik untuk warga yang mendaftar sendiri.
--   Tamu tidak punya baris di tabel users, jadi tidak masuk CHECK.
BEGIN;

UPDATE users SET role = 'Admin' WHERE role = 'Analis';

ALTER TABLE users DROP CONSTRAINT users_role_check;
ALTER TABLE users ADD CONSTRAINT users_role_check
    CHECK (role IN ('Admin', 'Operator', 'Publik'));

COMMIT;
