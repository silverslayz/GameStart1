"""
Procedural low-poly prop generators for Aetherfall's Haven hub.

Replaces the primitive placeholders: the anvil was a cube, the campfire a
cylinder, the quest and party boards flat cubes, the sign another cube.

Each prop is built to the placeholder's footprint so it drops into the existing
collider volume, and uses separate material slots so colour is a swap in Unity
rather than a re-export.

Run headless:
    blender --background --python Tools/Blender/gen_props.py -- <out_dir> <prop> [seed] [preview.png]

Props: anvil, campfire, questboard, partyboard, sign
"""

import bpy
import sys
import os
import math
import random

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
import lowpoly

IRON = (0.16, 0.17, 0.19, 1.0)
WOOD_DARK = (0.26, 0.17, 0.10, 1.0)
WOOD_LIGHT = (0.50, 0.36, 0.21, 1.0)
STONE = (0.34, 0.34, 0.36, 1.0)
EMBER = (0.95, 0.42, 0.10, 1.0)
PARCHMENT = (0.83, 0.78, 0.63, 1.0)
CLOTH_BLUE = (0.22, 0.32, 0.52, 1.0)


def box(name, size, location, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    o = bpy.context.active_object
    o.name = name
    o.scale = size
    o.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return o


def cyl(name, radius, depth, location, rotation=(0, 0, 0), verts=8):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=location)
    o = bpy.context.active_object
    o.name = name
    o.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    return o


def assign(objs, mat):
    for o in objs:
        o.data.materials.append(mat)
    return objs


# ---------------------------------------------------------------- props

def build_anvil(rng):
    """Classic anvil silhouette: splayed base, narrow waist, heavy body, horn.
    A cube reads as a crate; the waist and horn are what make it an anvil."""
    iron = lowpoly.make_material("Iron", IRON, roughness=0.45, metallic=0.7)
    wood = lowpoly.make_material("Wood", WOOD_DARK, roughness=0.9)

    parts = []
    stump = cyl("Stump", 0.30, 0.36, (0, 0, 0.18), verts=7)
    assign([stump], wood)

    base = box("Base", (0.46, 0.30, 0.10), (0, 0, 0.41))
    waist = box("Waist", (0.26, 0.20, 0.14), (0, 0, 0.53))
    body = box("Body", (0.62, 0.28, 0.16), (0, 0, 0.68))

    # Tapered and short. A long straight cylinder read as a pipe bolted to the side.
    bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=0.13, radius2=0.045, depth=0.26,
                                    location=(0.42, 0, 0.68),
                                    rotation=(0, math.pi / 2, 0))
    horn = bpy.context.active_object
    horn.name = "Horn"

    heel = box("Heel", (0.14, 0.24, 0.13), (-0.36, 0, 0.69))
    parts = [base, waist, body, horn, heel]
    assign(parts, iron)

    obj = lowpoly.join([stump] + parts, base)
    obj.name = "Prop_Anvil"
    return obj, 1.2


def build_campfire(rng):
    """Stone ring with crossed logs and a low ember cone. The cylinder placeholder
    read as a barrel."""
    stone = lowpoly.make_material("Stone", STONE, roughness=0.95)
    wood = lowpoly.make_material("Wood", WOOD_DARK, roughness=0.9)
    # Strength 3 blew the cone out to pale peach; ~1 keeps it reading as orange fire.
    ember = lowpoly.make_material("Ember", EMBER, roughness=0.4, emission=1.0)

    stones = []
    count = 8
    for i in range(count):
        a = (i / count) * math.tau + rng.uniform(-0.12, 0.12)
        r = rng.uniform(0.40, 0.46)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=rng.uniform(0.10, 0.15),
                                              location=(math.cos(a) * r, math.sin(a) * r, 0.07))
        s = bpy.context.active_object
        lowpoly.jitter_mesh(s, 0.03, rng)
        s.scale = (1.0, 1.0, rng.uniform(0.65, 0.85))
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        stones.append(s)
    assign(stones, stone)

    logs = []
    for i in range(3):
        a = (i / 3.0) * math.pi + rng.uniform(-0.2, 0.2)
        logs.append(cyl("Log%d" % i, 0.055, 0.72,
                        (0, 0, 0.13),
                        rotation=(math.pi / 2 * 0.82, 0, a), verts=6))
    assign(logs, wood)

    # Low and broad rather than a tall spike, which read as a tent.
    bpy.ops.mesh.primitive_cone_add(vertices=7, radius1=0.24, radius2=0.0, depth=0.26,
                                    location=(0, 0, 0.25))
    flame = bpy.context.active_object
    flame.name = "Ember"
    lowpoly.jitter_mesh(flame, 0.045, rng)
    assign([flame], ember)

    obj = lowpoly.join(stones + logs + [flame], stones[0])
    obj.name = "Prop_Campfire"
    return obj, 0.8


