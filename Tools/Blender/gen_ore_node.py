"""
Procedural low-poly ore node generator for Aetherfall.

Replaces the placeholder sphere used by TestResourceNode_IronOre with a rock
cluster carrying visible ore deposits, matching the convention used by stylized
mining-node asset packs: a faceted boulder, a few shards around the base, and
ore chunks on a separate material so one mesh serves every ore type.

Run headless:
    blender --background --python Tools/Blender/gen_ore_node.py -- <out_dir> [ore] [seed]

Example:
    blender --background --python Tools/Blender/gen_ore_node.py -- Assets/Models/Props iron 1

Output is a single object with two material slots (Rock, Ore) exported as FBX.
Keeping the ore on its own slot means colour is a material swap in Unity rather
than a re-export.
"""

import bpy
import bmesh
import sys
import os
import random
from mathutils import Vector

TARGET_HEIGHT = 1.4  # matches the collider volume of the placeholder it replaces

ORE_COLOURS = {
    "iron":   (0.38, 0.30, 0.26, 1.0),
    "copper": (0.72, 0.38, 0.18, 1.0),
    "silver": (0.78, 0.80, 0.84, 1.0),
    "gold":   (0.85, 0.66, 0.22, 1.0),
    "mana":   (0.35, 0.55, 0.85, 1.0),
}
ROCK_COLOUR = (0.30, 0.30, 0.32, 1.0)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for block in (bpy.data.meshes, bpy.data.materials):
        for item in list(block):
            if item.users == 0:
                block.remove(item)


def make_material(name, colour, roughness, metallic=0.0):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = colour
        bsdf.inputs["Roughness"].default_value = roughness
        if "Metallic" in bsdf.inputs:
            bsdf.inputs["Metallic"].default_value = metallic
    mat.diffuse_color = colour  # drives viewport/workbench preview
    return mat


def jitter_mesh(obj, amount, rng):
    """Push every vertex out along a random offset, then flat-shade: the classic
    low-poly rock look, and cheap in tris."""
    mesh = obj.data
    bm = bmesh.new()
    bm.from_mesh(mesh)
    for v in bm.verts:
        v.co += Vector((rng.uniform(-amount, amount),
                        rng.uniform(-amount, amount),
                        rng.uniform(-amount, amount)))
    bm.to_mesh(mesh)
    bm.free()


def add_rock(radius, subdiv, jitter, location, rng, flatten=(0.9, 1.05), decimate=0.0):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=subdiv, radius=radius, location=location)
    obj = bpy.context.active_object
    jitter_mesh(obj, jitter, rng)

    # Collapsing a denser, heavily displaced sphere gives irregular angular planes.
    # Displacing a low-subdiv sphere directly just yields a lumpy icosahedron.
    if decimate > 0.0:
        mod = obj.modifiers.new("Decimate", "DECIMATE")
        mod.ratio = decimate
        bpy.ops.object.modifier_apply(modifier=mod.name)

    # Kept close to 1 on Z: heavier flattening turned the boulder into a shell.
    obj.scale = (rng.uniform(0.9, 1.15), rng.uniform(0.9, 1.15), rng.uniform(*flatten))
    obj.rotation_euler = (rng.uniform(-0.25, 0.25), rng.uniform(-0.25, 0.25), rng.uniform(0, 6.28))
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return obj


