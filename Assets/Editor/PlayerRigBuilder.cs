using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameStart.EditorTools
{
    /// <summary>
    /// Two jobs that have to agree with each other: putting the player on the placeholder
    /// dummy, and giving the open scene a third-person camera that follows them.
    ///
    /// They live together because the camera follows PlayerCameraRoot, which lives inside
    /// the player's model - swap the model carelessly and the camera ends up tracking a
    /// destroyed transform, which is exactly why the player was left out of the first pass.
    /// </summary>
    public static class PlayerRigBuilder
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player.prefab";
        private const string DummyPath = "Assets/Kevin Iglesias/Human Character Dummy/Models/HumanCharacterDummy_F.fbx";
        private const string ModelChildName = "PlayerModel";
        private const string CameraRootName = "PlayerCameraRoot";
        private const string VirtualCameraName = "CM Player Camera";
        private const string LookActionPath = "Player/Look";

        // Behind and above the shoulder, looking at head height.
        private const float CameraDistance = 6f;
        private const float CameraHeight = 1.4f;

        /// <summary>Starting point, not a rule - scroll changes it and it's serialized.</summary>
        private const float DefaultFieldOfView = 100f;

        [MenuItem("GameStart/Player/Swap Player To Placeholder Dummy")]
        public static void SwapPlayerModelFromMenu()
        {
            if (!EditorUtility.DisplayDialog("Player Placeholder",
                    "Repoint Player.prefab's model at the Human Character Dummy?\n\nPlayerCameraRoot is preserved, and the animator/renderer references are re-pointed.",
                    "Swap", "Cancel"))
            {
                return;
            }

            bool ok = SwapPlayerModel();
            EditorUtility.DisplayDialog("Player Placeholder", ok ? "Player is on the dummy." : "Nothing changed - see the console.", "OK");
        }

        [MenuItem("GameStart/Player/Set Up Third Person Camera")]
        public static void BuildCameraRigFromMenu()
        {
            string message = BuildCameraRig();
            EditorUtility.DisplayDialog("Third Person Camera", message, "OK");
        }

        /// <summary>
        /// Replaces the player's body with the dummy, keeping the container that
        /// PlayerCameraRoot hangs off so the camera target survives.
        /// </summary>
        public static bool SwapPlayerModel()
        {
            var dummy = AssetDatabase.LoadAssetAtPath<GameObject>(DummyPath);
            if (dummy == null)
            {
                Debug.LogError($"PlayerRigBuilder: {DummyPath} not found.");
                return false;
            }

            using (var scope = new PrefabUtility.EditPrefabContentsScope(PlayerPrefabPath))
            {
                GameObject root = scope.prefabContentsRoot;
                Transform model = root.transform.Find(ModelChildName);
                if (model == null)
                {
                    Debug.LogError($"PlayerRigBuilder: no '{ModelChildName}' under the player.");
                    return false;
                }

                var oldAnimator = model.GetComponent<Animator>();
                RuntimeAnimatorController controller = oldAnimator != null ? oldAnimator.runtimeAnimatorController : null;

                // Everything under the model except the camera target is body, and goes.
                for (int i = model.childCount - 1; i >= 0; i--)
                {
                    Transform child = model.GetChild(i);
                    if (child.name == CameraRootName)
                    {
                        continue;
                    }

                    Object.DestroyImmediate(child.gameObject);
                }

                // The avatar belongs to the dummy's own root, so the Animator has to live
                // there rather than on the container - a humanoid avatar only maps bones
                // from the transform it was built on.
                if (oldAnimator != null)
                {
                    Object.DestroyImmediate(oldAnimator);
                }

                var body = (GameObject)PrefabUtility.InstantiatePrefab(dummy, model);
                body.name = "Body";
                body.transform.localPosition = Vector3.zero;
                body.transform.localRotation = Quaternion.identity;
                body.transform.localScale = Vector3.one;

                var animator = body.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = body.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;

                RepointReference(root, "PlayerAnimatorBridge", "animator", animator);
                RepointReference(root, "ClassAppearance", "targetRenderer", body.GetComponentInChildren<SkinnedMeshRenderer>(true));

                Debug.Log($"PlayerRigBuilder: player now uses {dummy.name}; camera root preserved.");
            }

            AssetDatabase.SaveAssets();
            return true;
        }

        /// <summary>
        /// Makes sure the open scene has a player, a camera and a Cinemachine rig wired to
        /// follow them. Safe to re-run: existing pieces are reused rather than duplicated.
        /// </summary>
        public static string BuildCameraRig()
        {
            GameObject player = FindOrSpawnPlayer(out string playerNote);
            if (player == null)
            {
                return "No player prefab found, so there's nothing for a camera to follow.";
            }

            Transform target = FindDeep(player.transform, CameraRootName);
            if (target == null)
            {
                // Better a camera that follows the player's middle than no camera at all.
                var created = new GameObject(CameraRootName);
                created.transform.SetParent(player.transform, false);
                created.transform.localPosition = Vector3.up * 1.86f;
                target = created.transform;
            }

            Camera brainCamera = EnsureMainCamera();
            CinemachineCamera vcam = EnsureVirtualCamera(target);

            EditorSceneManager.MarkSceneDirty(vcam.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();

            return $"{playerNote}\nCamera: {brainCamera.name} + {vcam.name}\nFollowing: {target.name}";
        }

        private static GameObject FindOrSpawnPlayer(out string note)
        {
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            {
                if (mb != null && mb.GetType().Name == "PlayerController")
                {
                    note = "Player: already in the scene.";
                    return mb.gameObject;
                }
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                note = "";
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = new Vector3(0f, 0.1f, -14f);
            note = "Player: spawned from the prefab.";
            return instance;
        }

        private static Camera EnsureMainCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                go.tag = "MainCamera";
                camera = go.GetComponent<Camera>();
                camera.transform.position = new Vector3(0f, 3f, -8f);
            }

            if (camera.GetComponent<CinemachineBrain>() == null)
            {
                camera.gameObject.AddComponent<CinemachineBrain>();
            }

            return camera;
        }

        private static CinemachineCamera EnsureVirtualCamera(Transform target)
        {
            CinemachineCamera vcam = null;
            foreach (var existing in Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include))
            {
                vcam = existing;
                break;
            }

            if (vcam == null)
            {
                var go = new GameObject(VirtualCameraName);
                vcam = go.AddComponent<CinemachineCamera>();
            }

            vcam.Target.TrackingTarget = target;
            vcam.Target.LookAtTarget = target;
            vcam.Lens.FieldOfView = DefaultFieldOfView;

            var follow = vcam.GetComponent<CinemachineOrbitalFollow>();
            if (follow == null)
            {
                follow = vcam.gameObject.AddComponent<CinemachineOrbitalFollow>();
            }

            // One ring rather than the default three: a plain shoulder orbit at a fixed
            // height reads predictably, where the 3-ring rig swings the camera down as you
            // look up and needs tuning to stop it clipping the floor.
            follow.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            follow.Radius = CameraDistance;
            follow.TargetOffset = Vector3.up * (CameraHeight - 1f);

            if (vcam.GetComponent<CinemachineRotationComposer>() == null)
            {
                vcam.gameObject.AddComponent<CinemachineRotationComposer>();
            }

            EnsureScrollZoom(vcam.gameObject);
            EnsureLookInput(vcam.gameObject);
            return vcam;
        }

        /// <summary>
        /// Puts the scroll wheel on the lens rather than on camera distance. The component
        /// supports both; on an orbital rig it defaults to moving the camera, which is not
        /// what's wanted here.
        /// </summary>
        private static void EnsureScrollZoom(GameObject vcamObject)
        {
            var zoom = vcamObject.GetComponent<GameStart.CameraSystems.CameraZoomController>();
            if (zoom == null)
            {
                zoom = vcamObject.AddComponent<GameStart.CameraSystems.CameraZoomController>();
            }

            var so = new SerializedObject(zoom);
            var mode = so.FindProperty("zoomTarget");
            if (mode != null)
            {
                mode.enumValueIndex = (int)GameStart.CameraSystems.CameraZoomController.ZoomTarget.FieldOfView;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        /// <summary>
        /// Points the orbit axes at the same Look action the rest of the game uses, so the
        /// camera turns with the mouse instead of sitting frozen behind the player.
        /// </summary>
        private static void EnsureLookInput(GameObject vcamObject)
        {
            var controller = vcamObject.GetComponent<CinemachineInputAxisController>();
            if (controller == null)
            {
                controller = vcamObject.AddComponent<CinemachineInputAxisController>();
            }

            controller.SynchronizeControllers();

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (actions == null)
            {
                Debug.LogWarning("PlayerRigBuilder: InputSystem_Actions not found; camera axes are unbound.");
                return;
            }

            InputAction look = actions.FindAction(LookActionPath);
            if (look == null)
            {
                Debug.LogWarning($"PlayerRigBuilder: no '{LookActionPath}' action; camera axes are unbound.");
                return;
            }

            var reference = InputActionReference.Create(look);
            foreach (var c in controller.Controllers)
            {
                c.Input.InputAction = reference;
            }

            Debug.Log($"PlayerRigBuilder: bound {controller.Controllers.Count} camera axis/axes to {LookActionPath}.");
        }

        private static void RepointReference(GameObject root, string componentType, string field, Object value)
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null || mb.GetType().Name != componentType)
                {
                    continue;
                }

                var so = new SerializedObject(mb);
                var prop = so.FindProperty(field);
                if (prop != null)
                {
                    prop.objectReferenceValue = value;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name)
            {
                return t;
            }

            foreach (Transform child in t)
            {
                Transform found = FindDeep(child, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
