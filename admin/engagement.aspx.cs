using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

public partial class admin_engagement : System.Web.UI.Page
{
    public int offset = -1;
    public int span = 30;
    public int eng = -1;

    protected void Page_Load(object sender, EventArgs e)
    {
        Page.Title = "Engagement Dashboard | Invesd";

        DataClassesDataContext db = new DataClassesDataContext();
        charts(db);
        tracking(db);
        engagement(db);
    }

    public void charts(DataClassesDataContext db)
    {
        try
        {
            if (!string.IsNullOrEmpty(Request.QueryString["offset"]))
                offset = Convert.ToInt32(Request.QueryString["offset"]);
        }
        catch { }
        try
        {
            if (!string.IsNullOrEmpty(Request.QueryString["span"]))
                span = Convert.ToInt32(Request.QueryString["span"]);
        }
        catch { }

        // previous stats
        var previous = from temp in db.users where temp.userID != 1 && temp.userID != 2 && temp.joined.HasValue && temp.joined.Value.Date < DateTime.Now.AddDays(-span).Date select temp;
        int previous_users = previous.Count();
        int previous_users_ver = previous.Where(b => b.verified.Value || b.state == "verified").Count();

        var previous_estimates_query = from temp in db.Actions where temp.analystID != 1 && temp.analystID != 2 && temp.user.joined.HasValue && temp.creationTime < DateTime.Now.AddDays(-span).Date select temp;
        int previous_estimates = previous_estimates_query.Count();
        int previous_estimates_verified = previous_estimates_query.Where(b => b.user.verified.Value || b.user.state == "verified").Count();
        int previous_positions = (from temp in db.ActionMonitors where temp.usermon!=2 && temp.usermon!=1 && temp.investment_date < DateTime.Now.AddDays(-span).Date select temp).Count();


        List<string> categories = new List<string>();
        List<int> estimates = new List<int>();
        List<int> estimates_ver = new List<int>();
        List<int> users = new List<int>();
        List<int> users_ver = new List<int>();
        List<int> positions = new List<int>();
        List<int> cum_users = new List<int>();
        List<int> cum_users_ver = new List<int>();
        List<int> cum_estimates = new List<int>();
        List<int> cum_estimates_ver = new List<int>();
        List<int> cum_positions = new List<int>();


        for (DateTime date = DateTime.Now.AddDays(-span).Date; date <= DateTime.Now.Date; date = date.AddDays(1))
        {
            string d = date.DayOfWeek + " " + date.Month + "/" + date.Day;

            // queries
            var actions = from temp in db.Actions where temp.analystID != 1 && temp.analystID != 2 && temp.user.joined.HasValue && temp.creationTime.Date == date.Date select temp;
            var actions_verified = actions.Where(b => b.user.verified.Value || b.user.state == "verified");
            var users_new = from temp in db.users where temp.userID != 1 && temp.userID != 2 && temp.joined.Value.Date == date.Date select temp;
            var positions_new = from temp in db.ActionMonitors where temp.usermon != 1 && temp.usermon != 2 && temp.investment_date.Date == date.Date select temp;
            int users_new_ver = users_new.Where(b => b.verified.Value || b.state == "verified").Count();

            // javascript
            categories.Add(d);
            estimates.Add(actions.Count());
            estimates_ver.Add(actions_verified.Count());
            users.Add(users_new.Count());
            users_ver.Add(users_new_ver);
            positions.Add(positions_new.Count());
            previous_users += users_new.Count();
            previous_users_ver += users_new_ver;
            cum_users.Add(previous_users);
            cum_users_ver.Add(previous_users_ver);
            previous_estimates += actions.Count();
            previous_estimates_verified += actions_verified.Count();
            previous_positions += positions_new.Count();
            cum_estimates.Add(previous_estimates);
            cum_estimates_ver.Add(previous_estimates_verified);
            cum_positions.Add(previous_positions);
        }

        push_to_javascript_string("categories", categories);
        push_to_javascript("estimates", estimates);
        push_to_javascript("estimates_ver", estimates_ver);
        push_to_javascript("users", users);
        push_to_javascript("users_ver", users_ver);
        push_to_javascript("positions", positions);
        push_to_javascript("cum_users", cum_users);
        push_to_javascript("cum_users_ver", cum_users_ver);
        push_to_javascript("cum_estimates", cum_estimates);
        push_to_javascript("cum_estimates_ver", cum_estimates_ver);
        push_to_javascript("cum_positions", cum_positions);
    }

