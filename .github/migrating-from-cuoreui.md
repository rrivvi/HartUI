# ![cui-32x](https://i.imgur.com/f84tWbH.png)Migrating from CuoreUI?

> [!CAUTION]
> HartUI is based on **CuoreUI.Winforms**. However, some of HartUI's public API and control behaviours are different from the original. Consider staying on CuoreUI in your old projects, unless you need the changes introduced by HartUI.

### If you still want to "migrate":

1. Uninstall the `CuoreUI.Winforms` NuGet package.
2. Install `HartUI.Winforms`.
3. Replace `CuoreUI` namespace mentions with `HartUI`. 
> In most cases, using **Find & Replace** to replace all mentions of `CuoreUI` with `HartUI` will work just fine
4. cuiLabel's `Content` property changed: **Upon switching to HartUI, already-existing CuoreUI `cuiLabel` controls will have their spaces (` `) represented as preceding with a backslash `\ `**. This is due to not using Regex.Unescape(...) in `Content`'s setter. Update your cuiLabel controls' `Content` values. 
> In most cases, using **Find & Replace** to replace all mentions of ` ` with `\ ` will work just fine, unless your project uses `\ ` somewhere else