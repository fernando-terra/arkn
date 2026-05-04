#!/usr/bin/env python3
"""
Arkn.Copilot Benchmark Scorer

Compiles each generated .cs file against the Arkn packages and checks
for ARK001-ARK004 analyzer warnings.

Usage:
  python3 score.py                    # score all results
  python3 score.py --model gpt-4o    # score specific model
  python3 score.py --verbose          # detailed output
"""

import os
import sys
import json
import subprocess
import argparse
from pathlib import Path
from datetime import datetime

RESULTS_DIR = Path(__file__).parent / "results"
ARKN_SLN    = Path(__file__).parent.parent.parent / "Arkn.slnx"
ARK_RULES   = {"ARK001", "ARK002", "ARK003", "ARK004"}

def score_file(cs_file: Path, verbose: bool = False) -> dict:
    """Compile a single .cs file in isolation and check for ARK warnings."""
    # Write a minimal csproj around the file
    tmp_dir  = cs_file.parent / ".tmp_score"
    tmp_dir.mkdir(exist_ok=True)
    csproj   = tmp_dir / "Score.csproj"

    csproj.write_text(f"""
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="{cs_file.resolve()}" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="{(ARKN_SLN.parent / 'src/Arkn.Results/Arkn.Results.csproj').resolve()}" />
    <ProjectReference Include="{(ARKN_SLN.parent / 'src/Arkn.Core/Arkn.Core.csproj').resolve()}" />
    <ProjectReference Include="{(ARKN_SLN.parent / 'src/Arkn.Jobs/Arkn.Jobs.csproj').resolve()}" />
    <ProjectReference Include="{(ARKN_SLN.parent / 'src/Arkn.Analyzers/Arkn.Analyzers.csproj').resolve()}" />
  </ItemGroup>
</Project>
""")

    result = subprocess.run(
        ["dotnet", "build", str(csproj), "--configuration", "Release", "--nologo"],
        capture_output=True, text=True
    )

    output   = result.stdout + result.stderr
    errors   = [l for l in output.splitlines() if ": error " in l]
    warnings = [l for l in output.splitlines() if any(r in l for r in ARK_RULES)]
    passed   = result.returncode == 0 and len(warnings) == 0

    if verbose:
        print(f"\n  {'✅' if passed else '❌'} {cs_file.name}")
        for e in errors:   print(f"     error: {e.strip()}")
        for w in warnings: print(f"     warn:  {w.strip()}")

    # Cleanup
    import shutil
    shutil.rmtree(tmp_dir, ignore_errors=True)

    return {
        "file":     str(cs_file),
        "passed":   passed,
        "errors":   errors,
        "warnings": warnings,
        "build_ok": result.returncode == 0,
    }

def score_model(model_dir: Path, verbose: bool) -> dict:
    cs_files = list(model_dir.rglob("*.cs"))
    if not cs_files:
        return {"model": model_dir.name, "total": 0, "passed": 0, "results": []}

    results = [score_file(f, verbose) for f in cs_files]
    passed  = sum(1 for r in results if r["passed"])

    return {
        "model":   model_dir.name,
        "total":   len(results),
        "passed":  passed,
        "rate":    round(passed / len(results) * 100, 1),
        "results": results,
    }

def main():
    parser = argparse.ArgumentParser(description="Arkn.Copilot Benchmark Scorer")
    parser.add_argument("--model",   help="Score only this model directory")
    parser.add_argument("--verbose", action="store_true", help="Show per-file details")
    parser.add_argument("--json",    action="store_true", help="Output JSON")
    args = parser.parse_args()

    if not RESULTS_DIR.exists():
        print("No results directory found. Generate results first.")
        sys.exit(1)

    if args.model:
        model_dirs = [RESULTS_DIR / args.model]
    else:
        model_dirs = [d for d in RESULTS_DIR.iterdir() if d.is_dir()]

    if not model_dirs:
        print("No model results found in results/")
        sys.exit(1)

    all_scores = [score_model(d, args.verbose) for d in model_dirs]

    if args.json:
        print(json.dumps(all_scores, indent=2))
        return

    print("\n" + "=" * 60)
    print("  Arkn.Copilot Benchmark Results")
    print(f"  {datetime.now().strftime('%Y-%m-%d %H:%M UTC')}")
    print("=" * 60)
    print(f"  {'Model':<30} {'Pass':>6} {'Total':>6} {'Rate':>8}")
    print("  " + "-" * 54)

    for s in all_scores:
        bar = "🟢" if s.get("rate", 0) >= 90 else ("🟡" if s.get("rate", 0) >= 60 else "🔴")
        print(f"  {s['model']:<30} {s.get('passed', 0):>6} {s.get('total', 0):>6}   {s.get('rate', 0):>5.1f}% {bar}")

    print("=" * 60)
    print("  Target: ≥ 90% with instructions.md active")
    print()

if __name__ == "__main__":
    main()
