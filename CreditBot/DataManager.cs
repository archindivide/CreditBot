using System;
using System.IO;
using System.Xml.Linq;

namespace CreditBot
{
    public static class DataManager
    {
        private static string _dbName = "CreditBot.xml";
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
        private static XElement XUsers => XDoc.Element("Root").Element("Users");

        public static void InitializeDatabase()
        {
            if (!File.Exists(_dbName))
                CreateDatabase();
        }

        public static User GetUser(string name)
        {
            XElement user = XUsers.Element(name);
            if (user != null)
                return new User(user.Name.ToString(), Convert.ToInt32(user.Element("Value").Value));
            else
                return null;
        }

        internal static void SaveUserData(User userObj)
        {
            XElement user = XUsers.Element(userObj.UserName);
            if (user != null)
            {
                user.Element("Value").Value = userObj.Value.ToString();
            }
            else
            {
                XUsers.Add(new XElement(userObj.UserName,
                                            new XElement("Value",
                                                userObj.Value)));
            }
            XDoc.Save(_dbName);
        }

        private static void CreateDatabase()
        {
            XDocument xdoc = new XDocument(new XDeclaration("1.0", "utf8", "yes"),
                                            new XElement("Root",
                                                new XElement("Users")));
            xdoc.Save(_dbName);
        }
    }
}
