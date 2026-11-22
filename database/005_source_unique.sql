-- SCH-06: nama sumber data unik, supaya seed dan pencarian sumber idempoten.
BEGIN;

ALTER TABLE hotspot_data_source
    ADD CONSTRAINT uq_source_nama UNIQUE (nama_sumber);

COMMIT;
