"""
Shared helpers for Aetherfall's low-poly asset generators.

Factored out once a second generator appeared, so scene setup, materials, the
faceted-rock displacement trick, FBX export settings and preview rendering stay
in one place instead of drifting between scripts.

Import from a sibling generator:

    import sys, os
    sys.path.append(os.path.dirname(os.path.abspath(__file__)))
    import lowpoly
"""

import bpy
import bmesh
import os
import random
from mathutils import Vector


def clear_scene():
    """Empties the default scene and drops orphaned datablocks."""
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for block in (bpy.data.meshes, bpy.data.materials, bpy.data.objects):
        for item in list(block):
            if item.users == 0:
                try:
                    block.remove(item)
                except (RuntimeError, ReferenceError):
                    pass


def make_material(name, colour, roughness=0.85, metallic=0.0):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = colour
        bsdf.inputs["Roughness"].default_value = roughness
        if "Metallic" in bsdf.inputs:
            bsdf.inputs["Metallic"].default_value = metallic
    mat.diffuse_color = colour  # drives the workbench/viewport preview
    return mat


def jitter_mesh(obj, amount, rng):
    """Random per-vertex displacement. Flat-shaded afterwards this is what gives
    the faceted low-poly look, and it costs no extra triangles."""
    mesh = obj.data
    bm = bmesh.new()
    bm.from_mesh(mesh)
    for v in bm.verts:
        v.co += Vector((rng.uniform(-amount, amount),
                        rng.uniform(-amount, amount),
                        rng.uniform(-amount, amount)))
    bm.to_mesh(mesh)
    bm.free()


def join(objects, active):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objects:
        o.select_set(True)
    bpy.context.view_layer.objects.active = active
    bpy.ops.object.join()
    return bpy.context.active_object


def seat_on_ground(obj):
    """Moves the origin to the object's base so it sits on the ground when placed
    at y=0 in Unity, rather than half-sunk."""
    bpy.ops.object.origin_set(type="ORIGIN_GEOMETRY", center="BOUNDS")
    obj.location = (0, 0, obj.dimensions.z / 2.0)
    bpy.ops.object.transform_apply(location=True, rotation=False, scale=False)


def normalise_height(obj, target_height):
    if obj.dimensions.z > 0:
        obj.scale = [target_height / obj.dimensions.z] * 3
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)


def export_fbx(obj, out_dir, name):
    """
    FBX_SCALE_ALL is what makes Unity import at 1:1. With apply_unit_scale alone
    meshes arrive 100x too small, because Blender's metre unit and Unity's FBX
    scale factor compound.
    """
    os.makedirs(out_dir, exist_ok=True)
    path = os.path.join(out_dir, name + ".fbx")
    bpy.ops.export_scene.fbx(filepath=path,
                             use_selection=False,
                             apply_unit_scale=True,
                             apply_scale_options="FBX_SCALE_ALL",
                             global_scale=1.0,
                             object_types={"MESH"})
    return path


def render_preview(path, camera_distance=6.0, height=2.5, look_at_z=None):
    """Three-point lit preview. A single light leaves whole faces black and makes
    the result impossible to judge.

    The camera is aimed with a Track To constraint rather than a hand-set rotation:
    fixed angles framed short props fine but cut the top off anything tall."""
    scene = bpy.context.scene

    for engine in ("BLENDER_EEVEE_NEXT", "BLENDER_EEVEE", "BLENDER_WORKBENCH"):
        try:
            scene.render.engine = engine
            break
        except TypeError:
            continue
    if scene.render.engine == "BLENDER_WORKBENCH":
        scene.display.shading.color_type = "MATERIAL"

    world = bpy.data.worlds.new("PreviewWorld")
    world.use_nodes = True
    bg = world.node_tree.nodes.get("Background")
    if bg:
        bg.inputs[0].default_value = (0.22, 0.23, 0.25, 1.0)
    scene.world = world

    d = camera_distance
    bpy.ops.object.camera_add(location=(d, -d, height))
    cam = bpy.context.active_object
    scene.camera = cam

    # Aim at the middle of the subject so tall objects stay in frame.
    target_z = look_at_z if look_at_z is not None else height * 0.4
    bpy.ops.object.empty_add(location=(0, 0, target_z))
    target = bpy.context.active_object
    track = cam.constraints.new(type="TRACK_TO")
    track.target = target
    track.track_axis = "TRACK_NEGATIVE_Z"
    track.up_axis = "UP_Y"

    for loc, rot, energy in (
        ((3, -4, 5), (0.7, 0.2, 0.5), 4.0),
        ((-4, -2, 2), (1.2, 0.0, -0.9), 1.5),
        ((-2, 4, 3), (1.0, 0.0, 3.6), 2.0),
    ):
        bpy.ops.object.light_add(type="SUN", location=loc)
        light = bpy.context.active_object
        light.data.energy = energy
        light.rotation_euler = rot

    scene.render.resolution_x = 640
    scene.render.resolution_y = 640
    scene.render.filepath = path
    scene.render.image_settings.file_format = "PNG"
    bpy.ops.render.render(write_still=True)


def report(obj):
    tris = sum(len(p.vertices) - 2 for p in obj.data.polygons)
    dims = tuple(round(d, 3) for d in obj.dimensions)
    print("GEN name=%s verts=%d tris=%d dims=%s" % (obj.name, len(obj.data.vertices), tris, dims))
    return tris
