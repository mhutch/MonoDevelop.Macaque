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
using System.IO;
using System.Text;
using CommonMark;
using CommonMark.Syntax;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;

namespace MonoDevelop.Macaque
{

	class TipHtmlFormatter : CommonMark.Formatters.HtmlFormatter
	{
		public TipHtmlFormatter (TextWriter target, CommonMarkSettings settings) : base (target, settings)
		{
		}

		protected override void WriteInline (Inline inline, bool isOpening, bool isClosing, out bool ignoreChildNodes)
		{
			if (inline.Tag == InlineTag.Link && !RenderPlainTextInlines.Peek ()) {
				if (inline.TargetUrl == "#command") {
					ignoreChildNodes = true;
					RenderCommand (inline, isOpening, isClosing);
					return;
				}
				if (inline.TargetUrl == "#menu") {
					ignoreChildNodes = true;
					RenderMenuItem (inline, isOpening, isClosing);
					return;
				}
				if (inline.TargetUrl == "#key") {
					ignoreChildNodes = true;
					RenderKey (inline, isOpening, isClosing);
					return;
				}
			}
			base.WriteInline (inline, isOpening, isClosing, out ignoreChildNodes);
		}

		void RenderCommand (Inline inline, bool isOpening, bool isClosing)
		{
			var cmd = IdeApp.CommandService.GetCommand (inline.FirstChild.LiteralContent);

			if (isOpening) {
				Write ("<span class=\"command\"");

				Write (" title=\"");
				RenderCommandDescription (cmd);
				Write ("\"");

				Write (">");

				WriteEncodedHtml (cmd.Text.Replace ("_", ""));

				var binding = cmd.KeyBinding;
				if (binding != null) {
					Write (" (");
					WriteEncodedHtml (KeyBindingManager.BindingToDisplayLabel (binding, true));
					Write (")");
				}
			}

			if (isClosing) {
				Write ("</span>");
			}
		}

		void RenderKey (Inline inline, bool isOpening, bool isClosing)
		{
			if (isOpening) {
				var binding = inline.FirstChild.LiteralContent;
				KeyBinding b;
				if (!KeyBinding.TryParse (binding, out b))
					throw new Exception ($"Invalid keybinding '{binding}'");
				Write ("<span class=\"keybinding\">");
				WriteEncodedHtml (KeyBindingManager.BindingToDisplayLabel (b, true));
			}

			if (isClosing) {
				Write ("</span>");
			}
		}

		void RenderCommandDescription (Command cmd)
		{
			WriteEncodedHtml (cmd.Text.Replace ("_", ""));

			var binding = cmd.KeyBinding;
			if (binding != null) {
				Write (" (");
				WriteEncodedHtml (KeyBindingManager.BindingToDisplayLabel (binding, true));
				Write (")");
			}

			if (cmd.Description != null) {
				Write ("\n\n");
				WriteEncodedHtml (cmd.Description);
				Write ("");
			}

			var menuPath = GetMenuPathString (cmd);
			if (menuPath != null) {
				Write ("\n\nMenu: ");
				WriteEncodedHtml (menuPath);
			}

			Write ("\"");
		}

		void RenderMenuItem (Inline inline, bool isOpening, bool isClosing)
		{
			var cmd = IdeApp.CommandService.GetCommand (inline.FirstChild.LiteralContent);

			if (isOpening) {
				Write ("<span class=\"menu-item\">");
				Write (GetMenuPathString (cmd));
			}
			if (isClosing) {
				Write ("</span>");
			}
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
				var set = ce as CommandEntrySet;
				if (set != null) {
					var result = FindMenuPath (set, cmdId);
					if (result != null) {
						result.Add (set);
						return result;
					}
				}
			}
			return null;
		}
	}
}
