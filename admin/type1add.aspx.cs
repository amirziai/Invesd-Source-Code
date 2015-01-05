using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_type1add : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        DataClassesDataContext db = new DataClassesDataContext();
        {

            if (!string.IsNullOrEmpty(Request.QueryString["article"]))
            {


                if (!IsPostBack)
                {
                    //if (Request.UrlReferrer != null)
                    //{
                    //    if (Request.UrlReferrer.PathAndQuery != Request.RawUrl)
                    //    {
                    //        urlholder.Text = Request.UrlReferrer.OriginalString;
                    //    }
                    //    else
                    //    {
                    //        urlholder.Text = "type1actions.aspx";
                    //    }

                    //}
                    //else
                    //{
                    //    urlholder.Text = "type1actions.aspx";
                    //}

                    urlholder.Text = "type1actions.aspx";

                    //if (!string.IsNullOrEmpty(Request.QueryString["ticker"]))
                    //{
                    //    urlholder.Text = System.Web.HttpUtility.UrlDecode(Request.QueryString["ticker"]);
                    //}


                    Action a = new Action();

                    var articlejoo = from temp in db.articles where temp.idarticle == Convert.ToInt32(Request.QueryString["article"]) select temp;

                    if (articlejoo.Count() > 0)
                    {
                        a.article = Convert.ToInt32(Request.QueryString["article"]);
                    }

                    ltitle.Text = articlejoo.First().title;
                    startdate.Text = articlejoo.First().date.ToString("yyyy-MM-dd");
                    a.startDate = articlejoo.First().date;

                    var actionjoo = from temp in db.Actions where temp.article1.idarticle == Convert.ToInt32(Request.QueryString["article"]) select temp;
                    lactioncount.Text = Convert.ToString(actionjoo.Count());

                    frame1.Attributes.Add("src", articlejoo.First().url);

                    targetdate.Text = articlejoo.First().date.AddYears(1).ToString("yyyy-MM-dd");
                    a.targetDate = Convert.ToDateTime(targetdate.Text);

                    string[] t = ltitle.Text.Split(' ');

                    foreach (string kalame in t)
                    {
                        if (kalame.IndexOf("%") != -1)
                        {
                            targetpercent.Text = kalame.Replace("%", "").Replace(":", "").Replace("-", "").Replace("?", "").Replace(",", "").Replace("/", "");
                        }
                        else if (kalame.IndexOf("$") != -1)
                        {
                            try
                            {

                                a.targetValue = Convert.ToDouble(kalame.Replace("$", "").Replace(":", "").Replace("-", "").Replace("?", "").Replace(",", "").Replace("/", ""));
                                targetprice.Text = Convert.ToString(a.targetValue);
                            }
                            catch
                            {
                                targetprice.Text = "INVALID";
                            }
                        }
                    }






                    // only works for the default
                    if (!string.IsNullOrEmpty(Request.QueryString["ticker"]))
                    {

                        Dictionary<string, string> ddm = new Dictionary<string, string>();
                        string[] tickera = Request.QueryString["ticker"].Split('-');
                        List<string> tickerSymbol = new List<string>();

                        foreach (string tik in tickera)
                        {
                            if (tik != "")
                            {
                                var symboljoo = from temp in db.funds where temp.fundID == Convert.ToInt32(tik) select temp;
                                try
                                {
                                    ddm.Add(tik, symboljoo.First().ticker);
                                }
                                catch
                                { }

                            }

                        }

                        stock.DataSource = ddm;
                        stock.DataTextField = "Value";
                        stock.DataValueField = "Key";
                        stock.DataBind();


                        try
                        {
                            var pricejoo = from temp in db.fund_values where temp.fundID == Convert.ToInt32(tickera.First()) && temp.date <= Convert.ToDateTime(startdate.Text) orderby temp.date descending select temp;

                            if (pricejoo.Count() > 0)
                            {
                                startprice.Text = Convert.ToString(pricejoo.First().closeValue);
                                a.startValue = Convert.ToDouble(pricejoo.First().closeValue);
                            }

                            if (!string.IsNullOrEmpty(targetpercent.Text))
                            {
                                targetprice.Text = Convert.ToString(Math.Round(Convert.ToDouble(startprice.Text) * (1 + Convert.ToDouble(targetpercent.Text) * 0.01), 2));
                                a.targetValue = Convert.ToDouble(targetprice.Text);
                            }

                            if (a.targetValue.GetHashCode() != 0 && a.targetDate.GetHashCode() != 0)
                            {
                                b1.Enabled = true;
                            }
                        }
                        catch { }
                    }

                }
                else
                {

                }





            }




        }
    }

    protected void addstock(object sender, EventArgs e)
    {
        buttona.Enabled = false;
        Response.Redirect("addticker.aspx?ticker=" + mticker.Text + "&s=" + System.Web.HttpUtility.UrlEncode(urlholder.Text));
    }

    protected void mtickerchanged(object sender, EventArgs e)
    {
        if (mticker.Text.Length > 0)
        {
            stock.Enabled = false;
            begard(1);
        }
        else
        {
            stock.Enabled = true;
            begard(0);
        }

    }

    protected void targetchanged(object sender, EventArgs e)
    {
        try
        {
            Convert.ToDouble(targetprice.Text);
            b1.Enabled = true;

        }
        catch
        {
            b1.Enabled = false;
        }
    }

    protected void stockchanged(object sender, EventArgs e)
    {
        begard(0);
    }

    protected void percentchanged(object sender, EventArgs e)
    {
        try
        {
            targetprice.Text = Convert.ToString(Math.Round(Convert.ToDouble(startprice.Text) * (1 + 0.01 * Convert.ToDouble(targetpercent.Text)), 2));
            b1.Enabled = true;
        }
        catch
        {
            targetprice.Text = "INVALID";
            b1.Enabled = false;
        }
    }

    protected void begard(int s)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            int t = 0;

            if (s == 0)
            {
                t = Convert.ToInt32(stock.SelectedValue);
            }
            else
            {
                var tickerjoo = from temp in db.funds where temp.ticker == mticker.Text select temp;
                if (tickerjoo.Count() > 0)
                {
                    t = tickerjoo.First().fundID;
                }
                else
                {
                    t = -1;
                }

            }

            if (t != -1)
            {
                var pricejoo = from temp in db.fund_values where temp.fundID == t && temp.date <= Convert.ToDateTime(startdate.Text) orderby temp.date descending select temp;

                if (pricejoo.Count() > 0)
                {
                    startprice.Text = Convert.ToString(pricejoo.First().closeValue);
                    startdate.Text = pricejoo.First().date.ToString("yyyy-MM-dd");
                    b1.Enabled = true;

                    if (targetpercent.Text.Length > 0)
                    {
                        targetprice.Text = Convert.ToString(Math.Round(Convert.ToDouble(startprice.Text) * (1 + 0.01 * Convert.ToDouble(targetpercent.Text)), 2));
                    }
                }
                else
                {
                    startprice.Text = "No data";
                    b1.Enabled = false;
                }

            }
            else
            {
                startprice.Text = "No data";
                b1.Enabled = false;
            }




        }
    }

    protected void b1click(object sender, EventArgs e)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            // actual start date
            // total return
            // creator
            // lower value

            Action a = new Action();
            a.targetDate = Convert.ToDateTime(targetdate.Text);
            a.targetValue = Convert.ToDouble(targetprice.Text);
            a.startDate = Convert.ToDateTime(startdate.Text);
            a.startValue = Convert.ToDouble(startprice.Text);
            a.creator = 2;
            a.TotalReturn = true;
            a.article = Convert.ToInt32(Request.QueryString["article"]);
            a.lowerValue = 0;

            a.minValue = a.startValue;
            a.maxValue = a.startValue;
            a.currentValue = a.startValue;
            a.actualStartDate = a.startDate;
            a.lastUpdated = a.actualStartDate;
            a.date_feed = a.startDate;
            a.matured = false;
            a.expired = false;
            a.breached = false;
            a.dividend = 0;
            a.creationTime = DateTime.Now;
            a.active = true;

            if (mticker.Text.Length > 0)
            {
                var tickerjoo = from temp in db.funds where temp.ticker == mticker.Text select temp;

                if (tickerjoo.Any())
                {
                    a.ticker = tickerjoo.First().fundID;
                }
            }
            else
            {
                a.ticker = Convert.ToInt32(stock.SelectedItem.Value);
            }

            a.rational = a.article1.summary;
            a.analystID = a.article1.origin;
            a.creationTime = DateTime.Now;
            db.Actions.InsertOnSubmit(a);

            try
            {
                db.SubmitChanges();
                //var articlejoo = from temp in db.articles where temp.idarticle == Convert.ToInt32(Request.QueryString["article"]) select temp;
                a.article1.action = true;
                a.article1.actions++;

                var at = from temp in db.ArticleTickers where temp.article == a.article && temp.ticker == a.ticker select temp;

                at.First().actions++;
                //if (articlejoo.Any())
                //{
                //    //    var tempaction = from temp in db.tempactions where temp.url == articlejoo.First().url select temp;

                //    //    if (tempaction.Count() > 0)
                //    //    {
                //    //        tempaction.First().action = true;
                //    //        db.SubmitChanges();
                //    //        Response.Redirect(urlholder.Text);
                //    //    }
                //    articlejoo.First().action = true;
                db.SubmitChanges();
                Response.Redirect(urlholder.Text);

            }
            catch
            {
                b1.Text = "Failed";
                b1.Enabled = false;
            }

            //fund_action fa = new fund_action();
            //fa.action = a.actionID;




            //var pricejoo = from temp in db.fund_values where temp.fundID == fa.fund && temp.date <= Convert.ToDateTime(startdate.Text) orderby temp.date descending select temp;

            //        if (pricejoo.Count() > 0)
            //        {
            //            startprice.Text = Convert.ToString(pricejoo.First().closeValue);
            //            //a.startValue = Convert.ToDouble(pricejoo.First().closeValue);
            //        }

            //        if (!string.IsNullOrEmpty(targetpercent.Text))
            //        {
            //            targetprice.Text = Convert.ToString(Math.Round(Convert.ToDouble(startprice.Text) * (1 + Convert.ToDouble(targetpercent.Text) * 0.01), 2));
            //            //a.targetValue = Convert.ToDouble(targetprice.Text);
            //        }

            //        if (a.targetValue.GetHashCode() != 0 && a.targetDate.GetHashCode() != 0)
            //        {
            //            b1.Enabled = true;
            //        }

            //fa.weight = 1;

            //db.fund_actions.InsertOnSubmit(fa);





        }





    }

}