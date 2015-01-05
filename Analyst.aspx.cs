using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;

public partial class Analyst : System.Web.UI.Page
{
    public string invested_actions;
    public int logged_in_page;

    public int analyst_id;
    protected void Page_Load(object sender, EventArgs e)
    {
        int tuser = DataBaseLayer.GetCheckUser();
        logged_in_page = tuser > 0 ? 1 : 0;

        if (!string.IsNullOrEmpty(Request.QueryString["analyst"]))
        {
            try
            {
                analyst_id = Convert.ToInt32(Request.QueryString["analyst"]);
                main_function(analyst_id);
            }
            catch (Exception err)
            {
                caught_error(Request.QueryString["analyst"],err);
            }
        }
        else
        {
            try
            {
                if (!string.IsNullOrEmpty(Page.RouteData.Values["id"].ToString()))
                {
                    analyst_id = Convert.ToInt32(Page.RouteData.Values["id"]);
                    main_function(analyst_id);
                }
                else
                {
                    no_querystring();
                }
                    
            }
            catch (Exception err)
            {
                caught_error(Page.RouteData.Values["id"].ToString(), err);
            }
            
        }
    }

    protected void main_function(int userid)
    {
        try
        {
            DataClassesDataContext db = new DataClassesDataContext();
            var auser = from temp in db.users where temp.userID == userid select temp;

            if (auser.Any())
            {
                var user = auser.First();
                int logged_in_use = DataBaseLayer.GetCheckUser();
                if (logged_in_use == 0)
                {
                    btn.Attributes.Remove("onclick");
                    btn.HRef = Page.ResolveUrl("~") + "Login.aspx?ReturnUrl=" + Request.Url;
                }

                l_broker.Text = Ancillary.string_cutter(user.bloomberg_broker.HasValue ? user.Bloomberg_Broker1.name : "", 15, true, "top");
                l_analyst.Text = user.display_name;
                Page.Title = l_analyst.Text + (!string.IsNullOrEmpty(l_broker.Text) ? ", " : "") + (user.bloomberg_broker.HasValue ? user.Bloomberg_Broker1.name : "") + " Analyst Profile | Invesd";

                image.ImageUrl = DataBaseLayer.get_user_pic(user.userID, true);
                bool follow = Users.follow_status(userid);
                btn.Visible = (userid != DataBaseLayer.GetCheckUser());
                btn.InnerText = follow ? "Following" : "Follow";
                btn.Attributes.Add("class", follow ? "btn btn-success" : "btn btn-info");

                // added by amir 2/1/2014
                List<int> action_monitors = Users.get_user_actionmonitor_action_ids(null);
                if (action_monitors.Any())
                {
                    foreach (var a in action_monitors)
                        invested_actions += (a + "_");
                }
            }
            else
            {
                DataBaseLayer.gripe(DataBaseLayer.GetCheckUser(), "analyst.aspx - user does not exist. UserID " + userid);
                Response.Redirect("/");
            }
        }
        catch (Exception exc)
        {
            DataBaseLayer.gripe(DataBaseLayer.GetCheckUser(), "caught an error in investor.aspx - " + exc.Message + " " + exc.StackTrace);
            Response.Redirect("/");
        }
    }

    protected void caught_error(string querystring,Exception e)
    {
        DataBaseLayer.gripe(DataBaseLayer.GetCheckUser(), "tried to load investor.aspx page without an investor querystring");
        Ancillary.email_amir("Error in analyst page querystring", "Tried to load analyst page with " + querystring + "<br><br>" + e.Message + "<br><br>" + e.StackTrace,"Error");
        Response.Redirect("Challenge.aspx");
    }

    protected void no_querystring()
    {
        Ancillary.email_amir("Analyst page", ", no querystring or route data", "Error");
        Response.Redirect("/");
    }

    [WebMethod]
    public static string follow(int followed)
    {
        return Users.follow(followed) ? "followed" : "unfollowed";
    }
}