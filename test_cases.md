# Test Cases – Item Processor Application

**Application URL:** `http://localhost:5000`  
**Default Credentials:** `admin@test.com` / `Admin@123`

---

## Module 1: Login

| TC# | Test Case | Input | Expected Result | Pass/Fail |
|-----|-----------|-------|-----------------|-----------|
| TC01 | Valid login | Email: `admin@test.com`, Pass: `Admin@123` | Redirect to Items page, sidebar visible | |
| TC02 | Wrong password | Email: `admin@test.com`, Pass: `wrongpass` | Error: "Invalid email or password." | |
| TC03 | Non-existent email | Email: `nobody@test.com`, Pass: `Admin@123` | Error: "Invalid email or password." | |
| TC04 | Empty email | Email: `""`, Pass: `Admin@123` | Validation: "Email is required." | |
| TC05 | Empty password | Email: `admin@test.com`, Pass: `""` | Validation: "Password is required." | |
| TC06 | Invalid email format | Email: `notanemail`, Pass: `Admin@123` | Validation: "Enter a valid email address." | |
| TC07 | Short password | Email: `admin@test.com`, Pass: `abc` | Validation: "Password must be at least 6 characters." | |
| TC08 | Both fields empty | Email: `""`, Pass: `""` | Both validation messages shown | |
| TC09 | Already logged in – access `/Auth/Login` | Session active | Redirect to Items page | |
| TC10 | Logout | Click Sign Out | Session cleared, redirect to Login | |
| TC11 | Access protected page without login | Navigate to `/Items` | Redirect to Login page | |

---

## Module 2: Items – Add

| TC# | Test Case | Input | Expected Result | Pass/Fail |
|-----|-----------|-------|-----------------|-----------|
| TC12 | Valid item | Name: `Steel Rod`, Weight: `50.5` | Item saved, success toast shown | |
| TC13 | Empty name | Name: `""`, Weight: `50` | Validation: "Item name is required." | |
| TC14 | Empty weight | Name: `Wood Block`, Weight: `""` | Validation: "Weight is required." | |
| TC15 | Zero weight | Name: `Wood Block`, Weight: `0` | Validation: "Weight must be greater than 0." | |
| TC16 | Negative weight | Name: `Wood Block`, Weight: `-5` | Validation: "Weight must be greater than 0." | |
| TC17 | Very long name (> 200 chars) | Name: 201-char string | Validation or truncation error | |
| TC18 | Decimal weight | Name: `Copper`, Weight: `0.0001` | Item saved successfully | |
| TC19 | No description (optional) | Name: `Iron`, Weight: `10`, Desc: `""` | Item saved; description shows "—" in list | |

---

## Module 3: Items – Update

| TC# | Test Case | Input | Expected Result | Pass/Fail |
|-----|-----------|-------|-----------------|-----------|
| TC20 | Edit existing item | Change name + weight | Updated record in list, success toast | |
| TC21 | Clear name on edit | Name: `""` | Validation: "Item name is required." | |
| TC22 | Set invalid weight on edit | Weight: `-1` | Validation: "Weight must be greater than 0." | |
| TC23 | Edit non-existent id | URL: `/Items/Edit/9999` | 404 Not Found | |

---

## Module 4: Items – Delete

| TC# | Test Case | Input | Expected Result | Pass/Fail |
|-----|-----------|-------|-----------------|-----------|
| TC24 | Delete unused item | Click Delete > Confirm | Item removed, success toast | |
| TC25 | Delete item used in processing | Item linked to ProcessedItem | Error: "Cannot delete – used in processing." | |
| TC26 | Cancel delete | Click Delete > Cancel button | No deletion, back to list | |
| TC27 | Delete non-existent id | POST to `/Items/Delete/9999` | 404 Not Found | |

---

## Module 5: Items – List & Search

| TC# | Test Case | Input | Expected Result | Pass/Fail |
|-----|-----------|-------|-----------------|-----------|
| TC28 | List all items | No filter | All items shown, most recent first | |
| TC29 | Search by name | Search: `Steel` | Only items with "Steel" in name shown | |
| TC30 | Search by description | Search: `alloy` | Items with "alloy" in description shown | |
| TC31 | Search – no results | Search: `XYZXYZ123` | "No items found" empty state shown | |
| TC32 | Clear search | Click "✕ Clear" | All items re-displayed | |
| TC33 | Case-insensitive search | Search: `sTEEL` | Matches "Steel" correctly | |

---

## Module 6: Process Item

| TC# | Test Case | Input | Expected Result | Pass/Fail |
|-----|-----------|-------|-----------------|-----------|
| TC34 | Valid process | Parent item selected, output weight: `450`, 2 children | ProcessedItem created, redirect to Tree | |
| TC35 | No parent selected | Parent: `""`, Weight: `450` | Validation: "Please select a parent item." | |
| TC36 | Zero parent output weight | Parent: selected, Weight: `0` | Validation: "Weight must be greater than 0." | |
| TC37 | Negative parent weight | Parent: selected, Weight: `-100` | Validation error on output weight | |
| TC38 | Empty parent output weight | Parent: selected, Weight: `""` | Validation: "Parent output weight is required." | |
| TC39 | Child with no item selected | Child item: `""`, Weight: `50` | Validation or child row skipped | |
| TC40 | Add multiple children (3+) | Click "+ Add Another Output" 3 times | 3 child rows visible and submitted | |
| TC41 | Remove a child row | Click "Remove" on child | Row removed; form still has ≥1 child | |
| TC42 | Single child only | 1 child, Remove button hidden | Remove button not visible for last child | |
| TC43 | Child weight zero | Child Weight: `0` | Validation: "Weight must be greater than 0." | |

---

## Module 7: Tree View (Processed Items)

| TC# | Test Case | Input | Expected Result | Pass/Fail |
|-----|-----------|-------|-----------------|-----------|
| TC44 | View tree after processing | Navigate to `/Process/Tree` | Root nodes shown with children indented | |
| TC45 | Empty tree | No processed data | "No processed items yet" message | |
| TC46 | Multi-level tree | Process a child node further | 3-level tree renders correctly | |
| TC47 | Delete root node | Click 🗑 Delete on root, confirm | Root + all children deleted from DB | |
| TC48 | Delete child node | Click 🗑 Delete on child, confirm | Only that child (and its sub-children) deleted | |
| TC49 | Cancel delete in tree | "Cancel" in browser confirm dialog | Node not deleted | |

---

## Edge Cases Summary

| Edge Case | Expected Behavior |
|-----------|-------------------|
| SQL Injection in name field | Input treated as text; EF Core parameterizes all queries |
| XSS in name/description | Razor auto-encodes output |
| Concurrent deletion of item in use | Referential integrity throws, error shown |
| Very deep recursion (100+ levels) | Tree renders but may be slow; no stack overflow (iterative load) |
| Weight with many decimals (e.g. `0.00001`) | Stored/displayed to 4 decimal places |
| Session expiry mid-session | Next request redirects to Login |
