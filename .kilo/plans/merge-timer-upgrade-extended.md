# Merge Plan: TimerUpgradeExtended → main

## Executive Summary

**Decision**: Merge TimerUpgradeExtended into main, then fix critical bugs in a follow-up branch.

## Branch Analysis

### Main Branch Issues
- **6 Critical** bugs (double loading, duplicate quest addition, recursive calls)
- **8 Major** issues (singleton issues, performance, hard-coded values)
- **8 Minor** issues (typos, dead code)

### TimerUpgradeExtended Branch Issues
- **4 Critical** bugs (crashes, data loss potential)
- **5 High** priority issues (race conditions, missing validation)
- **Multiple** commented-out code blocks indicating incomplete features

### Key Changes in TimerUpgradeExtended
1. Timer restoration and persistence
2. Hero selection state management (deferred loading pattern)
3. Quest tracking improvements (`heroesForQuest` field)
4. Refactored `PannelManager` and `HeroSelectionForQuestUI`
5. Fixed recursive call bug in `GoQuest()`

## Merge Plan

### Phase 1: Pre-Merge Preparation
1. Ensure main branch is clean (no uncommitted changes)
2. Fetch latest TimerUpgradeExtended branch
3. Create backup branch of main

### Phase 2: Execute Merge
```bash
git checkout main
git pull origin main
git merge TimerUpgradeExtended
```

### Phase 3: Resolve Conflicts (if any)
Expected conflict areas:
- `Assets/Scripts/Managers/SaveManager.cs`
- `Assets/Scripts/UI/PannelManager.cs`
- `Assets/Scripts/UI/HeroSelectionForQuestUI.cs`
- `Assets/save.json` (should be regenerated)

### Phase 4: Post-Merge Verification
1. Open project in Unity
2. Check for compilation errors
3. Run basic gameplay test (hero selection, quest start, save/load)

### Phase 5: Create Fix Branch
```bash
git checkout -b fix/timer-upgrade-critical-bugs
```

## Critical Bugs to Fix Post-Merge

### Priority 1: Crash Prevention
| Bug | File | Line | Fix |
|-----|------|------|-----|
| Null GameManager access | OngoingQuest.cs | 70 | Add null check before `RestoreButtons` |
| Missing bounds check | HeroSelectionForQuestUI.cs | 145 | Validate `ind < itemButtons.Count` |
| isLoaded set too early | SaveManager.cs | 207 | Move flag to end of LoadGame() |

### Priority 2: Data Integrity
| Bug | File | Fix |
|-----|------|-----|
| Dual storage inconsistency | OngoingQuest.cs + SaveManager.cs | Consolidate to single storage (JSON) |
| OnQuestComplete never called | PannelManager.cs:409 | Uncomment or properly integrate |
| heroesForQuest empty in saves | QuestData.cs | Populate when quest starts |

### Priority 3: Code Quality
| Issue | Files | Action |
|-------|-------|--------|
| Commented code blocks | PannelManager.cs, HeroSelectionForQuestUI.cs | Remove or implement |
| Typos in class names | PannelManager → PanelManager | Consider rename |
| Missing error handling | SaveManager.cs | Add try-catch for file I/O |

## Recommended Post-Merge Actions

1. **Create GitHub Issue** tracking these critical bugs
2. **Create fix branch** and address Priority 1 bugs immediately
3. **Add unit tests** for save/load system
4. **Document** the initialization order requirements
5. **Consider save data versioning** for future compatibility

## Merge Commands

```bash
# Pre-merge backup
git checkout main
git branch backup-main-$(date +%Y%m%d)

# Merge
git merge TimerUpgradeExtended -m "Merge TimerUpgradeExtended: Timer restoration and hero selection improvements"

# Push (if successful)
git push origin main
```

## Rollback Plan

If merge causes critical issues:
```bash
git reset --hard HEAD~1
# Or restore from backup
git checkout backup-main-YYYYMMDD
```

## Files Changed Summary

| File | Lines Changed | Risk |
|------|---------------|------|
| SaveManager.cs | +111/-84 | High |
| HeroSelectionForQuestUI.cs | +200/-156 | High |
| PannelManager.cs | +304/-400 | Medium |
| OngoingQuest.cs | +1 | Low |
| QuestData.cs | +2 | Low |
| UpgradeCounter.cs | +4/-4 | Low |

## Success Criteria

- [ ] Merge completes without conflicts
- [ ] Project compiles in Unity
- [ ] Basic gameplay functions (hero selection, quests, save/load)
- [ ] No console errors on startup
- [ ] Critical bugs documented in GitHub issue
