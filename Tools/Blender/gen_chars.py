"""
Procedural low-poly character generators for Aetherfall.

Replaces the capsule placeholders used for NPCs, monsters and the apex boss.
A capsule gives no read at all - villager, wolf and boss were the same shape at
different scales.

Run headless:
    blender --background --python Tools/Blender/gen_chars.py -- <out_dir> <kind> [seed] [preview.png]

Kinds: npc_shop, npc_elder, monster_wolf, boss_apex
"""

import bpy
import sys
import os
import math
import random

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
import lowpoly

SKIN = (0.72, 0.55, 0.42, 1.0)
CLOTH_GREEN = (0.28, 0.40, 0.26, 1.0)
CLOTH_PURPLE = (0.32, 0.24, 0.42, 1.0)
LEATHER = (0.34, 0.24, 0.16, 1.0)
FUR_GREY = (0.36, 0.34, 0.33, 1.0)
FUR_DARK = (0.20, 0.17, 0.18, 1.0)
BONE = (0.80, 0.77, 0.68, 1.0)
EYE_GLOW = (0.95, 0.35, 0.18, 1.0)


def box(name, size, location, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    o = bpy.context.active_object
    o.name = name
    o.scale = size
    o.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return o


def blob(name, radius, location, scale, rng, jitter=0.0):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=radius, location=location)
    o = bpy.context.active_object
    o.name = name
    if jitter > 0:
        lowpoly.jitter_mesh(o, jitter, rng)
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return o


def assign(objs, mat):
    for o in objs:
        o.data.materials.append(mat)
    return objs


# ---------------------------------------------------------------- humanoids

def build_humanoid(rng, cloth_colour, hooded):
    """Blocky humanoid: head, torso, arms, legs. Deliberately simple - these are
    background villagers, not hero characters."""
    skin = lowpoly.make_material("Skin", SKIN, roughness=0.85)
    cloth = lowpoly.make_material("Cloth", cloth_colour, roughness=0.9)
    leather = lowpoly.make_material("Leather", LEATHER, roughness=0.85)

    torso = box("Torso", (0.42, 0.26, 0.62), (0, 0, 1.18))
    hips = box("Hips", (0.38, 0.24, 0.20), (0, 0, 0.82))
    assign([torso, hips], cloth)

    legs = [box("LegL", (0.16, 0.18, 0.74), (-0.11, 0, 0.37)),
            box("LegR", (0.16, 0.18, 0.74), (0.11, 0, 0.37))]
    boots = [box("BootL", (0.19, 0.26, 0.12), (-0.11, -0.03, 0.06)),
             box("BootR", (0.19, 0.26, 0.12), (0.11, -0.03, 0.06))]
    assign(legs + boots, leather)

    arms = [box("ArmL", (0.13, 0.15, 0.56), (-0.30, 0, 1.18), rotation=(0, 0.10, 0)),
            box("ArmR", (0.13, 0.15, 0.56), (0.30, 0, 1.18), rotation=(0, -0.10, 0))]
    assign(arms, cloth)

    hands = [blob("HandL", 0.09, (-0.33, 0, 0.88), (1, 1, 0.9), rng),
             blob("HandR", 0.09, (0.33, 0, 0.88), (1, 1, 0.9), rng)]
    head = blob("Head", 0.20, (0, 0, 1.62), (1.0, 0.95, 1.1), rng)
    assign(hands + [head], skin)

    parts = [torso, hips] + legs + boots + arms + hands + [head]

    if hooded:
        # Marks the elder as distinct from the shopkeeper at a glance.
        hood = blob("Hood", 0.25, (0, 0.02, 1.66), (1.0, 1.05, 0.95), rng, jitter=0.02)
        assign([hood], cloth)
        parts.append(hood)
        staff = box("Staff", (0.05, 0.05, 1.55), (0.36, 0.05, 0.78), rotation=(0.06, 0, 0))
        assign([staff], leather)
        parts.append(staff)

    obj = lowpoly.join(parts, torso)
    return obj


# ---------------------------------------------------------------- creatures

def build_wolf(rng):
    """Quadruped: low slung body, forward head, four legs, tail. The silhouette is
    what distinguishes it from an upright NPC at distance."""
    fur = lowpoly.make_material("Fur", FUR_GREY, roughness=0.9)
    dark = lowpoly.make_material("FurDark", FUR_DARK, roughness=0.9)
    eye = lowpoly.make_material("Eye", EYE_GLOW, roughness=0.3, emission=1.0)

    # Body sits low and overlaps the legs. An earlier pass had a high body on long
    # thin legs, which read as a table rather than an animal.
    body = blob("Body", 0.40, (0, 0, 0.58), (1.25, 0.88, 0.78), rng, jitter=0.04)
    chest = blob("Chest", 0.31, (0.34, 0, 0.60), (1.0, 0.98, 0.98), rng, jitter=0.03)
    assign([body, chest], fur)

    head = blob("Head", 0.23, (0.66, 0, 0.68), (1.15, 0.90, 0.90), rng, jitter=0.03)
    snout = box("Snout", (0.24, 0.15, 0.13), (0.86, 0, 0.63))
    assign([head, snout], fur)

    ears = [box("EarL", (0.05, 0.10, 0.14), (0.60, -0.10, 0.86), rotation=(0.18, 0, 0)),
            box("EarR", (0.05, 0.10, 0.14), (0.60, 0.10, 0.86), rotation=(-0.18, 0, 0))]

    # Short and thick, tucked under the body so there is no gap at the shoulder.
    legs = []
    for i, (lx, ly) in enumerate([(0.34, -0.18), (0.34, 0.18), (-0.30, -0.18), (-0.30, 0.18)]):
        legs.append(box("Leg%d" % i, (0.16, 0.16, 0.46), (lx, ly, 0.23)))

    tail = box("Tail", (0.30, 0.09, 0.09), (-0.56, 0, 0.70), rotation=(0, -0.55, 0))
    assign(ears + legs + [tail], dark)

    eyes = [blob("EyeL", 0.045, (0.80, -0.10, 0.73), (1, 1, 1), rng),
            blob("EyeR", 0.045, (0.80, 0.10, 0.73), (1, 1, 1), rng)]
    assign(eyes, eye)

    obj = lowpoly.join([body, chest, head, snout] + ears + legs + [tail] + eyes, body)
    return obj


