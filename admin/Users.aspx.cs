using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_Users : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        get_user_list();
    }

    protected string horizon(double x)
    {
        if (x < 30)
        {
            return Math.Round(x, 0) + " days";
        }
        else
        {
            return Math.Round(x / 30, 0) + " months";
        }
    }

    protected void get_user_list()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var all = (from temp in db.trackings where temp.user.verified.HasValue && temp.analyst != 1 && temp.analyst != 2 && temp.analyst != 3193618 && temp.analyst != 2656147 && temp.analyst != 2911249 && temp.analyst != 2911432 select temp).GroupBy(b => b.user).OrderByDescending(b => b.Count());
        if (all.Any())
        {
            int c = 0;

            Response.Write("#,");
            Response.Write("name,");
            Response.Write("member for d,");
            Response.Write("hits,");
            Response.Write("positions,");
            Response.Write("positions active,");
            Response.Write("1st pos m,");
            Response.Write("2nd pos m,");
            Response.Write("3rd pos m,");
            Response.Write("4th pos m,");
            Response.Write("5th pos m,");
            Response.Write("6w pageviews,");
            Response.Write("5w pageviews,");
            Response.Write("4w pageviews,");
            Response.Write("3w pageviews,");
            Response.Write("2w pageviews,");
            Response.Write("1w pageviews,");
            Response.Write("<br>");

            foreach (var a in all)
            {
                Response.Write(c + 1);
                Response.Write(",");

                try
                {
                    Response.Write(a.First().user.display_name.Length > 0 ? a.First().user.display_name : a.First().user.employer);
                }
                catch
                {
                    Response.Write(a.First().user.employer);
                }

                Response.Write(",");
                Response.Write( Math.Round((DateTime.Now - a.OrderBy(b => b.timestamp).First().timestamp).TotalDays,0));
                Response.Write(",");
                Response.Write(a.Count());
                Response.Write(",");
                var am = from temp in db.ActionMonitors where temp.usermon == a.First().analyst select temp;
                Response.Write(am.Count());
                Response.Write(",");
                Response.Write(am.Where(b => b.active).Count());
                Response.Write(",");
                if (am.Any())
                {
                    Response.Write(Math.Round((am.OrderBy(b=>b.investment_date).First().investment_date - a.OrderBy(b => b.timestamp).First().timestamp).TotalMinutes, 0));

                    if (am.Count() > 1)
                    {
                        Response.Write(",");
                        Response.Write(Math.Round((am.OrderBy(b => b.investment_date).Skip(1).First().investment_date - a.OrderBy(b => b.timestamp).First().timestamp).TotalMinutes, 0));
                    }

                    if (am.Count() > 2)
                    {
                        Response.Write(",");
                        Response.Write(Math.Round((am.OrderBy(b => b.investment_date).Skip(2).First().investment_date - a.OrderBy(b => b.timestamp).First().timestamp).TotalMinutes, 0));
                    }

                    if (am.Count() > 3)
                    {
                        Response.Write(",");
                        Response.Write(Math.Round((am.OrderBy(b => b.investment_date).Skip(3).First().investment_date - a.OrderBy(b => b.timestamp).First().timestamp).TotalMinutes, 0));
                    }

                    if (am.Count() > 5)
                    {
                        Response.Write(",");
                        Response.Write(Math.Round((am.OrderBy(b => b.investment_date).Skip(4).First().investment_date - a.OrderBy(b => b.timestamp).First().timestamp).TotalMinutes, 0));
                    }

                    Response.Write(",");
                }

                // visits in the past 6 weeks
                for (int i = 5; i >= 0; i--)
                {
                    var week_n = a.Where(b => (DateTime.Now - b.timestamp).TotalDays > i * 7 && (DateTime.Now - b.timestamp).TotalDays <= (i+1) * 7);
                    Response.Write(week_n.Count());
                    Response.Write(",");
                }
                //Response.Write("<br>");
                // days active in the past 6 weeks
                for (int i = 5; i >= 0; i--)
                {
                    var week_n = a.Where(b => (DateTime.Now - b.timestamp).TotalDays > i * 7 && (DateTime.Now - b.timestamp).TotalDays <= (i + 1) * 7);
                    Response.Write(week_n.GroupBy(b=>b.timestamp.Date).Count());
                    Response.Write(",");
                }

                //// companies, analyst, investor, invest, challenge page trend in the past 6 weeks (page visits per week)
                //string[] pages = new string[7] {"Home","Companies","Company","Analyst","Challenge","Invest.","Investor" };
                //foreach (string p in pages)
                //{
                //    for (int i = 6; i >= 0; i--)
                //    {
                //        var week_n = a.Where(b => (DateTime.Now - b.timestamp).TotalDays > i * 7 && (DateTime.Now - b.timestamp).TotalDays <= (i + 1) * 7);
                //        Response.Write(week_n.Where(b=>b.hyperlink.Contains(p)));
                //        Response.Write(",");
                //    }
                //}
                    
                //1st session information
                //int cc = 0;
                //DateTime previous = new DateTime();

                //foreach (var hit in a.OrderBy(b => b.timestamp))
                //{


                //    if (cc == 0)
                //    {
                //        previous = hit.timestamp;
                //    }

                //    if ((hit.timestamp - previous).TotalHours > 1)
                //    {
                //        break;
                //    }

                //    previous = hit.timestamp;
                //    cc++;
                //}
                //var first_session = a.OrderBy(b => b.timestamp).Take(cc);
                //foreach (string p in pages)
                //{
                //    Response.Write(first_session.Where(b => b.hyperlink.Contains(p)).Count());
                //    Response.Write(",");
                //}
                //Response.Write(Math.Round((first_session.Skip(first_session.Count() - 1).First().timestamp - first_session.First().timestamp).TotalMinutes, 0));
                //Response.Write(",");

                //Response.Write(a.Where(b => b.hyperlink.Contains("noteworthy")).Count());
                //Response.Write(",");

                //var position_adjustments = from temp in db.Position_changes where temp.ActionMonitor.usermon == a.First().analyst select temp;
                //Response.Write(position_adjustments.Count());

                //foreach (string p in pages)
                //{
                //    Response.Write(a.Where(b => b.hyperlink.Contains(p)).Count());
                //    Response.Write(",");
                //}
                Response.Write("<br>");



                    //Response.Write("<br>");
                    //Response.Write(horizon((DateTime.Now - a.OrderBy(b => b.timestamp).First().timestamp).TotalDays));
                    //Response.Write("<br>");
                    //Response.Write("hits: " + a.Count());
                    //Response.Write("<br>");
                    //var am = from temp in db.ActionMonitors where temp.usermon == a.First().analyst select temp;
                    //Response.Write("Positions: " + am.Count() + ", active: " + am.Where(b => b.active).Count() + ", %: " + Math.Round(100 * (double) am.Where(b => b.active).Count() / (double) am.Count(), 0) + "%");
                    //Response.Write("<br>");
                    //var last_week = a.Where(b=>(DateTime.Now - b.timestamp).TotalDays<=7);
                    //var week_2 = a.Where(b => (DateTime.Now - b.timestamp).TotalDays > 7 && (DateTime.Now - b.timestamp).TotalDays <=14);
                    //var week_3 = a.Where(b => (DateTime.Now - b.timestamp).TotalDays > 14 && (DateTime.Now - b.timestamp).TotalDays <= 21);
                    //var week_4 = a.Where(b => (DateTime.Now - b.timestamp).TotalDays > 21 && (DateTime.Now - b.timestamp).TotalDays <= 28);

                    //Response.Write( week_4.Count() + ", " + week_3.Count() + ", " + week_2.Count() + ", " + last_week.Count() );
                    //Response.Write("<br>");

                    //Response.Write( "Last week visits: " + last_week.Count() + ", days: " + last_week.GroupBy(b=>b.timestamp.Date).Count() );
                    //Response.Write("<br>");
                    //Response.Write("Company: " + a.Where(b => b.hyperlink.Contains("Company")).Count() + ", analyst: " + a.Where(b => b.hyperlink.Contains("Analyst")).Count() + ", investor: " + a.Where(b => b.hyperlink.Contains("Investor")).Count());
                    //Response.Write("<br>");
                    //var first_time = a.GroupBy(b => b.timestamp.Date).OrderBy(b => b.First().timestamp.Date).First();
                    //Response.Write( "1st time companies: " + first_time.Where( b=>b.hyperlink.Contains("Company")).Count() + ", " );
                    //Response.Write( "analysts: " + first_time.Where(b => b.hyperlink.Contains("Analyst")).Count() + ", ");
                    //Response.Write( "invest: " + first_time.Where(b => b.hyperlink.Contains("Invest")).Count() + "<br>");

                    //foreach (var f in first_time)
                    //{
                    //    Response.Write( "<span style=\"color:gray;font-size:x-small\">" +  f.timestamp + ", " + f.hyperlink + "</span><br>");
                    //}

                    //Response.Write("<br>");

                    if (c > 200)
                        break;

                c++;
            }
        }
    }
}