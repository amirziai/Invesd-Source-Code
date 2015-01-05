using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using Encryption;

public partial class admin_testdecript : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var cr = (from temp in db.credentials where temp.userID == 1 select temp).First();
        Byte[] tempByte = Convert.FromBase64String(cr.authKey.Trim());
        string decPass = AESThenHMAC.SimpleDecrypt(cr.securePass, Convert.FromBase64String(cr.cryptKey.Trim()), Convert.FromBase64String(cr.authKey.Trim()));
    }
}