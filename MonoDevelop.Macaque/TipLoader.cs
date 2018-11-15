//
// TipLoader.cs
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
using System.Linq;
using System.Threading.Tasks;
using Mono.Addins;
using MonoDevelop.Core;
using Markdig;
using Markdig.Syntax;

namespace MonoDevelop.Macaque
{
	class TipLoader
	{
		public static readonly MarkdownPipeline Pipeline
			= new MarkdownPipelineBuilder ().UseAdvancedExtensions ().WithTipLinkExtensions ().Build ();

		const string macaqueConfigDir = "Macaque";
		Task<bool> tiploader;
		List<Tip> tips;

		static string ShownTipsStatePath {
			get {
				return UserProfile.Current.ConfigDir.Combine (macaqueConfigDir, "showntips.txt");
			}
		}

		public Task<bool> LoadTips ()
		{
			return tiploader ?? (tiploader = Task.Run (() => {
				try {
					var stateFile = ShownTipsStatePath;
					if (File.Exists (stateFile)) {
						shownTips = new HashSet<string> (File.ReadAllLines (stateFile));
					}
				} catch (Exception ex) {
					LoggingService.LogError ("Failed to load Macaque state", ex);
				}
				try {
					var tipDir = AddinManager.CurrentAddin.GetFilePath ("content");
					var tipFiles = Directory.EnumerateFiles (tipDir, "*.md", SearchOption.AllDirectories);
					tips = LoadTips (tipFiles);
					return true;
				} catch (Exception ex) {
					LoggingService.LogError ("Failed to load tips", ex);
				}
				return false;
			}));
		}

		Task SaveShownTipsState ()
		{
			return Task.Run (() => {
				try {
					var stateFile = ShownTipsStatePath;
					Directory.CreateDirectory (Path.GetDirectoryName (stateFile));
					File.WriteAllLines (stateFile, shownTips);
				} catch (Exception ex) {
					LoggingService.LogError ("Failed to load Macaque state", ex);
				}
			});
		}

		HashSet<string> shownTips = new HashSet<string> ();
		Random random = new Random ();

		public Tip GetNextTip ()
		{
			if (tips.Count == 0)
				throw new InvalidOperationException ("No tips loaded");

			if (shownTips.Count == tips.Count)
				shownTips.Clear ();

			List<Tip> high = null, normal = null, low = null;

			foreach (var t in tips) {
				if (shownTips.Contains (t.Id))
					continue;
				switch (t.Priority) {
				case Priority.High:
					if (high == null)
						high = new List<Tip> ();
					high.Add (t);
					break;
				case Priority.Normal:
					if (high != null)
						continue;
					if (normal == null)
						normal = new List<Tip> ();
					normal.Add (t);
					break;
				case Priority.Low:
					if (high != null || normal != null)
						continue;
					if (low == null)
						low = new List<Tip> ();
					low.Add (t);
					break;
				default:
					throw new ArgumentOutOfRangeException ();
				}
			}

			var tipList = high ?? normal ?? low;

			var tip = tipList [random.Next (0, tipList.Count - 1)];

			shownTips.Add (tip.Id);

			if (shownTips.Count == tips.Count)
				shownTips.Clear ();

			SaveShownTipsState ();

			return tip;
		}

		static string IdFromPath (string path)
		{
			string name = Path.GetFileName (path);
			string category = Path.GetFileName (Path.GetDirectoryName (name));
			return category + "." + name;
		}

		static List<Tip> LoadTips (IEnumerable<string> paths)
		{
			//FIXME: parse frontmatter
			//FIXME: preprocess this, don't read all the files every time
			var tips = new List<Tip> ();
			foreach (var p in paths) {
				var text = File.ReadAllText (p);
				var document = Markdig.Parsers.MarkdownParser.Parse (text, Pipeline);
				var title = GetTitle (document, p);
				tips.Add (new Tip (IdFromPath (p), text, document, Priority.Normal));
			}
			return tips;
		}

		static string GetTitle (MarkdownDocument document, string path)
		{
			var title = document.Descendants<HeadingBlock> ().FirstOrDefault ();
			if (title == null || title.Level != 1) {
				throw new Exception ($"No toplevel heading in {path}");
			}
			return ((Markdig.Syntax.Inlines.LiteralInline)title.Inline.FirstChild).Content.Text;
		}
	}
}

