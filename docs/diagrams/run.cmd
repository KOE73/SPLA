@echo off
cd /d "%~dp0"
title SPLA Architecture Visualizer
echo Starting Go server for SPLA Visualizer on http://localhost:8777...
go run server.go
pause
