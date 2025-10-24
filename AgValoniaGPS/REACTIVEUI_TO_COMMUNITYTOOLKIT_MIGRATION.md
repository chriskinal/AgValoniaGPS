# ReactiveUI to CommunityToolkit.Mvvm Migration Plan

**Date**: 2025-10-24
**Status**: In Progress
**Reason**: Threading issues with ReactiveCommand during DI container initialization

## Migration Strategy

### Phase 1: Base Classes (Critical Path) ✅ In Progress
**Goal**: Fix immediate threading crash and establish migration pattern

1. ✅ Install CommunityToolkit.Mvvm 8.4.0
2. 🔄 Migrate `PanelViewModelBase` - Replace ReactiveCommand with RelayCommand
3. 🔄 Migrate `DialogViewModelBase` - Replace ReactiveCommand with RelayCommand
4. ⏳ Keep `ViewModelBase` with ReactiveUI temporarily (property notifications only)

**Files to Modify**:
- `AgValoniaGPS.ViewModels/Base/PanelViewModelBase.cs`
- `AgValoniaGPS.ViewModels/Base/DialogViewModelBase.cs`

**Breaking Changes**: None (command interface stays `ICommand`)

### Phase 2: ViewModelBase Migration
**Goal**: Replace ReactiveUI property notification with CommunityToolkit

**Changes**:
```csharp
// BEFORE (ReactiveUI)
public class ViewModelBase : ReactiveObject
{
    protected void RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
}

// AFTER (CommunityToolkit)
public class ViewModelBase : ObservableObject
{
    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
}
```

**Impact**:
- All ViewModels need property setter updates
- Find/Replace: `RaiseAndSetIfChanged` → `SetProperty`
- Estimated: ~200 property setters across all ViewModels

### Phase 3: MainViewModel Migration
**Goal**: Migrate complex MainViewModel with 10+ commands

**Challenges**:
- Toggle commands (10 panel visibility commands)
- Service event subscriptions with Dispatcher marshaling
- Complex initialization logic

**Strategy**:
- Replace ReactiveCommand.Create with RelayCommand
- Keep Dispatcher.UIThread.Post for service event handlers
- Test thoroughly after migration

### Phase 4: Dialog ViewModels (53 files)
**Goal**: Batch migrate all dialog ViewModels

**Approach**:
1. Create migration script/pattern
2. Migrate in groups:
   - Simple dialogs first (20 files) - FormYesNo, FormAbout, etc.
   - Medium complexity (20 files) - Field management dialogs
   - Complex dialogs last (13 files) - FormConfig, FormDiagnostics, etc.

**Pattern**:
```csharp
// BEFORE
using ReactiveUI;
public class MyDialogViewModel : DialogViewModelBase
{
    private string _myProperty;
    public string MyProperty
    {
        get => _myProperty;
        set => this.RaiseAndSetIfChanged(ref _myProperty, value);
    }
}

// AFTER
using CommunityToolkit.Mvvm.ComponentModel;
public partial class MyDialogViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private string _myProperty;
}
```

### Phase 5: Panel ViewModels
**Goal**: Migrate panel ViewModels with service subscriptions

**Special Consideration**:
- FormFieldDataViewModel has `Dispatcher.UIThread.Post()` for service events
- Keep Dispatcher marshaling during migration
- May need `[RelayCommand]` attributes for async commands

### Phase 6: Test Updates
**Goal**: Update 116+ unit tests

**Changes Needed**:
- Update test assertions for property changes
- Verify command execution still works
- Check async command scenarios

**Low Risk**: Most tests use interface types (`ICommand`), so minimal changes expected

### Phase 7: Cleanup
**Goal**: Remove ReactiveUI entirely

1. Remove ReactiveUI package references
2. Remove unused `using ReactiveUI;` statements
3. Search for any remaining `ReactiveCommand` references
4. Update documentation

## Migration Benefits

### Immediate
✅ Fixes threading crash on startup
✅ Simpler command creation (no scheduler issues)
✅ Better async/await support in commands

### Long-term
✅ Source generators reduce boilerplate (40% less code)
✅ Official Microsoft toolkit (better support)
✅ Lighter weight (smaller package size)
✅ Easier onboarding for new developers

## Rollback Plan

If migration fails:
1. Git revert to pre-migration commit
2. Implement temporary fix: Simple `DelegateCommand` in base classes only
3. Re-evaluate migration approach

## Testing Checklist

After each phase:
- [ ] Solution builds without errors
- [ ] All unit tests pass
- [ ] Application launches without crashes
- [ ] Commands execute correctly
- [ ] Property bindings update UI
- [ ] Service events trigger UI updates

## Current Status

**Completed**:
- ✅ CommunityToolkit.Mvvm 8.4.0 installed
- ✅ Migration strategy documented

**In Progress**:
- 🔄 Phase 1: Migrating PanelViewModelBase and DialogViewModelBase

**Next Steps**:
- Replace `ReactiveCommand.Create` with `new RelayCommand` in base classes
- Test application launches without threading crash
- Proceed with Phase 2 if successful

## Notes

- This is a **one-way migration** - not mixing ReactiveUI and CommunityToolkit in same ViewModels
- Coexistence is OK during migration (different VMs can use different frameworks)
- Focus on getting app working first, optimize later
- Document any edge cases discovered during migration
