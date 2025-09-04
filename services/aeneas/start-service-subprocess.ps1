# Start Aeneas service using subprocess approach (no virtual environment)
#
# This script demonstrates how to run the Aeneas service using a subprocess
# approach instead of managing Python virtual environments.
#
# Prerequisites:
# 1. Install Aeneas on your system Python or specify path via AENEAS_PYTHON
# 2. Ensure the Python with Aeneas is in your PATH or set AENEAS_PYTHON

Write-Host "Starting Aeneas Alignment Service (Subprocess Mode)" -ForegroundColor Green

# Option 1: Use system Python (requires Aeneas to be installed globally)
# python -m app.main

# Option 2: Specify custom Python with Aeneas installed
# $env:AENEAS_PYTHON = "C:\Python39\python.exe"  # Example path
# python -m app.main

# Option 3: Use conda environment with Aeneas
# $env:AENEAS_PYTHON = "C:\Users\YourUser\miniconda3\envs\aeneas\python.exe"
# python -m app.main

Write-Host "Service configuration:"
Write-Host "- Current directory: $PWD"
Write-Host "- Python executable: $(if ($env:AENEAS_PYTHON) { $env:AENEAS_PYTHON } else { 'system default' })"
Write-Host "- Service will auto-detect Python with Aeneas installed"
Write-Host ""
Write-Host "Starting service on http://localhost:8082"
Write-Host "Press Ctrl+C to stop"
Write-Host ""

# Start the service
python -m app.main