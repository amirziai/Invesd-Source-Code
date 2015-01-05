using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Text;

public partial class admin_Default : System.Web.UI.Page
{
    public static int[] daily_users = new int[7] {0,0,0,0,0,0,0};
    public static double[] pages_user = new double[7] { 0, 0, 0, 0, 0, 0, 0 };
    public static int type1 = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        int admin_id = DataBaseLayer.GetCheckUser();
        if (admin_id > 2) {
            tbl_admin1.Visible = false;
            tbl_admin3.Visible = false;
        }

        var actions = from temp in db.Actions select temp;
        int gripes_count = (from temp in db.Gripes where !temp.message.Contains("(Google") && !temp.message.Contains("Changed/exists") select temp).Count();

        if (actions.Any()) {
            actions_all.Text = "<h3>" + string.Format("{0:N0}", actions.Count()) + "</h3>";

            actions = actions.Where(b => b.active);

            if (actions.Any()) {
                companies.Text = "<h3>" + string.Format("{0:N0}", actions.GroupBy(b => b.ticker).Count()) + "</h3>";
                analysts.Text = "<h3>" + string.Format("{0:N0}", actions.GroupBy(b => b.article1.origin).Count()) + "</h3>";
                actions_active.Text = "<h3>" + string.Format("{0:N0}", actions.Count()) + "</h3>";
                industries.Text = "<h3>" + string.Format("{0:N0}", actions.Where(b=>!b.user.bloomberg_broker.HasValue && b.analystID!=1 && b.analystID!=2).GroupBy(b=>b.ticker).Count()) + "</h3>";
                gripes.Text = (gripes_count > 0 ? ("<h3 style=\"color:red\">" + string.Format("{0:n0}", gripes_count) + "</h3>") : ("<h3 style=\"color:black\">" + string.Format("{0:n0}", gripes_count) + "</h3>"));
                gripes.NavigateUrl = "Gripe.aspx";
                type1 = (from temp in db.articles where temp.type == 1 && !temp.action select temp).Count();
                type1articles.Text = string.Format("{0:n0}", type1);
            }
        }

        var launch_tracker = from temp in db.Launch_Trackers select temp;


        if (launch_tracker.Any()) {
            //invited.Text = "<h3>" + string.Format("{0:N0}", launch_tracker.Where(b => b.invited.HasValue).Count()) + "</h3>";
            //signedup.Text = "<h3>" + string.Format("{0:N0}", launch_tracker.Where(b => b.signedup.HasValue).Count()) + "</h3>";
            //challenge.Text = "<h3>" + string.Format("{0:N0}", launch_tracker.Where(b => b.challenge.HasValue).Count()) + "</h3>";
            //positions.Text = "<h3>" + string.Format("{0:N0}", (from temp in db.ActionMonitors where temp.usermon > 2 && temp.usermon!=2656147 && temp.active select temp).Count()) + "</h3>";
        }


        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");
        sb.Append("var daily_users = new Array;");
        sb.Append("var pages_per_user = new Array;");
        sb.Append("var emails = new Array;");
        sb.Append("var actions = new Array;");
        sb.Append("var digest = new Array;");

        var tracks = from temp in db.trackings where (DateTime.Now - temp.timestamp).TotalDays < 6 && !Constants.excluded_users.Contains(temp.analyst) && temp.user.display_name.Length>0 select temp;

