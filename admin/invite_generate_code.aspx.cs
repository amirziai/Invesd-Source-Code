using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_invite_generate_code : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void invite_code(object sender, EventArgs e)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var user = from temp in db.users where temp.email == txt_email.Text select temp;

        if (user.Any())
        {
            if (user.First().verified == false || user.First().state != "verified")
            {

                link.Text = "EXISTS " + "https://invesd.com/Signup.aspx?uid=" + user.First() + "&rid=" + user.First().randomVariable;
            }
            else
            {
                link.Text = "verified user";
            }
        }
        else
        {
            user u = new user();
            u.display_name = txt_name.Text;
            u.email = txt_email.Text;
            u.roles = "investor";
            u.verified = false;
            u.randomVariable = Ancillary.RandomString(20);

            try
            {
                db.users.InsertOnSubmit(u);
                db.SubmitChanges();
                Ancillary.mini_launch_tracker(u.userID, "invited"); // track
                link.Text = "https://invesd.com/Signup.aspx?uid=" + u.userID + "&rid=" + u.randomVariable;
            }
            catch (Exception ee) {
                link.Text = "error" + ee.Message;
            }
        }
    }
}