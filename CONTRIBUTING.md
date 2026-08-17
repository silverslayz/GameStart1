# Contributing to GameStart1 (Aetherfall)

Thanks for jumping in. This is a Unity project under Git version control, so a
few Unity-specific habits matter more here than in a typical repo.

## 1. Get repo access

You need to be added as a collaborator on the GitHub repo (or invited to the
org, if we set one up). Ask the repo owner for an invite if you don't have
push access yet.

## 2. Install the exact same Unity Editor version

Everyone must use the same Unity Editor version as the project, or Unity will
silently upgrade/reserialize files the moment you open it - producing huge,
unreviewable diffs that have nothing to do with your actual change.

Current required version (check `ProjectSettings/ProjectVersion.txt` if this
ever goes stale):

```
6000.5.8f1
```

Install it via Unity Hub -> Installs -> Install Editor Version, and make sure
this project opens with that version specifically.

## 3. Install Git LFS

Textures, models, audio, and fonts are tracked through Git LFS (see
`.gitattributes`). Without LFS set up locally, those files show up as broken
text pointers instead of the real asset.

```bash
git lfs install
```

Run this once per machine, before your first clone/pull of the repo.

## 4. Enable Unity's Smart Merge for scenes and prefabs

`.unity` and `.prefab` files are YAML, and Git's default line-based merge
will corrupt them if two people touch the same file. `.gitattributes` already
routes these through `unityyamlmerge`, but you still need to point Git at it
locally.

**Easiest way:** Unity Editor -> Preferences (or Edit -> Preferences on
Windows) -> External Tools -> check **Enable** under "Version Control /
Smart Merge". Unity configures the merge driver for you.

**Manual way**, if you'd rather set it directly:

```bash
git config merge.unityyamlmerge.driver \
  '"<path-to-Unity-Editor>/Data/Tools/UnityYAMLMerge" merge -p "%O" "%A" "%B" "%A"'
```

On Windows this is typically something like:
`C:\Program Files\Unity\Hub\Editor\6000.5.8f1\Editor\Data\Tools\UnityYAMLMerge.exe`

## 5. The real rule: avoid concurrent edits to the same scene

Smart Merge helps, but it isn't magic - the safest habit by far is **don't
have two people editing `Assets/Scenes/SampleScene.unity` (or the same
prefab) at the same time.** Before you start a scene edit:

- Say so (chat, issue comment, whatever) so nobody else opens the same scene.
- Prefer building new features as their own prefabs/scripts where possible,
  and only touch the shared scene to wire references in - that shrinks the
  overlap window.
- Pull before you start, and push/merge promptly once you're done, rather
  than sitting on scene changes for a long time.

## 6. Workflow

This repo follows a standard branch + PR flow:

1. Create a branch off `main` for your change.
2. Commit as you go, with messages that explain *why*, not just *what*.
3. Open a PR into `main` with a summary of what changed and how you tested it
   (in-editor Play mode testing is the norm here - actually verify your
   change works before opening the PR, not just that it compiles).
4. Merge once it's reviewed (or self-merge for small, clearly-tested changes
   if that's the convention the team is using at the time).

## 7. Backlog

Work is tracked as GitHub issues: `[Epic]`-labeled issues are the major
systems, with individual `story`-labeled issues underneath them. Check the
"GameStart1 Backlog" project board for what's open and unclaimed before
starting something new, so two people don't build the same thing.
