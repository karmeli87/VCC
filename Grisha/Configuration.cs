using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace VCC
{
    #region -- Configuration Class --
    /// <summary>
    /// This Configuration class is basically just a set of 
    /// properties with a couple of static methods to manage
    /// the serialization to and deserialization from a
    /// simple XML file.
    /// </summary>
    [Serializable]
    public class Configuration
    {
        int _Version;
        string _StringItem;
        int _IntItem;

        public Configuration()
        {
            _Version = 1;
       /*     _ProjectPath = "";
            _LayoutPath = "";
            _ProgramPath = "";
            _MUXpath = "";
            _MUXport = "";
            _FTPusername = "";
            _FTPpassword = "";
            _FTPport = "";
            _FTPip = "";*/

        }
        public static void Serialize(string file, Configuration c)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(c.GetType());
            StreamWriter writer = File.CreateText(file);
            xs.Serialize(writer, c);
            writer.Flush();
            writer.Close();
        }
        public static Configuration Deserialize(string file)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(
                  typeof(Configuration));
            StreamReader reader = File.OpenText(file);
            Configuration c = (Configuration)xs.Deserialize(reader);
            reader.Close();
            return c;
        }
        public int Version
        {
            get { return _Version; }
            set { _Version = value; }
        }
        public string StringItem
        {
            get { return _StringItem; }
            set { _StringItem = value; }
        }
        public int IntItem
        {
            get { return _IntItem; }
            set { _IntItem = value; }
        }

    }
    #endregion

}
