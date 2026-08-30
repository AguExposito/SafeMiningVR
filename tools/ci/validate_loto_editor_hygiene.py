#!/usr/bin/env python3
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SCAN_ROOT = ROOT / "Assets/SafeMining"
PATTERN = re.compile(r"PREF_PH_")

def main():
    violations = []
    for path in SCAN_ROOT.rglob("*.cs"):
        try:
            text = path.read_text(encoding="utf-8")
        except UnicodeDecodeError:
            continue
        if PATTERN.search(text):
            violations.append(path.relative_to(ROOT))

    if violations:
        for path in violations:
            print(f"ERROR: {path} references retired PREF_PH_ placeholders", file=sys.stderr)
        return 1

    print("LOTO editor hygiene OK (no PREF_PH_ in SafeMining .cs).")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
