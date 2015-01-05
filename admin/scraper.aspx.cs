using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HtmlAgilityPack;
using System.Xml;


public partial class scraper : System.Web.UI.Page
{
    public int goodX;
    public int mediumX;
    public int badX;

    protected void toadd(object sender, EventArgs e)
    {
        Response.Redirect("nowadd.aspx");
    }

    protected void gotofilter(object sender, EventArgs e)
    {
        Response.Redirect("nowfilter.aspx");
    }

    protected void b1click(object sender, EventArgs e)
    {
        try
        {
            if (Convert.ToInt32(startpage.Text) <= Convert.ToInt32(endpage.Text))
            {
                scrapebaby(Convert.ToInt32(startpage.Text), Convert.ToInt32(endpage.Text),null);
            }
        }
        catch
        {
            b1.Text = "Invalid values - try again";
        }

    }

    protected void quickie(object sender, EventArgs e)
    {
        scrapebaby(1, 2,null);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        //additnow("XBTC.OB");

        if (Request.QueryString["type"] == "all")
        {
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

            foreach (var section in list_of_sections)
            {
                scrapebaby(1, 2, section);
            }
        }
    }

    protected DateTime breakitapart(string x)
    {
        string z;

        int abc = x.Length;

        if (x.Length > 52)
        {
            z = x.Remove(0, 5).Replace(" ET", "");
        }
        else
        {
            z = x.Remove(0, 5).Replace(" ET", "").Insert(7, " 2013");
        }

        DateTime a = Convert.ToDateTime(z);
        a.AddHours(-3);

        return a;
    }

    public void scrapebaby(int startpage, int endpage, string section)
    {
        HtmlWeb hw = new HtmlWeb();

        //db.tempactions.DeleteAllOnSubmit(db.tempactions);
        // loop pages
        for (int i = startpage; i <= endpage; i++)
        {
            if (string.IsNullOrEmpty(section))
                section = urlx.SelectedValue;
            HtmlDocument ho = hw.Load("http://seekingalpha.com/articles?filters=" + section + "&page=" + i.ToString());
            HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//div");

            // parse out all divs
            foreach (HtmlNode z in m)
            {
                if (z.Attributes.Count >= 1)
                {
                    // parse out all content
                    if (z.Attributes.First().Value == "content")
                    {

                        string sstring = "http://seekingalpha.com";
                        if (z.ChildNodes.ElementAtOrDefault(1).InnerText.Length == 0)
                        {
                            sstring += z.ChildNodes.ElementAtOrDefault(2).Attributes.First().Value;
                        }
                        else
                        {
                            sstring += z.ChildNodes.ElementAtOrDefault(1).Attributes.First().Value;
                        }

                        DataClassesDataContext db = new DataClassesDataContext();
                        {
                            var articlejoo = from temp in db.articles where temp.url == sstring select temp;

                            if (articlejoo.Any() == false)
                            {
                                //var tempjoo = from temp in db.tempactions where temp.url == sstring select temp;

                                //if (tempjoo.Count() == 0)
                                //{Write("<p>");
                                Response.Write("<p>");
                                ghalbe_dastan(z, i, db);
                                Response.Write("</p>");
                                //}
                            }
                            else
                            {
                                // this causes the scraper to stop scraping anytime it reaches
                                // a previously scraped article
                                i = 2000;
                                break;
                            }
                        }


                    }
                }

            }
        }

        Response.Write("<b>" + section + "</b><br>");
        Response.Write("Type 1: " + goodX + " out of " + Convert.ToString(goodX + badX + mediumX) + " - " + Convert.ToString(((double)goodX / ((double)goodX + (double)badX + (double)mediumX)) * 100) + "%<br>");
        Response.Write("Type 2: " + mediumX + " out of " + Convert.ToString(goodX + badX + mediumX) + " - " + Convert.ToString(((double)mediumX / ((double)goodX + (double)badX + (double)mediumX)) * 100) + "%<br>");
        Response.Write("Type 3: " + badX + " out of " + Convert.ToString(goodX + badX + mediumX) + " - " + Convert.ToString(((double)badX / ((double)goodX + (double)badX + (double)mediumX)) * 100) + "%<br><br>");
    }

