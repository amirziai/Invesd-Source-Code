using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Net.Mail;
using System.Web.UI.HtmlControls;

public partial class sendEmail : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        build_dropdown();
        build_tracker_table();
        txt_message.Text = Constants.message_invitation;
    }

    protected void build_dropdown() {
        DataClassesDataContext db = new DataClassesDataContext();
        var user = from temp in db.users where !temp.bloomberg_broker.HasValue select temp;
        pickuser.Items.Add(new ListItem("","0"));

        if (user.Any()) {
            foreach (var u in user) {
                pickuser.Items.Add(new ListItem(u.display_name, u.userID.ToString()));
            }
        }

    }

    protected void get_user_info(object sender, EventArgs e) {
        DataClassesDataContext db = new DataClassesDataContext();

        try
        {
            var user = from temp in db.users where temp.userID == Convert.ToInt32(pickuser.SelectedValue) select temp;

            if (user.Any())
            {
                TheEmail.Text = user.First().email.Trim();
                displayNameTb.Text = user.First().display_name;
            }
        }
        catch { }
    }

    protected void build_tracker_table() {
        DataClassesDataContext db = new DataClassesDataContext();
        var tracks = from temp in db.Launch_Trackers select temp;

        if (tracks.Any())
        {
            int c = 1;
            foreach (var t in tracks) {
                TableRow row = new TableRow();
                TableCell c1 = new TableCell();
                TableCell c2 = new TableCell();
                TableCell c3 = new TableCell();
                TableCell c4 = new TableCell();
                TableCell c5 = new TableCell();
                TableCell c6 = new TableCell();

                c1.Text = "<span style=\"display:block;text-align:center\">" + c.ToString() + "</span>";
                c2.Text = t.user.display_name;
                c3.Text = "<a href=\"track.aspx?user=" + t.investor + "\" class=\"urls\"><span style=\"display:block;text-align:right\">" + string.Format("{0:n0}",(from temp in db.trackings where temp.analyst == t.investor select temp).Count()) + "</span></a>";
                c4.Text = t.invited.HasValue ? "<i class=\"icon-ok\" style=\"color:#62c462;display:block;text-align:center\" title=\"" + (DateTime.Now- t.invited.Value).TotalDays + "\"></i>" : "";
                c5.Text = t.signedup.HasValue ? "<i class=\"icon-ok\" style=\"color:#62c462;display:block;text-align:center\"></i>" : "";
                c6.Text = t.challenge.HasValue ? "<i class=\"icon-ok\" style=\"color:#62c462;display:block;text-align:center\"></i>" : "";

                row.Cells.Add(c1);
                row.Cells.Add(c2);
                row.Cells.Add(c3);
                row.Cells.Add(c4);
                row.Cells.Add(c5);
                row.Cells.Add(c6);

                tbl.Rows.Add(row);

                c++;
            }
        }
        else {
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            cell.ColumnSpan = 6;
            cell.Text = "<p class=\"text-center\">No invitations sent yet</p>";
            row.Cells.Add(cell);
            tbl.Rows.Add(row);
        }
    }

    protected void Send_Click(object sender, EventArgs e)
    {
        string msg = "";
        try
        {
            DataClassesDataContext db = new DataClassesDataContext();
            user theUser = new user();
            var user = from temp in db.users where temp.email.ToLower() == TheEmail.Text.ToLower() select temp;
            if (user.Any())
            {
                msg += " Email exists.";
                theUser = user.First();
                if (string.IsNullOrEmpty(theUser.roles))
                {
                    theUser.roles = "investor";                    
                }
                else
                {
                    theUser.roles += ";investor";
                }
                theUser.display_name = displayNameTb.Text;
            }
            else
            {
                theUser.display_name = displayNameTb.Text;
                theUser.temp_email = TheEmail.Text.Trim();
                theUser.roles = "investor";
            }

            theUser.verified = false;
            string Rand = Ancillary.RandomString(20);
            theUser.randomVariable = Rand;

            if (!user.Any())
            {
                db.users.InsertOnSubmit(theUser);
            }
            try
            {
                db.SubmitChanges();
                Ancillary.mini_launch_tracker(theUser.userID, "invited"); // track
            }
            catch { }

            MailMessage newMsg = new MailMessage();
            newMsg.From = new MailAddress("info@invesd.com", "Invesd");
            newMsg.IsBodyHtml = true;
            newMsg.To.Add(new MailAddress(TheEmail.Text,displayNameTb.Text));
            newMsg.Subject = txt_subject.Text;
            //newMsg.Body = Ancillary.general_message_template(displayNameTb.Text,"https://invesd.com/Signup.aspx?uid=" + theUser.userID + "&rid=" + Rand,"Click here to join",txt_message.Text,false,null);

            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.UseDefaultCredentials = true;
                smtp.Send(newMsg);
            }
            status.Text = "Sent to " + TheEmail.Text + " <i class=\"icon-ok\" style=\"color:#62c462\"></i>" + msg; ;
        }

        catch (Exception e4)
        {
            status.Text = e4.Message + e4.StackTrace;
        }
    }

}