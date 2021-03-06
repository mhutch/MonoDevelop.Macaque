﻿//
// StartupHandler.cs
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
using System.Linq;
using AppKit;
using Foundation;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Macaque.Mac;
using MonoDevelop.MacInterop;

namespace MonoDevelop.Macaque
{
	enum Commands
	{
		ShowTipWindow
	}

	class StartupHandler : CommandHandler
	{
		protected override void Run ()
		{
			if (ShowAtStartup)
				ShowTipWindowHandler.ShowTipWindow ();
		}

		public static bool ShowAtStartup {
			get {
				// if built in tips are enabled, they override this
				if (IdeApp.Preferences.ShowTipsAtStartup)
					return false;
				return showAtStartup.Value;
			}
			set {
				// when enabling this, disable built in tips
				if (value) {
					IdeApp.Preferences.ShowTipsAtStartup.Value = false;
				}
				showAtStartup.Value = true;
			}
		}

		static readonly ConfigurationProperty<bool> showAtStartup = ConfigurationProperty.Create ("MonoDevelop.Macaque.ShowOnStartup", true);
	}

	class ShowTipWindowHandler : CommandHandler
	{
		protected override void Run ()
		{
			ShowTipWindow ();
		}

		internal static void ShowTipWindow ()
		{
			var tipLoader = new TipLoader ();

			if (!tipLoader.LoadTips ().Result) {
				return;
			}

			var oldMainWindow = NSApplication.SharedApplication.KeyWindow
				?? NSApplication.SharedApplication.MainWindow
				?? GtkQuartz.GetWindow (MessageService.RootWindow);

			using (var tipWindow = InflateFromNibResource<TipWindow> ("__xammac_content_TipWindow.nib")) {
				tipWindow.ParentWindow = oldMainWindow;
				tipWindow.MakeKeyAndOrderFront (tipWindow);
				tipWindow.ShowAtStartup = StartupHandler.ShowAtStartup;

				FilePath sharedAssetsPath = Mono.Addins.AddinManager.CurrentAddin.GetFilePath ("content");

				tipWindow.HandleUrlOpen = s => {
					if (s.IsFile)
						return false;
					DesktopService.ShowUrl (s.ToString ());
					return true;
				};

				void nextMessage (object sender, EventArgs e)
				{
					string html;
					string basePath = sharedAssetsPath;
					try {
						var tip = tipLoader.GetNextTip ();
						basePath = System.IO.Path.GetDirectoryName (tip.Filename);
						html = new TipView (tip, sharedAssetsPath).GenerateString ();
					} catch (Exception ex) {
						html = $"<h1>Error</h1><p>{ex.ToString ()}</p>";
					}
					tipWindow.LoadHtml (html, basePath);
				}

				tipWindow.NextTipClicked += nextMessage;
				nextMessage (null, null);

				tipWindow.Run ();

				StartupHandler.ShowAtStartup = tipWindow.ShowAtStartup;
			}

			GtkQuartz.FocusWindow (GtkQuartz.GetGtkWindow (oldMainWindow) ?? MessageService.RootWindow);
		}

		static T InflateFromNibResource<T> (string name)
		{
			var asm = typeof (T).Assembly;
			ObjCRuntime.Runtime.RegisterAssembly (asm);

			NSData data;
			using (var stream = asm.GetManifestResourceStream (name)) {
				data = NSData.FromStream (stream);
			}

			var nib = new NSNib (data, NSBundle.MainBundle);
			nib.InstantiateNibWithOwner (null, out NSArray topLevelObjects);
			var arr = NSArray.FromArray<NSObject> (topLevelObjects);

			return arr.OfType<T> ().Single ();
		}
	}
}
