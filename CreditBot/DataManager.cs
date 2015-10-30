using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CreditBot
{
    public static class DataManager
    {
        private static string _dbName = "CreditBot.xml";
        private const int _dbVersion = 1;   //Increment this value to mark older DB versions as out of date,
                                            //this renames their current DB so they don't lose any data
                                            //TODO: make the DB automatically carry over old values
        private static XDocument _xDoc;
        private static XDocument XDoc
        {
            get
            {
                if (_xDoc == null)
                    _xDoc = XDocument.Load(_dbName);

                return _xDoc;
            }
        }
        private static XElement XUsers => XDoc.Element("Users");

        public static void InitializeDatabase()
        {
            if (!File.Exists(_dbName))
                CreateDatabase();

            int ver = Convert.ToInt32(XUsers.Element("DB").Attribute("Version").Value);
            if (ver < _dbVersion)
            {
                string newname = "Old-Version" + ver + "-" + _dbName;
                File.Move(_dbName, newname);
                //TODO: Add popup here notifying the user of the DB changes
                CreateDatabase();
            }
        }

        private static XElement GetUserXElement(string name)
        {
            return  (from el in XUsers.Elements("User")
                    where el.Element("Name").Value == name
                    select el).SingleOrDefault();
        }

        public static User GetUser(string name)
        {
            XElement user = GetUserXElement(name);
            if (user != null)
                return new User(user.Element("Name").Value, Convert.ToInt32(user.Element("Value").Value));
            else
                return null;
        }

        internal static void SaveUserData(User userObj)
        {
            XElement user = GetUserXElement(userObj.UserName);
            if (user != null)
            {
                user.Element("Value").Value = userObj.Value.ToString();
            }
            else
            {
                XUsers.Add(
                    new XElement("User",
                        new XElement("Name", userObj.UserName),
                        new XElement("Value", userObj.Value)));
            }
            XDoc.Save(_dbName);
        }

        //Data format:
        //<Users>
        //  <User>
        //      <Name>string Username</Name>
        //      <Value>int credits</Value>
        //  </User>
        private static void CreateDatabase()
        {
            XDocument xdoc = new XDocument(
                new XDeclaration("1.0", "utf8", "yes"),
                new XElement("Users",
                    new XElement("DB", new XAttribute("Version", _dbVersion))));

            xdoc.Save(_dbName);
        }
    }
}