        if (tracks.Any()) {
            int c = 0;
            foreach (var track_day in tracks.GroupBy(b => b.timestamp.Date).OrderBy(b=>b.First().timestamp.Date)) {
                var http = track_day.Where(b => b.hyperlink.Contains("http"));

                int users = http.GroupBy(b => b.analyst).Count();
                sb.Append("daily_users.push(" + users + ");");
                sb.Append("emails.push(" + track_day.Where(b => b.hyperlink.Contains("email_")).GroupBy(D => D.analyst).Count() + ");");
                sb.Append("digest.push(" + track_day.Where(b => b.hyperlink.Contains("daily_digest")).GroupBy(D => D.analyst).Count() + ");");
                sb.Append("pages_per_user.push(" + Math.Round((double)http.Count() / (double)users,2) + ");");
                c++;
            }
        }
        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), "TestScript", sb.ToString());

    }

    [WebMethod]
    public static string scrape() {

        string[] list_of_sections ={
        "long-ideas",
        "quick-picks-lists",
        "fund-holdings",
        "cramers-picks",
        "short-ideas",
        "insider-ownership",
        "ipo-analysis",
        "options",
        "investing-ideas,editors-picks,articles",                           
        "dividend-ideas",
        "income-investing-strategy",
        "dividend-quick-picks-lists",
        "reits",
        //"bonds",
        "retirement",
        "portfolio-strategy-asset-allocation",
        "etf-long-short-ideas",
        "etf-analysis",
        "etf-quick-picks-and-lists",
        "closed-end-funds",
        "market-outlook",
        "gold-and-precious-metals",
        "commodities",
        "economy",
        "forex",
        "real-estate",
        "demographics",
        };

        int[] count = new int[2]{0,0};
        int[] tmp = new int[2]{0,0};

        try
        {
            foreach (var section in list_of_sections)
            {
                tmp = AdminBackend.scrapebaby(1, 20, section);
                count[0] += tmp[0];
                count[1] += tmp[1];
            }
        }
        catch {
            return "Error";
        }

        return count[1].ToString();
    }

    [WebMethod]
    public static void update_actions_6pm() {
        string error_msg = "";

        try
        {
            string msg = AdminBackend.updateAllDB_Click(false);
            if (string.IsNullOrEmpty(msg))
                error_msg += msg;
            else
                error_msg += "Successfully fetched fund values<br>";

            Ancillary.email_amir("6pm report, 1/8 fund fetch complete", msg, "Report");
        }
        catch (Exception e)
        {
            error_msg += "Error fetching fund values<br>";
            Ancillary.email_amir("6pm report, 1/8 fund fetch error", e.Message + "<br><br>" + e.StackTrace, "Error");
        }
        try
        {
            AdminBackend.update_action_monitor(true);
            error_msg += "Successfully updated user positions<br>";
            Ancillary.email_amir("6pm report, 2/8 positions complete", "Good", "Report");
        }
        catch (Exception e)
        {
            error_msg += "Error updating user positions<br>";
            Ancillary.email_amir("6pm report, 2/8 positions error", e.Message + "<br><br>" + e.StackTrace, "Error");
        }
        try
        {
            AdminBackend.update_All_Actions_v2(false);
            error_msg += "Successfully updated actions<br>";
            Ancillary.email_amir("6pm report, 3/8 actions complete", "Good", "Report");
        }
        catch (Exception e)
        {
            error_msg += "Error updating actions<br>";
            Ancillary.email_amir("6pm report, 3/8 actions error", e.Message + "<br><br>" + e.StackTrace, "Error");
        }
        try
        {
            AdminBackend.update_live_analyst_performance();
            error_msg += "Successfully updated live analyst performance<br>";
            Ancillary.email_amir("6pm report, 4/8 actions complete", "Good", "Report");
        }
        catch (Exception e)
        {
            error_msg += "Error updating live analyst performance<br>";
            Ancillary.email_amir("6pm report, 4/8 live analyst performance error", e.Message + "<br><br>" + e.StackTrace, "Error");
        }
        try
        {
            AdminBackend.analyst_performance_aggregation();
            error_msg += "Successfully updated analyst performance industry<br>";
            Ancillary.email_amir("6pm report, 5/8 scoring aggregation complete", "Good", "Report");
        }
        catch (Exception e)
        {
            error_msg += "Error updating analyst performance industry<br>";
            Ancillary.email_amir("6pm report, 5/8 scoring aggregation error", e.Message + "<br><br>" + e.StackTrace, "Error");
        }
        try
        {
            AdminBackend.update_confidence();
            error_msg += "Successfully updated analyst confidence<br>";
            Ancillary.email_amir("6pm report, 6/8 confidence complete", "Good", "Report");
        }
        catch (Exception e)
        {
            error_msg += "Error updating analyst confidence<br>";
            Ancillary.email_amir("6pm report, 6/8 confidence error", e.Message + "<br><br>" + e.StackTrace, "Error");
        }
        try
        {
            AdminBackend.metric_ticker();
            error_msg += "Successfully updated aggregate ticker<br>";
            Ancillary.email_amir("6pm report, 7/8 ticker aggregate complete", "Good", "Report");
        }
        catch (Exception e) {
            error_msg += "Error updating aggregate ticker<br>";
            Ancillary.email_amir("6pm report, 7/8 ticker aggregate error", e.Message + "<br><br>" + e.StackTrace, "Error");
        }
        try
        {
            AdminBackend.metrics_industry_sector();
            error_msg += "Successfully updated aggregate sector & industry<br>";
            Ancillary.email_amir("6pm report, 8/8 industry/sector aggregate complete", "Good", "Report");
        }
        catch (Exception e) {
            error_msg += "Error updating aggregate sector & industry<br>";
            Ancillary.email_amir("6pm report, 8/8 industry/sector aggregate error", e.Message + "<br><br>" + e.StackTrace, "Error");
        }
        //error_msg += AdminBackend.user_analytics();
        Ancillary.send_email("report@invesd.com", "Invesd Report", "arziai@gmail.com", "Amir", "Evening walkthrough report", error_msg, true);
    }

    [WebMethod]
    public static void update_actions_1pm()
    {
        AdminBackend.update_actions_1pm_guts(0);
    }

    

    [WebMethod]
    public static string update_some_tickers(string tickers)
    {
        string step1 = AdminBackend.updateSomeFunds_Click(tickers);
        return step1;   
    }

    [WebMethod]
    public static string add_tickers(string tickers)
    {
        string step1 = AdminBackend.fundAdd_Click(tickers);
        return step1;
    }

    [WebMethod]
    public static string quick_wall_st() {
        string status = AdminBackend.quick_wall_st_data();
        Ancillary.send_email("report@invesd.com", "Invesd Report", "arziai@gmail.com", "Amir", "Quick Wall St. update report", status, true);
        return status;
    }

    [WebMethod]
    public static string full_wall_st()
    {
        string status = AdminBackend.full_wall_st_data();
        Ancillary.send_email("report@invesd.com", "Invesd Report", "arziai@gmail.com", "Amir", "Full Wall St. update report", status, true);
        return status;
    }

    [WebMethod]
    public static string inactivate_fund(string tickers)
    {
        return AdminBackend.InactivateFund(tickers);
    }

}