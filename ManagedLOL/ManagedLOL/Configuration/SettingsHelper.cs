using System.IO;
using System.Linq;

namespace ManagedLOL.Configuration
{
    public static class SettingsHelper
    {
        public static bool HasLolPath { get { return IsLolPath(Properties.Settings.Default.IC_LOLPath); } }

        public static string LolPath { get { return Properties.Settings.Default.IC_LOLPath; } }

        private static string _champions = null;

        public static string LolPathImagesChampions
        {
            get
            {
                if (_champions == null)
                {
                    if (_projectslolairclientVersion == null)
                        _projectslolairclientVersion = DetectNewestVersion(Properties.Settings.Default.IC_LOLPath + @"\rads\projects\lol_air_client\releases");
                    _champions = Properties.Settings.Default.IC_LOLPath + @"\rads\projects\lol_air_client\releases\" + _projectslolairclientVersion + @"\deploy\assets\images\champions";
                }
                return _champions;
            }
        }

        private static string _items = null;

        public static string LolPathImagesItems
        {
            get
            {
                if (_items == null)
                {
                    if (_projectslolairclientVersion == null)
                        _projectslolairclientVersion = DetectNewestVersion(Properties.Settings.Default.IC_LOLPath + @"\rads\projects\lol_air_client\releases");
                    _items = Properties.Settings.Default.IC_LOLPath + @"\rads\projects\lol_air_client\releases\" + _projectslolairclientVersion + @"\deploy\assets\images\items";
                }
                return _items;
            }
        }

        private static string _itemsets = null;

        public static string LolPathItemsCharacters
        {
            get
            {
                if (_itemsets == null)
                {
                    if (_solutionslolgameclientslnVersion == null)
                        _solutionslolgameclientslnVersion = DetectNewestVersion(Properties.Settings.Default.IC_LOLPath + @"\rads\solutions\lol_game_client_sln\releases");
                    _itemsets = Properties.Settings.Default.IC_LOLPath + @"\rads\solutions\lol_game_client_sln\releases\" + _solutionslolgameclientslnVersion + @"\deploy\DATA\characters";
                }
                return _itemsets;
            }
        }

        public static string ChampionPortraitSearchPattern { get { return "*_Square_0.png"; } }

        public static bool IsLolPath(string path)
        {
            return
                // is valid Directory
                !string.IsNullOrWhiteSpace(path) && Directory.Exists(path) &&

                // Has projects lolairclient
                Directory.Exists(path + @"\rads\projects\lol_air_client\releases\");
        }

        private static string _projectslolairclientVersion = null;
        private static string _solutionslolgameclientslnVersion = null;

        private static string DetectNewestVersion(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            var dirs = di.EnumerateDirectories("0.0.0.*");
            var newest = dirs.Last();
            if (newest == null) return "";
            return newest.Name;
        }
    }
}