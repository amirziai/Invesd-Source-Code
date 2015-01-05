using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;

public partial class investor_Invest : System.Web.UI.Page
{
    public bool logged_in = true;
    public int default_shares = 100;
    public double profit = 0;
    public double portfolio_value = 0;
    public double loss = 0;
    public int ticker_id = 0;
    public double current_value = 0;
    public double average;
    public double target_undo = 0;
    public double consensus_target = 0;
    public double deployment_percentage = 0;
    public double deployment_den = 0;
    public double cash = 0;
    public double diversification = 0;
    public int actionid = 0;
    public double actionid_target = 0;
    public int copyid = 0;
    public int selected_analyst_action = 0;
    public int selected_investor_action = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        // user must be logged in to use this page

        int userid = DataBaseLayer.GetCheckUser();
        DataBaseLayer.track(userid, HttpContext.Current.Request.Url.AbsoluteUri);
        if (userid == 0)
            Response.Redirect(Page.ResolveUrl("~") + "Login.aspx");

        DataClassesDataContext db = new DataClassesDataContext();
        var user = from temp in db.users where temp.userID == userid && temp.verified.Value select temp;
        if (user.Any())
        {
            login.Visible = false;
        }
        else
        {
            loggedin.Visible = false;
        }


        if (!string.IsNullOrEmpty(Request.QueryString["ticker"]))
        {

            // Portfolio data
            portfolio_header portfolio = new portfolio_header();
            portfolio = DataBaseLayer.portfolio_stats();
            portfolio_value = portfolio.invested + portfolio.profit;
            profit = portfolio.upside;
            loss = portfolio.downside;
            //l_before_target.Text = string.Format("{0:n1}", portfolio_value>0?100 * profit / portfolio_value:0) + "%";
            //l_after_target.Text = l_before_target.Text;
            //l_before_loss.Text = string.Format("{0:n1}", portfolio_value>0?100 * loss / portfolio_value:0) + "%";
            //l_after_loss.Text = l_before_loss.Text;
            cash = portfolio.cash;
            diversification = portfolio.diversification;

            string ticker = Request.QueryString["ticker"].Trim();
            var current = from temp in db.fund_values where temp.fund.ticker == ticker && temp.isLatest.Value select temp;
            var exchange = (from temp in db.funds where temp.ticker == ticker select temp).First().exchange;
            double CP;
            var currentPrice = AdminBackend.GetLatestValue(ticker, exchange);
            

            if (current.Any())
            {
                if (currentPrice == null)
                {
                    CP = current.First().closeValue.Value;
                }
                else
                {
                    CP = currentPrice.Value;
                }
                ticker_id = current.First().fundID;
                current_value = CP;//current.First().closeValue.Value;
                l_current.InnerText = string.Format("{0:c2}", current_value);
                l_change.Text = string.Format("$0.00");

                var metrics = from temp in db.Aggregate_Tickers where temp.ticker == ticker_id select temp;

                if (metrics.Any())
                {
                    consensus_target = metrics.First().consensus>0?metrics.First().consensus:metrics.First().consensus_flat;
                    consensus.InnerHtml = "<span style=\"font-weight:bold;color:" + (consensus_target > current_value ? "#62c462" : consensus_target < current_value ? "#ee5f5b" : "gray") + "\">" + string.Format("{0:c2}", consensus_target) + "</span>";
                    //circle_conensus.Attributes.Add("style", "margin:auto;border-radius:50%;width:70px;height:70px;border:5px solid " + (consensus_target > current_value ? "#62c462" : consensus_target < current_value ? "#ee5f5b" : "gray"));
                    //average_buysell.InnerHtml = consensus_target > current_value ? "<i class=\"icon-circle\" style=\"color:#62c462;position:absolute;top:5px;right:5px\"></i>" : consensus_target < current_value ? "<i class=\"icon-circle\" style=\"color:#ee5f5b;position:absolute;top:5px;right:5px\"></i>" : "";
                }
            }
            else // error if there's no current price for ticker
            { 
                Response.Redirect(Ancillary.log_and_redirect_to_error("Invest.aspx", "No last price for ticker", userid));
            }

            // analyst action data
            if (!string.IsNullOrEmpty(Request.QueryString["action"]))
            {
                
                try
                {
                    actionid = Convert.ToInt32(Request.QueryString["action"]);
                    var action = from temp in db.Actions where temp.fund.ticker == ticker && temp.actionID == actionid select temp;

                    if (action.Any())
                    {
                        selected_analyst_action = action.First().actionID;
                        double target = action.First().targetValue;
                        double target_days = (action.First().targetDate - DateTime.Now).TotalDays/30;

                        txt_target.Text = string.Format("{0:c2}", target);
                        l_selected.InnerText = txt_target.Text;
                        target_undo = target;

                        l_term.Text = (target_days >= 18 ? "LONG" : target_days >= 6 ? "MEDIUM" : "SHORT") + " TERM";
                        l_target_percentage.Text = string.Format("{0:n1}", Math.Abs(100 * (target / current_value - 1))) + "%";
                        txt_stoploss.Text = string.Format("{0:c2}", action.First().lowerValue);
                        l_stoploss_percentage.Text = "100%";

                        txt_target.Text = string.Format("{0:c2}", target);
                        txt_target_percentage(current_value, target, null);
                        
                        // analyst info
                        analyst_name.Text = Ancillary.string_cutter(action.First().article1.user.display_name, 15, false, "top");
                        analyst_broker.Text = Ancillary.string_cutter(action.First().article1.user.Bloomberg_Broker1.name, 15, false, "top");
                        analyst_img.Src = DataBaseLayer.get_user_pic(action.First().article1.origin,true);
                        var ap = from temp in db.AnalystPerformances where temp.ticker == ticker_id && temp.analyst == action.First().article1.origin select temp;
                        if (ap.Any()) {
                            if (ap.First().confidence.HasValue)
                                analyst_confidence.InnerHtml = "<img src=\"" + Page.ResolveUrl("~") + "images/signal" + (ap.First().confidence >= .8 ? "4" : ap.First().confidence >= .6 ? "3" : ap.First().confidence >= .4 ? "2" : ap.First().confidence >= .2 ? "1" : ap.First().confidence >= 0 ? "0" : "0") + ".png\" alt=\"Confidence\" title=\"Confidence\" >";
                        }

                        average_variation.InnerText = target != consensus_target ? Math.Round((100 * (target / consensus_target - 1)), 1) + "%" : "";
                        l_above_below.InnerText = target > consensus_target ? "% ABOVE" : target < consensus_target ? "% BELOW" : "";

                        tr_selected_details.Style.Remove("display");
                        tr_info.Style.Add("display", "none");
                        tr_consensus_set.Style.Remove("display");
                        tr_consensus_variation.Style.Remove("display");
                    }
                    else {
                        text_target();
                    }
                }
                catch {
                    text_target();
                }
            }
            // invest actionmonitor data (copy)
            else if (!string.IsNullOrEmpty(Request.QueryString["copy"]))
            {
                try
                {
                    copyid = Convert.ToInt32(Request.QueryString["copy"]);
                    var copy = from temp in db.ActionMonitors where temp.fund.ticker == ticker && temp.ID == copyid select temp;

                    if (copy.Any())
                    {
                        selected_investor_action = copyid;
                        double target = copy.First().targetValue;
                        double target_days = (copy.First().monitorEnd - DateTime.Now).TotalDays / 30;

                        //txt_target.Text = string.Format("{0:c2}", target);
                        target_undo = target;

                        l_term.Text = (target_days >= 18 ? "LONG" : target_days >= 6 ? "MEDIUM" : "SHORT") + " TERM";
                        l_target_percentage.Text = string.Format("{0:n1}", Math.Abs(100 * (target / current_value - 1))) + "%";
                        l_stoploss_percentage.Text = string.Format("{0:n1}", Math.Abs(100 * (copy.First().lowerValue / current_value - 1))) + "%";
                        txt_target.Text = string.Format("{0:c2}", target);
                        l_selected.InnerText = txt_target.Text;
                        txt_target_percentage(current_value, target, copy.First().lowerValue);

                        // analyst info
                        analyst_name.Text = Ancillary.string_cutter(copy.First().user.display_name, 15, false, "top");
                        analyst_broker.Text = "Investor";//Ancillary.string_cutter(copy.First().user.Bloomberg_Broker1.name, 15, false, "top");
                        analyst_img.Src = DataBaseLayer.get_user_pic(copy.First().usermon, true);

                        average_variation.InnerText = target != consensus_target ? Math.Round((100 * (target / consensus_target - 1)), 1) + "%" : "";
                        l_above_below.InnerText = target > consensus_target ? "% ABOVE" : target < consensus_target ? "% BELOW" : "";

                        tr_selected_details.Style.Remove("display");
                        tr_info.Style.Add("display", "none");
                        tr_consensus_set.Style.Remove("display");
                        tr_consensus_variation.Style.Remove("display");
                    }
                    else
                    {
                        text_target();
                    }
                }
                catch
                {
                    text_target();
                }
            }
                
            // consensus data
            else {
                text_target();
            }

        }
        else {
            DataBaseLayer.gripe(userid, "Invest.aspx - Tried to access without a querystring");
            Response.Redirect("Companies.aspx");
        }
    }
    protected void text_target()
    {
        if (consensus_target > current_value)
        {
            txt_target.Text = string.Format("{0:c2}", consensus_target);
            l_selected.InnerText = txt_target.Text;
            target_undo = consensus_target;
            txt_target_percentage(current_value, consensus_target,null);
            //text_longshort(consensus_target, current_value);
        }
        else
        {
            div_alert.Attributes.Remove("class");
            div_alert.Attributes.Add("class", "alert alert-danger");
            hint.InnerText = "Consensus is lower than the current price";
            l_term.Text = "Medium";
            //header.InnerHtml = "<div class=\"alert alert-danger\" style=\"margin-bottom:10px\"><table border=\"0\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\"><tr><td width=\"100%\">Consensus is lower than current price (" + string.Format("{0:c2}",current_value) + ")</td></tr></table>";
        }

        average_variation.InnerHtml = "&nbsp;";
    }

    protected void txt_target_percentage(double current,double target,double? stoploss) {
        string value = string.Format("{0:n1}", Math.Abs(100 * (target / current - 1)));

        l_term.Text = "Medium";
        l_target_percentage.Text = value + "%";

        if (stoploss == null)
        {
            if (target >= current)
                txt_stoploss.Text = "$0.00";
            else
                txt_stoploss.Text = string.Format("{0:c2}", current * 2);
        }
        else
        {
            txt_stoploss.Text = string.Format("{0:c2}", stoploss);
        }
    }

    [WebMethod]
    public static string invest(string s_shares, string s_target, string date, string s_stoploss,string ticker,string rationale, double current_value_in,int actionid,double consensus_target,int selected_investor_action)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        ActionMonitor a = new ActionMonitor();
        int user = DataBaseLayer.GetCheckUser();

        if (user > 0)
        {
            try
            {
                int shares = Convert.ToInt32(s_shares);
                double target = Convert.ToDouble(s_target);
                double stoploss = Convert.ToDouble(s_stoploss);
                
                var theFund = (from temp in db.funds where temp.ticker == ticker select temp).First();
                int ticker_id = theFund.fundID;
                string tickerExchange = theFund.exchange;
               // var fund_values = (from temp in db.fund_values where temp.fundID == ticker_id && temp.isLatest.Value select temp).First();
                var currentPrice = AdminBackend.GetLatestValue(ticker, tickerExchange);
                var currentDate = AdminBackend.GetLatestValueDate(ticker, tickerExchange);
                double CP;
                if (currentPrice != null)
                {
                    CP = currentPrice.Value;

                    if (CP >= current_value_in * .995 && CP <= current_value_in * 1.005)
                    {
                        var user_cash = from temp in db.users where temp.userID == user select temp;

                        if (user_cash.Any())
                        {
                            if (user_cash.First().cash >= shares * current_value_in)
                            {
                                if (shares > 0)
                                {
                                    a.LongPosition = true;
                                    a.usermon = user;
                                    a.units = shares;
                                    a.targetValue = target;
                                    a.originalTargetValue = target;
                                    a.monitorEnd = DateTime.Now.AddMonths(Convert.ToInt32(date));
                                    a.lowerValue = stoploss;
                                    a.ticker = (from temp in db.funds where temp.ticker == ticker select temp).First().fundID;
                                    a.minValue = current_value_in;
                                    a.maxValue = a.minValue;
                                    a.active = true;
                                    a.breached = false;
                                    a.cashDividend = 0;
                                    a.currentValue = a.minValue;
                                    a.expired = false;
                                    a.lastUpdated = currentDate.Value;
                                    a.matured = false;
                                    a.monitorInitialValue = a.minValue;
                                    a.monitorStart = a.lastUpdated;
                                    a.investment_date = DateTime.Now;
                                    a.TotalReturn = false;
                                    a.rationale = rationale;
                                    if (consensus_target == a.targetValue)
                                        a.equals_consensus = true;

                                    if (actionid > 0)
                                    {
                                        var selected_action = from temp in db.Actions where temp.actionID == actionid select temp;
                                        if (selected_action.Any())
                                        {
                                            if (selected_action.First().actionID == a.targetValue)
                                                a.action = actionid;
                                        }
                                    }

                                    if (selected_investor_action > 0)
                                    {
                                        var selected_action_monitor = from temp in db.ActionMonitors where temp.active && temp.ID == selected_investor_action select temp;
                                        if (selected_action_monitor.Any())
                                        {
                                            if (selected_action_monitor.First().targetValue == a.targetValue)
                                                a.copy_actionmonitor = selected_investor_action;
                                        }
                                    }

                                    // 2/9/2014, amir, set S&P500 values
                                    double sp500 = Portfolio.get_benchmark_value(null);
                                    if (sp500>0)
                                    {
                                        a.benchmark_beg = sp500;
                                        a.benchmark_end = sp500;
                                    }

                                    db.ActionMonitors.InsertOnSubmit(a);
                                    db.SubmitChanges();

                                    user_cash.First().cash -= shares * current_value_in;
                                    db.SubmitChanges();

                                    DataBaseLayer.AddNotification("Bought", user, null, true, false, false, user_cash.First().display_name.Trim() + ";" + theFund.name.Trim() + ";" + theFund.ticker.Trim() + ";" + theFund.fundID + ";" + target + ";" + current_value_in);
                                    Ancillary.send_email("estimate@invesd.com", "Invesd Challenge", "arziai@gmail.com", "Amir", "New " + target + " " + ticker + " position by " + user_cash.First().display_name.Trim() + (!string.IsNullOrEmpty(rationale) ? " [rationale]" : ""), (!string.IsNullOrEmpty(rationale) ? (rationale + "<br>") : "") + "http://invesd.com/Company.aspx?ticker=" + ticker, true);
                                    DataBaseLayer.track(user, "Invested in " + ticker);
                                    string text = create_twitter_string(theFund.ticker.Trim(), user_cash.First().display_name, rationale, consensus_target, target);
                                    if (rationale != "")
                                    {
                                        if (a.user.verified.HasValue)
                                        {
                                            if (a.user.verified.Value)
                                            {
                                                social.sendStockTwitsStatus(text);
                                                social.sendTwitterStatus(text);
                                            }
                                        }
                                    }
                                    
                                    return "success";
                                }
                                else
                                {
                                    return "zero";
                                }
                            }
                            else
                            {
                                return "nocash";
                            }
                        }
                        else
                        {
                            return "error";
                        }
                    }
                    else
                    {
                        return "value_change";
                    }
                }
                else
                {
                    return "error";
                }
            }
            catch
            {
                return "error";
            }
        }
        else {
            return "error";
        }
    }

    public static string create_twitter_string(string symbol,string investor,string analysis,double mean,double pt)
    {
        // stocktwits && twitter
        string text = "";
        string name = Ancillary.cutter(investor, 20,false);
        string ticker = "$" + symbol;
        string url = "http://invesd.com/Company.aspx?ticker=" + symbol + "&ref=challenge";
        string target = "$" + Math.Round(pt, (pt >= 10 ? 0 : (pt >= 5 ? 1 : 2)));
        string consensus = "";
        string rationale = !string.IsNullOrEmpty(analysis) ? (@" """ + Ancillary.cutter(analysis, 30,true) + @"..."" ") : "";

        if (mean>0)
            consensus += ", $" + Math.Round(mean, ( mean>=10 ? 0 : ( mean>=5 ? 1 :2 ) )) + " PT consensus";

        text = name + " bought " + ticker + " with " + target + " PT " + rationale + consensus + " " + url + " #challenge";
        return text;
    }
}