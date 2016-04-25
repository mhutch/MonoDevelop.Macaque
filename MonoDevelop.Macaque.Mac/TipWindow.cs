//
// TipWindow.cs
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
using AppKit;
using Foundation;
using WebKit;

namespace MonoDevelop.Macaque.Mac
{
	public partial class TipWindow : NSPanel
	{
		WebView webView;

		#region Constructors

		// Called when created from unmanaged code
		public TipWindow (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public TipWindow (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		// Shared initialization code
		void Initialize ()
		{
			Delegate = new TipWindowDelegate (this);
		}

		#endregion

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();

			var frame = scrollView.Frame;
			frame.X = 0;
			frame.Y = 0;
			webView = new WebView (frame, "FrameName", "GroupName") {
				ShouldCloseWithWindow = true,
				AutoresizesSubviews = true,
				AutoresizingMask = scrollView.AutoresizingMask
			};
			scrollView.AddSubview (webView);
			scrollView.AutoresizesSubviews = true;

			MinSize = ContentView.Frame.Size;
		}

		internal void ResizeWebContent ()
		{
			webView.ResizeWithOldSuperviewSize (webView.Frame.Size);
		}

		partial void clickClose (NSObject sender)
		{
			NSApplication.SharedApplication.StopModal ();
			OrderOut (this);
		}

		partial void clickNext (NSButton sender)
		{
			NextTipClicked?.Invoke (this, EventArgs.Empty);
		}

		public event EventHandler NextTipClicked;

		public bool ShowAtStartup {
			get {
				return showAtStartupCheck.State == NSCellStateValue.On;
			}
			set {
				showAtStartupCheck.State = value? NSCellStateValue.On : NSCellStateValue.Off;
			}
		}

		public void LoadHtml (string html, string baseDirectory)
		{
			webView.MainFrame.LoadHtmlString (html, new NSUrl (baseDirectory, true));
		}

		public void Run ()
		{
			IntPtr session = NSApplication.SharedApplication.BeginModalSession (this);
			NSRunResponse result = NSRunResponse.Continues;

			while (result == NSRunResponse.Continues) {
				using (var pool = new NSAutoreleasePool ()) {
					// Starting with 10.10.3, it seems that this loop will never get back to NSApplication.run
					// loop so it can do its magic and empty the event queue. Manually flush the events
					// one by one.
					// https://developer.apple.com/library/prerelease/mac/documentation/Cocoa/Reference/ApplicationKit/Classes/NSApplication_Class/#//apple_ref/occ/instm/NSApplication/sendEvent:
					NSApplication.SharedApplication.SendEvent (NSApplication.SharedApplication.NextEvent (NSEventMask.AnyEvent, NSDate.DistantFuture, NSRunLoop.NSDefaultRunLoopMode, true));

					// Run the window modally until there are no events to process
					result = (NSRunResponse)(long)NSApplication.SharedApplication.RunModalSession (session);

					// Give the main loop some time
					NSRunLoop.Current.LimitDateForMode (NSRunLoopMode.Default);
				}
			}

			NSApplication.SharedApplication.EndModalSession (session);
		}
	} 
}
