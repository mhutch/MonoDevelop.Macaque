//
// Tip.cs
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
using Markdig.Syntax;

namespace MonoDevelop.Macaque
{
	class Tip
	{
		public Tip (string id, string filename, string title, MarkdownDocument content, Priority priority)
		{
			Id = id ?? throw new ArgumentNullException (nameof (id));
			Title = title ?? throw new ArgumentNullException (nameof (title));
			Filename = filename ?? throw new ArgumentNullException (nameof (filename));
			Content = content ?? throw new ArgumentNullException (nameof (content));
			Priority = priority;
		}

		public string Id { get; }
		public string Filename { get; }
		public string Title { get; }
		public MarkdownDocument Content { get; }
		public Priority Priority { get; }
	}

	enum Priority
	{
		High,
		Normal,
		Low
	}
}
