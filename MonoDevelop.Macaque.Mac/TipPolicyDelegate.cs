using System;
using AppKit;
using Foundation;
using WebKit;

namespace MonoDevelop.Macaque.Mac
{
	class TipPolicyDelegate : WebPolicyDelegate
	{
		TipWindow tipWindow;

		public TipPolicyDelegate (TipWindow tipWindow)
		{
			this.tipWindow = tipWindow;
		}

		public override void DecidePolicyForNavigation (WebView webView, NSDictionary actionInformation, NSUrlRequest request, WebFrame frame, NSObject decisionToken)
		{
			NSApplication.SharedApplication.InvokeOnMainThread (() => {
				if (tipWindow.HandleUrlOpen != null && tipWindow.HandleUrlOpen (new Uri (request.Url.AbsoluteString))) {
					WebView.DecideIgnore (decisionToken);
				} else {
					WebView.DecideUse (decisionToken);
				}
			});
		}
	}
}