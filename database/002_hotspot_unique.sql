-- SCH-01: satu hotspot unik per (sumber, posisi, waktu akuisisi).
-- Syarat upsert "INSERT ... ON CONFLICT DO NOTHING" saat sinkronisasi berulang (Fase 4).
BEGIN;

ALTER TABLE hotspot
    ADD CONSTRAINT uq_hotspot_source_pos_time
    UNIQUE (source_id, latitude, longitude, waktu_akuisisi);

COMMIT;
