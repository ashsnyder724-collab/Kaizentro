"""Small, dependency-free guard for obvious credentials in tracked files.

This complements a full secret scanner; it does not scan Git history and does
not print matching values. Run before committing and in CI.
"""

from __future__ import annotations

import argparse
import re
import shutil

# Fixed Git argv, resolved executable, no shell.
import subprocess  # nosec B404
from pathlib import Path

_PATTERNS = {
    "private key material": re.compile(r"-----BEGIN (?:RSA |EC |OPENSSH |DSA )?PRIVATE KEY-----"),
    "GitHub credential": re.compile(
        r"\b(?:gh[pousr]_[A-Za-z0-9]{36,}|github_pat_[A-Za-z0-9_]{40,})\b"
    ),
    "AWS access key identifier": re.compile(r"\b(?:AKIA|ASIA)[A-Z0-9]{16}\b"),
    "literal Android signing password": re.compile(
        r"\b(?:storePassword|keyPassword)\s+['\"][^'\"\r\n]+['\"]"
    ),
}
_SKIP_DIRECTORIES = {".git", ".venv", "venv", "node_modules", "__pycache__", ".pytest_cache"}


def scan_file(path: Path, root: Path) -> list[str]:
    relative = path.relative_to(root).as_posix()
    name = path.name.lower()
    if path.suffix.lower() in {".jks", ".keystore", ".p12", ".pfx"} or (
        name.endswith(".b64") and ("keystore" in name or "private-key" in name)
    ):
        return [f"{relative}: tracked signing/credential container"]
    if name == ".env" or (
        name.startswith(".env.") and name not in {".env.example", ".env.sample", ".env.template"}
    ):
        return [f"{relative}: tracked local environment file"]
    if path.is_symlink():
        return []
    data = path.read_bytes()
    if b"\0" in data:
        return []
    text = data.decode("utf-8", errors="replace")
    return [
        f"{relative}:{text.count(chr(10), 0, match.start()) + 1}: {label}"
        for label, pattern in _PATTERNS.items()
        for match in pattern.finditer(text)
    ]


def scan_path(root: Path) -> list[str]:
    root = root.resolve()
    try:
        git = shutil.which("git")
        if git is None:
            raise FileNotFoundError("Git is not installed")
        # Resolved Git executable and fixed argv; shell execution is disabled.
        result = subprocess.run(  # nosec B603
            [git, "-C", str(root), "ls-files", "-z"],
            check=True,
            capture_output=True,
            shell=False,
        )
        paths = [root / name for name in result.stdout.decode("utf-8").split("\0") if name]
    except (subprocess.CalledProcessError, FileNotFoundError):
        paths = [
            p
            for p in root.rglob("*")
            if not _SKIP_DIRECTORIES.intersection(p.relative_to(root).parts)
        ]
    return [finding for path in paths if path.is_file() for finding in scan_file(path, root)]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path.cwd())
    args = parser.parse_args()
    findings = scan_path(args.root)
    for finding in findings:
        print(finding)
    print(f"Secret guard: {len(findings)} finding(s). Values are never printed.")
    return 1 if findings else 0


if __name__ == "__main__":
    raise SystemExit(main())
