using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Configuration;
using System.Web.UI.HtmlControls;
using BCrypt.Net;
using System.Text.RegularExpressions;
using System.Web.Services;
using System.Net.Mail;

public partial class Signup4 : System.Web.UI.Page
{
    public string ReturnUrl;
    public string page_resolve_url_root;

    protected void Page_Load(object sender, EventArgs e)
    {
        set_redirect();

        // user logged in? redirect to home or returnURL
        if (DataBaseLayer.GetCheckUser() == 0)
        {
            Ancillary.redirect_to_ssl();
            page_resolve_url_root = Page.ResolveUrl("~");
            Page.Title = "Login to Invesd";
        }
        else
            Response.Redirect(ReturnUrl);
    }

    public void set_redirect()
    {
        if (!string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]))
            ReturnUrl = Request.QueryString["ReturnUrl"];
        else
            ReturnUrl = "/";
    }
}