def build_boss(rng):
    """Hulking biped: broad shoulders, horns, glowing eyes. Reads as a threat next
    to the wolf rather than just a larger capsule."""
    hide = lowpoly.make_material("Hide", FUR_DARK, roughness=0.9)
    bone = lowpoly.make_material("Bone", BONE, roughness=0.7)
    eye = lowpoly.make_material("Eye", EYE_GLOW, roughness=0.3, emission=1.5)

    torso = blob("Torso", 0.62, (0, 0, 1.45), (1.15, 0.85, 1.15), rng, jitter=0.06)
    shoulders = blob("Shoulders", 0.52, (0, 0, 1.95), (1.55, 0.95, 0.62), rng, jitter=0.05)
    hips = blob("Hips", 0.46, (0, 0, 1.02), (1.1, 0.9, 0.7), rng, jitter=0.04)
    assign([torso, shoulders, hips], hide)

    legs = [box("LegL", (0.28, 0.30, 1.05), (-0.30, 0, 0.52)),
            box("LegR", (0.28, 0.30, 1.05), (0.30, 0, 0.52))]
    arms = [box("ArmL", (0.24, 0.26, 1.00), (-0.72, 0, 1.55), rotation=(0, 0.16, 0)),
            box("ArmR", (0.24, 0.26, 1.00), (0.72, 0, 1.55), rotation=(0, -0.16, 0))]
    assign(legs + arms, hide)

    head = blob("Head", 0.30, (0, 0, 2.30), (1.1, 0.95, 0.95), rng, jitter=0.03)
    assign([head], hide)

    horns = []
    for side in (-1, 1):
        bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=0.10, radius2=0.0, depth=0.46,
                                        location=(0.17 * side, -0.02, 2.52),
                                        rotation=(0.25, 0.5 * side, 0))
        h = bpy.context.active_object
        h.name = "Horn"
        horns.append(h)
    claws = []
    for side in (-1, 1):
        bpy.ops.mesh.primitive_cone_add(vertices=5, radius1=0.09, radius2=0.0, depth=0.30,
                                        location=(0.78 * side, 0, 0.98),
                                        rotation=(math.pi, 0, 0))
        c = bpy.context.active_object
        c.name = "Claw"
        claws.append(c)
    assign(horns + claws, bone)

    eyes = [blob("EyeL", 0.07, (-0.12, -0.26, 2.34), (1, 1, 1), rng),
            blob("EyeR", 0.07, (0.12, -0.26, 2.34), (1, 1, 1), rng)]
    assign(eyes, eye)

    obj = lowpoly.join([torso, shoulders, hips] + legs + arms + [head] + horns + claws + eyes, torso)
    return obj


BUILDERS = {
    "npc_shop":     (lambda rng: build_humanoid(rng, CLOTH_GREEN, hooded=False), 2.0, "Char_ShopNPC"),
    "npc_elder":    (lambda rng: build_humanoid(rng, CLOTH_PURPLE, hooded=True), 2.0, "Char_Elder"),
    "monster_wolf": (lambda rng: build_wolf(rng), 1.35, "Char_FieldWolf"),
    "boss_apex":    (lambda rng: build_boss(rng), 3.0, "Char_ApexBoss"),
}


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    out_dir = argv[0] if argv else "."
    kind = (argv[1] if len(argv) > 1 else "npc_shop").lower()
    seed = int(argv[2]) if len(argv) > 2 else 1
    preview = argv[3] if len(argv) > 3 else ""

    if kind not in BUILDERS:
        print("GEN unknown kind:", kind, "expected", ", ".join(sorted(BUILDERS)))
        return

    builder, height, name = BUILDERS[kind]
    rng = random.Random(seed)
    lowpoly.clear_scene()

    obj = builder(rng)
    obj.name = name

    bpy.ops.object.shade_flat()
    lowpoly.seat_on_ground(obj)
    lowpoly.normalise_height(obj, height)

    lowpoly.report(obj)
    path = lowpoly.export_fbx(obj, out_dir, name)
    print("GEN exported", path, os.path.exists(path))

    if preview:
        h = obj.dimensions.z
        # Low, near side-on. A high angle foreshortens legs and makes a quadruped
        # read as a table.
        lowpoly.render_preview(preview, camera_distance=max(h * 2.0, 3.0),
                               height=h * 0.55, look_at_z=h * 0.5)
        print("GEN preview", preview, os.path.exists(preview))


main()
