using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Net.Mail;

public partial class sendEmail : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void update_metrics(object sender, EventArgs e)
    {
        l_performance.Text = "";
        l_alpha.Text = "";
        l_beta.Text = "";
        l_rank.Text = "";
        l_actions.Text = "";
        l_articles.Text = "";
        l_ratio.Text = "";
        l_sector.Text = "";
        l_sector_rank.Text = "";
        l_sector_performance.Text = "";
        l_stock.Text = "";
        l_stock_rank.Text = "";
        l_stock_performance.Text = "";

        DataClassesDataContext db = new DataClassesDataContext();
        {


            var analyst = from temp in db.AnalystPerformances where temp.analyst == Convert.ToInt32(SelectedUser.SelectedValue) && temp.horizon == 0 && temp.sector == null && temp.ticker == null select temp;
            if (analyst.Any())
            {
                l_performance.Text = string.Format("{0:N2}",100*analyst.First().return_average) + "%";
                l_alpha.Text = string.Format("{0:N2}", 100 * analyst.First().alpha_average) + "%";
                //l_beta.Text = string.Format("{0:N2}", 100 * analyst.First().) + "%";
                l_rank.Text = analyst.First().rank.ToString();
                l_analyst.Text = SelectedUser.SelectedItem.Text.Trim().ToString();
                l_analyst_name.Text = l_analyst.Text;
                l_actual_analyst_return.Text = Math.Round(100 * analyst.First().return_average, 2) + "%";
                l_actual_return.Text = l_actual_analyst_return.Text;
                l_actual_alpha.Text = Math.Round(100*analyst.First().alpha_average,2) + "%";
                l_actual_analyst_actions.Text = analyst.First().actions.ToString();

                double active_actions = 0;
                double articles_active = 0;
                var a1 = (from temp in db.Actions where temp.article1.origin == Convert.ToInt32(SelectedUser.SelectedValue) select temp).Where(b => b.is_ticket == false);
                if (a1.Any())
                    active_actions=a1.Count();
                var a2 = (from temp in db.articles where temp.origin == Convert.ToInt32(SelectedUser.SelectedValue) select temp).Where(b => b.is_ticket == false && b.deleted == false);
                if (a2.Any())
                    articles_active = a2.Count();
                //double action_assignment = active_actions / articles_active;

                l_actual_articles.Text = articles_active.ToString();
                l_actual_actions.Text = active_actions.ToString();

                var sector = from temp in db.AnalystPerformances where temp.sector != null && temp.horizon == 0 && temp.analyst == Convert.ToInt32(SelectedUser.SelectedValue) select temp;

                if (sector.Any())
                {
                    var top_sector_found = sector.OrderBy(b => b.rank).ThenByDescending(b=>b.return_average).First();
                    l_actual_sector.Text = top_sector_found.Sector1.sector1.Trim();
                    l_actual_sector_rank.Text = top_sector_found.rank.ToString();
                    l_actual_sector_return.Text = Math.Round(100 * top_sector_found.return_average, 2) + "%";
                    l_sector_rank.Text = l_actual_sector_rank.Text;
                    l_sector.Text = l_actual_sector.Text;
                }

                var ticker = from temp in db.AnalystPerformances where temp.ticker != null && temp.horizon == 0 && temp.analyst == Convert.ToInt32(SelectedUser.SelectedValue) select temp;

                if (ticker.Any())
                {
                    var top_ticker_found = ticker.OrderBy(b => b.rank).ThenByDescending(b=>b.return_average).First();
                    l_actual_ticker.Text = top_ticker_found.fund.name.Trim();
                    l_actual_ticker_rank.Text = top_ticker_found.rank.ToString();
                    l_actual_ticker_return.Text = Math.Round(100 * top_ticker_found.return_average,2) + "%";
                    l_stock.Text = l_actual_ticker.Text;
                    l_stock_rank.Text = l_actual_ticker_rank.Text;
                }

                var first = from temp in db.AnalystPerformances where temp.sector == null && temp.ticker == null && temp.horizon == 0 select temp;

                if (first.Any())
                {
                    var first_found = first.OrderBy(b => b.rank).First();
                    var last_found = first.OrderByDescending(b => b.rank).First();

                    l_actual_first_actions.Text = first_found.actions.ToString();
                    //l_actual_first_return.Text = Math.Round(100 * first_found.return_average,2) + "%";

                    l_actual_last.Text = last_found.rank.ToString();
                    l_actual_last_actions.Text = last_found.actions.ToString();
                    //l_actual_last_return.Text = Math.Round(100 * last_found.return_average,2) + "%";
                }
            }
        }

        change_subject_main();
    }

    protected void LinqDataSource1_Selecting(object sender, LinqDataSourceSelectEventArgs e)
    {
        
    }

    protected void change_subject(object sender, EventArgs e)
    {
        change_subject_main();
    }

    protected void change_subject_main()
    {
        if (dd_type.SelectedIndex == 1)
        {
            if (l_sector_rank.Text !="")
            {
                txt_subject.Text = SelectedUser.SelectedItem.Text.Trim().ToString();
                txt_subject.Text += ", You Rank ";
                try
                {
                    if (Convert.ToInt32(l_sector_rank.Text) == 1)
                    {
                        txt_subject.Text += "1st";
                    }
                    else if (Convert.ToInt32(l_sector_rank.Text) == 2)
                    {
                        txt_subject.Text += "2nd";
                    }
                    else if (Convert.ToInt32(l_sector_rank.Text) == 3)
                    {
                        txt_subject.Text += "3rd";
                    }
                    else
                    {
                        txt_subject.Text += l_sector_rank.Text + "th";
                    }
                }
                catch { }

                txt_subject.Text += " in " + l_sector.Text;
            }
        }
        else if (dd_type.SelectedIndex == 2)
        {
            if (l_stock.Text != "")
            {
                txt_subject.Text = SelectedUser.SelectedItem.Text.Trim().ToString();
                txt_subject.Text += ", You Rank ";
                try
                {
                    if (Convert.ToInt32(l_stock_rank.Text) == 1)
                    {
                        txt_subject.Text += "1st";
                    }
                    else if (Convert.ToInt32(l_stock_rank.Text) == 2)
                    {
                        txt_subject.Text += "2nd";
                    }
                    else if (Convert.ToInt32(l_stock_rank.Text) == 3)
                    {
                        txt_subject.Text += "3rd";
                    }
                    else
                    {
                        txt_subject.Text += l_stock_rank.Text + "th";
                    }
                }
                catch { }

                txt_subject.Text += " in " + l_stock.Text;
            }
        }
        else
        {
            txt_subject.Text = "";
        }
       
    }

    protected void Send_Click(object sender, EventArgs e)
    {
        try
        {
            DataClassesDataContext db = new DataClassesDataContext();
            if (SelectedUser.SelectedValue != null)
            {
                bool testing = false;
                string Rand = RandomString(20);
                var CUser = from temp in db.users where temp.userID == Convert.ToInt32(SelectedUser.SelectedValue) select temp;
                if (CUser.Any())
                {
                    var AUser = from temp in db.users where temp.email.ToLower() == TheEmail.Text.ToLower() && temp.userID != Convert.ToInt32(SelectedUser.SelectedValue) select temp;
                    if (AUser.Any())
                    {
                        status.Text = "Another account with this email exits.";
                    }
                    else
                    {
                        CUser.First<user>().randomVariable = Rand;
                        if (!testing)
                        {
                            CUser.First<user>().email = TheEmail.Text;
                            CUser.First<user>().roles = "normal;analyst";
                        }
                        db.SubmitChanges();

                        var AnalystPerf = from temp in db.AnalystPerformances where temp.analyst == CUser.First().userID && temp.horizon == 0 && temp.sector == null && temp.ticker == null select temp;

                        MailMessage newMsg = new MailMessage();
                        newMsg.From = new MailAddress("analyst@invesd.com", "Invesd");
                        newMsg.IsBodyHtml = true;
                        newMsg.To.Add(new MailAddress(TheEmail.Text));
                        newMsg.Bcc.Add(new MailAddress("launch@invesd.com","Invesd Launch"));
                        newMsg.Subject = txt_subject.Text;
                        double active_actions = (from temp in db.Actions where temp.article1.origin == CUser.First<user>().userID select temp).Where(b => b.matured == false && b.expired == false && b.breached == false && b.active == true).GroupBy(b => b.article).Count();
                        double articles_active = (from temp in db.articles where temp.origin == CUser.First<user>().userID select temp).Where(b => b.not_actionable == false && b.deleted == false && b.is_ticket == false).Count();
                        double action_assignment = active_actions / articles_active;

                        var sector = from temp in db.AnalystPerformances where temp.sector != null && temp.horizon == 0 && temp.analyst == CUser.First().userID select temp;
                        string top_sector = null;
                        int rank_sector = 0;
                        int return_sector = 0;

                        if (sector.Any())
                        {
                            var top_sector_found = sector.OrderBy(b => b.rank).ThenByDescending(b=>b.return_average).First();
                            top_sector = top_sector_found.Sector1.sector1;
                            rank_sector = top_sector_found.rank;
                            return_sector = (int)Math.Round(100 * top_sector_found.return_average);
                        }

                        var ticker = from temp in db.AnalystPerformances where temp.ticker != null && temp.horizon == 0 && temp.analyst == CUser.First().userID select temp;
                        string top_ticker = null;
                        int rank_ticker = 0;
                        int return_ticker = 0;

                        if (ticker.Any())
                        {
                            var top_ticker_found = ticker.OrderBy(b => b.rank).ThenByDescending(b => b.return_average).First();
                            top_ticker = top_ticker_found.fund.name.Trim();
                            rank_ticker = top_ticker_found.rank;
                            return_ticker = (int)Math.Round(100 * top_ticker_found.return_average);
                        }

                        var first = from temp in db.AnalystPerformances where temp.sector == null && temp.ticker == null && temp.horizon == 0 select temp;
                        double rank_1_return = 0;
                        int rank_1_actions = 0;
                        int last_rank = 0;
                        double last_return = 0;
                        int last_actions = 0;

                        if (first.Any())
                        {
                            var first_found = first.OrderBy(b => b.rank).First();
                            var last_found = first.OrderByDescending(b => b.rank).First();

                            rank_1_actions = first_found.actions;
                            rank_1_return = Math.Round(100 * first_found.return_average, 2);

                            last_rank = last_found.rank;
                            last_actions = last_found.actions;
                            last_return = Math.Round(100 * last_found.return_average, 2);
                        }

                        // added last
                        double avg_return = 0;
                        double avg_alpha = 0;
                        int actions = 0;

                        if (AnalystPerf.Any()) {
                            avg_return = Math.Round(100 * AnalystPerf.First().return_average, 2);
                            avg_alpha = Math.Round(100 * AnalystPerf.First().alpha_average, 2);
                            actions = AnalystPerf.First().actions;
                        }
                        

                        //newMsg.Body = "Hi " + CUser.First<user>().firstname.Trim() + " " + CUser.First<user>().lastname.Trim() + ",\n\n Please confirm your account by clicking on the following link:\n\nhttp://www.invesd.com/launch/Activation.aspx?uid=" + CUser.First<user>().userID + "&rid=" + CUser.First<user>().randomVariable + "\n\nThanks,\nInvesd.com";
                        //newMsg.Body = "<html><head><title>Invesd</title></head><body><img src=\"http://www.invesd.com/images/logo/aapl.png\"/></body></html>";
                        //newMsg.Body = "<html><head><title>Invesd</title></head><body><table style=\"width:900px\"><tr><td style=\"width:450px\"><b>Rank</b></td><td style=\"width:450px\"><b>Name</b></td></tr><tr><td>1</td><td>" + CUser.First().firstname.Trim() + "</td></tr></table></body></html>";
                        newMsg.Body = @"<html>
                <table style=""margin-right:auto;margin-left:auto;"">
                    <tr>
                        <td style=""width:550px;"">
                            <img src=""http://invesd.com/images/invesd_logo.png"" width=""130"" height=""32"" style=""width:130px;height:32px"" alt=""Invesd"">
                        </td>
                    </tr>
                    <tr>
                        <td style=""width:550px;font-size:15px;font-weight:100;font-family:helvetica;background-color:#1b1b1b;color:#cccccc;letter-spacing:1px"">
                            &nbsp;Don't just invest!
                        </td>
                    </tr>
                    <tr>
                        <td style=""width:550px"">
                            <p style=""font-family:helvetica;color:#000000;font-size:10pt;margin:10px 0 0 0"">Hello " + CUser.First<user>().display_name + @",</p>
                            <p style=""font-family:helvetica;color:#000000;font-size:10pt;margin:10px 0 0 0"">Congratulations! You have been selected as one of the leading analysts to open a pre-launch Invesd account.</p>
                            <p style=""font-family:helvetica;color:#000000;font-size:10pt;margin:10px 0 0 0"">Invesd tracks, quantifies, and ranks the performance of analytical and forward-looking investment content provided by individual analysts and research firms.</p>
                                <div style=""width:550px"">
                                <p style=""text-align:center;margin:10px 0 10px 0"">
                                    <a href=""https://invesd.com/analyst.aspx?analyst=" + CUser.First().userID + @"&rid=" + Rand + @""" style=""background: #62c462;
padding-top: 6px;
padding-right: 10px;
padding-bottom: 6px;
padding-left: 10px;
-webkit-border-radius: 4px;
-moz-border-radius: 4px;
border-radius: 4px;
color: #fff;
font-size:14px;
font-weight: 100;
text-decoration: none;
font-family: Helvetica, Arial, sans-serif;"">See your detailed performance</a>
                                </p>
                            </div>
                            <table width=""550"" cellpadding=""1"" cellspacing=""1"" style=""max-width: 100%;
  background-color: transparent;
  border-collapse: collapse;
  border-spacing: 0;
"">
                                <tr>
                                    <td style=""text-align:center;width:250px;font-family:helvetica;font-weight:bold;font-size:10pt""><strong>Activity</strong></td>
                                    <td width=""50"">&nbsp;</td>
                                    <td style=""text-align:center;width:250px;font-family:helvetica;font-weight:bold;font-size:10pt""><strong>Average Return</strong></td>
                                </tr>
                                <tr>
                                    <td style=""text-align:center;width:250px;border:1px solid Silver;height:100px;font-size:x-large;font-family:helvetica"">
                                        <table border=""0"" width=""100%"">
                                            <tr>
                                                <td style=""width:50%;font-family:helvetica;font-size:12pt"">
                                                    " + active_actions + @"
                                                </td>
                                                <td style=""width:50%;font-family:helvetica;font-size:12pt"">
                                                    " + articles_active + @"
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""width:50%;font-family:helvetica;font-size:10pt"">
                                                    Actions
                                                </td>
                                                <td style=""width:50%;font-family:helvetica;font-size:10pt"">
                                                    Articles
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                    <td width=""50"">&nbsp;</td>
                                    <td style=""text-align:center;width:250px;border:1px solid Silver;height:100px;font-size:12pt;font-family:helvetica"">
                                        <table border=""0"" width=""100%"">
                                            <tr>
                                                <td style=""width:50%;font-family:helvetica;font-size:12pt"">
                                                    " + avg_return + @"%</td>
                                                <td style=""width:50%;font-family:helvetica;font-size:12pt"">
                                                    " + avg_alpha + @"%</td>
                                            </tr>
                                            <tr>
                                                <td style=""width:50%;font-family:helvetica;font-size:10pt"">
                                                    Total return
                                                </td>
                                                <td style=""width:50%;font-family:helvetica;font-size:10pt"">
                                                    Alpha
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr><td>&nbsp;</td><td></td><td></td></tr>
                                <tr>
                                    <td style=""text-align:center;width:250px;font-family:helvetica;font-weight:bold;font-size:10pt;"">Top Sector</td>
                                    <td width=""50"">&nbsp;</td>
                                    <td style=""text-align:center;width:250px;font-family:helvetica;font-weight:bold;font-size:10pt;"">Top Stock</td>
                                </tr>
                                <tr>
                                    <td style=""text-align:center;width:250px;border:1px solid Silver;height:100px;font-size:12pt;font-family:helvetica"">
                                        <table border=""0"" width=""100%"">
                                            <tr>
                                                <td width=""100%"" style=""text-align:center;width:100%;font-size:12pt;font-weight:bold;"" colspan=""2"">" + top_sector + @"</td>
                                            </tr>
                                            <tr>
                                                <td style=""width:50%;font-family:helvetica;font-size:12pt"">" + rank_sector + @"</td>
                                                <td style=""width:50%;font-family:helvetica;font-size:12pt"">" + return_sector + @"%</td>
                                            </tr>
                                            <tr>
                                                <td style=""width:50%;font-family:helvetica;font-size:10pt"">Rank</td>
                                                <td style=""width:50%;font-family:helvetica;font-size:10pt"">Average Return</td>
                                            </tr>
                                        </table>
                                    </td>
                                    <td width=""50"">&nbsp;</td>
                                    <td style=""text-align:center;width:250px;border:1px solid Silver;height:100px;font-size:12pt;font-family:helvetica"">
                                        <table border=""0"" width=""100%"">
                                            <tr>
                                                <td width=""100%"" style=""text-align:center;width:100%;font-size:12pt;font-weight:bold;"" colspan=""2"">" + top_ticker + @"</td>
                                            </tr>
                                            <tr>
                                                <td style=""width:50%;font-size:12pt"">" + rank_ticker + @"</td>
                                                <td style=""width:50%;font-size:12pt"">" + return_ticker + @"%</td>
                                            </tr>
                                            <tr>
                                                <td style=""width:50%;font-size:10pt"">Rank</td>
                                                <td style=""width:50%;font-size:10pt"">Average Return</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                            <span>&nbsp;</span>
                            <table style=""width:550px;border:1px solid Silver;border-collapse:collapse;border-spacing:0;"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td style=""width:55px;text-align:center;font-weight:bold;font-family:helvetica;font-size:10pt;border-bottom:1px solid Silver"">Rank</td>
                                    <td style=""width:165px;text-align:center;font-weight:bold;font-family:helvetica;font-size:10pt;border-bottom:1px solid Silver"">Analyst</td>
                                    <td style=""width:165px;text-align:center;font-weight:bold;font-family:helvetica;font-size:10pt;border-bottom:1px solid Silver"">Average Return</td>
                                    <td style=""width:165px;text-align:center;font-weight:bold;font-family:helvetica;font-size:10pt;border-bottom:1px solid Silver"">Actions</td>
                                </tr>
                                <tr>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">1</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">?</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">?</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">" + rank_1_actions + @"</td>
                                </tr>
                                <tr>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">...</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">...</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">...</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">...</td>
                                </tr>
                                <tr>
                                    <td style=""text-align:center;font-weight:bold;font-family:helvetica;font-size:10pt;background-color:#62c462;color:#ffffff"">?</td>
                                    <td style=""text-align:center;font-weight:bold;font-family:helvetica;font-size:10pt;background-color:#62c462;color:#ffffff"">" + CUser.First().display_name + @"</td>
                                    <td style=""text-align:center;font-weight:bold;font-family:helvetica;font-size:10pt;background-color:#62c462;color:#ffffff"">" + avg_return + @"%</td>
                                    <td style=""text-align:center;font-weight:bold;font-family:helvetica;font-size:10pt;background-color:#62c462;color:#ffffff"">" + actions + @"</td>
                                </tr>
                                <tr>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">...</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">...</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">...</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">...</td>
                                </tr>
                                <tr>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">" + last_rank + @"</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">?</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">?</td>
                                    <td style=""text-align:center;font-weight:normal;font-family:helvetica;font-size:10pt"">" + last_actions + @"</td>
                                </tr>
                            </table>
                            <br>
                            <p style=""font-family:helvetica;margin:10px 0 10px 0"">
                                Your performance is based on your publicly available articles. This is an opportunity to load more free and premium content into your account and increase your rankings prior to our public launch. <a style=""color:#0088cc;font-family:helvetica"" href=""https://invesd.com/analyst.aspx?analyst=" + CUser.First().userID + @"&rid=" + Rand + @""">Click here for more information.</a>
                            </p>
                            <p style=""font-family:helvetica"">
                            Thank you,<br />
                            Invesd
                            </p>
                            <p style=""font-size:8pt;color:Gray"">
                                This email was sent to you because you publish investment articles. Cick <a href=""https://invesd.com/analyst.aspx?analyst=" + CUser.First().userID + @"&rid=" + Rand + @"&unsubscribe=true"" style=""font-family:helvetica;color:Gray"">here</a> if you are not interested in receiving such emails in the future.
                            </p>
                        </td>
                    </tr>
                </table>
</html>
    ";


                        using (SmtpClient smtp = new SmtpClient())
                        {
                            smtp.UseDefaultCredentials = true;
                            smtp.Send(newMsg);
                        }
                        status.Text = "Invitation was successfully sent to " + TheEmail.Text;
                    }
                }
            }
        }
        catch (Exception e4)
        {
            status.Text = e4.Message + e4.StackTrace;
        }
    }

    private string RandomString(int size)
    {
        StringBuilder builder = new StringBuilder();
        Random random = new Random();
        char ch;
        for (int i = 0; i < size; i++)
        {
            ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
            builder.Append(ch);
        }

        return builder.ToString();
    }

}