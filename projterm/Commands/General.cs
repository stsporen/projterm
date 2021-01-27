using ProjectTerminal.Properties;
using PuppeteerSharp;
using System;
using System.Linq;


// All console commands must be in the sub-namespace Commands:
namespace ProjectTerminal.Commands
{
    // Must be a public static class:
    public static class general
    {
        // Methods used as console commands must be public and must return a string


        // Downloads Chromium into local directory
        public static string initialize()
        {
            Console.WriteLine(string.Format(ConsoleFormatting.Indent(2) + strings.general_initialize_Download));

            RevisionInfo revision = new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision).Result;

            return string.Format(ConsoleFormatting.Indent(2) + strings.general_initialize_Complete, revision.Revision);
        }

        // Lists installed Chromium versions
        public static string installed()
        {
            var revision = new BrowserFetcher().LocalRevisions();

            string result = "";

            if (revision.Any())
            {
                int c = 0;

                foreach (int i in revision)
                {
                    result += string.Format(ConsoleFormatting.Indent(2) + strings.general_installed_ChromiumVersion, i);
                    c++;

                    if (c < revision.Count())
                    {
                        result += Environment.NewLine;
                    }
                }
            } 
            else
            {
                result = string.Format(ConsoleFormatting.Indent(2) + strings.general_installed_NotInstalled);
            }

            return result;
        }

        public static string install(int installrevision)
        {
            Console.WriteLine(string.Format(ConsoleFormatting.Indent(2) + strings.general_install_Download, installrevision));

            RevisionInfo revision = new BrowserFetcher().DownloadAsync(installrevision).Result;

            if (revision.Local)
            {
                return string.Format(ConsoleFormatting.Indent(2) + strings.general_install_CompleteSuccess, revision.Revision);
            }
            else
            {
                return string.Format(ConsoleFormatting.Indent(2) + strings.general_install_CompleteFailure, revision.Revision);
            }
        }

        public static string uninstall(int revision)
        {
            new BrowserFetcher().Remove(revision);

            return string.Format(ConsoleFormatting.Indent(2) + strings.general_uninstall_RemovedRevision, revision);
        }

        public static string clean()
        {
            // Remove all Chromium installs
            var revision = new BrowserFetcher().LocalRevisions();

            foreach (int i in revision)
            {
                new BrowserFetcher().Remove(i);
            }

            return string.Format(ConsoleFormatting.Indent(2) + strings.general_clean_Complete);
        }


        public static string exit()
        {
            System.Environment.Exit(-1);
            return "";
        }

        public static string quit()
        {
            System.Environment.Exit(-1);
            return "";
        }
    }
}
