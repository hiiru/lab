using System.IO;
using System.Xml.Serialization;
using ManagedLOL.Configuration;

namespace ManagedLOL.ItemChanger.Items
{
    [XmlRoot("Item")]
    public class Item
    {
        public enum ItemAvailability : uint
        {
            None = 0,
            Removed = 0,
            Both = 1,
            Classic = 2,
            Dominion = 3,
            Viktor = 4,
        }

        [XmlElement("ID")]
        public int ID { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("ImageFile")]
        public string ImageFile { get; set; }

        [XmlElement("Available")]
        public ItemAvailability Available { get; set; }

        [XmlIgnore]
        public string ImagePath
        {
            get
            {
                return SettingsHelper.LolPathImagesItems + @"\" + ImageFile;
            }
            set
            {
                FileInfo fi = new FileInfo(value);
                ImageFile = fi.Name;
            }
        }

        public Item(int id, string name, string imagepath)
        {
            ID = id;
            Name = name;
            ImagePath = imagepath;
        }

        public Item()
        {
        }
    }
}