using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

public partial class admin_track : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        int user=0;

        try
        {
            if (!string.IsNullOrEmpty(Request.QueryString["user"]))
                user = Convert.ToInt32(Request.QueryString["user"]);
        }
        catch
        {
        }

        if (!string.IsNullOrEmpty(Request.QueryString["records"]) && !string.IsNullOrEmpty(Request.QueryString["skipped"]))
        {
            try
            {
                track_table(Convert.ToInt32(Request.QueryString["records"]), Convert.ToInt32(Request.QueryString["skipped"]),user);
            }
            catch
            {
                track_table(20, 0,user);
            }
        }
        else
        {
            track_table(20, 0,user);
        }

        //if (!string.IsNullOrEmpty(Request.QueryString["user"])) { 
        //    LinqDataSource2.Where = "!hyperlink.Contains(\"localhost\") && analyst!=2 && analyst!=1 && analyst==" + Request.QueryString["user"];
        //}
    }

    protected void track_table(int records, int skipped,int user)
    {   
        List<string> spider_bot_blacklist = new List<string>();
        spider_bot_blacklist.Add("Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)");
        spider_bot_blacklist.Add("New-Sogou-Spider/1.0 (compatible; MSIE 5.5; Windows 98)");
        spider_bot_blacklist.Add("Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)");

        DataClassesDataContext db = new DataClassesDataContext();
        var tracks = from temp in db.trackings where !temp.hyperlink.Contains("localhost") && !Constants.excluded_users.Contains(temp.analyst) && ( temp.user.description == null || (!temp.user.description.Contains("bot") && !temp.user.description.Contains("ysearch") && !temp.user.description.Contains("spider") && !temp.user.description.Contains("PycURL"))) select temp;
        //var tracks = from temp in db.trackings where !temp.hyperlink.Contains("localhost") && !Constants.excluded_users.Contains(temp.analyst) select temp;
        //var tracks = from temp in db.trackings where !temp.hyperlink.Contains("localhost") select temp;

        if (user>0)
        {
            tracks = tracks.Where(b => b.analyst == user);
            grab_user_data(user);
        }
        tracks = tracks.OrderByDescending(b=>b.timestamp).Skip(records * skipped).Take(records);

        if (tracks.Any())
        {
            foreach (var t in tracks)
            {
                System.Web.UI.HtmlControls.HtmlTableRow row = new System.Web.UI.HtmlControls.HtmlTableRow();
                System.Web.UI.HtmlControls.HtmlTableCell c1 = new System.Web.UI.HtmlControls.HtmlTableCell();
                System.Web.UI.HtmlControls.HtmlTableCell c2 = new System.Web.UI.HtmlControls.HtmlTableCell();
                System.Web.UI.HtmlControls.HtmlTableCell c3 = new System.Web.UI.HtmlControls.HtmlTableCell();
                //System.Web.UI.HtmlControls.HtmlTableCell c4 = new System.Web.UI.HtmlControls.HtmlTableCell();

                string name = "";
                if (string.IsNullOrEmpty(t.user.display_name) && !string.IsNullOrEmpty(t.user.employer)){
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load("http://freegeoip.net/xml/" + t.user.employer);
                        XmlNodeList n2 = doc.GetElementsByTagName("City");
                        XmlNodeList n1 = doc.GetElementsByTagName("CountryName");
                        name = "[" + n1[0].InnerText + ", " + n2[0].InnerText + "]";
                    }
                    catch { }
                }
                else
                {
                    name = t.user.display_name;
                }

                c1.InnerHtml = "<a href=\"?user=" + t.user.userID + "\">" + name.Replace("United States","US") + "</a>";
                c2.InnerHtml = "<a href=\"" + t.hyperlink + "\">" + Ancillary.string_cutter(t.hyperlink.Replace("http://invesd.com", "").Replace("https://invesd.com", "").Replace("http://www.invesd.com", "").Replace("https://www.invesd.com", "").Trim(), 100, false, "top") + "</a>";
                c3.InnerText = string.Format("{0:MMM d,yy hh:mm tt}", t.timestamp);
                //c4.InnerHtml = "<a class=\"urls\" href=\"Gripe.aspx?delete=" + g.id + "\"><i class=\"icon-trash\"></i></a>";

                row.Cells.Add(c1);
                row.Cells.Add(c2);
                row.Cells.Add(c3);
                //row.Cells.Add(c4);

                tbl.Rows.Add(row);

            }
        }

        if (skipped == 0)
        {
            hyp_previous.Enabled = false;
        }
        else
        {
            hyp_previous.Enabled = true;
            hyp_previous.NavigateUrl = "Track.aspx?records=" + records.ToString() + "&skipped=" + (skipped - 1).ToString();
        }

        hyp_next.NavigateUrl = "Track.aspx?records=" + records.ToString() + "&skipped=" + (skipped + 1).ToString();

    }

    protected void grab_user_data(int userid)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var user = from temp in db.users where temp.userID == userid select temp;

        if (user.Any())
        {
            txt_user_details.Text = user.First().description + "<br>" + user.First().employer + "<br>" + user.First().sex; 
        }
    }
}