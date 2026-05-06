# This script is the epitome of boredom and procrastination. I spent way too long on it.
# Enjoy.
#
# - Redstone

import os
import json
import shutil
import hashlib
import zipfile
import subprocess

from log import Logger
from halo import Halo
from tqdm import tqdm

DRY_RUN = False
IGNORE_CACHE = False
PROFILE = "Release"
FRAMEWORK = "net472"
GAME_DIR = "/home/redstone/.local/share/Steam/steamapps/common/Subnautica"

ROOT = os.path.dirname(os.path.dirname(__file__))

projects: dict[str, tuple[str, list[str]]] = {
    "AqueousEngineering": (
        "AqueousEngineering",
        ["Sounds", "Textures", "XML", "current-version.txt"],
    ),
    "Auroresource": (
        "Auroresource",
        ["Sounds", "Textures", "XML", "current-version.txt"],
    ),
    "DragonIndustries": (
        "DragonIndustries_AlterraDivision",
        ["Textures", "XML", "current-version.txt", "geysers.xml"],
    ),
    "Ecocean": (
        "Ecocean",
        [
            "Sounds",
            "Textures",
            "XML",
            "current-version.txt",
            "mountain-flow-vectors.csv",
            "mountain-flow-vectors-2D.csv",
        ],
    ),
    "Exscansion": ("Exscansion", ["Textures", "current-version.txt"]),
    "Reefbalance": ("Reefbalance", ["Textures", "current-version.txt"]),
    "Sea2Sea": (
        "Sea2Sea",
        [
            "Assets",
            "Sounds",
            "TerrainPatches",
            "Textures",
            "XML",
            "current-version.txt",
            "statdump.xml",
            "worldhash.dat",
        ],
    ),
}

deploy_dir = os.path.join(GAME_DIR, "BepInEx/plugins")
cache_dir = os.path.join(ROOT, ".build")
cache_index = os.path.join(cache_dir, "cache.json")

# Per-Project Steps:
# 1. Scan files
# 2. Hash code
# 3. Hash assets
# 4. Check hashes
# 5. Build project
# 6. Clean tempdir
# 7. Create tempdir
# 8. Copy artifacts
# 9. Copy extra files
# 10. Create archive
# 11. Save cache

# Post Per-Project Steps:
# 1. Deploy files to game

# Per-Project Step Count: (11 * projects)
# Post Step Count: (1 * projects)

total_steps = 12 * len(projects)
log = Logger(list(projects.keys()), total_steps)

if not os.path.exists(cache_dir):
    os.makedirs(cache_dir)

# key = project name, value = [code hashes, assets hashes, zip hash]
cache: dict[str, tuple[dict[str, str], dict[str, str], str | None]] = {}


def save_cache():
    with open(cache_index, "w") as f:
        f.write(json.dumps(cache))


if os.path.exists(cache_index):
    with open(cache_index, "r") as f:
        try:
            cache: dict[str, tuple[dict[str, str], dict[str, str], str | None]] = (
                json.loads(f.read())
            )
        except json.decoder.JSONDecodeError:
            log.warn("Cache index malformed, invalidating all caches!")

save_cache()


def is_extra_file(path: str, extra: list[str]) -> bool:
    if path in extra:
        return True

    for item in extra:
        if path.startswith(item):
            return True

    return False


def check_hashes(stored: dict[str, str], found: dict[str, str]) -> bool:
    hit = 0

    for k in found.keys():
        if k in stored:
            if stored[k] != found[k]:
                return False
            
            hit += 1

    if hit != len(stored):
        return False

    return True


