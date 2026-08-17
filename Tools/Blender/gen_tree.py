"""
Procedural low-poly tree generator for Aetherfall.

Replaces the placeholder "trees" in SampleScene, which were a cylinder with a
sphere balanced on top - 24 of them, two primitives each.

Produces a tapered trunk with a clustered canopy of faceted blobs, on two
material slots (Bark, Foliage) so colour is a material swap rather than a
re-export. Seeds give distinct variants so a stand of trees isn't 24 copies of
one silhouette.

Run headless:
    blender --background --python Tools/Blender/gen_tree.py -- <out_dir> [seed] [preview.png]

Example, three variants:
    for s in 1 2 3; do blender --background --python Tools/Blender/gen_tree.py -- Assets/Models/Props $s; done
"""

import bpy
import sys
import os
import math
import random

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
import lowpoly

TARGET_HEIGHT = 5.0

BARK = (0.24, 0.16, 0.11, 1.0)
FOLIAGE = (0.24, 0.42, 0.19, 1.0)


def build_trunk(rng, bark):
    """Tapered, slightly leaning trunk. A straight cylinder is what made the
    placeholder read as a lamp post."""
    height = rng.uniform(2.6, 3.4)
    bpy.ops.mesh.primitive_cone_add(
        vertices=7,
        radius1=rng.uniform(0.20, 0.28),   # base
        radius2=rng.uniform(0.10, 0.15),   # where the canopy starts
        depth=height,
        location=(0, 0, height / 2.0),
    )
    trunk = bpy.context.active_object
    lowpoly.jitter_mesh(trunk, 0.02, rng)

    lean = rng.uniform(0.0, 0.10)
    trunk.rotation_euler = (lean, rng.uniform(-0.05, 0.05), rng.uniform(0, 6.28))
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)

    trunk.data.materials.append(bark)
    return trunk, height


def build_canopy(rng, foliage, trunk_height):
    """Two or three overlapping blobs rather than one sphere, so the outline is
    irregular and reads as leaf mass."""
    blobs = []
    count = rng.randint(2, 3)
    base_z = trunk_height * rng.uniform(0.78, 0.92)

    for i in range(count):
        radius = rng.uniform(0.85, 1.25) * (1.0 if i == 0 else rng.uniform(0.6, 0.85))
        angle = rng.uniform(0, 6.28)
        spread = 0.0 if i == 0 else rng.uniform(0.3, 0.6)

        bpy.ops.mesh.primitive_ico_sphere_add(
            subdivisions=1,
            radius=radius,
            location=(math.cos(angle) * spread,
                      math.sin(angle) * spread,
                      base_z + rng.uniform(0.25, 0.75) + i * 0.25),
        )
        blob = bpy.context.active_object
        lowpoly.jitter_mesh(blob, 0.10, rng)
        # Slightly squashed: perfectly spherical canopies look like lollipops.
        blob.scale = (rng.uniform(0.95, 1.2), rng.uniform(0.95, 1.2), rng.uniform(0.72, 0.9))
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

        blob.data.materials.append(foliage)
        blobs.append(blob)

    return blobs


def build(seed):
    rng = random.Random(seed)
    lowpoly.clear_scene()

    bark = lowpoly.make_material("Bark", BARK, roughness=0.95)
    foliage = lowpoly.make_material("Foliage", FOLIAGE, roughness=0.8)

    trunk, height = build_trunk(rng, bark)
    blobs = build_canopy(rng, foliage, height)

    tree = lowpoly.join([trunk] + blobs, trunk)
    tree.name = "Tree_%02d" % seed

    bpy.ops.object.shade_flat()
    lowpoly.seat_on_ground(tree)
    lowpoly.normalise_height(tree, TARGET_HEIGHT * rng.uniform(0.85, 1.15))

    return tree


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    out_dir = argv[0] if argv else "."
    seed = int(argv[1]) if len(argv) > 1 else 1
    preview = argv[2] if len(argv) > 2 else ""

    tree = build(seed)
    lowpoly.report(tree)

    path = lowpoly.export_fbx(tree, out_dir, tree.name)
    print("GEN exported", path, os.path.exists(path))

    if preview:
        lowpoly.render_preview(preview, camera_distance=8.0, height=4.5,
                               look_at_z=tree.dimensions.z * 0.5)
        print("GEN preview", preview, os.path.exists(preview))


main()
