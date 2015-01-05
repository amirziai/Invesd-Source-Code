using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class TicketManagement_Edit : System.Web.UI.Page
{
    // approve or reject an action/article edit/delete
    // send user confirmation

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(Request.QueryString["id"]))
            {
                int id = Convert.ToInt32(Request.QueryString["id"]);

                if (Request.QueryString["type"] == "action")
                {
                    if (Request.QueryString["event"] == "delete")
                    { 
                        // delete the original action, delete the ticket
                        // send user a confirmation email
                    }
                    else if (Request.QueryString["event"] == "edit")
                    {

                    }
                    else
                    {
                        Shoot_User_To_Error_Page();
                    }
                }
                else if (Request.QueryString["type"] == "article")
                {
                    if (Request.QueryString["event"] == "delete")
                    {

                    }
                    else if (Request.QueryString["event"] == "edit")
                    {

                    }
                    else
                    {
                        Shoot_User_To_Error_Page();
                    }
                }
                else if (Request.QueryString["type"] == "reject")
                { 
                    // reject article or action modification and send user an email
                    // explaining the situation

                }
                else
                {
                    Shoot_User_To_Error_Page();
                }
            }
            else
            {
                Shoot_User_To_Error_Page();
            }
        }
        catch
        {
            Shoot_User_To_Error_Page();
        }
    }

    protected void Shoot_User_To_Error_Page()
    {
        // shoot to error 
        Response.Redirect(Page.ResolveUrl("~/") + "Error.aspx");
    }
}