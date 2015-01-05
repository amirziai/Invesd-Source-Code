using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class Home : System.Web.UI.Page
{
    public int user;
    protected void Page_Load(object sender, EventArgs e)
    {
        Page.Title = "Home | Invesd";
        user = DataBaseLayer.GetCheckUser();

        // navigation buttons
        HyperLink h = (HyperLink)Master.FindControl("hyp_home");
        h.Visible = false;
        HtmlGenericControl div = (HtmlGenericControl)Master.FindControl("div_home");
        div.Attributes.Remove("class");
        div.Attributes.Add("class", "text-center navbar_button_active");
        Label l = new Label();
        l.Text = "Home";
        l.ForeColor = System.Drawing.Color.White;
        div.Controls.Add(l);
    }
}