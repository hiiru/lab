using System;
using System.Collections.Generic;

namespace kuroneko
{
    internal class Program
    {
        private static bool MakeScreenShot = false;
        private static bool Quiet = false;
        private static int Timeout = 0;
        private static List<Uri> domains = new List<Uri>();

        private static void Main(string[] args)
        {
            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("Usage: " + AppDomain.CurrentDomain.FriendlyName + " [-ss] [-q] [-t 60] www.google.com http://www.google.ch");
                Console.WriteLine("Note: if no http:// is provided, it will be added to the domain");
                return;
            }
            bool paramTimeout = false;
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-v":
                        Console.WriteLine("Verbose currently not possible, sould come back with Awesomium 1.7 RC3 (if RessouceInspector is implemented)");
                        return;

                    case "-ss":
                        MakeScreenShot = true;
                        break;

                    case "-q":
                        Quiet = true;
                        break;

                    case "-t":
                        paramTimeout = true;
                        break;
                    default:
                        if (paramTimeout && (!int.TryParse(arg, out Timeout) || Timeout <= 0))
                        {
                            Console.WriteLine("Timeout is invalid");
                            return;
                        }
                        string url = arg.StartsWith("http") ? arg : "http://" + arg;
                        Uri uri;
                        if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                            domains.Add(uri);
                        break;
                }
            }
            if (domains.Count == 0)
            {
                Console.WriteLine("No domains set");
                return;
            }

            WebRequestTimer wrt = new WebRequestTimer(Timeout);
            foreach (Uri uri in domains)
            {
                TimeSpan domReady;
                if (MakeScreenShot)
                    wrt.ScreenShot = uri.Host + ".png";
                var time = wrt.MessureRequest(uri, out domReady);
                if (!Quiet)
                {
                    if (domReady.TotalMilliseconds != 0)
                        Console.WriteLine(uri.Host + " DomReady: " + domReady.TotalMilliseconds + " ms");
                    Console.WriteLine(uri.Host + " Loaded: " + time.TotalMilliseconds + " ms");
                }
                else
                {
                    if (domReady.TotalMilliseconds != 0)
                        Console.Write(domReady.TotalMilliseconds + " ");
                    Console.WriteLine(time.TotalMilliseconds);
                }
            }
        }
    }
}