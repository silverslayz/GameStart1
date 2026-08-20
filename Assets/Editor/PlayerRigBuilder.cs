using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GameStart.CameraSystems;

namespace GameStart.EditorTools
{
    /// <summary>
    /// Two jobs that have to agree with each other: putting the player on the placeholder
    /// dummy, and giving the open scene a camera that follows them.
    ///
    /// They live together because the model swap decides what the camera has left to follow.
    /// </summary>
    public static class PlayerRigBuilder
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player.prefab";
        private const string DummyPath = "Assets/Kevin Iglesias/Human Character Dummy/Models/HumanCharacterDummy_F.fbx";
        private const string ModelChildName = "PlayerModel";
        private const string CameraRootName = "PlayerCameraRoot";

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
            EditorUtility.DisplayDialog("Third Person Camera", BuildCameraRig(), "OK");
        }

        /// <summary>
        /// Replaces the player's body with the dummy, keeping the container that
        /// PlayerCameraRoot hangs off so anything following it survives.
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

                for (int i = model.childCount - 1; i >= 0; i--)
                {
                    Transform child = model.GetChild(i);
                    if (child.name != CameraRootName)
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                }

                // The avatar belongs to the dummy's own root, so the Animator has to live
                // there - a humanoid avatar only maps bones from the transform it was built on.
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
        /// Makes sure the open scene has a player and a camera that follows them. Safe to
        /// re-run: existing pieces are reused rather than duplicated.
        /// </summary>
        public static string BuildCameraRig()
        {
            GameObject player = FindOrSpawnPlayer(out string playerNote);
            if (player == null)
            {
                return "No player prefab found, so there's nothing for a camera to follow.";
            }

            string removed = RemoveCinemachineRig();

            Camera camera = EnsureMainCamera();
            var follow = camera.GetComponent<ThirdPersonCamera>();
            if (follow == null)
            {
                follow = camera.gameObject.AddComponent<ThirdPersonCamera>();
            }

            // Orbit the player's own transform rather than PlayerCameraRoot: that root sits
            // inside the model and turns when the player turns, which drags the camera round
            // with it. The camera lifts itself to eye level instead.
            var so = new SerializedObject(follow);
            var targetProp = so.FindProperty("target");
            if (targetProp != null)
            {
                targetProp.objectReferenceValue = player.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(camera.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();

            return $"{playerNote}\nCamera: {camera.name} with ThirdPersonCamera\nFollowing: {player.name}{removed}";
        }

        /// <summary>
        /// Strips the Cinemachine rig. Leaving it means two systems both placing the same
        /// camera, and whichever writes last wins at random.
        /// </summary>
        private static string RemoveCinemachineRig()
        {
            int removed = 0;

            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            {
                if (mb == null)
                {
                    continue;
                }

                string type = mb.GetType().Name;
                bool isCinemachine = type.StartsWith("Cinemachine");
                bool isOldLook = type == "PlayerCameraLook";

                if (!isCinemachine && !isOldLook)
                {
                    continue;
                }

                // A virtual camera is its own object; a brain rides on the real camera.
                if (isCinemachine && mb.GetComponent<Camera>() == null && mb.transform.parent == null)
                {
                    Object.DestroyImmediate(mb.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(mb);
                }

                removed++;
            }

            return removed > 0 ? $"\nRemoved {removed} leftover camera component(s)." : "";
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
            }

            if (camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }

            return camera;
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
    }
}
