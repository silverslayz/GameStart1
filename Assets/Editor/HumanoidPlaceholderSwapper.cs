using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameStart.EditorTools
{
    /// <summary>
    /// Repoints every humanoid prefab at the Human Character Dummy, so all the people in
    /// the game read as obviously placeholder rather than as three different half-finished
    /// art directions.
    ///
    /// Safe to re-run: it swaps the "Model" child and nothing else, so gameplay components,
    /// colliders and the animator controller on each prefab survive untouched. Both rigs are
    /// Humanoid, so the existing StarterAssetsThirdPerson clips retarget onto the dummy
    /// rather than needing to be reauthored.
    /// </summary>
    public static class HumanoidPlaceholderSwapper
    {
        private const string DummyPath = "Assets/Kevin Iglesias/Human Character Dummy/Models/HumanCharacterDummy_F.fbx";
        private const string PrefabFolder = "Assets/Prefabs";
        private const string ModelChildName = "Model";

        /// <summary>
        /// The player is deliberately excluded. Its armature carries PlayerCameraRoot, which
        /// the Cinemachine rig follows by direct reference - swapping the model out from
        /// under that leaves the camera pointing at a destroyed transform. Doing the player
        /// means rehoming that transform too, which deserves its own change.
        /// </summary>
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player.prefab";

        [MenuItem("GameStart/Swap Humanoids To Placeholder Dummy")]
        public static void SwapFromMenu()
        {
            List<string> targets = FindHumanoidPrefabs();

            if (targets.Count == 0)
            {
                EditorUtility.DisplayDialog("Humanoid Placeholder", "No humanoid prefabs found to swap.", "OK");
                return;
            }

            string list = string.Join("\n   ", targets);
            if (!EditorUtility.DisplayDialog("Humanoid Placeholder",
                    $"Repoint these at {System.IO.Path.GetFileNameWithoutExtension(DummyPath)}?\n\n   {list}\n\nThe player is skipped on purpose.",
                    "Swap", "Cancel"))
            {
                return;
            }

            int done = Swap();
            EditorUtility.DisplayDialog("Humanoid Placeholder", $"Swapped {done} prefab(s).", "OK");
        }

        /// <summary>Runs the swap and returns how many prefabs changed. No dialogs.</summary>
        public static int Swap()
        {
            var dummy = AssetDatabase.LoadAssetAtPath<GameObject>(DummyPath);
            if (dummy == null)
            {
                Debug.LogError($"HumanoidPlaceholderSwapper: {DummyPath} not found.");
                return 0;
            }

            int changed = 0;
            foreach (string path in FindHumanoidPrefabs())
            {
                if (SwapOne(path, dummy))
                {
                    changed++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"HumanoidPlaceholderSwapper: repointed {changed} prefab(s) at {dummy.name}.");
            return changed;
        }

        /// <summary>
        /// Humanoid means "the animator is driving a human avatar" rather than a name match,
        /// so a new NPC prefab is picked up without anyone remembering to list it here.
        /// </summary>
        private static List<string> FindHumanoidPrefabs()
        {
            var results = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path == PlayerPrefabPath)
                {
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null || prefab.transform.Find(ModelChildName) == null)
                {
                    continue;
                }

                var animator = prefab.GetComponentInChildren<Animator>(true);
                if (animator != null && animator.avatar != null && animator.avatar.isHuman)
                {
                    results.Add(path);
                }
            }

            results.Sort();
            return results;
        }

        private static bool SwapOne(string path, GameObject dummy)
        {
            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                GameObject root = scope.prefabContentsRoot;
                Transform model = root.transform.Find(ModelChildName);
                if (model == null)
                {
                    return false;
                }

                // Keep how the old model sat and how it was driven; only the art changes.
                Vector3 localPosition = model.localPosition;
                Quaternion localRotation = model.localRotation;
                Vector3 localScale = model.localScale;

                RuntimeAnimatorController controller = null;
                bool applyRootMotion = false;
                AnimatorCullingMode culling = AnimatorCullingMode.CullUpdateTransforms;

                var oldAnimator = model.GetComponent<Animator>();
                if (oldAnimator != null)
                {
                    controller = oldAnimator.runtimeAnimatorController;
                    applyRootMotion = oldAnimator.applyRootMotion;
                    culling = oldAnimator.cullingMode;
                }

                if (PrefabUtility.IsPartOfPrefabInstance(model.gameObject))
                {
                    // The old model is usually itself a prefab instance; it has to be
                    // unpacked before Unity will let it be destroyed inside this prefab.
                    PrefabUtility.UnpackPrefabInstance(
                        PrefabUtility.GetOutermostPrefabInstanceRoot(model.gameObject),
                        PrefabUnpackMode.Completely,
                        InteractionMode.AutomatedAction);
                }

                Object.DestroyImmediate(model.gameObject);

                var replacement = (GameObject)PrefabUtility.InstantiatePrefab(dummy, root.transform);
                replacement.name = ModelChildName;
                replacement.transform.localPosition = localPosition;
                replacement.transform.localRotation = localRotation;
                replacement.transform.localScale = localScale;

                var animator = replacement.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = replacement.AddComponent<Animator>();
                    animator.avatar = FindAvatar(dummy);
                }

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = applyRootMotion;
                animator.cullingMode = culling;

                ClearStaleTintExclusions(root);
            }

            return true;
        }

        private static Avatar FindAvatar(GameObject dummy)
        {
            string path = AssetDatabase.GetAssetPath(dummy);
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Avatar avatar)
                {
                    return avatar;
                }
            }

            return null;
        }

        /// <summary>
        /// CharacterTint keeps a list of renderers to leave untinted. Those pointed at the
        /// old model's renderers, which no longer exist, so the list is emptied rather than
        /// left full of nulls for someone to puzzle over later.
        /// </summary>
        private static void ClearStaleTintExclusions(GameObject root)
        {
            foreach (var component in root.GetComponentsInChildren<Component>(true))
            {
                if (component == null || component.GetType().Name != "CharacterTint")
                {
                    continue;
                }

                var so = new SerializedObject(component);
                var excluded = so.FindProperty("excluded");
                if (excluded != null && excluded.isArray && excluded.arraySize > 0)
                {
                    excluded.ClearArray();
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }
    }
}
