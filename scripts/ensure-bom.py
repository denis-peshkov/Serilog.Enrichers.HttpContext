#!/usr/bin/env python3
"""Add UTF-8 BOM to .cs, .csproj, .sln files that lack it."""
import os
import sys

BOM = b"\xef\xbb\xbf"
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def ensure_bom(paths=None):
    if paths is None:
        paths = []
        for root, dirs, files in os.walk(ROOT):
            dirs[:] = [d for d in dirs if d not in ("bin", "obj", ".git")]
            for fn in files:
                if fn.endswith((".cs", ".csproj", ".sln")):
                    paths.append(os.path.join(root, fn))

    fixed = []
    for path in paths:
        path = os.path.normpath(path)
        if not os.path.exists(path):
            continue
        with open(path, "rb") as f:
            content = f.read()
        if not content.startswith(BOM):
            with open(path, "wb") as f:
                f.write(BOM + content)
            fixed.append(path)
    return fixed


if __name__ == "__main__":
    paths = sys.argv[1:] if len(sys.argv) > 1 else None
    for p in ensure_bom(paths):
        print("Added BOM:", p)
