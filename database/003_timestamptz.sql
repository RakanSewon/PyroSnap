-- SCH-02: semua kolom waktu menjadi TIMESTAMPTZ.
-- Data NASA FIRMS dalam UTC; nilai lama (TIMESTAMP tanpa zona) dianggap UTC.
BEGIN;

ALTER TABLE hotspot
    ALTER COLUMN waktu_akuisisi TYPE TIMESTAMPTZ USING waktu_akuisisi AT TIME ZONE 'UTC';

ALTER TABLE hotspot_data_source
    ALTER COLUMN waktu_sinkronisasi TYPE TIMESTAMPTZ USING waktu_sinkronisasi AT TIME ZONE 'UTC';

ALTER TABLE hotspot_report
    ALTER COLUMN tanggal_dibuat TYPE TIMESTAMPTZ USING tanggal_dibuat AT TIME ZONE 'UTC';

ALTER TABLE alert
    ALTER COLUMN "timestamp" TYPE TIMESTAMPTZ USING "timestamp" AT TIME ZONE 'UTC';

ALTER TABLE filter_criteria
    ALTER COLUMN waktu_mulai TYPE TIMESTAMPTZ USING waktu_mulai AT TIME ZONE 'UTC',
    ALTER COLUMN waktu_akhir TYPE TIMESTAMPTZ USING waktu_akhir AT TIME ZONE 'UTC';

COMMIT;
