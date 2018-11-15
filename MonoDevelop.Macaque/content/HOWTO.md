# How to write tips

Tips are in markdown.

Each tip MUST have title, as a toplevel markdown header.

Tips may have YAML frontmatter, which can define key/value pairs. All keys are optional. Valid keys are:

* id: Provides an ID for the tip. If not defined, it will be determined from the
  parent folder and the filename, e.g. `editor/generate-code.md` has an ID of `editor.generate-code`.
* priority: May be 'low', 'normal', or 'high'. By default it is 'normal.
* after: A list of IDs of other tips that must have been shown before this one. These IDs are relative
  for example, if tip `foo.bar ` has `after: baz`, that's shorthand for `after: foo.baz`

Tips may use any kind of markdown formatting, but it is recommended to use only the following:

* Basic text formatting, e.g. emphasis and underlines
* Images, especially gifs!
* Code blocks and inline code
* Links
* Lists
* Tables

There are also several special link values. A link with these URLs will displayed with
a special formatting, using the link text as an argument. For example,
`[MonoDevelop.Ide.Commands.FileCommands.OpenFile](#command)` will be rendered as the
command name and keyboard shortcut, with a tooltip showing the command
description.

* #command: An IDE command. The argument is the command ID.
* #key: A keyboard key. The argument is the GTK# key name.
* #prefs: The ID of a preferences panel section (its sub-path under the
  `/MonoDevelop/Ide/GlobalOptionsDialog` extension point).
* #pad: A pad. the argument is the ID of the pad.
* #menu: A main menu item. The ID is the command ID.