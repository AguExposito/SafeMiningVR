#!/usr/bin/env python3
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]

SECURITY_GLOBS = (
    "Assets/SafeMining/LOTOSimulation/Code/Adapters/",
    "Assets/SafeMining/LOTOSimulation/Code/Data/",
    "Assets/XR Standard Menu/Code/LOTOSimulation/Managers/SyncQueueManager.cs",
    "Assets/XR Standard Menu/Code/LOTOSimulation/Managers/SimulationSessionManager.cs",
    "Assets/XR Standard Menu/Code/Managers/IBManager.cs",
    "Assets/XR Standard Menu/Code/Managers/ScoreManager.cs",
    "Assets/XR Standard Menu/Code/Managers/PersistantDataManager.cs",
)

SECRET_PATTERNS = [
    (re.compile(r'(?i)(api[_-]?key|apikey)\s*=\s*"[^"]{8,}"'), "[HIGH] Hardcoded API key pattern"),
    (re.compile(r'(?i)(password|passwd|pwd)\s*=\s*"[^"]+"'), "[HIGH] Hardcoded password"),
    (re.compile(r'(?i)(unity[_-]?password|unity[_-]?serial)\s*=\s*"[^"]+"'), "[CRITICAL] Hardcoded Unity credential"),
    (re.compile(r'(?i)Bearer\s+[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+'), "[CRITICAL] JWT bearer token in source"),
    (re.compile(r'eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+'), "[CRITICAL] JWT-like string in source"),
]

LOG_LEAK_PATTERNS = [
    (re.compile(r'Debug\.Log\([^)]*(token|jwt|password|authorization|bearer)[^)]*\)', re.I), "[HIGH] Possible credential in Debug.Log"),
    (re.compile(r'Debug\.LogWarning\([^)]*(token|jwt|password)[^)]*\)', re.I), "[HIGH] Possible credential in Debug.LogWarning"),
]

HTTP_PATTERNS = [
    (re.compile(r'"http://[^"]+"'), "[HIGH] Cleartext HTTP URL (require HTTPS per RNF06)"),
]

PLAYERPREFS_TOKEN = re.compile(
    r'PlayerPrefs\.(SetString|GetString)\s*\(\s*"[^"]*(token|jwt|password|auth)[^"]*"',
    re.I,
)

IBMANAGER_CREAR = re.compile(r'/crearSimReporte')

def git_changed_files():
    result = subprocess.run(
        ["git", "diff", "--name-only", "HEAD"],
        cwd=ROOT,
        capture_output=True,
        text=True,
        check=False,
    )
    files = set(result.stdout.splitlines()) if result.returncode == 0 else set()
    staged = subprocess.run(
        ["git", "diff", "--name-only", "--cached"],
        cwd=ROOT,
        capture_output=True,
        text=True,
        check=False,
    )
    if staged.returncode == 0:
        files.update(staged.stdout.splitlines())
    if not files:
        result = subprocess.run(
            ["git", "ls-files"],
            cwd=ROOT,
            capture_output=True,
            text=True,
            check=False,
        )
        if result.returncode == 0:
            files = {f for f in result.stdout.splitlines() if matches_security_surface(f)}
    return sorted(f for f in files if f.strip())

def matches_security_surface(rel_path):
    normalized = rel_path.replace("\\", "/")
    for pattern in SECURITY_GLOBS:
        if pattern.endswith("/"):
            if normalized.startswith(pattern):
                return True
        elif normalized == pattern:
            return True
    return False

def scan_file(path):
    findings = []
    try:
        content = path.read_text(encoding="utf-8")
    except (OSError, UnicodeDecodeError):
        return [(str(path), "[MEDIUM] Unable to read file for security scan")]
    rel = path.relative_to(ROOT).as_posix()
    lines = content.splitlines()
    for i, line in enumerate(lines, start=1):
        for pattern, message in SECRET_PATTERNS + LOG_LEAK_PATTERNS + HTTP_PATTERNS:
            if pattern.search(line):
                findings.append((f"{rel}:{i}", message))
        if PLAYERPREFS_TOKEN.search(line):
            findings.append((f"{rel}:{i}", "[HIGH] Token stored in PlayerPrefs — verify encryption and expiry"))
    return findings

def check_ibmanager_integrity():
    ib_path = ROOT / "Assets/XR Standard Menu/Code/Managers/IBManager.cs"
    if not ib_path.exists():
        return [("[CRITICAL]", "IBManager.cs missing — legacy pipeline broken")]
    content = ib_path.read_text(encoding="utf-8")
    findings = []
    if "SendScoreToCloud" not in content:
        findings.append(("IBManager.cs", "[CRITICAL] SendScoreToCloud method missing"))
    if not IBMANAGER_CREAR.search(content):
        findings.append(("IBManager.cs", "[HIGH] /crearSimReporte endpoint reference missing"))
    return findings

def main():
    all_findings = []
    targets = git_changed_files() if "--changed-only" in sys.argv else None

    if targets is not None:
        scan_paths = [ROOT / f for f in targets if matches_security_surface(f)]
    else:
        scan_paths = []
        for pattern in SECURITY_GLOBS:
            if pattern.endswith("/"):
                base = ROOT / pattern
                if base.exists():
                    scan_paths.extend(base.rglob("*.cs"))
            else:
                p = ROOT / pattern
                if p.exists():
                    scan_paths.append(p)

    for path in scan_paths:
        all_findings.extend(scan_file(path))

    all_findings.extend(check_ibmanager_integrity())

    if not all_findings:
        print("Security surface scan OK.")
        return 0

    critical = 0
    high = 0
    for location, message in all_findings:
        print(f"{message} @ {location}")
        if message.startswith("[CRITICAL]"):
            critical += 1
        elif message.startswith("[HIGH]"):
            high += 1

    if critical > 0:
        print(f"FAILED: {critical} CRITICAL, {high} HIGH finding(s).", file=sys.stderr)
        return 1

    print(f"PASSED WITH WARNINGS: {high} HIGH finding(s). Review before merge.")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
