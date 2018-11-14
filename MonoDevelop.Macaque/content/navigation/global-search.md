---
priority: high
---

# Global navigation

The [MonoDevelop.Components.MainToolbar.Commands.NavigateTo](#command) command allows you to quickly search and navigate your
solution and workspace.

* jump to any file, type or member in your solution
* execute commands, including hidden commands
* search for NuGet packages
* start a solution-wide full-text search

The search uses fuzzy camelCase matching, so you don't need to type the whole word. For example, `vico` or `viCo` would
match `ViewController`.

Simply type your search term, select an item from the results list using the arrow keys if necessary, and hit [Return](#key).
