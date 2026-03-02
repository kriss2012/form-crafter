# Form Crafter - Priority TODO List

⚠️ **ARCHIVED (2025-10-22):** This file has been consolidated into TODO.md. All completed items have been marked as done in the main TODO file. Please refer to TODO.md for the current development roadmap.

---

## Phase 1: Testing & Quality (Current Focus)

### ✅ COMPLETED
- [x] Set up test project with xUnit, bUnit, Moq, FluentAssertions
- [x] Add unit tests for LocalStorageService (20+ tests)
- [x] Add unit tests for HistoryService (20+ tests)
- [x] Add unit tests for FormElement models (30+ tests)

### 🔴 HIGH PRIORITY - Immediate Fixes

#### 1. Fix 23 Failing Unit Tests
**Status:** ✅ COMPLETED
**Effort:** 1-2 hours (Actual: 1.5 hours)
**Impact:** Critical - needed for CI/CD and code confidence

- [x] Fix default value mismatches (colors: "#ffffff" vs "#FFFFFF")
- [x] Fix nullable property assertions (MaxLength, MaxFileSize)
- [x] Fix JSRuntime mock setup for LocalStorageService
- [x] Verify all 71 tests pass
- [ ] Add test run to CI pipeline

#### 2. Advanced Styling UI (Phase 2.2)
**Status:** ✅ COMPLETED
**Effort:** 4-6 hours (Actual: 4 hours)
**Impact:** High - completes Phase 2 features

- [x] Add border controls to Properties Panel (style, width, color, radius)
- [x] Add background controls (color, opacity)
- [x] Add shadow controls (box-shadow presets: none, small, medium, large)
- [x] Add spacing controls (padding, margin with individual sides)
- [x] Add opacity slider (0-100%)
- [x] Live preview of style changes on canvas

#### 3. Form Validation Framework (Phase 2.3)
**Status:** ✅ COMPLETED
**Effort:** 6-8 hours (Actual: 5 hours)
**Impact:** High - essential for production forms

- [x] Create validation service/system
- [x] Add required field validation
- [x] Add input type validation (email, phone, URL, number ranges)
- [x] Add custom validation rules support
- [x] Add form-level validation summary
- [x] Unit tests for validation logic (34 new tests)
- [ ] Add validation error display in preview mode (deferred to Phase 2.5)

#### 4. Accessibility Improvements (WCAG 2.1 AA)
**Status:** ✅ COMPLETED (Phase 1 & 2)
**Effort:** 8-10 hours (Actual: 8 hours)
**Impact:** High - legal compliance & inclusive design

**Phase 1 (Completed):**
- [x] Add ARIA labels to all canvas elements
- [x] Add screen reader announcements for drag/drop actions
- [x] Implement proper tab order management
- [x] Add focus management for dialogs/modals
- [x] Add alt text field to ImageElement
- [x] Add keyboard shortcuts help dialog

**Phase 2 (Completed 2025-10-21):**
- [x] Set up axe-core accessibility testing infrastructure
- [x] Create AccessibilityAudit.razor page for automated testing
- [x] Implement WCAG 2.1 AA compliance scanning
- [x] Fix keyboard accessibility issues:
  - [x] ElementLibrary buttons now keyboard accessible (Enter/Space)
  - [x] SharedAppBar logo converted to proper button
  - [x] All interactive elements have role="button" and tabindex="0"
- [x] Add semantic HTML landmarks:
  - [x] <main> landmark on Forms and Editor pages
  - [x] <nav> landmark for navigation bars
  - [x] <aside> landmarks for sidebars
  - [x] role="toolbar" for Editor toolbar
- [x] Improve ARIA labels:
  - [x] Navigation regions properly labeled
  - [x] Form elements library labeled
  - [x] Properties panel labeled
  - [x] Decorative icons marked with aria-hidden="true"

