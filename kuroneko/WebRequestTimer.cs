using System;
using System.Text;
using System.Threading;
using Awesomium.Core;

namespace kuroneko
{
    internal class WebRequestTimer
    {
        private const int TIMEOUT_REQUEST = 30;

        private int _timeoutRequest;

        private DateTime timeStart;
        private DateTime timeDomReady;
        private DateTime timeStop;

        private WebView _webView;
        private StringBuilder _log = null;

        private string _screenShot = null;

        public string ScreenShot
        {
            get { return _screenShot; }
            set { _screenShot = value; }
        }

        public WebRequestTimer(int request = TIMEOUT_REQUEST, WebConfig? config = null)
        {
            if (!config.HasValue)
                config = new WebConfig { LogLevel = LogLevel.None };
            WebCore.Initialize(config.Value);
            if (request <= 0) request = TIMEOUT_REQUEST;

            _timeoutRequest = request;
        }

        #region WebView related

        private void InitWebView(int width = 1680, int height = 2100)
        {
            if (_webView != null) return;
            timeDomReady = DateTime.MinValue;
            timeStop = DateTime.MinValue;
            timeStart = DateTime.MinValue;
            WebPreferences pref = new WebPreferences();
            pref.CustomCSS = "::-webkit-scrollbar { visibility: hidden; }";
            pref.AppCache = false;
            WebSession sess = WebCore.CreateWebSession(pref);
            _webView = WebCore.CreateWebView(width, height, sess);
            _webView.DocumentReady += OnDomReady;
        }

        private void OnDomReady(object sender, EventArgs e)
        {
            timeDomReady = DateTime.Now;
        }

        private void WaitForRequest()
        {
            timeStart = DateTime.Now;
            while (_webView.IsLoading)
            {
                WebCore.Update();

                // is timeout reached?
                if (timeStart.AddSeconds(_timeoutRequest) < DateTime.Now)
                {
                    _webView.Stop();
                    PrintMessage("Request timeout reached");
                    break;
                }
            }
            if (timeStop == DateTime.MinValue)
                timeStop = DateTime.Now;
            //// one additional Update to trigger LoadingFrameComplete
            //WebCore.Update();
        }

        #endregion WebView related

        #region Utillity

        private void PrintMessage(string msg)
        {
            if (_log == null || string.IsNullOrWhiteSpace(msg)) return;
            _log.AppendFormat("[{0:HH:mm:ss}] ", DateTime.Now);
            _log.AppendLine(msg);
        }

        #endregion Utillity

        public TimeSpan MessureRequest(Uri uri)
        {
            TimeSpan discard;
            return MessureRequest(uri, out discard);
        }

        public TimeSpan MessureRequest(Uri uri, out TimeSpan DomReady)
        {
            DomReady = new TimeSpan(0);
            if (uri == null || !uri.IsAbsoluteUri) return new TimeSpan(0);
            _log = new StringBuilder();
            InitWebView();
            if (_webView == null) throw new Exception("WebView init failed");
            _webView.LoadURL(uri);
            WaitForRequest();
            if (!string.IsNullOrWhiteSpace(_screenShot))
            {
                // speed for an additional 10 ms and Update the Webcore again so we have something to render
                // without waiting some pages will be rendered without content
                Thread.Sleep(10);
                WebCore.Update();
                BitmapSurface surface = (BitmapSurface)_webView.Surface;
                surface.SaveToPNG(_screenShot, true);
            }
            _webView = null;

            if (timeDomReady != DateTime.MinValue)
                DomReady = (timeDomReady - timeStart);
            return (timeStop - timeStart);
        }
    }
}