def build(ore_name, seed):
    rng = random.Random(seed)
    clear_scene()

    rock_mat = make_material("Rock", ROCK_COLOUR, 0.9)
    ore_mat = make_material("Ore_" + ore_name, ORE_COLOURS.get(ore_name, ORE_COLOURS["iron"]), 0.45, 0.6)

    parts = []

    # Main boulder. Subdiv 1 keeps facets large and chunky; subdiv 2 read too smooth
    # and domed once flat-shaded.
    # Broad, low boulder: smoother facets and a wide flat footprint, which reads as a
    # ground-level ore seam rather than a standing rock.
    main = add_rock(0.62, 2, 0.055, (0, 0, 0.5), rng, flatten=(0.6, 0.95))
    parts.append(main)

    # Shards around the base, placed on a proper polar ring and sunk slightly into
    # the ground so they read as part of the cluster rather than loose props.
    shard_count = rng.randint(3, 5)
    for i in range(shard_count):
        angle = (i / shard_count) * 6.283 + rng.uniform(-0.35, 0.35)
        # Kept tight to the boulder so the shards overlap its base rather than
        # reading as loose props sitting nearby.
        dist = rng.uniform(0.32, 0.45)
        r = rng.uniform(0.15, 0.24)
        import math
        parts.append(add_rock(r, 1, 0.05,
                              (math.cos(angle) * dist, math.sin(angle) * dist, r * 0.45), rng))

    # Ore deposits: large chunks set into the boulder, spread across it rather than
    # clustered. Each is seated below the surface by a fraction of its own radius, so
    # only the crown shows - the buried remainder is what makes it read as ore in the
    # rock instead of a pebble glued on.
    ore_parts = []
    main_verts = [v.co.copy() for v in main.data.vertices if v.co.z > -0.15]
    rng.shuffle(main_verts)

    # Rejection sampling on a minimum separation: plain random picks kept clumping
    # every deposit onto one face.
    chosen = []
    for cand in main_verts:
        if all((cand - c).length > 0.42 for c in chosen):
            chosen.append(cand)
        if len(chosen) >= rng.randint(7, 10):
            break

    for base in chosen:
        direction = base.normalized() if base.length > 0 else Vector((0, 0, 1))

        radius = rng.uniform(0.17, 0.26)
        sink = radius * rng.uniform(0.6, 0.8)
        pos = main.location + base - direction * sink

        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=radius, location=pos)
        nub = bpy.context.active_object
        jitter_mesh(nub, 0.035, rng)
        nub.scale = (rng.uniform(0.85, 1.15), rng.uniform(0.85, 1.15), rng.uniform(0.7, 0.95))
        nub.rotation_euler = (rng.uniform(0, 3.14), rng.uniform(0, 3.14), rng.uniform(0, 3.14))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        ore_parts.append(nub)

    # Assign materials before joining so each slot survives the merge.
    for p in parts:
        p.data.materials.append(rock_mat)
    for p in ore_parts:
        p.data.materials.append(ore_mat)

    bpy.ops.object.select_all(action="DESELECT")
    for p in parts + ore_parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = main
    bpy.ops.object.join()

    node = bpy.context.active_object
    node.name = "OreNode_" + ore_name.capitalize()

    # Faceted look, and origin at the base so it sits on the ground in Unity.
    bpy.ops.object.shade_flat()
    bpy.ops.object.origin_set(type="ORIGIN_GEOMETRY", center="BOUNDS")

    # Normalise on the largest axis, which keeps the broad low profile: the node is
    # wider than it is tall, matching the placeholder sphere's 1.4 footprint.
    dims = node.dimensions
    if max(dims) > 0:
        node.scale = [TARGET_HEIGHT / max(dims)] * 3
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    node.location = (0, 0, node.dimensions.z / 2.0)
    bpy.ops.object.transform_apply(location=True, rotation=False, scale=False)

    return node


def render_preview(path):
    scene = bpy.context.scene

    for engine in ("BLENDER_EEVEE_NEXT", "BLENDER_EEVEE", "BLENDER_WORKBENCH"):
        try:
            scene.render.engine = engine
            break
        except TypeError:
            continue

    if scene.render.engine == "BLENDER_WORKBENCH":
        scene.display.shading.color_type = "MATERIAL"

    # Mid-grey world so unlit faces don't read as pure black in the preview.
    world = bpy.data.worlds.new("PreviewWorld")
    world.use_nodes = True
    bg = world.node_tree.nodes.get("Background")
    if bg:
        bg.inputs[0].default_value = (0.22, 0.23, 0.25, 1.0)
        bg.inputs[1].default_value = 1.0
    scene.world = world

    bpy.ops.object.camera_add(location=(2.6, -2.6, 1.9))
    cam = bpy.context.active_object
    cam.rotation_euler = (1.15, 0, 0.785)
    scene.camera = cam

    # Three-point: key on the camera side, fill opposite, rim behind. The first pass
    # lit only from behind and left the whole front face unreadable.
    bpy.ops.object.light_add(type="SUN", location=(3, -4, 5))
    key = bpy.context.active_object
    key.data.energy = 4.0
    key.rotation_euler = (0.7, 0.2, 0.5)

    bpy.ops.object.light_add(type="SUN", location=(-4, -2, 2))
    fill = bpy.context.active_object
    fill.data.energy = 1.5
    fill.rotation_euler = (1.2, 0.0, -0.9)

    bpy.ops.object.light_add(type="SUN", location=(-2, 4, 3))
    rim = bpy.context.active_object
    rim.data.energy = 2.0
    rim.rotation_euler = (1.0, 0.0, 3.6)

    scene.render.resolution_x = 640
    scene.render.resolution_y = 640
    scene.render.filepath = path
    scene.render.image_settings.file_format = "PNG"
    bpy.ops.render.render(write_still=True)


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    out_dir = argv[0] if argv else "."
    ore = argv[1] if len(argv) > 1 else "iron"
    seed = int(argv[2]) if len(argv) > 2 else 1
    preview = argv[3] if len(argv) > 3 else ""

    node = build(ore, seed)

    tris = sum(len(p.vertices) - 2 for p in node.data.polygons)
    print(f"GEN name={node.name} verts={len(node.data.vertices)} tris={tris} dims={tuple(round(d,3) for d in node.dimensions)}")

    os.makedirs(out_dir, exist_ok=True)
    fbx = os.path.join(out_dir, node.name + ".fbx")
    # FBX_SCALE_ALL is what makes Unity import this at 1:1. With apply_unit_scale
    # alone the mesh arrived 100x too small (bounds ~0.014 for a 1.4m node), because
    # Blender's metre unit and Unity's FBX scale factor compound.
    bpy.ops.export_scene.fbx(filepath=fbx,
                             use_selection=False,
                             apply_unit_scale=True,
                             apply_scale_options="FBX_SCALE_ALL",
                             global_scale=1.0,
                             object_types={"MESH"})
    print("GEN exported", fbx, os.path.exists(fbx))

    if preview:
        render_preview(preview)
        print("GEN preview", preview, os.path.exists(preview))


main()