**Phase 3 (Completed 2025-10-22):**
- [x] Created floating AccessibilityAuditWidget for testing all pages
- [x] Run full accessibility audit on all pages
- [x] Fixed all CRITICAL violations (2 total):
  - [x] Forms page: Added aria-label to Delete button (Forms.razor:115-116)
  - [x] Editor page: Fixed aria-required-children by converting Add Page div to button (PageTabs.razor:31-37)
- [x] Fixed modal dialog issues:
  - [x] ConfirmDialog.razor: Fixed cascading parameter to use IMudDialogInstance
  - [x] Forms.razor & Editor.razor: Fixed dialog result checking with proper pattern matching
- [ ] DEFERRED: Fix 16 SERIOUS color-contrast violations (Forms: 6, Editor: 6, Preview: 4)
  - Note: Color contrast issues deferred as they require design review and are not blocking

---

## Phase 2: Missing Phase 2 Features

### 🟡 MEDIUM PRIORITY

#### 5. Preview Mode (Phase 2.5)
**Status:** ✅ COMPLETED
**Effort:** 4-6 hours (Actual: 2 hours)
**Impact:** Medium - user testing essential

- [x] Implement functional form preview (fillable fields)
- [x] Add desktop preview mode
- [x] Add tablet preview mode (768px)
- [x] Add mobile preview mode (375px)
- [ ] Add print preview mode (deferred)
- [x] Add preview toolbar with device switcher

#### 6. Element Rotation (Phase 2.4)
**Status:** ✅ COMPLETED
**Effort:** 3-4 hours (Actual: 2 hours)
**Impact:** Medium - nice-to-have feature

- [x] Add rotation handle to selected elements
- [x] Add rotation angle input in Properties Panel
- [x] Update CSS transform to include rotation
- [x] Add rotation to undo/redo history
- [x] Test rotation with resize handles

#### 7. Large Component Refactoring
**Status:** DEFERRED (Optional)
**Effort:** 10-12 hours
**Impact:** Medium - improves maintainability
**Reason:** Current codebase is stable (105/105 tests passing). Refactoring would be time-consuming with marginal immediate benefit. JavaScript handlers already separated.

- [ ] Break Canvas.razor.cs into smaller components
  - [ ] DragDropHandler component
  - [ ] ResizeHandler component
  - [ ] AlignmentGuides component
  - [ ] ElementRenderer (factory pattern)
- [ ] Replace switch statements with factory pattern
- [ ] Extract element rendering logic
- [ ] Add component-level unit tests

---

## Phase 3: New Features & Enhancements

### 🟢 LOW PRIORITY

#### 8. Multi-Page Form Support
**Status:** TODO
**Effort:** 8-10 hours
**Impact:** Low - future enhancement

- [ ] Add page management UI
- [ ] Add page navigation controls
- [ ] Support page breaks in forms
- [ ] Add page templates
- [ ] Test multi-page serialization

#### 9. Mobile Touch Support
**Status:** TODO
**Effort:** 6-8 hours
**Impact:** Low - mobile editing nice-to-have

- [ ] Add touch event handlers for drag/drop
- [ ] Optimize canvas for mobile viewports
- [ ] Add pinch-to-zoom for mobile
- [ ] Test on actual mobile devices

#### 10. Performance Optimization
**Status:** TODO
**Effort:** 8-10 hours
**Impact:** Low - optimize when needed

- [ ] Implement centralized state management (Fluxor or similar)
- [ ] Add debouncing to auto-save (currently 30s)
- [ ] Add virtual scrolling for element library
- [ ] Add memoization for expensive renders
- [ ] Load test with 100+ element forms
- [ ] Profile and optimize bottlenecks

---

## Phase 4: Backend Integration (Phase 3-7 from roadmap)

### ⚪ FUTURE WORK

#### 11. Backend API Setup
- [ ] Create ASP.NET Core Web API project
- [ ] Set up Entity Framework Core
- [ ] Create database schema (PostgreSQL/Azure SQL)
- [ ] Implement form CRUD endpoints
- [ ] Add authentication endpoints

