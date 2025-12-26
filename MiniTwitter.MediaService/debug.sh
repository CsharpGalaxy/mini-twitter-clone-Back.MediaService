#!/bin/bash

echo "🔍 بررسی وضعیت سرویس‌ها..."
echo ""

echo "📋 SQL Server:"
docker logs media-mssql | tail -5
echo ""

echo "📋 MinIO:"
docker logs media-minio | tail -5
echo ""

echo "📋 MediaService:"
docker logs media-service-api | tail -10
echo ""

echo "🌐 تست اتصال MinIO:"
curl -v http://localhost:8000/minio/health/live
echo ""

echo "✅ تمام سرویس‌ها آماده است"
