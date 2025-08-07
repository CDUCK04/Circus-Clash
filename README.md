UNITY GIT WORKFLOW RULES TO AVOID MERGE CONFLICTS
==================================================

To collaborate smoothly on this Unity project and avoid merge conflicts, follow the rules below.

1. ALWAYS PULL BEFORE YOU PUSH
------------------------------
- Run 'git pull' before pushing any changes.
- Resolve any conflicts locally before pushing.
- Never push unresolved or broken states.

2. ONLY ONE PERSON EDITS A SCENE OR PREFAB AT A TIME
----------------------------------------------------
- Coordinate with team members.
- Assign scene/prefab ownership.
- Avoid working on the same scene or prefab simultaneously.

3. NEVER RENAME OR MOVE FILES UNNECESSARILY
-------------------------------------------
- Renaming or moving files changes their .meta files and can break references.
- Only do this if absolutely necessary and after informing the team.

4. USE FEATURE BRANCHES
-----------------------
- Create a new branch for each feature or bugfix.
- Never commit directly to 'main' or 'dev'.
- Merge via pull requests only.

5. REVIEW AND TEST BEFORE MERGING
---------------------------------
- Test your changes in Unity before opening a PR.
- Make sure changes are functional and do not break others’ work.
- Request reviews from your teammates.

6. COMMIT OFTEN, BUT CLEANLY
----------------------------
- Use clear and descriptive commit messages.
- Group related changes together.
- Avoid committing auto-generated files unless needed.