#### 12. User Authentication
- [ ] Implement JWT authentication
- [ ] Add user registration/login
- [ ] Add password reset flow
- [ ] Implement role-based access control

#### 13. Form Sharing & Submissions
- [ ] Add public form links
- [ ] Implement form submission endpoint
- [ ] Create submission storage
- [ ] Add form response viewer

#### 14. PDF Export
- [ ] Research PDF generation libraries
- [ ] Implement form-to-PDF converter
- [ ] Add PDF download feature

#### 15. Azure Deployment
- [ ] Set up Azure Static Web Apps
- [ ] Configure Azure Functions
- [ ] Deploy database to Azure
- [ ] Set up CI/CD pipeline

---

## Current Sprint: Quality & Accessibility
**Goal:** Achieve production-ready quality with accessibility compliance

**Completed:**
1. ✅ Fix 23 failing tests (100% pass rate)
2. ✅ Add advanced styling UI
3. ✅ Complete validation framework (34 tests)
4. ✅ Accessibility improvements - Phase 1 (ARIA, screen readers, keyboard shortcuts)
5. ✅ Preview mode implementation (Phase 2.5)
6. ✅ Element rotation (Phase 2.4)
7. ✅ Accessibility audit infrastructure (axe-core integration)
8. ✅ WCAG 2.1 AA compliance improvements - Phase 2

**Next Sprint:**
1. Run full accessibility audits on all pages
2. Fix critical/serious WCAG violations
3. Large component refactoring (Phase 2) - Optional
4. Multi-page form support or Performance optimization

---

## Metrics & Goals

**Test Coverage:**
- Current: 105 tests, 105 passing (100%) ✅
- Target: 105 tests, 105 passing (100%) ✅ ACHIEVED
- Future: 150+ tests with component & E2E tests

**Code Quality:**
- Current: No linting, some large components
- Target: ESLint setup, max 300 lines per component

**Accessibility:**
- Current: ARIA labels, screen reader support, keyboard navigation, tab order
- Target: WCAG 2.1 AA compliant (Phase 1 complete, audit pending)

**Performance:**
- Current: Not tested
- Target: Support 50+ elements with smooth UX

---

## 🐛 Manual Testing - Bug Fixes

**Bug Fix Sprint Summary:**
- **9 of 9 bugs fixed** (100% completion rate)
- **3 CRITICAL** bugs resolved
- **3 HIGH** priority bugs resolved
- **3 MEDIUM** priority bugs resolved
- **1 LOW** priority bug deferred (Print Preview - marked as optional in Phase 2.5)

All bugs fixed in a single session on **2025-10-20**.

---

### 🔴 CRITICAL (Blocks Core Functionality) - ALL FIXED ✅

#### BUG-1: Undo Causes Null Reference Exception ✅
**Status:** COMPLETED (2025-10-20)
**Severity:** CRITICAL
**Impact:** Application crashes when using undo
**Error:** "Unhandled exception rendering component: Object reference not set to an instance of an object"
**Location:** Editor.razor / HistoryService
**Fix:** Added `currentTemplate.ClearSelection()` and `InvokeAsync(StateHasChanged)` in both Undo() and Redo() methods to properly clear selections and force canvas refresh (Editor.razor:943-979)

#### BUG-2: Element Resize Not Working ✅
**Status:** COMPLETED (2025-10-20)
**Severity:** CRITICAL
**Impact:** Cannot resize elements by dragging handles
**Location:** Canvas.razor.cs / resizeHandler.js
**Fix:** Fixed coordinate conversion in startResize() method by converting client coordinates to canvas-relative coordinates using getBoundingClientRect() (resizeHandler.js:24-43)

#### BUG-3: Delete Form Not Working ✅
**Status:** COMPLETED (2025-10-20)
**Severity:** CRITICAL
**Impact:** Cannot delete forms from My Forms page
**Location:** Forms.razor
**Fix:** Changed from @bind-IsVisible pattern to conditional rendering with @if statement for MudDialog (Forms.razor:120-142)

