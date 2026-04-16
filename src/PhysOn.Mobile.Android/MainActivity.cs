using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Net;
using Android.Net.Http;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;

namespace PhysOn.Mobile.Android;

[Activity(
    Label = "@string/app_name",
    MainLauncher = true,
    Exported = true,
    LaunchMode = LaunchMode.SingleTask,
    Theme = "@style/KoTalkTheme",
    ConfigurationChanges =
        ConfigChanges.Orientation |
        ConfigChanges.ScreenSize |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.ScreenLayout |
        ConfigChanges.UiMode |
        ConfigChanges.KeyboardHidden |
        ConfigChanges.Density)]
public class MainActivity : Activity
{
    private const string AppVersion = "0.1.0-alpha.11";
    private const string HomeUrl = "https://vstalk.phy.kr";

    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "vstalk.phy.kr",
        "download-vstalk.phy.kr"
    };

    private WebView? _webView;
    private ProgressBar? _loadingBar;
    private View? _offlineOverlay;
    private ImageButton? _retryButton;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        ConfigureWindowChrome();
        SetContentView(Resource.Layout.activity_main);

        _webView = FindViewById<WebView>(Resource.Id.app_webview);
        _loadingBar = FindViewById<ProgressBar>(Resource.Id.loading_bar);
        _offlineOverlay = FindViewById<View>(Resource.Id.offline_overlay);
        _retryButton = FindViewById<ImageButton>(Resource.Id.retry_button);

        if (_webView is null || _loadingBar is null || _offlineOverlay is null || _retryButton is null)
        {
            throw new InvalidOperationException("KoTalk Android shell layout failed to load.");
        }

        _retryButton.Click += HandleRetryClick;
        ConfigureWebView(_webView, _loadingBar);

        if (savedInstanceState is not null)
        {
            _webView.RestoreState(savedInstanceState);
        }
        else
        {
            _webView.LoadUrl(HomeUrl);
        }
    }

    protected override void OnSaveInstanceState(Bundle outState)
    {
        base.OnSaveInstanceState(outState);
        _webView?.SaveState(outState);
    }

    protected override void OnDestroy()
    {
        if (_retryButton is not null)
        {
            _retryButton.Click -= HandleRetryClick;
        }

        if (_webView is not null)
        {
            _webView.StopLoading();
            _webView.Destroy();
            _webView = null;
        }

        base.OnDestroy();
    }

    public override void OnBackPressed()
    {
        if (_webView?.CanGoBack() == true)
        {
            _webView.GoBack();
            return;
        }

#pragma warning disable CA1422
        base.OnBackPressed();
#pragma warning restore CA1422
    }

    private void HandleRetryClick(object? sender, EventArgs e)
    {
        _webView?.Reload();
    }

    private void ConfigureWindowChrome()
    {
        Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

        if (Window is null)
        {
            return;
        }

        Window.SetStatusBarColor(Color.ParseColor("#F7F3EE"));
        Window.SetNavigationBarColor(Color.ParseColor("#F7F3EE"));
    }

    private void ConfigureWebView(WebView webView, ProgressBar loadingBar)
    {
        WebView.SetWebContentsDebuggingEnabled(System.Diagnostics.Debugger.IsAttached);

        var settings = webView.Settings!;
        settings.JavaScriptEnabled = true;
        settings.DomStorageEnabled = true;
        settings.DatabaseEnabled = true;
        settings.AllowFileAccess = false;
        settings.AllowContentAccess = false;
        settings.SetSupportZoom(false);
        settings.BuiltInZoomControls = false;
        settings.DisplayZoomControls = false;
        settings.LoadWithOverviewMode = true;
        settings.UseWideViewPort = true;
        settings.MixedContentMode = MixedContentHandling.NeverAllow;
        settings.CacheMode = CacheModes.Default;
        settings.MediaPlaybackRequiresUserGesture = true;
        settings.UserAgentString = $"{settings.UserAgentString} KoTalkAndroid/{AppVersion}";

        var cookies = CookieManager.Instance;
        cookies?.SetAcceptCookie(true);
        cookies?.SetAcceptThirdPartyCookies(webView, false);

        webView.SetBackgroundColor(Color.ParseColor("#F7F3EE"));
        webView.SetWebChromeClient(new KoTalkWebChromeClient(loadingBar));
        webView.SetWebViewClient(
            new KoTalkWebViewClient(
                AllowedHosts,
                ShowOfflineOverlay,
                HideOfflineOverlay));
    }

    private void ShowOfflineOverlay()
    {
        RunOnUiThread(() =>
        {
            if (_offlineOverlay is not null)
            {
                _offlineOverlay.Visibility = ViewStates.Visible;
            }

            if (_loadingBar is not null)
            {
                _loadingBar.Visibility = ViewStates.Invisible;
            }
        });
    }

    private void HideOfflineOverlay()
    {
        RunOnUiThread(() =>
        {
            if (_offlineOverlay is not null)
            {
                _offlineOverlay.Visibility = ViewStates.Gone;
            }
        });
    }

    private sealed class KoTalkWebChromeClient(ProgressBar loadingBar) : WebChromeClient
    {
        public override void OnProgressChanged(WebView? view, int newProgress)
        {
            base.OnProgressChanged(view, newProgress);
            loadingBar.Progress = newProgress;
            loadingBar.Visibility = newProgress >= 100 ? ViewStates.Invisible : ViewStates.Visible;
        }
    }

    private sealed class KoTalkWebViewClient(
        IReadOnlySet<string> allowedHosts,
        Action showOfflineOverlay,
        Action hideOfflineOverlay) : WebViewClient
    {
        public override bool ShouldOverrideUrlLoading(WebView? view, IWebResourceRequest? request)
        {
            if (request?.Url is null)
            {
                return false;
            }

            var url = request.Url;
            var scheme = url.Scheme?.ToLowerInvariant();
            var host = url.Host?.ToLowerInvariant();

            if (scheme is "http" or "https" && host is not null && allowedHosts.Contains(host))
            {
                return false;
            }

            if (view?.Context is null)
            {
                return true;
            }

            try
            {
                var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url.ToString()));
                intent.AddFlags(ActivityFlags.NewTask);
                view.Context.StartActivity(intent);
            }
            catch (ActivityNotFoundException)
            {
                showOfflineOverlay();
            }

            return true;
        }

        public override void OnPageStarted(WebView? view, string? url, Bitmap? favicon)
        {
            base.OnPageStarted(view, url, favicon);
            hideOfflineOverlay();
        }

        public override void OnPageFinished(WebView? view, string? url)
        {
            base.OnPageFinished(view, url);
            hideOfflineOverlay();
        }

        public override void OnReceivedError(
            WebView? view,
            IWebResourceRequest? request,
            WebResourceError? error)
        {
            base.OnReceivedError(view, request, error);

            if (request?.IsForMainFrame ?? true)
            {
                showOfflineOverlay();
            }
        }

        public override void OnReceivedSslError(WebView? view, SslErrorHandler? handler, SslError? error)
        {
            handler?.Cancel();
            showOfflineOverlay();
        }
    }
}
