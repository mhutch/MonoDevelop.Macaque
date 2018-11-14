---
priority: high
after: global-search
---

# Navigation line suffixes

When navigating to a file using the [MonoDevelop.Components.MainToolbar.Commands.NavigateTo](#command) command,
you can jump to a specific line by suffixing your search with a colon followed by the line number.

For example, `Ma:25` could be used to jump to line 25 in the file `Main.cs`.

You can use a second suffix to specify the column, e.g. `Ma:25:10` to jump to line 25, column 10 in the file Main.cs.

You can also omit the filename to jump within the current file, e.g. `:24` to jump to line 24 in the current file.