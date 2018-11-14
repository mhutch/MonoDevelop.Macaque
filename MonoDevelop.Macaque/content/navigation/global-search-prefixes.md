---
priority: high
after: global-search
---

# Navigation filter prefixes

When using the [MonoDevelop.Components.MainToolbar.Commands.NavigateTo](#command) command, you can search for a specific kind
of result by starting your search with a filter prefix:

<table>
<tr><th>Kind     <th>Prefix       <th>Short Prefix
<tr><td>Files    <td><pre>file:   <td><pre>f:
<tr><td>Types    <td><pre>type:   <td><pre>t:
<tr><td>Members  <td><pre>member: <td><pre>m:
<tr><td>Commands <td><pre>command:<td><pre>c:
</table>


The [MonoDevelop.Ide.Commands.SearchCommands.GotoType](#command) and [MonoDevelop.Ide.Commands.SearchCommands.GotoFile](#command)
commands offer a quick way to start a search with the type or file filter prefixes.