def build_board(rng, accent, name):
    """Two posts, a plank face, a pitched roof and pinned notices. The flat cube
    gave no read at all at distance."""
    wood = lowpoly.make_material("Wood", WOOD_DARK, roughness=0.9)
    plank = lowpoly.make_material("Plank", WOOD_LIGHT, roughness=0.85)
    paper = lowpoly.make_material("Notice", accent, roughness=0.8)

    posts = [box("PostL", (0.11, 0.11, 1.7), (-0.62, 0, 0.85)),
             box("PostR", (0.11, 0.11, 1.7), (0.62, 0, 0.85))]
    # Pitched about X so the planks slope front-to-back over the board face. Rotating
    # about Y instead just scissored them flat, like propeller blades.
    roofL = box("RoofL", (1.45, 0.42, 0.05), (0, -0.17, 1.78), rotation=(0.40, 0, 0))
    roofR = box("RoofR", (1.45, 0.42, 0.05), (0, 0.17, 1.78), rotation=(-0.40, 0, 0))
    assign(posts + [roofL, roofR], wood)

    face = box("Face", (1.30, 0.07, 1.05), (0, 0, 1.05))
    assign([face], plank)

    notices = []
    for i in range(rng.randint(3, 5)):
        w = rng.uniform(0.16, 0.26)
        h = rng.uniform(0.20, 0.30)
        notices.append(box("Notice%d" % i, (w, 0.02, h),
                           (rng.uniform(-0.48, 0.48), -0.05, rng.uniform(0.72, 1.38)),
                           rotation=(0, rng.uniform(-0.12, 0.12), 0)))
    assign(notices, paper)

    obj = lowpoly.join(posts + [roofL, roofR, face] + notices, posts[0])
    obj.name = name
    return obj, 1.8


def build_sign(rng):
    """Post with an angled plank, so it reads as signage from the side too."""
    wood = lowpoly.make_material("Wood", WOOD_DARK, roughness=0.9)
    plank = lowpoly.make_material("Plank", WOOD_LIGHT, roughness=0.85)

    post = box("Post", (0.10, 0.10, 1.5), (0, 0, 0.75))
    assign([post], wood)

    face = box("Face", (0.85, 0.06, 0.46), (0, -0.02, 1.20), rotation=(0.12, 0, 0))
    brace = box("Brace", (0.60, 0.05, 0.05), (0, 0.06, 0.95))
    assign([face, brace], plank)

    obj = lowpoly.join([post, face, brace], post)
    obj.name = "Prop_Sign"
    return obj, 1.5


BUILDERS = {
    "anvil": lambda rng: build_anvil(rng),
    "campfire": lambda rng: build_campfire(rng),
    "questboard": lambda rng: build_board(rng, PARCHMENT, "Prop_QuestBoard"),
    "partyboard": lambda rng: build_board(rng, CLOTH_BLUE, "Prop_PartyBoard"),
    "sign": lambda rng: build_sign(rng),
}


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    out_dir = argv[0] if argv else "."
    prop = (argv[1] if len(argv) > 1 else "anvil").lower()
    seed = int(argv[2]) if len(argv) > 2 else 1
    preview = argv[3] if len(argv) > 3 else ""

    if prop not in BUILDERS:
        print("GEN unknown prop:", prop, "expected one of", ", ".join(sorted(BUILDERS)))
        return

    rng = random.Random(seed)
    lowpoly.clear_scene()

    obj, target_height = BUILDERS[prop](rng)

    bpy.ops.object.shade_flat()
    lowpoly.seat_on_ground(obj)
    lowpoly.normalise_height(obj, target_height)

    lowpoly.report(obj)
    path = lowpoly.export_fbx(obj, out_dir, obj.name)
    print("GEN exported", path, os.path.exists(path))

    if preview:
        h = obj.dimensions.z
        lowpoly.render_preview(preview, camera_distance=max(h * 1.9, 2.2),
                               height=h * 1.25, look_at_z=h * 0.5)
        print("GEN preview", preview, os.path.exists(preview))


main()
