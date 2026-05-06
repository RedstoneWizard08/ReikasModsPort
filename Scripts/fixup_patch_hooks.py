import os
import re

ROOT = "../AqueousEngineering/Patches"
SC_ROOT = os.path.dirname(__file__)

root_dir = os.path.join(SC_ROOT, ROOT)

for file in os.listdir(root_dir):
    file = os.path.join(root_dir, file)

    if "PatchLib" in file or file.count(".") < 2:
        continue

    with open(file, "r") as f:
        lines = f.readlines()
    
    pos = 0
    found = False

    for line in lines:
        if line.strip() == '"ReikaKalseki.AqueousEngineering.AEHooks",' and '"' in lines[pos + 1]:
            found = True
            break

        pos += 1
    
    pos += 1

    if not found or '"' not in lines[pos]:
        continue

    name = lines[pos - 1].strip().removeprefix('"').removesuffix('",').split(".")[-1]
    method = lines[pos].split('"')[1]

    lines[pos] = lines[pos].replace(f"\"{method}\"", f"nameof({name}.{method})")

    with open(file, "w") as f:
        f.write("".join(lines))

    # break
