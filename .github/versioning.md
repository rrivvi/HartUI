CuoreUI-ng uses ***date versioning*** (***CalVer***).

## Format
The format is: `[year]`.`[month]`.`[day]`.`[revisions]`

> [!TIP]  
> The `[revisions]` field is optional. 
> There are 2 use cases for the `[revisions]` field:
> 1) The version had a bug which the revision fixes ∙ *(Example commit: [3c8fb30](https://github.com/rrivvi/CuoreUI-ng/commit/3c8fb3047b0cc7bd88e548f6730cf215383da3ea))*
> 2) The change was too small to call it a new version ∙ *(Example commit: [349f2ca](https://github.com/rrivvi/CuoreUI-ng/commit/349f2ca801945464aa484ff46d7324f263b3a2cc))*

## Explanation
**The Gregorian calendar is assumed.**

`[year]` - The year the release was published<br>
`[month]` - The month the release was published<br>
`[day]` - The day the release was published<br>
`[revisions]` - How many times the version was modified **after** its initial release
### Examples:
- `2025.9.19` ∙ 2025 September 19th, no revisions
- `2025.9.19.1` ∙ 2025 September 19th, 1 revision
- `2024.12.16 ` ∙ 2024 December 16th, no revisions

## Automation
The root directory's [/nuget automation.bat](https://github.com/rrivvi/CuoreUI-ng/blob/master/nuget%20automation.bat) file automates the editing of the [/CuoreUI-ng.Winforms.nuspec](https://github.com/rrivvi/CuoreUI-ng/blob/master/CuoreUI-ng.Winforms.nuspec) file aswell as the NuGet packaging process.
