-- SCH-08: satu alert per (hotspot, penerima); sinkronisasi ulang tidak menggandakan alert.
BEGIN;

ALTER TABLE alert
    ADD CONSTRAINT uq_alert_hotspot_user UNIQUE (hotspot_id, target_user_id);

COMMIT;