---

### 🟡 HIGH PRIORITY (Major Features Broken) - ALL FIXED ✅

#### BUG-4: Multi-Select Copy/Paste Only Copies One Element ✅
**Status:** COMPLETED (2025-10-20)
**Severity:** HIGH
**Impact:** Multi-element operations broken
**Location:** Editor.razor - OnCopy/OnPaste/OnDuplicate methods
**Fix:** Changed from single `clipboardElement` to `List<FormElement> clipboardElements`. Updated CopySelectedElement(), PasteElement(), and DuplicateSelectedElement() to handle multiple elements using LINQ `.Where(e => e.IsSelected)` (Editor.razor:849, 1318-1430)

#### BUG-5: Ctrl+A Selects Page Text Instead of Canvas Elements ✅
**Status:** COMPLETED (2025-10-20)
**Severity:** HIGH
**Impact:** Keyboard shortcut doesn't work as expected
**Location:** keyboardShortcuts.js
**Fix:** Added Ctrl+A handler in keyboardShortcuts.js with e.preventDefault() and created OnSelectAll() method in Editor.razor that selects all canvas elements (keyboardShortcuts.js:71-77, Editor.razor:1477-1491)

#### BUG-6: Preview Mode Not Responsive (Mobile/Tablet) ✅
**Status:** COMPLETED (2025-10-20)
**Severity:** HIGH
**Impact:** Preview mode device switching doesn't work correctly
**Location:** PreviewMode.razor - GetScaleFactor/device width
**Fix:** Removed CSS transform approach and implemented proper mathematical scaling. Added GetScaledWidth() and GetScaledHeight() methods, scaled element positions/dimensions in render loop, and accounted for padding in scale factor calculation (PreviewMode.razor:45-126)

---

### 🟢 MEDIUM PRIORITY (UX Issues) - ALL FIXED ✅

#### BUG-7: Toast Messages Block Preview Button ✅
**Status:** COMPLETED (2025-10-20)
**Severity:** MEDIUM
**Impact:** Annoying UX - toast overlaps UI controls
**Location:** Editor.razor - Snackbar position
**Fix:** Configured MudBlazor Snackbar globally in Program.cs to BottomRight position with MaxDisplayedSnackbars=3, ShowCloseIcon=true, and VisibleStateDuration=3000ms (Program.cs:13-22)

#### BUG-8: Properties Panel Border Highlighting Issue ✅
**Status:** COMPLETED (2025-10-20)
**Severity:** MEDIUM
**Impact:** Visual bug when expanding collapsible sections
**Location:** Editor.razor - Properties panel styling
**Fix:** Added Elevation="0" and DisableBorders="true" to MudExpansionPanels component to remove border highlighting (Editor.razor:592)

#### BUG-9: Duplicate Success Toast Only Shows in Editor ✅
**Status:** COMPLETED (2025-10-20)
**Severity:** MEDIUM
**Impact:** Inconsistent feedback - toast should show on My Forms page too
**Location:** Forms.razor - HandleDuplicate method
**Fix:** Verified that Snackbar.Add("Form duplicated successfully!", Severity.Success) already exists in Forms.razor:209. Toast was appearing but likely being missed due to positioning. Fixed by BUG-7 toast positioning changes - toast now appears in BottomRight where it's visible.

---

### ⚪ LOW PRIORITY (Deferred Features)

#### BUG-10: Print Preview Not Implemented
**Status:** TODO (Deferred)
**Severity:** LOW
**Impact:** Feature was marked as deferred in Phase 2.5
**Location:** PreviewMode.razor - PrintPreview method
**Note:** Print button exists but functionality not implemented. Add browser print API integration.

---

*Last Updated: 2025-10-21*
*Priority: 🔴 Critical | 🟡 High | 🟢 Medium | ⚪ Low/Deferred*