for name, (dir_name, extra_files) in projects.items():
    dir = os.path.join(ROOT, dir_name)
    temp_dir = os.path.join(cache_dir, dir_name)
    csproj_path = os.path.join(dir, dir_name + ".csproj")
    archive_path = os.path.join(cache_dir, name + ".zip")
    extra_files_dirs = [os.path.join(dir, it) for it in extra_files]

    log.step(name, "Scanning files...")

    code_files: list[str] = []
    asset_files: list[str] = []

    for root, _, files in os.walk(dir):
        for file in files:
            path = os.path.join(root, file)

            if path.endswith(".cs"):
                code_files.append(path)
            elif is_extra_file(path, extra_files_dirs):
                asset_files.append(path)

    log.progress(f"Found {len(code_files)} code files and {len(asset_files)} assets.")

    log.step(name, "Collecting code hashes...")

    code_hashes: dict[str, str] = {}

    for file in tqdm(code_files, leave=False):
        with open(file, "rb") as f:
            code_hashes[file] = hashlib.sha256(f.read()).hexdigest()

    log.step(name, "Collecting asset hashes...")

    asset_hashes: dict[str, str] = {}

    for file in tqdm(asset_files, leave=False):
        with open(file, "rb") as f:
            asset_hashes[file] = hashlib.sha256(f.read()).hexdigest()

    log.step(name, "Checking hashes...")

    is_cache_valid = False

    if not IGNORE_CACHE and os.path.exists(archive_path) and name in cache:
        with open(archive_path, "rb") as f:
            archive_hash = hashlib.sha256(f.read()).hexdigest()

        (stored_code_hashes, stored_asset_hashes, stored_archive_hash) = cache[name]

        # checking the archive's hash is more of a sanity check, it doesn't prove the code didn't change.
        is_cache_valid = (
            archive_hash == stored_archive_hash
            and check_hashes(stored_code_hashes, code_hashes)
            and check_hashes(stored_asset_hashes, asset_hashes)
        )

    if is_cache_valid:
        log.skip(7)
        log.info("Cache is valid, skipping build.")
        continue

    log.step(name, "Building project...")

    cmd = [
        "dotnet",
        "build",
        csproj_path,
        "-c",
        PROFILE,
        "--nologo",
        "--verbosity",
        "normal",
    ]

    cmd_str = " ".join(cmd)

    log.exec(cmd_str)

    if not DRY_RUN:
        spin = Halo("Building project...", spinner="dots")
        spin.start()
        proc = subprocess.run(
            cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, cwd=ROOT, shell=False
        )
        spin.stop()

        if proc.returncode != 0:
            log.fatal("Failed to build project. Details:")

            for line in proc.stdout.decode().splitlines():
                log.debug(line)

        cache[name] = (code_hashes, asset_hashes, None)
        save_cache()

    log.step(name, "Cleaning temp dir...")

    if os.path.exists(temp_dir):
        shutil.rmtree(temp_dir)

    log.step(name, "Creating temp dir...")

    os.makedirs(temp_dir)

    log.step(name, "Copying artifacts...")

    bin_dir = os.path.join(dir, "bin", PROFILE, FRAMEWORK)

    dll_path = os.path.join(bin_dir, dir_name + ".dll")
    pdb_path = os.path.join(bin_dir, dir_name + ".pdb")

    dll_dst_path = os.path.join(temp_dir, dir_name + ".dll")
    pdb_dst_path = os.path.join(temp_dir, dir_name + ".pdb")

    shutil.copyfile(dll_path, dll_dst_path)
    shutil.copyfile(pdb_path, pdb_dst_path)

    log.step(name, "Copying assets...")

    for file in tqdm(asset_files, leave=False):
        new_path = file.replace(dir, temp_dir)
        parent = os.path.dirname(new_path)

        if not os.path.exists(parent):
            os.makedirs(parent)

        shutil.copyfile(file, new_path)

    log.step(name, "Creating archive...")

    # if os.path.exists(archive_path):
    #     os.remove(archive_path)

    # with zipfile.ZipFile(archive_path, "w") as zip:
    #     for root, _, files in os.walk(temp_dir):
    #         for file in files:
    #             path = os.path.join(root, file)
    #             zip_path = dir_name + "/" + os.path.relpath(path, temp_dir)

    #             zip.write(path, zip_path)

    # with open(archive_path, "rb") as f:
    #     archive_hash = hashlib.sha256(f.read()).hexdigest()

    log.step(name, "Saving cache...")

    cache[name] = (code_hashes, asset_hashes, archive_hash)
    save_cache()

for name, (dir_name, extra_files) in projects.items():
    dir = os.path.join(ROOT, dir_name)
    temp_dir = os.path.join(cache_dir, dir_name)
    target_dir = os.path.join(deploy_dir, dir_name)

    log.step(name, "Deploying files...")

    to_copy: dict[str, str] = {}

    for root, _, files in os.walk(temp_dir):
        for file in files:
            path = os.path.join(root, file)
            target_path = path.replace(temp_dir, target_dir)

            to_copy[path] = target_path
    
    for (src, dst) in tqdm(to_copy.items(), leave=False):
        parent = os.path.dirname(dst)

        if not os.path.exists(parent):
            os.makedirs(parent)
        
        if os.path.exists(dst):
            os.remove(dst)
        
        shutil.copyfile(src, dst)
