#!/usr/bin/env bash
# Jalankan skrip SQL bernomor secara berurutan pada database PyroSnap.
#
# Pemakaian:
#   database/migrate.sh <dari-nomor> [opsi psql...]
#
# Contoh:
#   database/migrate.sh 001 -h localhost -U postgres -d pyrosnap   # database kosong (tabel belum ada)
#   database/migrate.sh 002 -h localhost -U postgres -d pyrosnap   # database yang sudah berisi 001
#
# Jalankan sebagai pemilik tabel (mis. postgres), bukan user aplikasi.
# 000_create_database.sql tidak ikut; buat database terpisah (createdb pyrosnap).
# Setiap berkas hanya boleh dijalankan sekali; berhenti di error pertama.
set -euo pipefail

if [[ $# -lt 1 || ! "$1" =~ ^[0-9]{3}$ ]]; then
    echo "Pemakaian: $0 <dari-nomor, mis. 001 atau 002> [opsi psql...]" >&2
    exit 1
fi

dari="$1"
shift
dir="$(cd "$(dirname "$0")" && pwd)"

for file in "$dir"/[0-9][0-9][0-9]_*.sql; do
    nomor="$(basename "$file" | cut -c1-3)"
    [[ "$nomor" == "000" || "$nomor" < "$dari" ]] && continue
    echo "==> $(basename "$file")"
    psql -v ON_ERROR_STOP=1 -q "$@" -f "$file"
done

echo "Selesai."
