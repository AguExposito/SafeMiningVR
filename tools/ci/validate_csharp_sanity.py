#!/usr/bin/env python3
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]

SCAN_ROOTS = (
    ROOT / "Assets/SafeMining/LOTOSimulation/Code",
)

STRING_LITERAL = re.compile(r'"(?:\\.|[^"\\])*"')
SINGLE_LINE_COMMENT = re.compile(r"//.*")
MULTI_LINE_COMMENT = re.compile(r"/\*.*?\*/", re.DOTALL)

def strip_strings_and_comments(source):
    without_block = MULTI_LINE_COMMENT.sub("", source)
    without_line = SINGLE_LINE_COMMENT.sub("", without_block)
    return STRING_LITERAL.sub('""', without_line)

def brace_balance(source):
    stripped = strip_strings_and_comments(source)
    depth = 0
    for ch in stripped:
        if ch == "{":
            depth += 1
        elif ch == "}":
            depth -= 1
            if depth < 0:
                return False
    return depth == 0

def scan_file(path):
    errors = []
    try:
        source = path.read_text(encoding="utf-8")
    except UnicodeDecodeError:
        errors.append(f"{path.relative_to(ROOT)}: not valid UTF-8")
        return errors

    if not brace_balance(source):
        errors.append(f"{path.relative_to(ROOT)}: unbalanced braces")

    return errors

def main():
    errors = []
    scanned = 0
    for scan_root in SCAN_ROOTS:
        if not scan_root.exists():
            continue
        for path in scan_root.rglob("*.cs"):
            scanned += 1
            errors.extend(scan_file(path))

    if errors:
        for err in errors:
            print(f"ERROR: {err}", file=sys.stderr)
        return 1

    print(f"C# sanity OK ({scanned} files scanned).")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