    public void engagement(DataClassesDataContext db)
    {
        try
        {
            if (!string.IsNullOrEmpty(Request.QueryString["eng"]))
                eng = 1;
        }
        catch { }

        if (eng == 1)
        {
            var engagement = (from temp in db.Actions where temp.user.joined.HasValue && temp.analystID != 1 && temp.analystID != 2 select temp).GroupBy(b => b.analystID);
            if (engagement.Any())
            {
                List<int> vals_verified = new List<int>() { 0, 0, 0, 0, 0, 0 };
                List<int> vals_not_verified = new List<int>() { 0, 0, 0, 0, 0, 0 };

                foreach (var a in engagement)
                {
                    var user_item = a.First();
                    int count = a.Count();
                    int index = count > 20 ? 5 : (count > 10 ? 4 : (count > 5 ? 3 : (count >= 3 ? 2 : (count >= 2 ? 1 : 0))));
                    if (user_item.user.verified.Value || user_item.user.state == "verified")
                        vals_verified[index]++;
                    else
                        vals_not_verified[index]++;
                }

                push_to_javascript("engagement_not_verified", vals_not_verified);
                push_to_javascript("engagement_verified", vals_verified);
            }
        }
    }

    public void tracking(DataClassesDataContext db){
        Dictionary<string, int> ticker = new Dictionary<string, int>();

        try
        {
            if (!string.IsNullOrEmpty(Request.QueryString["offset"]))
                offset = Convert.ToInt32( Request.QueryString["offset"]);
        }
        catch { }

        if (offset > -1)
        {
            var track = from temp in db.trackings where temp.user.display_name.Length > 0 && temp.timestamp.Date == DateTime.Now.AddDays(-offset).Date && temp.analyst != 2 && temp.analyst != 1 select temp;
            if (track.Any())
            {
                foreach (var t in track.GroupBy(b => b.analyst).OrderByDescending(b => b.Count()))
                {
                    //
                    var user = t.First().user;
                    output.Text += user.userID + ", " + user.email + ", " + user.display_name + ((user.verified.Value || user.state == "verified") ? "*" : "") + ", " + t.Count() + "<br>";
                    //Response.Write(user.userID + ", " + user.email + ", " + user.display_name + ((user.verified.Value || user.state == "verified") ? "*" : "") + ", " + t.Count() + "<br>");
                    var actions = from temp in db.Actions where temp.creationTime.Date == DateTime.Now.AddDays(-offset).Date && temp.analystID == user.userID select temp;
                    if (actions.Any())
                    {
                        output.Text +="<font color=green>";
                        //Response.Write("<font color=green>");
                        foreach (var a in actions)
                        {
                            string ticker_string = a.fund.ticker.Trim();
                            output.Text += ticker_string + ", ";
                            int count = 0;
                            ticker.TryGetValue(ticker_string, out count);
                            if (count > 0)
                            {
                                ticker[ticker_string]++;
                            }
                            else
                            {
                                ticker.Add(ticker_string, 1);
                            }
                            //Response.Write(a.fund.ticker.Trim() + ", ");
                        }
                        output.Text += "</font><br>";
                        //Response.Write("</font><br>");
                    }

                    foreach (var x in t.GroupBy(b => b.hyperlink).OrderByDescending(b => b.Count()).Take(10))
                    {
                        output.Text += "<font color=gray>" + x.First().hyperlink.Replace("invesd.com/", "").Replace("http://", "").Replace("https://", "").Replace("www.", "") + "(" + x.Count() + ")" + "</font><br>";
                        //Response.Write("<font color=gray>" + x.First().hyperlink.Replace("invesd.com/", "").Replace("http://", "").Replace("https://", "").Replace("www.", "") + "(" + x.Count() + ")" + "</font><br>");
                    }
                    output.Text += "<br>";
                    //Response.Write("<br>");
                }

                foreach (var x in ticker.OrderByDescending(b=>b.Value))
                {
                    string size = x.Value >= 10 ? "x-large": (x.Value>=5?"large":(x.Value>=2?"normal":"small")) ;
                    ticker_frequency.Text += "<span style=\"font-size:" + size + "\">" + x.Key + "</span><span style=\"font-size:x-small\"> (" + x.Value + ")</span> , ";

                }
            }
        }
    }

    public void push_to_javascript(string name,List<int> values)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");
        sb.Append("var " + name + " = new Array;");

        foreach (var v in values)
            sb.Append(name + ".push(" + v + ");");

        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), name, sb.ToString());
    }

    public void push_to_javascript_string(string name, List<string> values)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");
        sb.Append("var " + name + " = new Array;");

        foreach (var v in values)
            sb.Append(name + ".push('" + v + "');");

        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), name, sb.ToString());
    }
}