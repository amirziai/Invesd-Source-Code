using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_Stats : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        x();
    }

    protected void x() {
        DataClassesDataContext db = new DataClassesDataContext();

        var actions = from temp in db.Actions where temp.active select temp;

        if (actions.Any()) {
            Response.Write("<table border=0 width=\"100%\"><tr><td width=\"20%\" style=\"vertical-align:top\">");

            var sec = actions.Where(b=>b.fund.sector.HasValue && b.fund.peer_group.HasValue).GroupBy(b => b.fund.sector).OrderByDescending(b => b.Count());

            if (sec.Any())
            {
                Response.Write("<b>Sectors</b><table border=1><tr><td>Sector</td><td>Count</td><td>Companies</td><td>Per company</td></tr>");
                foreach (var s in sec)
                {
                    Response.Write("<tr><td>" + s.Last().fund.Sector1.sector1 + "</td><td>" + s.Count() + "</td><td>" + s.GroupBy(b => b.ticker).Count() + "</td><td>" + s.Count() / s.GroupBy(b => b.ticker).Count() + "</td></tr>");
                }
                Response.Write("</table>");
            }

            Response.Write("</td><td width=\"40%\" style=\"vertical-align:top\">");

            var ind = actions.Where(b=>b.fund.sector.HasValue && b.fund.peer_group.HasValue).GroupBy(b => b.fund.peer_group).OrderByDescending(b => b.Count());

            int c = 1;
            if (ind.Any())
            {
                Response.Write("<b>Industries</b><table border=1><tr><td>#</td><td>Industry</td><td>Count</td><td>Companies</td><td>Per company</td></tr>");
                foreach (var i in ind)
                {
                    Response.Write("<tr><td>" + c + "</td><td>" + i.First().fund.Peer_Group1.name + "</td><td>" + i.Count() + "</td><td>" + i.GroupBy(b => b.ticker).Count() + "</td><td>" + i.Count() / i.GroupBy(b => b.ticker).Count() + "</td></tr>");
                    c++;
                }
                Response.Write("</table>");
            }

            Response.Write("</td><td width=\"40%\" style=\"vertical-align:top\">");
            Response.Write("<b>" + string.Format("{0:n0}", actions.GroupBy(b=>b.ticker).Count()) + " companies, " + string.Format("{0:n0}", actions.Count()) + " actions</b><br><br>");
            var co = actions.GroupBy(b=>b.ticker).Where(b => b.First().fund.name=="" || !b.First().fund.sector.HasValue || !b.First().fund.peer_group.HasValue).OrderByDescending(b=>b.Count());
            Response.Write("<b>Tickers</b><table border=1><tr><td>#</td><td>Ticker</td><td>Name</td><td>Sector</td><td>Industry</td><td>Actions</td></tr>");

            if (co.Any()) {
                int count = 1;
                foreach (var cc in co) {
                    Response.Write("<tr><td>" + count + "</td><td>" + cc.First().fund.ticker + "</td><td>" + cc.First().fund.name + "</td><td>" + (cc.First().fund.sector.HasValue ? cc.First().fund.Sector1.sector1 : "") + "</td><td>" + (cc.First().fund.peer_group.HasValue?cc.First().fund.Peer_Group1.name:"") + "</td><td>" + cc.Count() + "</td></tr>");
                    count++;
                }
            }

            Response.Write("</td></tr></table>");

        }
    }
}