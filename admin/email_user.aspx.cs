using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_email_user : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        
    }

    protected void btn_click(object sender, EventArgs e)
    {
        try
        {
            //string body = Ancillary.general_message_template(txt_name.Text, txt_link.Text, txt_button.Text, txt_message.Text,false,null);
            //Ancillary.send_email(txt_from_email.Text, txt_from.Text, txt_email.Text, txt_name.Text, txt_subject.Text,body, true);
            status.Text = "Success";
        }
        catch
        {
            status.Text = "Error";
        }
        
    }
}