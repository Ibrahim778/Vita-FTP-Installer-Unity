/*
 * This file is made by jllopisol@gmail.com all credits and I mean ALL credits of this file go to him
 * You can donate to him via paypal:
 * https://www.paypal.com/donate/?cmd=_s-xclick&hosted_button_id=RMFDRTBU49E8E
 */

namespace PeXploit
{
    public class SecureFileInfo
    {
        public SecureFileInfo(
          string name,
          string id,
          string securefileid,
          string dischashkey,
          bool isprotected)
        {
            this.Name = name;
            this.GameIDs = id.Trim('[', ']').Split('/');
            this.SecureFileID = securefileid;
            this.DiscHashKey = dischashkey;
            this.Protected = isprotected;
        }

        public string Name { get; set; }

        public string[] GameIDs { get; set; }

        public string SecureFileID { get; set; }

        public string DiscHashKey { get; set; }

        public bool Protected { get; set; }
    }
}
