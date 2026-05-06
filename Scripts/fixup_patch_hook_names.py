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
        if line.strip() == '"ReikaKalseki.AqueousEngineering.AEHooks",' and 'nameof' in lines[pos + 1]:
            found = True
            break

        pos += 1
    
    pos += 1

    if not found or 'nameof' not in lines[pos]:
        continue

    line = lines[pos]
    name = line.split(".")[1].split(")")[0]
    orig = name
    fc = name[0]

    if fc != fc.lower():
        continue

    name = fc.upper() + name[1:]
    lines[pos] = line.replace(orig, name)

    with open(file, "w") as f:
        f.write("".join(lines))
