-- SET-03: sumber data bawaan. Idempoten (butuh 005_source_unique.sql).
-- {KEY} hanya penanda: key NASA FIRMS asli TIDAK disimpan di database,
-- aplikasi mengambilnya dari FIRMS_MAP_KEY (.env).
-- Nama sumber sama dengan HotspotDataSource.NasaFirms(..., "VIIRS_NOAA20_NRT").
BEGIN;

INSERT INTO hotspot_data_source (nama_sumber, endpoint_url)
VALUES ('NASA FIRMS (VIIRS_NOAA20_NRT)',
        'https://firms.modaps.eosdis.nasa.gov/api/area/csv/{KEY}/VIIRS_NOAA20_NRT/95,-11,141,6/1')
ON CONFLICT (nama_sumber) DO NOTHING;

COMMIT;
