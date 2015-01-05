using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Services;

public partial class Competition : System.Web.UI.Page
{
    public static int userid = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Page.Title = "$100k investment challenge - gain confidence, showcase your skills & win prizes | Invesd";

            // navigation buttons
            HyperLink h = (HyperLink)Master.FindControl("hyp_challenge");
            h.Visible = false;
            HtmlGenericControl div = (HtmlGenericControl)Master.FindControl("div_challenge");
            div.Attributes.Remove("class");
            div.Attributes.Add("class", "text-center navbar_button_active");
            Label l = new Label();
            l.Text = "Challenge";
            l.ForeColor = System.Drawing.Color.White;
            div.Controls.Add(l);


            userid = DataBaseLayer.GetCheckUser();
            

            if (userid>0)
            {
                DataClassesDataContext db = new DataClassesDataContext();
                var investor = from temp in db.users where temp.userID == userid select temp;
                l_investor.Text = investor.First().display_name;
                investor = investor.Where(b => b.competition_entered.Value);
                image.ImageUrl = DataBaseLayer.get_user_pic(userid, true);

                if (investor.Any())
                {
                    // logged in and competing
                    Leaderboard_Profit_Deployed_Sectors data = new Leaderboard_Profit_Deployed_Sectors();
                    data = Portfolio.investor_profit_deployed_sectors_guts(userid);

                    teaser.Visible = false;
                    profit.Text = data.profit > 0 ? ("<span style=\"color:#62c462\">" + string.Format("{0:c0}", data.profit) + "</span>") : (data.profit < 0 ? "<span style=\"color:#ee5f5b\">" + string.Format("{0:c0}", -data.profit) + "</span>" : string.Format("{0:c0}", -data.profit));
                    funds.Text = string.Format("{0:c0}", investor.First().cash.HasValue ? investor.First().cash.Value : 0);
                    deployed.Text = string.Format("{0:n0}", 100 * data.deployed) + "%";
                    stocks.Text = string.Format("{0:c0}", data.market_value_stocks);
                }
                else { 
                    // logged in, NOT competing
                    loginorsignup.Visible = false;
                    competing1.Visible = false;
                    competing2.Visible = false;
                }
            }
            else { 
                // not logged in
                logged_in.Visible = false;
                optin.Visible = false;
            }
        }   
    }

    [WebMethod]
    public static string enter() {
        DataClassesDataContext db = new DataClassesDataContext();
        int userid = DataBaseLayer.GetCheckUser();
        try
        {
            var user = from temp in db.users where temp.userID == userid select temp;

            if (user.Any())
            {
                user.First().competition_entered = true;
                user.First().joined_challenge = DateTime.Now;
                db.SubmitChanges();
                Ancillary.mini_launch_tracker(userid, "challenge");
                return "success";
            }
            else {
                return "error";
            }
        }
        catch {
            return "error";
        }
    }

}