using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_type1actions : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected string get_tickers(int id,DateTime date)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            var ats = from temp in db.ArticleTickers where temp.article == id select temp;
            string x="";
            string value = "";

            if (ats.Any()) {
                foreach (var at in ats) {
                    var v = from temp in db.fund_values where temp.fundID == at.ticker && temp.date == date select temp;
                    if (v.Any())
                        value = string.Format("{0:c2}",v.First().closeValue);

                    x += "<a href=\"type1add.aspx?article=" + id + "&ticker=" + at.ticker + "\"><span class=\"label label-success\" title=\"" + value + "\">" + at.fund.ticker.Trim() + "</span></a> ";
                }
            }

            x += "<a href=\"type1add.aspx?article=" + id + "\"><span class=\"label label-info\">...</span></a>";

            return x;

        }
    }
}