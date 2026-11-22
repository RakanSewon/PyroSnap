-- SCH-03 / BUG-11: aplikasi mengekspor TXT, jadi format TXT harus diterima.
BEGIN;

ALTER TABLE hotspot_report DROP CONSTRAINT hotspot_report_format_check;
ALTER TABLE hotspot_report ADD CONSTRAINT hotspot_report_format_check
    CHECK (format IN ('TXT', 'CSV', 'PDF', 'XLSX'));

COMMIT;
