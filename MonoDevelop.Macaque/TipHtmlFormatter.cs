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

using System.IO;
using CommonMark;
using CommonMark.Syntax;
using MonoDevelop.Ide;
using MonoDevelop.Components.Commands;

namespace MonoDevelop.Macaque
{

	class TipHtmlFormatter : CommonMark.Formatters.HtmlFormatter
	{
		public TipHtmlFormatter (TextWriter target, CommonMarkSettings settings) : base (target, settings)
		{
		}

		protected override void WriteInline (Inline inline, bool isOpening, bool isClosing, out bool ignoreChildNodes)
		{
			if (inline.Tag == InlineTag.Link && !RenderPlainTextInlines.Peek () && inline.TargetUrl == "#command") {
				ignoreChildNodes = true;
				var cmd = IdeApp.CommandService.GetCommand (inline.FirstChild.LiteralContent);

				if (isOpening) {
					Write ("<span class=\"command\"");

					if (cmd.Description != null) {
						Write (" title=\"");
						WriteEncodedHtml (cmd.Description);
						Write ("\"");
					}

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
			} else {
				// in all other cases the default implementation will output the correct HTML
				base.WriteInline (inline, isOpening, isClosing, out ignoreChildNodes);
			}
		}
	}
}
