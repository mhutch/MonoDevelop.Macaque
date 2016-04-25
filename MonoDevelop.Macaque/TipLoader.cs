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

using Newtonsoft.Json;
using System.IO;
using Mono.Addins;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using MonoDevelop.Core;

namespace MonoDevelop.Macaque
{
	class TipLoader
	{
		Task tiploader;
		List<Tip> tips;

		public Task LoadTips ()
		{
			return tiploader ?? (tiploader = Task.Run (() => {
				try {
					var tipFile = AddinManager.CurrentAddin.GetFilePath ("content", "Tips.json");
					tips = LoadTips (tipFile);
				} catch (Exception ex) {
					LoggingService.LogError ("Failed to load tips", ex);
				}
			}));
		}

		int i;

		public Tip GetNextTip ()
		{
			return tips [(i++ % tips.Count)];
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
			if (!Enum.TryParse<T> ((string)prop.Value, true, out value)) {
				throw new FormatException ($"Value '{prop.Value}' not valid for {typeof (T)}");
			}

			return value;
		}
	}
}