    protected void ghalbe_dastan(HtmlNode z, int i,DataClassesDataContext db)
    {
            article sa = new article();
            int editor = 0;

            // EDITOR's Pick
            if (z.ChildNodes.ElementAtOrDefault(1).InnerText.Length == 0)
            {
                editor = 1;
                Response.Write("<strong><font color=red>Editor's Pick: </font></strong>");

                if ((z.ChildNodes.ElementAtOrDefault(2).InnerText.IndexOf("$") != -1 || z.ChildNodes.ElementAtOrDefault(2).InnerText.IndexOf("%") != -1))
                {
                    Response.Write("<strong><font color=green size=3>" + z.ChildNodes.ElementAtOrDefault(2).InnerText + "</font></strong><br>");
                    goodX++;

                    sa.type = 1;
                    sa.title = z.ChildNodes.ElementAtOrDefault(2).InnerText;
                }
                else if (z.ChildNodes.ElementAtOrDefault(2).InnerText.IndexOf("$") == -1 && z.ChildNodes.ElementAtOrDefault(2).InnerText.IndexOf("%") == -1 && z.ChildNodes.ElementAtOrDefault(2).InnerText.IndexOf("Buy") != -1 || z.ChildNodes.ElementAtOrDefault(2).InnerText.IndexOf("Invest") != -1 || z.ChildNodes.ElementAtOrDefault(2).InnerText.IndexOf("buy") != -1)
                {
                    Response.Write("<strong><font color=blue size=3>" + z.ChildNodes.ElementAtOrDefault(2).InnerText + "</font></strong><br>");
                    mediumX++;

                    sa.type = 2;
                    sa.title = z.ChildNodes.ElementAtOrDefault(2).InnerText;
                }
                else
                {
                    Response.Write("<strong>" + z.ChildNodes.ElementAtOrDefault(2).InnerText + "</strong><br>");
                    badX++;

                    sa.type = 3;
                    sa.title = z.ChildNodes.ElementAtOrDefault(2).InnerText;
                }

                Response.Write(z.ChildNodes.ElementAtOrDefault(4).ChildNodes.ElementAtOrDefault(1).InnerText + "<br>");

                int author_id=check_author(db, Convert.ToString(z.ChildNodes.ElementAtOrDefault(4).ChildNodes.ElementAtOrDefault(1).InnerText).Trim());
                if (author_id != -100)
                    sa.origin = author_id;
                //else
                //    sa.author = Convert.ToString(z.ChildNodes.ElementAtOrDefault(4).ChildNodes.ElementAtOrDefault(1).InnerText).Trim();
            }
            // NOT edit's pick
            else
            {

                if ((z.ChildNodes.ElementAtOrDefault(1).InnerText.IndexOf("$") != -1 || z.ChildNodes.ElementAtOrDefault(1).InnerText.IndexOf("%") != -1) && z.ChildNodes.ElementAtOrDefault(1).InnerText.IndexOf("?") == -1)
                {
                    Response.Write("<strong><font color=green size=3>" + z.ChildNodes.ElementAtOrDefault(1).InnerText + "</font></strong><br>");
                    goodX++;

                    sa.type = 1;
                    sa.title = z.ChildNodes.ElementAtOrDefault(1).InnerText;
                }
                else if (z.ChildNodes.ElementAtOrDefault(1).InnerText.IndexOf("$") == -1 && z.ChildNodes.ElementAtOrDefault(1).InnerText.IndexOf("%") == -1 && z.ChildNodes.ElementAtOrDefault(1).InnerText.IndexOf("Buy") != -1 || z.ChildNodes.ElementAtOrDefault(1).InnerText.IndexOf("Invest") != -1 || z.ChildNodes.ElementAtOrDefault(1).InnerText.IndexOf("buy") != -1)
                {
                    Response.Write("<strong><font color=blue size=3>" + z.ChildNodes.ElementAtOrDefault(1).InnerText + "</font></strong><br>");
                    mediumX++;

                    sa.type = 2;
                    sa.title = z.ChildNodes.ElementAtOrDefault(1).InnerText;
                }
                else
                {
                    Response.Write("<strong>" + z.ChildNodes.ElementAtOrDefault(1).InnerText + "</strong><br>");
                    badX++;

                    sa.type = 3;
                    sa.title = z.ChildNodes.ElementAtOrDefault(1).InnerText;
                }

                Response.Write(z.ChildNodes.ElementAtOrDefault(3).ChildNodes.ElementAtOrDefault(1).InnerText + "<br>");

                int author_id = check_author(db,z.ChildNodes.ElementAtOrDefault(3).ChildNodes.ElementAtOrDefault(1).InnerText.ToString().Trim());
                if (author_id != -100)
                    sa.origin = author_id;
                //else
                //    sa.author = z.ChildNodes.ElementAtOrDefault(3).ChildNodes.ElementAtOrDefault(1).InnerText.ToString();
            }

            bool date = false;
            string ticker = null;

            if (z.ChildNodes.ElementAtOrDefault(3 + editor).ChildNodes.ElementAtOrDefault(5).Attributes.First().Value == "text/javascript")
            {
                //sa.tickers = "";
                Response.Write("<font color=red>Was not added to the database</font><br>");
            }
            else
            {
                // this can be later REMOVED
                //sa.tickers = z.ChildNodes.ElementAtOrDefault(3 + editor).ChildNodes.ElementAtOrDefault(5).InnerText;
                ticker = z.ChildNodes.ElementAtOrDefault(3 + editor).ChildNodes.ElementAtOrDefault(5).InnerText;
                Response.Write(z.ChildNodes.ElementAtOrDefault(3 + editor).ChildNodes.ElementAtOrDefault(8).InnerText + "<font color=gray size=2>" + i.ToString() + "</font><br>");
                Response.Write("http://seekingalpha.com" + z.ChildNodes.ElementAtOrDefault(1+editor).Attributes.First().Value + "<br>");
                sa.date = breakitapart(z.ChildNodes.ElementAtOrDefault(3 + editor).ChildNodes.ElementAtOrDefault(8).InnerText);
                date = true;
                sa.url = "http://seekingalpha.com" + z.ChildNodes.ElementAtOrDefault(1+editor).Attributes.First().Value;
            }

            if (!string.IsNullOrEmpty(ticker) && !string.IsNullOrEmpty(sa.url) && date)
            {
                sa.text = "";
                sa.provider = 36;
                sa.action = false;
                sa.deleted = false;
                sa.is_ticket = false;
                sa.actions = 0;
                sa.premium = false;
                sa.not_actionable = false;
                sa.Publish = true;
                sa.price = 0;
                sa.backdated = false;

                try
                {
                    db.articles.InsertOnSubmit(sa);
                    db.SubmitChanges();

                    try
                    {
                        string[] s = z.ChildNodes.ElementAtOrDefault(3 + editor).ChildNodes.ElementAtOrDefault(5).InnerText.Split(',');
                        int fundid = 0;
                        //int count = 1;

                        foreach (string k in s)
                        {
                            Response.Write(k + "<br>");
                            fundid = find_or_insert_ticker(db, k.Trim());

                            if (fundid != 0)
                            {
                                ArticleTicker at = new ArticleTicker();
                                at.article = sa.idarticle;
                                at.ticker = fundid;
                                db.ArticleTickers.InsertOnSubmit(at);
                                db.SubmitChanges();
                            }
                        }
                    }
                    catch
                    {
                        Response.Write("<font color=red>Error in inserting tickers</font>");
                    }
                }
                catch
                {
                    Response.Write("<font color=red>Error in inserting article into the database</font><br>");
                }
            }
    }

    protected int check_author(DataClassesDataContext db,string author_name)
    {
        //var author = from temp in db.users where temp.firstname.Trim() == author_name || (temp.firstname.Trim() + " " + temp.lastname.Trim()) == author_name select temp;
        var author = from temp in db.users where temp.seekingalpha_name.Trim() == author_name.Trim() select temp;

        if (author.Any())
        {
            return author.First().userID;
        }
        else
        {
            user new_author = new user();

            new_author.seekingalpha_name = author_name.Trim();
            new_author.display_name = author_name.Trim();
            new_author.progress_description = false;
            new_author.progress_photo = false;
            new_author.progress_photo = false;

            try
            {
                db.users.InsertOnSubmit(new_author);
                db.SubmitChanges();
                return new_author.userID;
            }
            catch
            {
                return -100;
            }
        }
    }

    protected int find_or_insert_ticker(DataClassesDataContext db, string ticker)
    {
        var tickers = from temp in db.funds where temp.ticker == ticker select temp;

        if (tickers.Any())
        {
            // return ticker ID
            return tickers.First().fundID;
        }
        else
        {
            return DataBaseLayer.additnow(ticker);
        }
    }

}