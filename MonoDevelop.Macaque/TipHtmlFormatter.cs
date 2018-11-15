//
// TipHtmlFormatter.cs
//
// Author:
//       Mikayla Hutchinson <m.j.hutchinson@gmail.com>
//
// Copyright (c) 2016 Xamarin Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using Mono.Addins;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace MonoDevelop.Macaque
{
	public static class TipFormattingExtensions
	{
		public static MarkdownPipelineBuilder WithTipLinkExtensions (this MarkdownPipelineBuilder pipeline)
		{
			pipeline.Extensions.Add (new TipsLinksExtension ());
			return pipeline;
		}
	}

	public class TipsLinksExtension : IMarkdownExtension
	{
		public void Setup (MarkdownPipelineBuilder pipeline)
		{
		}

		public void Setup (MarkdownPipeline pipeline, IMarkdownRenderer renderer)
		{
			var linkRenderer = renderer.ObjectRenderers.FindExact<LinkInlineRenderer> ();
			linkRenderer.TryWriters.Remove (TryLinkInlineRenderer);
			linkRenderer.TryWriters.Add (TryLinkInlineRenderer);
		}

		bool TryLinkInlineRenderer (HtmlRenderer renderer, LinkInline link)
		{
			switch (link.Url) {
			case "#command":
				RenderCommand (renderer, link.Title);
				return true;
			case "#menu":
				RenderMenuItem (renderer, link.Title);
				return true;
			case "#key":
				RenderKey (renderer, link.Title);
				return true;
			case "#pad":
				RenderPad (renderer, link.Title);
				return true;
			case "#prefs":
				RenderCommand (renderer, link.Title);
				return true;
			}
			return false;
		}

		void RenderCommand (HtmlRenderer renderer, string commandId)
		{
			var cmd = IdeApp.CommandService.GetCommand (commandId);

			renderer.Write ("<span class=\"command\"");

			renderer.Write (" title=\"");
			RenderCommandDescription (renderer, cmd);
			renderer.Write ("\"");

			renderer.Write (">");

			renderer.WriteEscape (cmd.Text.Replace ("_", ""));

			var binding = cmd.KeyBinding;
			if (binding != null) {
				renderer.Write (" (");
				renderer.WriteEscape (KeyBindingManager.BindingToDisplayLabel (binding, true));
				renderer.Write (")");
			}
			renderer.Write ("</span>");
		}

		void RenderKey (HtmlRenderer renderer, string keyBinding)
		{
			if (!KeyBinding.TryParse (keyBinding, out KeyBinding b))
				throw new Exception ($"Invalid keybinding '{keyBinding}'");

			renderer.Write ("<span class=\"keybinding\">");
			renderer.WriteEscape (KeyBindingManager.BindingToDisplayLabel (b, true));
			renderer.Write ("</span>");
		}

		void RenderCommandDescription (HtmlRenderer renderer, Command cmd)
		{
			renderer.WriteEscape (cmd.Text.Replace ("_", ""));

			var binding = cmd.KeyBinding;
			if (binding != null) {
				renderer.Write (" (");
				renderer.WriteEscape (KeyBindingManager.BindingToDisplayLabel (binding, true));
				renderer.Write (")");
			}

			if (cmd.Description != null) {
				renderer.Write ("\n\n");
				renderer.WriteEscape (cmd.Description);
				renderer.Write ("");
			}

			var menuPath = GetMenuPathString (cmd);
			if (menuPath != null) {
				renderer.Write ("\n\nMenu: ");
				renderer.WriteEscape (menuPath);
			}

			renderer.Write ("\"");
		}

		void RenderMenuItem (HtmlRenderer renderer, string commandId)
		{
			var cmd = IdeApp.CommandService.GetCommand (commandId);
			renderer.Write ("<span class=\"menu-item\">");
			renderer.Write (GetMenuPathString (cmd));
			renderer.Write ("</span>");
		}

		static string GetMenuPathString (Command cmd)
		{
			CommandEntrySet menu = IdeApp.CommandService.CreateCommandEntrySet ("/MonoDevelop/Ide/MainMenu");
			var path = FindMenuPath (menu, cmd.Id.ToString ());
			if (path == null)
				return null;

			path.Reverse ();

			var sb = new StringBuilder ();
			foreach (var ces in path) {
				sb.Append (ces.Name.Replace ("_", ""));
				sb.Append (" > ");
			}
			sb.Append (cmd.Text.Replace ("_", ""));
			return sb.ToString ();
		}

		static List<CommandEntrySet> FindMenuPath (CommandEntrySet ces, object cmdId)
		{
			foreach (CommandEntry ce in ces) {
				if (ce.CommandId.Equals (cmdId)) {
					return new List<CommandEntrySet> ();
				}
				if (ce is CommandEntrySet set) {
					var result = FindMenuPath (set, cmdId);
					if (result != null) {
						result.Add (set);
						return result;
					}
				}
			}
			return null;
		}

		void RenderPad (HtmlRenderer renderer, string padId)
		{
			var pad = IdeApp.Workbench.Pads.Find (p => p.Id == padId);
			if (pad == null) {
				throw new Exception ($"Did not find pad '{padId}'");
			}

			renderer.Write ("<span class=\"pad\">");
			renderer.Write (pad.Title);
			renderer.Write ("</span>");
		}

		void RenderPrefs (HtmlRenderer renderer, string path)
		{
			var panel = AddinManager.GetExtensionNode ("/MonoDevelop/Ide/GlobalOptionsDialog/" + path);
			if (panel == null) {
				throw new Exception ($"Did not find prefs panel '{path}'");
			}

			var prop = panel.GetType ().GetProperty ("Label", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

			string name = (string)prop.GetValue (panel);
			while (panel.Parent != null && panel.Parent.GetType () == panel.GetType ()) {
				panel = panel.Parent;
				name = (string)prop.GetValue (panel) + " > " + name;
			}

			renderer.Write ("<span class=\"prefs-panel\">");
			renderer.Write (name);
			renderer.Write ("</span>");
		}
	}
}
