# ![cui-32x](https://i.imgur.com/f84tWbH.png)Migrating from CuoreUI?

> [!CAUTION]
> HartUI is based on **CuoreUI.Winforms**. However, some of HartUI's public API and control behaviours are different from the original. Consider staying on CuoreUI in your old projects, unless you need the changes introduced by HartUI.

# If you still want to "migrate":

## Installation
1. Uninstall the `CuoreUI.Winforms` NuGet package.
2. Install `HartUI.Winforms`.
---
<br>

## Namespace changed
3. Replace `CuoreUI` namespace mentions with `HartUI`. 
> In most cases, using **Find & Replace** to replace all mentions of `CuoreUI` with `HartUI` will work just fine
---
<br>

## cuiLabel `behaviour changed`
4. cuiLabel's `Content` property changed: **Upon switching to HartUI, already-existing CuoreUI `cuiLabel` controls will have their spaces (` `) represented as preceding with a backslash `\ `**. This is due to not using Regex.Unescape(...) in `Content`'s setter. Update your cuiLabel controls' `Content` values. 
> In most cases, using **Find & Replace** to replace all mentions of ` ` with `\ ` will work just fine, unless your project uses `\ ` somewhere else
---
<br>

## cuiCalendarDatePicker `properties changed`
5. cuiCalendarDatePicker: The `Icon` and `IconTint` properties had their names changed to `Icon` and `NormalImageTint`/`HoverImageTint`/`PressedImageTint`. It now follows a consistent naming scheme (same as cuiButton and cuiButtonGroup), and has correct per-state colours. In case you gave your cuiCalendarDatePicker controls custom names, changes are probably going to be manual: Switching out mentions of `Icon` for `Image`.

6. Additionally, the `Theme` enum & property was deprecated in favor of the new `DialogBackColor` and `DialogForeColor` properties.
> Changes needed:
> 1. Re-check the image tints (`IconTint` was separated into 3 state-dependent properties: `NormalImageTint`, `HoverImageTint` and `PressedImageTint`) in the designer.cs files, and possibly your own code as well.
> 2. Change mentions of your `cuiCalendarDatePicker`s' `Icon` to `Image`
> 3. Remove mentions of your `cuiCalendarDatePicker`s' `Theme`; Don't forget to check the values of: `DialogBackColor` and `DialogForeColor`
---
<br>

## cuiMessageDialog `properties changed`
7. cuiMessageDialog: The `Rounding` property now refers to the buttons' corner radius. To change the dialog's corner radius, change `DialogRounding`.
> `cuiMessageDialog`'s buttons can now be fully customized, the `cuiMessageDialog` exposes the same color properties as the `cuiButton`.
