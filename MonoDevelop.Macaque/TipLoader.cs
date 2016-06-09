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
using System.Threading.Tasks;
using Mono.Addins;
using MonoDevelop.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonoDevelop.Macaque
{
	class TipLoader
	{
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
					var tipFile = AddinManager.CurrentAddin.GetFilePath ("content", "Tips.json");
					tips = LoadTips (tipFile);
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

		static List<Tip> LoadTips (string tipFile)
		{
			var tips = new List<Tip> ();
			using (var tf = File.OpenText (tipFile))
			using (var r = new JsonTextReader (tf) { SupportMultipleContent = true }) {
				do {
					if (r.TokenType == JsonToken.StartObject) {
						tips.Add (ReadTip (r));
					}
				} while (r.Read ());
			}
			return tips;
		}

		static Tip ReadTip (JsonTextReader r)
		{
			var obj = (JObject)JToken.ReadFrom (r);
			return new Tip (
				(string)obj.Property ("id"),
				(string)obj.Property ("title"),
				(string)obj.Property ("content"),
				EnumFromProperty<Priority> (obj.Property ("priority"))
			);
		}

		static T EnumFromProperty<T> (JProperty prop, T defaultVal = default (T)) where T : struct
		{
			if (prop == null)
				return defaultVal;

			T value;
			if (!Enum.TryParse ((string)prop.Value, true, out value)) {
				throw new FormatException ($"Value '{prop.Value}' not valid for {typeof (T)}");
			}

			return value;
		}
	}
}

