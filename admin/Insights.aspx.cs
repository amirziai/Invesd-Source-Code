using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

public partial class admin_Insights : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        insights();
    }

    protected void insights()
    {
        DataClassesDataContext db = new DataClassesDataContext();

        try
        {
            var fund = from temp in db.funds where temp.ticker == Request.QueryString["ticker"] select temp;
            if (fund.Any())
            {
                var fund_first = fund.First();
                high_level.Text = "<b>" + fund_first.name + "</b><br>" + fund_first.Sector1.sector1 + "<br>" + fund_first.Peer_Group1.name + "<br><br>" + fund_first.description;

                List<string> sector = new List<string>();
                List<string> timeline = new List<string>();

                var agg = (from temp in db.Aggregate_Sectors_Histories where temp.sector == fund_first.sector orderby temp.date descending select temp).Take(30);
                if (agg.Any())
                {
                    foreach (var a in agg.OrderBy(b => b.date))
                    {
                        sector.Add((a.average_flat * 100).ToString() );
                        timeline.Add( a.date.Month + "/" + a.date.Day );
                    }

                    push_to_javascript("sector", sector, false);
                    push_to_javascript("timeline", timeline, true);
                }

                List<string> industry = new List<string>();
                var ind = (from temp in db.Aggregate_Industries_Histories where temp.industry == fund_first.peer_group orderby temp.date descending select temp).Take(30);
                if (ind.Any())
                {
                    foreach (var a in ind.OrderBy(b => b.date))
                        industry.Add((a.average_flat * 100).ToString());

                    push_to_javascript("industry", industry, false);
                }

                List<string> benchmark_industry = new List<string>();
                string benchmark_industry_string = Request.QueryString["industry"];
                if (!string.IsNullOrEmpty(benchmark_industry_string))
                {
                    var fund_x = (from temp in db.fund_values where temp.fund.ticker == benchmark_industry_string orderby temp.date descending select temp).Take(30 + 1);
                    if (fund_x.Any())
                    {
                        double prev = 0;
                        double sum = 0;
                        int c = 0;
                        foreach (var a in fund_x.OrderBy(b => b.date))
                        {
                            if (c > 0)
                            {
                                sum += (a.adjValue/prev-1) * 100;
                                benchmark_industry.Add( sum.ToString() );
                            }

                            prev = a.adjValue;
                            c++;
                        }

                        push_to_javascript("benchmark_industry", benchmark_industry, false);
                    }
                }

                //var comps = from temp in db.Aggregate_Tickers where temp.fund.ticker == 

            }
        }
        catch { }
    }

    public void push_to_javascript(string name, List<string> values,bool is_string)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");
        sb.Append("var " + name + " = new Array;");

        foreach (var v in values)
        {
            if (is_string)
                sb.Append(name + ".push('" + v + "');");
            else
                sb.Append(name + ".push(" + v + ");");
        }
            

        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), name, sb.ToString());
    }
}