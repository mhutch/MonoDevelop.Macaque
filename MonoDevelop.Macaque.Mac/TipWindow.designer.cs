// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace MonoDevelop.Macaque.Mac
{
	[Register ("TipWindow")]
	partial class TipWindow
	{
		[Outlet]
		AppKit.NSScrollView scrollView { get; set; }

		[Outlet]
		AppKit.NSButton showAtStartupCheck { get; set; }

		[Action ("clickClose:")]
		partial void clickClose (Foundation.NSObject sender);

		[Action ("clickNext:")]
		partial void clickNext (AppKit.NSButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (showAtStartupCheck != null) {
				showAtStartupCheck.Dispose ();
				showAtStartupCheck = null;
			}

			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}
		}
	}
}
