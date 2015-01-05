using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Default2 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (DataBaseLayer.GetCheckUser() > 0)
            Response.Redirect("home");
        else
            Response.Redirect("company");
        
        //return;

        //if (!IsPostBack)
        //{
            
        //    int userid = DataBaseLayer.GetCheckUser();

        //    if ( userid> 0)
        //        Response.Redirect("Home.aspx");

        //    Scaffolding.check_loop_back();

        //    // track guest
        //    Scaffolding.track_guest();

        //    // check for invitation, add a cookie if need be
        //    if (!string.IsNullOrEmpty(Request.QueryString["inviter"]))
        //    {
        //        if (Request.Cookies["InvesdInviter"] == null)
        //        {
        //            DataClassesDataContext db = new DataClassesDataContext();
        //            try{
        //                var inviter = from temp in db.users where temp.userID == Convert.ToInt32(Request.QueryString["inviter"]) select temp;
        //                if (inviter.Any())
        //                {
        //                    HttpCookie invitee_cookie = new HttpCookie("InvesdInviter");
        //                    invitee_cookie.Value = inviter.First().userID.ToString();
        //                    invitee_cookie.Expires = DateTime.Now.AddYears(2);
        //                    invitee_cookie.Domain = "invesd.com";
        //                    Response.Cookies.Add(invitee_cookie);
        //                }
        //            }
        //            catch{}
        //        }
        //    }
        //}
    }

}