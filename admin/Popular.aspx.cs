using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_Popular : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        now();
    }

    public void now()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var tracking = (from temp in db.trackings where temp.hyperlink.Contains("ticker=") && !temp.hyperlink.Contains("localhost") && !Constants.excluded_users.Contains(temp.analyst) && !temp.user.description.Contains("bot") && !temp.user.description.Contains("ysearch/slurp") && !temp.user.description.Contains("spider") && !temp.user.description.Contains("PycURL") && temp.timestamp.Date >= DateTime.Now.AddDays(-7).Date select temp).GroupBy(b=>b.timestamp.Date).OrderByDescending(b=>b.First().timestamp) ;
        if (tracking.Any())
        {
            foreach (var t in tracking)
            {
                var date = t.First();
                content.Text += "<b>" + date.timestamp.DayOfWeek + "- " + date.timestamp.Month + "/" + date.timestamp.DayOfWeek + "</b><br>";

                foreach (var a in t.GroupBy(b=>extract_ticker(b.hyperlink) ).OrderByDescending(b=>b.Count()).Take(10)  )
                    content.Text += extract_ticker( a.First().hyperlink) + " (" + a.Count() + ")<br>";

                content.Text += "<br><br>";
            }
        }

    }

    public string extract_ticker(string hyperlink)
    {
        int ticker_position = hyperlink.IndexOf("ticker=");
        int and_position = hyperlink.IndexOf("&");
        if (and_position > ticker_position)
            return hyperlink.Substring(ticker_position + 7, and_position - (ticker_position + 7));
        else
            return hyperlink.Substring(ticker_position + 7, hyperlink.Length - (ticker_position + 7) );
    }
}