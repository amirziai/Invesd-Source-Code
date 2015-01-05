using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.Globalization;
using System.Diagnostics;

using TweetSharp;
using System.Configuration;

using System.Xml;

public partial class admin_test : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        description();
        //leaderboard_simulator();
        //AdminBackend.update_live_analyst_performance();
        //decompose_the_tweet();
        //pull_ratings_from_twitter();
        //get_analyst_rationales();
    }

    public List<Tuple<string, int>> description_list = new List<Tuple<string,int>>();
    protected void description()
    {
        if (!string.IsNullOrEmpty(Request.QueryString["id"]))
        {
            DataClassesDataContext db = new DataClassesDataContext();
            int userid = Convert.ToInt32(Request.QueryString["id"]);
            var user = from temp in db.users where temp.userID == userid select temp;
            if (user.Any())
            {
                string out_string = "";
                out_string += "<b>" + user.First().display_name + "</b><br>";
                var actions = from temp in db.Actions where temp.analystID == userid select temp; int actions_count = actions.Count();
                var positions = from temp in db.ActionMonitors where temp.usermon == userid select temp;
                var aps = from temp in db.AnalystPerformances where temp.analyst == userid select temp;

                if (!actions.Any() && !positions.Any() && !aps.Any())
                {
                    out_string += "<br>No estimates or positions yet";
                }
                else if (actions.Any() && !positions.Any()) // no positions
                {
                    out_string += description_action(actions.ToList(), aps.ToList(), actions_count);    
                }
                else if (!actions.Any() && positions.Any()) // no estimates
                {
                    out_string += description_position(positions.ToList());
                }
                else // both
                {
                    out_string += description_action(actions.ToList(), aps.ToList(), actions_count);
                    out_string += description_position(positions.ToList());
                }

                Response.Write(out_string);

                Response.Write("<BR><BR>");
                foreach (var a in description_list.OrderBy(b=>b.Item2))
                {
                    Response.Write(a.Item2 + "- " + a.Item1 + "<br>");
                }
            }
        }
    }

    protected string description_action(List<Action> actions, List<AnalystPerformance> aps, int actions_count)
    {
        string out_string = "<b>Estimate</b>";
        // focus, win ratio, covering, bullish/bearish
        var actions_active = actions.Where(b => b.active);
        if (actions_active.Any())
        {
            bool concentrated_sector = false;
            bool concentrated_industry = false;
            int actions_active_count = actions_active.Count();
            var actions_active_sector = actions_active.GroupBy(b => b.fund.sector).OrderByDescending(b => b.Count());
            if (actions_active_sector.First().Count() > actions_active_count / 2 && actions_active_sector.First().First().fund.sector.HasValue)
            {
                description_list.Add(new Tuple<string, int>("Estimates concentrated in: " + actions_active_sector.First().First().fund.Sector1.sector1, 1));
                out_string += "<br>" + "Concentrated in " + actions_active_sector.First().First().fund.Sector1.sector1;
                concentrated_sector = true;
            }

            var actions_active_industry = actions_active.GroupBy(b => b.fund.peer_group).OrderByDescending(b => b.Count());
            if (actions_active_industry.First().Count() > actions_active_count / 2 && actions_active_industry.First().First().fund.sector.HasValue)
            {
                description_list.Add(new Tuple<string, int>("Estimates concentrated in: " + actions_active_industry.First().First().fund.Peer_Group1.name + " industry", 2));
                out_string += "<br>" + "Concentrated in " + actions_active_industry.First().First().fund.Peer_Group1.name;
                concentrated_industry = true;
            }

            if (concentrated_industry)
            {
                var actions_concentrated_industry = actions_active.Where(b => b.fund.peer_group == actions_active_industry.First().First().fund.peer_group && !b.matured);
                if (actions_concentrated_industry.Any())
                {
                    // bullish or bearish
                    int actions_concentrated_industry_count = actions_concentrated_industry.Count();
                    int actions_concentrated_industry_bullish = actions_concentrated_industry.Where(b => !b.@short).Count();
                    if (actions_concentrated_industry_bullish >= actions_concentrated_industry_count * 0.8)
                    {
                        double x = Math.Round(100 * actions_concentrated_industry.Average(b => b.targetValue / b.currentValue - 1));
                        description_list.Add(new Tuple<string, int>("Bullish, " + x + "% upside", 1));
                        out_string += "<br>" + "Bullish, " + x + "% upside";
                    }
                    else if ((actions_concentrated_industry_count - actions_concentrated_industry_bullish) >= actions_concentrated_industry_count * 0.8)
                    {
                        double x = Math.Round(100 * actions_concentrated_industry.Average(b => b.targetValue / b.currentValue - 1));
                        description_list.Add(new Tuple<string, int>("Bearish, " + x + "% downside", 1));
                        out_string += "<br>" + "Bullish, " + x + "% downside";
                    }
                }
            }
            else if (concentrated_sector)
            {
                var actions_concentrated_sector = actions_active.Where(b => b.fund.sector == actions_active_sector.First().First().fund.sector && !b.matured);
                if (actions_concentrated_sector.Any())
                {
                    // bullish or bearish
                    int actions_concentrated_sector_count = actions_concentrated_sector.Count();
                    int actions_concentrated_sector_bullish = actions_concentrated_sector.Where(b => !b.@short).Count();
                    if (actions_concentrated_sector_bullish >= actions_concentrated_sector_count * 0.8)
                    {
                        double x = Math.Round(100 * actions_concentrated_sector.Average(b => b.targetValue / b.currentValue - 1));
                        description_list.Add(new Tuple<string, int>("Bullish, " + x + "% upside", 1));
                        out_string += "<br>" + "Bullish, " + x + "% upside";
                    }
                    else if ((actions_concentrated_sector_count - actions_concentrated_sector_bullish) >= actions_concentrated_sector_count * 0.8)
                    {
                        double x = Math.Round(100 * actions_concentrated_sector.Average(b => b.targetValue / b.currentValue - 1));
                        description_list.Add(new Tuple<string, int>("Bearish, " + x + "% upside", 1));
                        out_string += "<br>" + "Bearish, " + x + "% upside";
                    }
                }
            }

            

        }

        // win ratio, average return
        int win = actions.Where(b => !b.@short && (b.currentValue + b.dividend) >= b.startValue).Count() + actions.Where(b => b.@short && b.currentValue <= b.startValue).Count();
        double win_ratio = (double)win / (double)actions_count;
        //out_string += "<br>win: " + win + ", all: " + actions_count;
        if (win_ratio > 0.5)
        {
            double x = Math.Round(win_ratio * 100, 1);
            description_list.Add(new Tuple<string, int>(x + "% win ratio",1 ));
            out_string += "<br>" + x + "% win ratio";
        }

        // average return
        var actions_pos = actions.Where(b => !b.@short);
        var actions_neg = actions.Where(b => b.@short);

        double ret = actions_pos.Count() * (actions_pos.Any() ? actions_pos.Average(b => ((b.currentValue + b.dividend) / b.startValue - 1)) : 0) + actions_neg.Count() * (actions_neg.Any() ? actions_neg.Average(b => (1 - b.currentValue / b.startValue)) : 0);
        ret /= actions_count;
        if (ret >= 0.05)
        {
            double x = Math.Round(100 * ret, 1);
            description_list.Add(new Tuple<string, int>(x + "% average estimate return",1));
            out_string += "<br>" + x + "% return";
        }

        if (aps.Any())
        {
            var top_aps = aps.Where(b => b.confidence >= 0.9 && b.accuracy_average >= .8);
            if (top_aps.Any())
            {
                out_string += "<br>Great track record: ";
                int c = 2;
                var top_aps_sorted = top_aps.OrderByDescending(b => b.confidence).Take(3); 
                foreach (var top in top_aps_sorted )
                {
                    description_list.Add(new Tuple<string,int>("Top track record: " + top.fund.name + "(" + top.fund.ticker + ")",c));
                    out_string += top.fund.name + "(" + top.fund.ticker.Trim() + "), ";
                    c++;
                }
            }
        }

        return out_string;
    }

    protected string description_position(List<ActionMonitor> positions)
    {
        string out_string = "<br><b>Position</b>";

        var notable = positions.Where(b => b.LongPosition && ((b.currentValue + b.cashDividend) / b.monitorInitialValue - 1) >= .1);
        if (notable.Any())
        {
            out_string += "<br>Notable: ";
            int c = 2;
            var notable_sorted = notable.OrderByDescending(b => (b.currentValue + b.cashDividend) / b.monitorInitialValue).Take(3);
            foreach (var n in notable_sorted)
            {
                double x = Math.Round(100 * ((n.currentValue + n.cashDividend) / n.monitorInitialValue - 1), 1);
                description_list.Add(new Tuple<string, int>("Notable investment: " + n.fund.name + "(" + n.fund.ticker + ") with " + x + "% return" , c));
                out_string += n.fund.name + "(" + n.fund.ticker + ") " + x + "%" + ", ";
                c++;
            }
        }

        double profit_long = positions.Where(b => b.LongPosition).Any() ? positions.Where(b => b.LongPosition).Sum(b => b.units * (b.currentValue + b.cashDividend - b.monitorInitialValue)) : 0;
        double profit_short = positions.Where(b => !b.LongPosition).Any() ? positions.Where(b => !b.LongPosition).Sum(b => b.units * (b.monitorInitialValue - b.cashDividend - b.currentValue)) : 0;
        if (profit_long + profit_short > 0)
        {
            double o = Math.Round((profit_long + profit_short) / 1e3, 1);
            description_list.Add(new Tuple<string, int>("Portfolio return: " + o + "%", o>=5?1:2 ));
            out_string += "<br>Return: " + o + "%";
        }

        double sp500_pos = positions.Where(b => b.LongPosition && b.benchmark_end.HasValue && b.benchmark_beg.HasValue).Any() ? positions.Where(b => b.LongPosition && b.benchmark_end.HasValue && b.benchmark_beg.HasValue).Sum(b => ((b.currentValue + b.cashDividend) / b.monitorInitialValue - b.benchmark_end.Value / b.benchmark_beg.Value) * b.units * b.monitorInitialValue) : 0;
        double sp500_neg = positions.Where(b => !b.LongPosition && b.benchmark_end.HasValue && b.benchmark_beg.HasValue).Any() ? positions.Where(b => b.LongPosition && b.benchmark_end.HasValue && b.benchmark_beg.HasValue).Sum(b => (1 - (b.currentValue + b.cashDividend) / b.monitorInitialValue - (b.benchmark_end.Value / b.benchmark_beg.Value - 1)) * b.units * b.monitorInitialValue) : 0;

        if (sp500_pos + sp500_neg > 0)
        {
            out_string += "<br>Outperformance: " + Math.Round((sp500_neg + sp500_pos) / 1e3, 1) + "%";
        }

        var position_active = positions.Where(b => b.active);
        if (position_active.Any())
        {
            double position_active_sum = position_active.Sum(b => b.units * b.currentValue);
            var position_active_sector = position_active.Where(b => b.fund.sector.HasValue).GroupBy(b => b.fund.sector);
            foreach (var p in position_active_sector)
            {
                double p_sum = p.Sum(b => b.units * b.currentValue);
                if (p_sum > position_active_sum / 2)
                {
                    description_list.Add(new Tuple<string, int>("Portfolio concentrated in " + p.First().fund.Sector1.sector1,1));
                    out_string += "<br>Sector concentration: " + p.First().fund.Sector1.sector1;
                }
            }

            var position_active_industry = position_active.Where(b => b.fund.peer_group.HasValue).GroupBy(b => b.fund.peer_group);
            foreach (var p in position_active_industry)
            {
                double p_sum = p.Sum(b => b.units * b.currentValue);
                if (p_sum > position_active_sum / 2)
                {
                    description_list.Add(new Tuple<string, int>("Portfolio concentrated in " + p.First().fund.Peer_Group1.name, 1));
                    out_string += "<br>Industry concentration: " + p.First().fund.Peer_Group1.name;
                }
            }

        }

        return out_string;
    }

    protected void main_archive()
    {
        //std();

        //AdminBackend.weekly_report(true);
        //AdminBackend.daily_diget(true);

        //technicals();
        //fun_with_industries();
        //archive_previous_season("Q2 2014");
        //insert_notification();
        //AdminBackend.update_action_monitor(true);
        //AdminBackend.updateAllDB_Click(true);
        //AdminBackend.metric_ticker();

        //List<int> users = new List<int>() { 5001841, 5001658, 5001475, 5000560, 5000377, 5000194, 4999828, 4999645, 4999462, 4999279, 4999096, 4998913, 4998730 };
        //List<int> users = new List<int>() { 5001475 };
        //delete_user(users);
        //List<string> x = new List<string>() { "MZDAF", "ALPMF" };
        //DataClassesDataContext db = new DataClassesDataContext();
        //foreach (var a in x)
        //{
        //    delete_all_targets_for_stock_and_history((from temp in db.funds where temp.ticker == a select temp).First().fundID);
        //}
        //analyst_test();
        //std();
        //update_amir();
        //estimize_twitter();
        //give_me_similar_cos();
        //counter();
        //all_new();
        //delete_crap();
        //find_unverified(5, 6);

        //var watch = Stopwatch.StartNew();
        //Estimate.GetNextTicker_v2_guts(800, 0,1);
        //watch.Stop();
        //Response.Write("<font color=green>" + watch.ElapsedMilliseconds + "</font><br><br>");

        //watch = Stopwatch.StartNew();
        //Estimate.GetNextTicker_v2_guts(800, 0, 2);
        //watch.Stop();
        //Response.Write("<font color=green>" + watch.ElapsedMilliseconds + "</font><br><br>");
        //leaderboard_wall_st(234);
        //views_feed_v3(0, 785, "confidence");
        //xyz(0);
        //leaderboard_wallst();
        //industry();
        //DataClassesDataContext db = new DataClassesDataContext();
        //Comment1 c = new Comment1();
        //c = (from temp in db.Comment1s where temp.id == 9 select temp).First();
        //Response.Write( social.comment(c) );
        //delete_crap();
        //delete_funds_zombie(new List<string>() { });

        //List<string> cos = new List<string>() { "GRUB" };
        //delete_funds_zombie(cos);
        //new_feed();
        //total_actions();
        //avergae_no_of_covered_wall_St_stocks();
        //delete_crap();
        //int offset = 0;
        //int days = 30;

        //try
        //{
        //    offset = Convert.ToInt32( Request.QueryString["offset"]);
        //    days = Convert.ToInt32(Request.QueryString["days"]);
        //}
        //catch { }

        //list(days, offset);

    }

    protected void leaderboard_simulator()
    {
        DataClassesDataContext db = new DataClassesDataContext();

        bool invesd_only = false;
        if (!string.IsNullOrEmpty(Request.QueryString["invesd"]))
        {
            invesd_only = true;
        }


        var aps_initial = from temp in db.AnalystPerformances where temp.confidence.HasValue select temp;

        if (invesd_only)
            aps_initial = aps_initial.Where(b => !b.user.bloomberg_broker.HasValue);

        var aps = aps_initial.GroupBy(b => b.analyst).OrderByDescending(b => b.Sum(c => c.confidence));

        if (aps.Any())
        {
            int c = 1;
            foreach (var ap in aps)
            {
                Response.Write(c + "- " + ap.First().user.display_name + "\t" + Math.Round(ap.Sum(b => b.confidence.Value), 1) + "<br>");
                c++;
            }
        }

        //    var ams_amir_start = from temp in db.ActionMonitors where temp.usermon == 2 orderby temp.monitorStart select temp;
        //    if (ams_amir_start.Any())
        //    {
        //        DateTime start = ams_amir_start.First().monitorStart;
        //        List<ActionMonitor> list = ams_amir_start.ToList();

        //        double cash = 100000;
        //        double portfolio = 0;
        //        List<ActionMonitor> positions = new List<ActionMonitor>();

        //        for (DateTime i = start; i <= DateTime.Now; i = i.AddDays(1))
        //        {
        //            var ams_today = ams_amir_start.Where(b => b.monitorStart == i);
        //            if (ams_today.Any())
        //            {
        //                foreach (var ams_today_element in ams_today)
        //                {
        //                    cash -= ams_today_element.units * ams_today_element.monitorInitialValue;
        //                    positions.Add(ams_today_element);
        //                }
        //            }

        //            var ams_today_sold = ams_amir_start.Where(b => b.lastUpdated == i && !b.active);
        //            if (ams_today_sold.Any())
        //            {
        //                foreach (var ams_today_element in ams_today_sold)
        //                {
        //                    cash += ams_today_element.units * ams_today_element.currentValue;
        //                    positions.Remove(ams_today_element);
        //                }
        //            }

        //            foreach (var p in positions)
        //            {
        //                var value = from temp in db.fund_values where temp.fundID == p.ticker && temp.date <= i orderby temp.date descending select temp.adjValue;
        //                if (value.Any())
        //                {
        //                    //portfolio += p.units * 
        //                }
        //            }

        //        }
        //    }
    }

    protected void update_1pm_steps(int steps)
    {
        AdminBackend.update_actions_1pm_guts(steps);
    }

    protected void learn_about_linq()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var watch = Stopwatch.StartNew();
        var actions = (from temp in db.Actions where !temp.breached && !temp.expired select temp).ToList();
        watch.Stop();
        Response.Write(watch.ElapsedMilliseconds);
        Response.Write("<br><br>");

        if (actions.Any())
        {
            watch = Stopwatch.StartNew();
            var actions_grouped = actions.GroupBy(b => b.ticker).OrderByDescending(b => b.Count()).ToList();
            foreach (var a in actions_grouped)
            {
                Debug.WriteLine(a.First().targetValue);
            }

            watch.Stop();
            Response.Write(watch.ElapsedMilliseconds);
            //var watch = Stopwatch.StartNew();
            

            //var watch = Stopwatch.StartNew();
            //foreach (var a in actions.ToList())
            //{
            //    double t = a.targetValue;
            //    Debug.WriteLine(t);
            //}
            

            //watch = Stopwatch.StartNew();
            //foreach (var a in actions)
            //{
            //    double t = a.targetValue;
            //    Debug.WriteLine(t);
            //}
            //watch.Stop();
            //Response.Write("<br><br>");
            //Response.Write(watch.ElapsedMilliseconds);

            //watch = Stopwatch.StartNew();
            //foreach (var a in actions.ToList())
            //{
            //    double t = a.targetValue;
            //    Debug.WriteLine(t);
            //}
            //watch.Stop();
            //Response.Write("<br><br>");
            //Response.Write(watch.ElapsedMilliseconds);

            //watch = Stopwatch.StartNew();
            //foreach (var a in actions)
            //{
            //    double t = a.targetValue;
            //    Debug.WriteLine(t);
            //}
            //watch.Stop();
            //Response.Write("<br><br>");
            //Response.Write(watch.ElapsedMilliseconds);
            
        }
    }

    protected string fix_double_actives()
    {
        int test = 0;

        DataClassesDataContext db = new DataClassesDataContext();
        var actions_by_ticker = (from temp in db.Actions where temp.active && temp.user.bloomberg_broker.HasValue select temp).GroupBy(b=>b.ticker);
        foreach (var actions_by_ticker_item in actions_by_ticker)
        {
            foreach (var actions_by_ticker_by_analyst in actions_by_ticker_item.GroupBy(b=>b.analystID) )
            {
                if (actions_by_ticker_by_analyst.Count() > 1)
                {
                    //test++;
                    //if (test > 100)
                    //    return "";

                    int c = 0;
                    foreach (var a in actions_by_ticker_by_analyst.OrderByDescending(b => b.targetDate))
                    {
                        if (c>0){

                            a.active = false;
                            Response.Write("<font color=gray>Deactivated " + a.fund.ticker + " analyst " + a.user.display_name + " " + a.targetDate + " " + a.targetValue + "</font><br>");
                            db.SubmitChanges();

                        }
                        else
                        {
                            if (a.targetDate <= DateTime.Now)
                            {
                                Response.Write("<font color=red>OH OH</font><BR>");
                                a.active = false;
                                db.SubmitChanges();
                            }
                                
                            Response.Write("<font color=green>Keeping " + a.fund.ticker + " analyst " + a.user.display_name + " " + a.targetDate + " " + a.targetValue + "</font><br>");
                        }
                            

                        c=1;
                    }

                    Response.Write("<br><br>");
                }
            }
        }

        return "all set";
    }

    protected void get_analyst_rationales()
    {
        List<string> list = new List<string>() { "analyst-color", "downgrades", "upgrades", "initiation", "price-target" };

        foreach (string l in list)
        {
            for (int i = 0; i <= 30; i++)
            {
                benzinga_v2(i.ToString(), l);
            }
        }

        for (int i = 0; i < 50; i++)
            benzinga_v2(i.ToString(), null);
        loop_through_tickers();
    }

    protected void pull_ratings_from_twitter()
    {
        find_twitter("RatingsNetwork");
        find_twitter("usratings");
        find_twitter("InvestorWand");
        find_twitter("SmarterAnalyst");   
    }

    protected void find_twitter(string handle)
    {
        //int ratings_network_id = 992182100;
        //Twitterser
        //ListTweetsOnUserTimelineOptions x = new ListTweetsOnUserTimelineOptions();

        var service = new TwitterService(ConfigurationManager.AppSettings["TwitterAPIKey"], ConfigurationManager.AppSettings["TwitterAPISecret"]);
        service.AuthenticateWith(ConfigurationManager.AppSettings["InvesdTwitterAccessToken"], ConfigurationManager.AppSettings["InvesdTwitterAccessSecret"]);

        var tweets = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions
        {
            ScreenName = handle,
            //Count = 1000
        });

        string email_this = "";

        foreach (var tweet in tweets)
        {
            DataClassesDataContext db = new DataClassesDataContext();
            var exists = from temp in db.AnalystRatings where temp.tweet == tweet.Text select temp;
            int c = 1;
            if (!exists.Any())
            {
                AnalystRating a = new AnalystRating();
                a.tweet = tweet.Text;
                a.timestamp = tweet.CreatedDate;

                db.AnalystRatings.InsertOnSubmit(a);
                db.SubmitChanges();
                Response.Write(c + "- " +  tweet.Text + " " + tweet.CreatedDate + "<br><br>");

                if (tweet.Text.Contains("PT") && !tweet.Text.Contains("reiterated"))
                    email_this += tweet.Text + "<br>";

                c++;
            }

            

            
        }

        if (email_this.Length > 0)
            Ancillary.send_email("tweet@invesd.com", "Invesd Twitter", "arziai@gmail.com", "Amir", "Found new Twitter PTs", email_this, false);

        //TwitterStatus temp = service.SendTweet(new SendTweetOptions { Status = "Hello" });
        //ListTweetsOnUserTimelineOptions options = new ListTweetsOnUserTimelineOptions();
        //options.UserId = ratings_network_id;
        //options.Count = 10;

        //Action<IEnumerable<TwitterStatus>, TwitterResponse> action = new Action<IEnumerable<TwitterStatus>,TwitterResponse>();
        //ListTweetsOnUserTimeline 


    }

    protected void fix_aapl()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = (from temp in db.Actions where temp.ticker == 785 && !temp.active && temp.targetDate>DateTime.Now select temp).GroupBy(b => b.analystID);
        foreach (var analyst in actions)
        {
            //int c = 0;
            foreach (var a in analyst.OrderByDescending(b => b.targetDate))
            {
                a.active = true;
                //db.SubmitChanges();
                break;
            }
        }
    }

    public static void list(int days,int offset)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var x = from temp in db.trackings where temp.timestamp <= DateTime.Now.AddDays(-days * offset) && temp.timestamp >= DateTime.Now.AddDays(-days * (offset + 1)) && temp.analyst!=1 && temp.analyst!=2 && !temp.user.description.Contains("bot") && !temp.user.description.Contains("ysearch/slurp") && !temp.user.description.Contains("spider") && !temp.user.description.Contains("PycURL") select temp;
        
        if (x.Any())
        {
            write("from " + DateTime.Now.AddDays(-days * (offset + 1)) + " to " + DateTime.Now.AddDays(-days * offset));
            write("total pageviews: " + x.Count());
            write("total uniques: " + x.GroupBy(b=>b.analyst).Count() );
            write("users: " + x.Where(c=>c.user.display_name.Length>0).GroupBy(b=>b.analyst).Count() );

            foreach (var a in x)
            {
                write( a.timestamp.Date.ToString() + " " + a.user.display_name );
            }
        }

    }

    public static void write(string input){
        HttpContext.Current.Response.Write(input + "<br>");
    }

    public static void avergae_no_of_covered_wall_St_stocks()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = (from temp in db.Actions where temp.active && temp.user.bloomberg_broker.HasValue select temp).GroupBy(b=>b.analystID);
        if (actions.Any())
        {
            HttpContext.Current.Response.Write( actions.Average(b => b.GroupBy(c => c.ticker).Count()));
        }
    }

    public static void total_actions()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = db.Actions.Where(b => !b.user.bloomberg_broker.HasValue && b.active).Select(b => new { id = b.actionID, date = b.startDate, user = b.user.display_name, type = false, target = b.targetValue, ticker = b.fund.ticker, rationale = b.rational, fund_name = b.fund.name, analyst = b.analystID.Value, matured = b.matured, revised = b.Action_Previous.HasValue }).Concat(db.ActionMonitors.Where(b => b.active).Select(c => new { id = c.ID, date = c.investment_date, user = c.user.display_name, type = true, target = c.targetValue, ticker = c.fund.ticker, rationale = c.rationale, fund_name = c.fund.name, analyst = c.usermon, matured = c.matured, revised = false }));

        if (actions.Any())
        {
            int c = 1;
            foreach (var a in actions.GroupBy(b => b.analyst).OrderByDescending(b => b.Count()))
            {
                HttpContext.Current.Response.Write(c + "- " + a.First().user + ": " + a.Count());
                HttpContext.Current.Response.Write("<br>");
                c++;
            }
        }

    }

    public static void new_feed(){
        DataClassesDataContext db = new DataClassesDataContext();
        //var actions = (from temp in db.Actions orderby temp.startDate descending select temp).Take(100);
        //var actions = (from temp1 in db.Actions select temp1).Union(from temp in db.ActionMonitors select temp.ID);
        //var actions = db.Actions.Select(b => new { b.fund, b.startDate,b.user,b.targetValue,b.rational }).Union(db.ActionMonitors.Select(c => new {c.fund, startDate = c.investment_date,c.user,c.targetValue, rational = c.rationale }));
        var actions = db.Actions.Where(b=>!b.user.bloomberg_broker.HasValue && b.active && b.startDate>=DateTime.Now.AddDays(-30) ).Select(b => new { id = b.actionID, date = b.startDate, user = b.user.display_name, type = false, target = b.targetValue, ticker = b.fund.ticker, rationale = b.rational, fund_name = b.fund.name, analyst = b.analystID.Value, matured = b.matured, revised = b.Action_Previous.HasValue }).Concat(db.ActionMonitors.Where(b=>b.active && b.monitorStart>=DateTime.Now.AddDays(-30) ).Select(c => new { id = c.ID, date = c.investment_date, user = c.user.display_name, type=true , target = c.targetValue, ticker = c.fund.ticker, rationale = c.rationale, fund_name = c.fund.name, analyst = c.usermon, matured = c.matured, revised = false }));
        
        if (actions.Any())
        {
            int c = 1;
            foreach (var a in actions.Where(b=>b.analyst!=2 && b.analyst!=1).GroupBy(b => b.analyst).OrderByDescending(b=>b.Count()))
            {
                HttpContext.Current.Response.Write(c + " -" + a.First().user + ": " + a.Count() );
                HttpContext.Current.Response.Write("<br>");
                c++;
            }

            HttpContext.Current.Response.Write("<br>");
            HttpContext.Current.Response.Write("<br>");

            foreach (var a in actions.OrderByDescending(b=>b.date).ThenByDescending(b=>b.id).Take(10))
            {
                HttpContext.Current.Response.Write( (a.type?"[actionmonitor]":"[action]") + " " + a.user + " " + (a.ticker + " $" + a.target) + (a.rationale.Length > 0 ? (" <font style=\"color:gray\">\"" + a.rationale + "\"</font>") : "") + " " + (a.matured?"[MATURED] ":"") + ( a.revised?"[revised]":"" ) );
                HttpContext.Current.Response.Write("<br>");
            }
        }
    }

    public static void delete_funds_zombie(List<string> cos)
    {
        foreach (var c in cos){
            DataClassesDataContext db = new DataClassesDataContext();

            var financials = from temp in db.Financials where temp.fund.ticker == c select temp;
            if (financials.Any()){
                foreach (var f in financials)
                {
                    db.Financials.DeleteOnSubmit(f);
                    db.SubmitChanges();
                }
            }

            var ats = from temp in db.ArticleTickers where temp.fund.ticker == c select temp;
            if (ats.Any())
            {
                foreach (var a in ats)
                {
                    db.ArticleTickers.DeleteOnSubmit(a);
                    db.SubmitChanges();
                }
            }

            var fund = from temp in db.funds where temp.ticker == c select temp;
            if (fund.Any())
            {
                db.funds.DeleteOnSubmit(fund.First());
                db.SubmitChanges();
            }
                
        }
        
    }

    public static void high_52w()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var funds = (from temp in db.funds select temp).Take(5);
        foreach (var f in funds)
        {
            var values = from temp in db.fund_values where temp.fundID == f.fundID select temp;
            if (values.Any())
            {
                
            }
        }

    }

    protected void industry()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var selected = from temp in db.funds where temp.sector.HasValue && !temp.peer_group.HasValue select temp;
        if (selected.Any())
        {
            foreach (var s in selected.Take(10))
            {
                s.peer_group = DataBaseLayer.get_peer_group(s.ticker.Trim().ToUpper());
                db.SubmitChanges();
                Response.Write( s.ticker + "- " + s.peer_group + "<br>"); 
            }
        }

        //Response.Write( DataBaseLayer.get_peer_group("GOGO") );
    }

    public static void leaderboard_wallst()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        //var agg = (from temp in db.AnalystPerformances where temp.fund.sector.HasValue select temp).GroupBy(b=>b.fund.sector);
        //if (agg.Any())
        //{
        //    foreach (var sector in agg)
        //    {
        //        int c = 1;
        //        HttpContext.Current.Response.Write(sector.First().fund.Sector1.sector1 + "<br>");
        //        foreach (var analyst in sector.GroupBy(b => b.analyst).OrderByDescending(b=>b.Sum(d=>d.confidence)))
        //        {
        //            HttpContext.Current.Response.Write(c + "- " + analyst.First().user.display_name + ", " + (analyst.First().user.bloomberg_broker.HasValue ? analyst.First().user.Bloomberg_Broker1.name : "indie") + ", " + analyst.Sum(b => b.confidence) + ", " + analyst.Average(b=>b.confidence) + ", " + "<br>");

        //            if (c == 5)
        //                break;

        //            c++;
        //        }
        //        c = 1;
        //        foreach (var analyst in sector.GroupBy(b => b.analyst).Where(b => b.Sum(d => d.confidence) >= 5).OrderByDescending(b => b.Average(d => d.confidence)))
        //        {
        //            HttpContext.Current.Response.Write("<font color=gray>" + c + "- " + analyst.First().user.display_name + ", " + (analyst.First().user.bloomberg_broker.HasValue ? analyst.First().user.Bloomberg_Broker1.name : "indie") + ", " + analyst.Sum(b => b.confidence) + ", " + analyst.Average(b => b.confidence) + ", " + "</font><br>");

        //            if (c == 5)
        //                break;

        //            c++;
        //        }

        //        HttpContext.Current.Response.Write("<br>");
        //    }
        //}

        var agg2 = (from temp in db.AnalystPerformances where temp.fund.peer_group.HasValue select temp).GroupBy(b => b.fund.peer_group);
        if (agg2.Any())
        {
            foreach (var industry in agg2)
            {
                int c = 1;
                HttpContext.Current.Response.Write(industry.First().fund.Peer_Group1.name + "<br>");
                foreach (var analyst in industry.GroupBy(b => b.analyst).OrderByDescending(b => b.Sum(d => d.confidence)))
                {
                    HttpContext.Current.Response.Write(c + "- " + analyst.First().user.display_name + ", " + (analyst.First().user.bloomberg_broker.HasValue ? analyst.First().user.Bloomberg_Broker1.name : "indie") + ", " + analyst.Sum(b => b.confidence) + ", " + analyst.Average(b => b.confidence) + ", " + analyst.Count() + "<br>");

                    if (c == 5)
                        break;

                    c++;
                }
                c = 1;
                foreach (var analyst in industry.GroupBy(b => b.analyst).Where(b => b.Sum(d => d.confidence) >= 5).OrderByDescending(b => b.Average(d => d.confidence)))
                {
                    HttpContext.Current.Response.Write("<font color=gray>" + c + "- " + analyst.First().user.display_name + ", " + (analyst.First().user.bloomberg_broker.HasValue ? analyst.First().user.Bloomberg_Broker1.name : "indie") + ", " + analyst.Sum(b => b.confidence) + ", " + analyst.Average(b => b.confidence) + ", " + analyst.Count() + "</font><br>");

                    if (c == 5)
                        break;

                    c++;
                }

                HttpContext.Current.Response.Write("<br>");
            }
        }
    }

    public static List<competitors_highlevel> xyz(int sector)
    {
        List<competitors_highlevel> comp = new List<competitors_highlevel>();
        DataClassesDataContext db = new DataClassesDataContext();

        //var actions_raw = from temp in db.Actions where !temp.article1.user.bloomberg_broker.HasValue select temp;
        var actions_raw = from temp in db.Actions where !temp.user.bloomberg_broker.HasValue && temp.analystID.HasValue select temp;
        if (sector > 0)
            actions_raw = actions_raw.Where(b => b.fund.sector == sector);
        var actions = actions_raw.GroupBy(b => b.analystID);

        if (actions.Any())
        {
            var watch = Stopwatch.StartNew();

            foreach (var r in actions)
            {
                watch.Stop();
                HttpContext.Current.Response.Write("" + r.Count() + "<br>");
                HttpContext.Current.Response.Write("<font color=green>" + watch.ElapsedMilliseconds + "</font><br><br>");

                competitors_highlevel data = new competitors_highlevel();
                var r_element = r.First();
                data.name = r_element.user.display_name;
                data.userid = r_element.analystID.Value;
                data.membership_duration = r_element.user.joined.HasValue ? (DateTime.Now - r_element.user.joined.Value).TotalDays : -1;
                data.profit_return = r.Sum(b => (b.currentValue / b.startValue - 1) * (b.@short ? -1 : 1));
                
                data.deployed = r.Count();

                comp.Add(data);

                watch.Start();
            }
        }

        return comp;
    }

    public void leaderboard_wall_st(int sector)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var abc = (from temp in db.AnalystPerformances where temp.fund.peer_group == sector select temp).GroupBy(b=>b.analyst).OrderByDescending(b=>b.Sum(c=>c.confidence)).Take(20);
        //var abc = (from temp in db.AnalystPerformances where temp.fund.sector == sector select temp).GroupBy(b => b.analyst).OrderByDescending(b => b.Average(c => c.confidence)).Take(20);

        if (abc.Any())
        {
            foreach (var a in abc)
            {
                Response.Write( "<img src=\"http://invesd.com/images/user/"  + a.First().analyst + ".png\">" + a.First().user.display_name + ", " + a.First().user.Bloomberg_Broker1.name + ", " + a.Sum(b=>b.confidence) +", " + a.Average(b=>b.confidence) + ", " + a.First().analyst + "<br>" );
            }
        }
    }

    protected void find_unverified(int pick,int skip)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var unvs = (from temp in db.users where temp.temp_email.Length > 0 orderby temp.userID select temp).Skip(skip).Take(pick);
        if (unvs.Any())
        {
            foreach (var u in unvs)
            {
                send_test_email(u.userID,u.randomVariable,u.display_name,u.temp_email );       
            }
        }
    }

    protected void send_test_email(int userid,string rnd,string name,string email)
    {
        string url = "https://www.invesd.com/Signup.aspx?uid=" + userid + "&rid=" + rnd;
        Ancillary.send_email("info@invesd.com", "Invesd", email, name, "Welcome to Invesd, please verify your email", Ancillary.general_message_template(name, url, "Verify your email to continue", "Welcome to the Invesd community. Share your estimates, get feedback from your peers and build your track record.<br><p style=\"text-align:center\"><a href=\"" + url + "\"><img src=\"https://invesd.com/images/you_vs_ws.png\" style=\"width:450px\"></a></p>", true, null,0,"test"),true);
    }

    protected void delete_crap()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        List<string> x = new List<string>();
        x.Add("alakidelete@gmail.com");
        x.Add("xom@invesd.com");
        x.Add("mehrad.tavakoli2@gmail.com");
        x.Add("mehradtavakoli2@gmail.com");
        x.Add("mehradtavakoli3@gmail.com");
        
        
        //x.Add("mehrad.tavakoli4@gmail.com");
        //x.Add("mehradtavakoli2@gmail.com");
        //x.Add("daibfidabf@dfbaidfh.com");
        //x.Add("a@invesd.com");
        //x.Add("a@c.com");
        //x.Add("xmlxml@gmail.com");
        //x.Add("a@d.com");
        //x.Add("amirtest@gmail.com");
        //x.Add("asdfasdfasdf@abc.com");
        //x.Add("asfasdfasdf@asdfasdfasdf.com");
        //x.Add("arziai@gmail.co");
        //x.Add("test@user.co");
        //x.Add("test@user.com");
        //x.Add("test@user.coo");
        //x.Add("jan@goolian.com");
        //x.Add("test@test.com");
        //x.Add("aza@invesd.com");
        ////x.Add("jerry_jarod@hotmail.com");
        //x.Add("yousef.zokaei@gmail.com");
        //x.Add("amiramirtestest@invesd.com");
        //x.Add("mehradtavakoliiii2@gmail.com");
        //x.Add("tst@lsdjl.vpf");
        //x.Add("test@nsjdf.com");
        //x.Add("test@nsjdf.com");
        //x.Add("aol@aol.com");
        //x.Add("mehradtavakolii@gmail.com");
        //x.Add("mehrad2tavakoli@gmail.com");
        //x.Add("a@invesd.om");
        //x.Add("");
        //x.Add("");
        //x.Add("");

        //var all = from temp in db.users where temp.temp_email.Length > 0 && (x.Contains(temp.temp_email) || temp.temp_email.Contains("@invesd.com")) select temp;
        var all = from temp in db.users where (temp.email.Contains("@invesd.com") && temp.email != "blog@invesd.com") || x.Contains(temp.email) select temp;

        //foreach (var a in all)
        //{
        //    write(a.display_name);
        //}

        if (all.Any())
        {
            foreach (var a in all)
            {
                var pay = from temp in db.payments where temp.userID == a.userID select temp;
                if (pay.Any())
                {
                    foreach (var p in pay)
                    {
                        db.payments.DeleteOnSubmit(p);
                        db.SubmitChanges();
                    }
                }

                var subs = from temp in db.Subscriptions where temp.analyst == a.userID select temp;
                if (subs.Any())
                {
                    foreach (var s in subs)
                    {
                        db.Subscriptions.DeleteOnSubmit(s);
                        db.SubmitChanges();
                    }
                }

                var at = from temp in db.ArticleTickers where temp.article1.origin == a.userID select temp;
                if (at.Any())
                {
                    foreach (var aa in at)
                    {
                        db.ArticleTickers.DeleteOnSubmit(aa);
                        db.SubmitChanges();
                    }
                }

                var nots = from temp in db.Notifications where temp.investor == a.userID select temp;
                foreach (var not in nots)
                {
                    db.Notifications.DeleteOnSubmit(not);
                    db.SubmitChanges();
                }

                var follows = from temp in db.Follow_Histories where temp.followed == a.userID select temp;
                foreach (var f in follows)
                {
                    db.Follow_Histories.DeleteOnSubmit(f);
                    db.SubmitChanges();
                }

                var fs = from temp in db.Follows where temp.followed == a.userID select temp;
                foreach (var f in fs)
                {
                    db.Follows.DeleteOnSubmit(f);
                    db.SubmitChanges();
                }

                var ts = from temp in db.trackings where temp.analyst == a.userID select temp;
                foreach (var t in ts)
                {
                    db.trackings.DeleteOnSubmit(t);
                    db.SubmitChanges();
                }

                var ips = from temp in db.user_IP_and_clients where temp.userid == a.userID select temp;
                foreach (var ip in ips)
                {
                    db.user_IP_and_clients.DeleteOnSubmit(ip);
                    db.SubmitChanges();
                }

                var ws = from temp in db.Weekly_Reports where temp.investor == a.userID select temp;
                foreach (var w in ws)
                {
                    db.Weekly_Reports.DeleteOnSubmit(w);
                    db.SubmitChanges();
                }

                var actions = from temp in db.Actions where temp.article1.origin == a.userID select temp;
                foreach (var aa in actions)
                {
                    db.Actions.DeleteOnSubmit(aa);
                    db.SubmitChanges();
                }

                var articles = from temp in db.articles where temp.origin == a.userID select temp;
                foreach (var ax in articles)
                {
                    db.articles.DeleteOnSubmit(ax);
                    db.SubmitChanges();
                }

                var follow_Sector = from temp in db.Follow_History_Company_Industry_Sectors where temp.follower == a.userID select temp;
                foreach (var fss in follow_Sector)
                {
                    db.Follow_History_Company_Industry_Sectors.DeleteOnSubmit(fss);
                    db.SubmitChanges();
                }

                var follow_fund = from temp in db.Follow_funds where temp.follower == a.userID select temp;
                foreach (var ff in follow_fund)
                {
                    db.Follow_funds.DeleteOnSubmit(ff);
                    db.SubmitChanges();
                }

                var positions = from temp in db.ActionMonitors where temp.usermon == a.userID select temp;
                foreach (var p in positions)
                {
                    db.ActionMonitors.DeleteOnSubmit(p);
                    db.SubmitChanges();
                }

                db.users.DeleteOnSubmit(a);
                db.SubmitChanges();
                Response.Write(a.temp_email + ", " + a.display_name + "<br>");
                //}
                //catch { }
            }
        }

    }

    protected void all_new()
    {
        DataClassesDataContext db = new DataClassesDataContext();

        var actions = from temp in db.Actions where temp.active && temp.startDate >= DateTime.Now.AddDays(-10) && temp.article1.user.bloomberg_broker.HasValue select temp;
        if (actions.Any())
        {
            Response.Write( actions.Count() + "<br>" );

            foreach (var b in actions.GroupBy(b=>b.ticker))
            {
                Response.Write(b.First().fund.ticker + "<br>");
                foreach (var a in b)
                {
                    double prev = a.Action5.targetValue;
                    double now = a.targetValue;
                    if ( Math.Abs(now/prev - 1)>=0.05 )
                    {
                        string beg = "";
                        string end = "";
                        if ((DateTime.Now - a.startDate).TotalDays <= 3)
                        {
                            beg = "<font color=green>";
                            end = "</font>";
                        }
                        
                        Response.Write( beg + a.article1.user.display_name + ", " + a.article1.user.Bloomberg_Broker1.name + ", " + a.targetValue + ", " + a.Action5.targetValue + ", " + Math.Round((now / prev - 1) * 100, 0) + "%" + end + "<br>");
                    }
                        
                }
                Response.Write("<br>");
            }
        }
    }

    protected void counter()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var users = from temp in db.users select temp;

        if (users.Any())
        {
            Response.Write("WS: " + users.Where(b => b.bloomberg_broker.HasValue).Count() + "<br>");
            Response.Write("All:" + users.Where(b=>b.verified.Value || b.state == "verified").Count() + "<br>" );
        }

        var ams = from temp in db.ActionMonitors where temp.active select temp;
        if (ams.Any())
        {
            Response.Write("ams active: " + ams.Count() + "<br>");
            Response.Write("ams users: " + ams.GroupBy(b=>b.usermon).Count() + "<br>");

            Response.Write("<br>");

            foreach (var aa in ams.Where(b => (DateTime.Now - b.investment_date).TotalDays <= 2).OrderByDescending(b=>b.investment_date))
            {
                Response.Write(aa.user.display_name + ", " + aa.fund.ticker + "<br>");
            }

            Response.Write("<br>");
        }

        var a = from temp in db.Actions where temp.active select temp;
        if (a.Any())
        {
            var aa = a.Where(b => !b.article1.user.bloomberg_broker.HasValue && b.matured);
            foreach (var bb in aa)
            {
                Response.Write(bb.article1.user.display_name + ", " + bb.fund.ticker + "<br>");
            }

            Response.Write("a active: " + a.Count() + "<br>");
            Response.Write("a tickers: " + a.GroupBy(b=>b.ticker).Count() + "<br>");
            Response.Write("a users (ppl): " + a.Where(b=>!b.article1.user.bloomberg_broker.HasValue).GroupBy(b=>b.article1.user).Count() + "<br>");
            Response.Write("a users (ppl ver.): " + a.Where(b => !b.article1.user.bloomberg_broker.HasValue && b.article1.user.temp_email.Length == 0).GroupBy(b => b.article1.user).Count() + "<br>");
            Response.Write("a users: " + a.Where(b => !b.article1.user.bloomberg_broker.HasValue).Count() + "<br>");
            Response.Write("a analysts: " + a.Where(b => b.article1.user.bloomberg_broker.HasValue).GroupBy(b=>b.article1.user).Count() + "<br>");
        }

        Response.Write("<br>");

        foreach (var b in a.Where(b => !b.article1.user.bloomberg_broker.HasValue).OrderByDescending(b=>b.actionID))
        {
            Response.Write( b.article1.user.display_name + ", " + b.fund.ticker + "<br>" );
        }

    }

    protected void give_me_similar_cos() {
        
        DataClassesDataContext db = new DataClassesDataContext();
        var fund = (from temp in db.funds where temp.ticker == Request.QueryString["t"] select temp).First();
        List<string> n = new List<string>();
        n = Scaffolding.similar_companies(fund, db);

        foreach (var nn in n)
        {
            Response.Write(nn + "<br>");
        }
    }

    protected void estimize_twitter()
    {
        FileInfo newFile = new FileInfo(HttpContext.Current.Server.MapPath("~") + "\\files\\emails.xlsx");
        ExcelPackage pck = new ExcelPackage(newFile);
        var ws = pck.Workbook.Worksheets["Sheet1"];
        int row = 2;
        int col = 2;

        try
        {
            while (ws.Cells[row, col].Value != null)
            {
                //string twitter = ws.Cells[row, col].Value.ToString();
                string twitter = ws.Cells[row,col].Value.ToString();
                if (twitter.IndexOf(",") > 0)
                {
                    twitter = twitter.Substring(0, twitter.IndexOf(","));
                }

                twitter = twitter.Replace(",", "").Replace(" ", ".").Trim() + "@gmail.com";
                Ancillary.send_email("amir@invesd.com","Invesd",twitter + "@gmail.com",twitter,"Join 4,500+ investment professionals","Build your track record and view the positions and estimates of analyst and investors on 4,000+ stocks: http://invesd.com/?t",true);

                if (row == 100)
                {
                    break;
                }

                row++;
            }
        }
        catch (Exception e) {
            Response.Write("error " + e.Message);
        }
    }

    protected void update_amir()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var user = from temp in db.users where temp.userID == 2 select temp;
        if (user.Any())
        {
            user.First().progress = 0;
            db.SubmitChanges();
            Response.Write( user.First().investor_type + "<br>" );
            Response.Write(user.First().investor_risk + "<br>");
        }
    }

    protected void std()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var tickers = (from temp in db.fund_values where !temp.fund.std_1yr_return.HasValue select temp).GroupBy(b=>b.fundID);

        //Response.Write( tickers.Count() );

        foreach (var ticker in tickers)
        {
            if ( ticker.Where(b=>b.date>=DateTime.Now.AddDays(-365)).Count()>=30 )
            {
                bool ok = true;
                var values = (from temp in db.fund_values where temp.fund.ticker == ticker.First().fund.ticker && temp.date >= DateTime.Now.AddDays(-365) select temp).OrderBy(b => b.date);
                if (values.Any())
                {
                    double past = values.First().adjValue;
                    List<double> ret = new List<double>();
                    if (past > 0)
                    {
                        foreach (var v in values.OrderBy(b => b.date).Skip(1))
                        {
                            ret.Add(v.adjValue / past - 1);
                            past = v.adjValue;
                            if (past == 0)
                            {
                                ok = false;
                                Response.Write(v.fund.ticker + "<br>");
                                break;
                            }
                        }
                    }
                    else
                    {

                    }

                    if (ok)
                    {
                        double std = Math.Round(Ancillary.standard_deviation(ret, false), 5);
                        //foreach (var r in ret)
                        //{
                        //    Response.Write("<font color=gray>" + r + "</font><br>");
                        //}

                        try
                        {
                            Response.Write(ticker.First().fund.ticker + ": " + Math.Round(100 * std, 1) + "%<br>");
                            ticker.First().fund.std_1yr_return = std;
                            db.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            Response.Write(e.Message + "<br>");
                        }
                    }

                    //var fund = from temp in db.funds where temp.fundID == ticker.First().fundID select temp;
                    //if (fund.Any())
                    //{
                    //    try
                    //    {
                    //        Response.Write(ticker.First().fund.ticker + ": " + Math.Round(100 * std, 1) + "%<br>");
                    //        fund.First().std_1yr_return = std;
                    //        db.SubmitChanges();
                    //    }
                    //    catch (Exception e) { 
                    //        Response.Write( "Error " + e.Message + ticker.First().fund.ticker + ", " + std + "<br>" );
                    //    }
                    //}
                }
                else
                {
                    Response.Write("no value<br>");
                }
            }
            
        }

    }

    public void analyst_test()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        Func<DateTime, int> weekProjector = 
        d => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
             d,
             CalendarWeekRule.FirstFourDayWeek,
             DayOfWeek.Sunday);

        var analysts = (from temp in db.trackings where temp.hyperlink.Contains("=1713148") && (temp.analyst != 2 || temp.analyst != 1) select temp).GroupBy(b=> b.timestamp);
        if (analysts.Any())
        {
            foreach (var a in analysts)
            {
                Response.Write(a.First().timestamp.Date + ", " + a.Count() + "<br>");
                //Response.Write(a.hyperlink.Replace("Analyst.aspx?analyst=", "").Replace("http://invesd.com/analyst.aspx?analyst=", "") + "<br>");
                //var get = (from t in a.hyperlink where char.IsDigit(t) select t).ToArray();
                //Response.Write(new string(get)  + "<br>");
            }
            


        }

    }

    public void delete_all_targets_for_stock_and_history(int fundid)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var stock = from temp in db.Actions where temp.ticker == fundid select temp;
        if (stock.Any())
        {
            int c = 1;
            foreach (var a in stock.OrderByDescending(b=>b.startDate))
            {
                var article = (from temp in db.articles where temp.idarticle == a.article select temp).First();
                try
                {
                    db.Actions.DeleteOnSubmit(a);
                    //Response.Write("Deleted action " + a.actionID + "<br>");
                    db.SubmitChanges();
                    db.articles.DeleteOnSubmit(article);
                    //Response.Write("Deleted article " + a.article + "<br>");
                    db.SubmitChanges();
                    
                    Response.Write(c);
                }
                catch { 
                    Response.Write("Error in action " + a.actionID + "<br>");
                }
                c++;
            }
        }

        var agg = from temp in db.Aggregate_Tickers where temp.ticker == fundid select temp;
        if (agg.Any())
        {
            try
            {
                db.Aggregate_Tickers.DeleteOnSubmit(agg.First());
                //Response.Write("Deleted agg " + agg.First().id + "<br>");
                db.SubmitChanges();
            }
            catch { }
            
        }

        var ap = from temp in db.AnalystPerformances where temp.ticker == fundid select temp;
        if (ap.Any())
        {
            foreach (var a in ap)
            {
                try
                {
                    db.AnalystPerformances.DeleteOnSubmit(a);
                    //Response.Write("Deleted AP " + a.id + "<br>");
                    db.SubmitChanges();
                }
                catch { }
                
            }
        }
    }

    public void delete_user(List<int> user)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var users = from temp in db.users where user.Contains(temp.userID) select temp;
        if (users.Any())
        {
            foreach (var u in users)
            {
                var notification = from temp in db.Notifications where temp.investor == u.userID select temp;
                if (notification.Any())
                {
                    foreach (var n in notification)
                    {
                        db.Notifications.DeleteOnSubmit(n);
                        db.SubmitChanges();
                    }
                }

                var follow_history = from temp in db.Follow_Histories where temp.followed == u.userID select temp;
                if (follow_history.Any())
                {
                    foreach (var f in follow_history)
                    {
                        db.Follow_Histories.DeleteOnSubmit(f);
                        db.SubmitChanges();
                    }
                }

                var follow = from temp in db.Follows where temp.followed == u.userID select temp;
                if (follow.Any())
                {
                    foreach (var f in follow)
                    {
                        db.Follows.DeleteOnSubmit(f);
                        db.SubmitChanges();
                    }
                }

                var track = from temp in db.trackings where temp.analyst == u.userID select temp;
                if (track.Any())
                {
                    foreach (var t in track)
                    {
                        db.trackings.DeleteOnSubmit(t);
                        db.SubmitChanges();
                    }
                }

                var cash = from temp in db.Invite_Cash_Addeds where temp.invitee == u.userID || temp.invitor == u.userID select temp;
                if (cash.Any())
                {
                    foreach (var c in cash)
                    {
                        db.Invite_Cash_Addeds.DeleteOnSubmit(c);
                        db.SubmitChanges();
                    }
                }

                var gripes = from temp in db.Gripes where temp.userid == u.userID select temp;
                if (gripes.Any())
                {
                    foreach (var g in gripes)
                    {
                        db.Gripes.DeleteOnSubmit(g);
                        db.SubmitChanges();
                    }
                }

                db.users.DeleteOnSubmit(u);
                db.SubmitChanges();

            }
        }
    }

    protected void insert_notification()
    {
        //string msg = "<a href=\"/Blog/?post=7\">Big data goes Hollywood</a>";
        string msg = "We are kicking off <a href=\"Challenge.aspx\">Q2 2014 challenge</a>";
        DataBaseLayer.AddNotification("General", null, null, true, false, false, msg);
    }

    protected void archive_previous_season(string season)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var find_season = from temp in db.Challenge_Seasons where temp.challenge_name == season select temp;
        if (find_season.Any())
        {

            List<competitors_highlevel> y = Portfolio.leaderboard_season_guts("current");
            int count = 1;
            foreach (var x in y.OrderByDescending(b=>b.profit_return))
            {
                Challenge c = new Challenge();
                c.user = x.userid;
                c.deployment = x.deployed;
                c.challenge1 = find_season.First().id;
                c.profit = x.profit_return;
                c.rank = count;
                db.Challenges.InsertOnSubmit(c);
                db.SubmitChanges();

                count++;
            }

            //if (challenge.Any())
            //{
            //    int count = 1;
            //    foreach (var c in challenge.OrderByDescending(b=>b.profit))
            //    {
            //        Challenge data = new Challenge();
            //        data.user = c.user;
            //        data.deployment = c.deployment;
            //        data.profit = c.profit;
            //        data.rank = count;
            //        data.challenge1 = find_season.First().id;
            //        count++;

            //        if (!(from temp in db.Challenges where temp.user == c.user && temp.challenge1 == find_season.First().id select temp).Any())
            //        {
            //            db.Challenges.InsertOnSubmit(data);
            //            db.SubmitChanges();
            //        }
            //    }
            //}
        }
    }

    protected void fun_with_industries()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var industry = (from temp in db.AnalystPerformances where temp.industry == 212 orderby temp.confidence descending select temp).Take(10);
        foreach (var i in industry)
        {
            Response.Write(i.user.display_name + ", " + i.user.Bloomberg_Broker1.name + i.confidence + "<br>");
        }

    }

    protected void fix_caaed()
    {
        //DataClassesDataContext db = new DataClassesDataContext();
        //var xxs = from temp in db.Actions where temp.ticker == 9914 select temp;

        

        //var xs =  xxs.Where(b=>b.Action_Next.HasValue || b.actionID == 665520 || b.actionID == 665523);

        //Response.Write(xxs.Count() + "to reset<BR>" + xs.Count() + " to deactivate<br><BR>");

        //foreach (var x in xs.OrderByDescending(b=>b.startDate))
        //{
        //    Response.Write(x.actionID + "- " + x.article1.user.display_name + x.startDate + x.targetValue + "<br>");
        //    x.active = false;
        //    db.SubmitChanges();
        //}

        
        //Response.Write("<BR><BR>TO RESET<BR>");

        //foreach (var x in xxs)
        //{
        //    Response.Write(x.actionID + "- " + x.article1.user.display_name + x.startDate + x.targetValue + "<br>");
        //    AdminBackend.reset_action(x, db);
        //}

        //var xs = (from temp in db.Actions where temp.ticker == 9914 select temp).GroupBy(b => b.article1.origin);
        //if (xs.Any())
        //{
        //    foreach (var x in xs)
        //    {
        //        foreach (var y in x)
        //        {
        //            if (y.Action_Next.HasValue)
        //            {
        //                y.active = false;
        //                db.SubmitChanges();
        //            }
        //        }
        //    }
        //}


    }

    protected void yahoo_ipo_list()
    {
        HtmlWeb hw = new HtmlWeb();
        DataClassesDataContext db = new DataClassesDataContext();

        for (char c = 'b'; c <= 'z'; c++)
        {

            string url = "http://biz.yahoo.com/ipo/comp_" + c + ".html";
            HtmlDocument ho = hw.Load(url);
            HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//tr");

            foreach (HtmlNode z in m)
            {
                if (z.HasChildNodes)
                {
                    try
                    {
                        string ticker = z.ChildNodes.ElementAtOrDefault(1).InnerText.Trim();
                        string check = z.ChildNodes.ElementAtOrDefault(7).InnerText;
                        if (!string.IsNullOrEmpty(ticker) && ticker.Length<10)
                        {
                            var find = from temp in db.funds where temp.ticker.Trim() == ticker select temp;
                            if (!find.Any())
                            {
                                Response.Write(ticker + ": ");
                                Response.Write(AdminBackend.fundAdd_Click(ticker));
                                Response.Write("<br>");
                            }
                                
                        }
                            
                    }
                    catch { }
                    
                }
                    
            }

        }
    }

    protected void list_of_stocks_not_covered()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        //var list = (from temp in db.Actions select temp.ticker).Distinct().ToList();

        var not_covered = from temp in db.funds select temp;
        if (not_covered.Any())
        {
            foreach (var nc in not_covered)
            {
                var funds = from temp in db.Actions where temp.active && temp.ticker == nc.fundID select temp;
                if (!funds.Any())
                    Response.Write(nc.ticker + "<br>");
            }
        }

    }

    protected void similar_analysts()
    {
        DataClassesDataContext db = new DataClassesDataContext();
            var analyst_ticker = from temp in db.Actions where temp.active && temp.fund.peer_group.HasValue && temp.article1.origin == Convert.ToInt32( Request.QueryString["analyst"] ) select temp;
            var analyst = analyst_ticker.GroupBy(b => b.fund.peer_group);
            if (analyst.Any())
            {
                int c=1;
                List<int> list = new List<int>();
                List<int> list_ticker = new List<int>();

                foreach (var ticker in analyst_ticker.GroupBy(b=>b.ticker).OrderByDescending(b=>b.Count()))
                {
                    list_ticker.Add( ticker.First().ticker );
                    if (c >= 3)
                        break;

                    c++;
                }

                c = 1;
                foreach (var industry in analyst){
                    Response.Write(industry.First().fund.peer_group + "- " + industry.First().fund.Peer_Group1.name + "<br>");
                    list.Add( industry.First().fund.peer_group.Value );

                    if (c>=3)
                        break;

                    c++;
                }

                var similar = (from temp in db.Actions where temp.active && list.Contains(temp.fund.peer_group.Value) && list_ticker.Contains( temp.ticker ) select temp).GroupBy(b=>b.article1.origin).OrderByDescending(b=>b.Count());
                if (similar.Any())
                {
                    c = 1;
                    foreach (var s in similar)
                    {
                        Response.Write("<a href=\"../Analyst.aspx?analyst=" + s.First().article1.origin + "\">" + s.First().article1.user.display_name + "</a><br>");

                        if (c >= 5)
                            break;

                        c++;
                    }
                }
            }
    }

    protected void technicals()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        bool test = true;
        int ticker_id = Convert.ToInt32( Request.QueryString["ticker"] );
        //List<int> MA_spans = new List<int>() { 3,5,10,20,50, 100,150, 200 };
        List<int> MA_spans = new List<int>() { 3, 50, 200 };

        var funds = from temp in db.funds select temp;
        if (test)
            funds = funds.Where(b => b.fundID == ticker_id);

        if (funds.Any())
        {
            foreach (var fund in funds)
            {
                
                foreach (int MA_span in MA_spans)
                {
                    DateTime date = new DateTime(2014, 1, 1);

                    Response.Write(MA_span + "<br>");
                    while ( date<=DateTime.Now ){
                        var fund_values = (from temp in db.fund_values where temp.fundID == ticker_id && temp.date < date orderby temp.date descending select temp.closeValue.Value).Take(MA_span);
                        if (fund_values.Count() == MA_span)
                        {
                            Response.Write(string.Format("{0:MMM d, yy}", date) + ": " + fund_values.Average() + ", " + Ancillary.standard_deviation(fund_values.ToList(),false) + "<br>");
                        }
                        date = date.AddDays(1);
                    }

                    Response.Write("<br><br>");
                }
            }
        }


    }

    protected void list_invested_and_followed()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var invested = (from temp in db.ActionMonitors select temp).GroupBy(b => b.ticker);
        if (invested.Any())
        {
            foreach (var i in invested)
            {
                Response.Write("<img src=\"" + Page.ResolveUrl("~") + "images/logo/" + i.First().fund.ticker.Trim() + ".png\"></img>" + i.First().fund.ticker.Trim() + ", " + i.Count() + " <br>");
            }
        }

        Response.Write("Followed:");

        var followed = (from temp in db.Follow_funds select temp).GroupBy(b => b.fund);
        if (followed.Any())
        {
            foreach (var i in followed)
            {
                Response.Write("<img src=\"" + Page.ResolveUrl("~") + "images/logo/" + i.First().fund1.ticker.Trim() + ".png\"></img>" + i.First().fund1.ticker.Trim() + ", " + i.Count() + " <br>");
            }
        }

    }

    private static int jewish_bastard(){
        DataClassesDataContext db = new DataClassesDataContext();

        var all = from temp in db.trackings where temp.user.employer == "79.177.110.21" select temp;
        var original = from temp in db.users where temp.employer == "79.177.110.21" orderby temp.userID select temp;

        int c = 0;
        foreach (var o in original)
        {
            if (c > 0)
            {
                db.users.DeleteOnSubmit(o);
                db.SubmitChanges();
            }
            c++;
            
        }

        return (from temp in db.users where temp.employer == "79.177.110.21" select temp.userID).Count();
    }

    protected void fix_life_temp()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var agg = from temp in db.Aggregate_Tickers where temp.fund.ticker == "LIFE" select temp;
        if (agg.Any())
        {
            if (agg.Count() == 1)
            {
                db.Aggregate_Tickers.DeleteOnSubmit(agg.First());
                db.SubmitChanges();
            }
        }

        AdminBackend.metrics_industry_sector();
    }


    protected string revive_inactivated_actions()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        //var actions = from temp in db.Actions where !temp.active select temp;
        //var all = from temp in db.Actions where !temp.active && temp.expired && (DateTime.Now - temp.lastUpdated).TotalDays<2  select temp;
        //var all = from temp in db.Actions where !temp.active && temp.matured && temp.targetDate >= DateTime.Now && !temp.Action_Next.HasValue select temp;
        var all = from temp in db.Actions where temp.actionID == 0 select temp;

        foreach (var a in all)
        {
            a.active = true;
            a.expired = false;
            db.SubmitChanges();
        }

        return all.Count().ToString();

    }




    // scrape rationales

    protected void get_biotech_something()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where temp.startDate >= DateTime.Now.AddDays(-420) && temp.fund.Peer_Group1.name == "Biotechnology" select temp;

        if (actions.Any())
        {
            Response.Write(actions.Count() + "<br>");
            foreach (var by_ticker in actions.GroupBy(b=>b.ticker))
            {
                Response.Write("<img style=\"width:50px;height:50px\" src=\"http://invesd.com/images/logo/" + by_ticker.First().fund.ticker.Trim() + ".png\"><br><br>");

                foreach (var by_analyst in by_ticker.GroupBy(b=>b.article1.origin))
                {
                    Response.Write(by_analyst.First().article1.user.display_name + ", " + by_analyst.First().article1.user.Bloomberg_Broker1.name + "<br>");
                    foreach (var action in by_analyst.OrderBy(b=>b.startDate))
                    {
                        Response.Write(action.targetValue );

                        if (action.Action_Previous.HasValue)
                        {
                            var action_previous = from temp in db.Actions where temp.actionID == action.Action_Previous.Value select temp;
                            if (action_previous.Any())
                            {
                                Response.Write(" (" + Math.Round(100 * (action.targetValue / action_previous.First().targetValue - 1),1) + "%)");
                            }
                        }

                        Response.Write(", ");
                    }
                    Response.Write("<br>");
                }
                Response.Write("<br><br>");
            }
        }
    }

    protected void streetinsider_control(string ticker)
    {
        string url_string = "http://www.streetinsider.com/rating_history.php?q=" + ticker;
        streetinsider_main(url_string, ticker);
    }

    protected void streetinsider_main(string url_string, string ticker)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        string base_url = "http://www.streetinsider.com/";

        HtmlWeb hw = new HtmlWeb();
        HtmlDocument ho = hw.Load(url_string);
        HtmlNodeCollection tables_first = ho.DocumentNode.SelectNodes("//table[@class='rating_history']");

        if (tables_first != null)
        {
            foreach (HtmlNode table_first in tables_first)
            { // two of these tables exist for main, one (1) for page within

                int count_tr = 0;
                foreach (var tr in table_first.ChildNodes)
                {

                    if (count_tr > 1)
                    {

                        HtmlNodeCollection tds = tr.SelectNodes("td");
                        streetinsider si = new streetinsider();

                        if (tds != null)
                        {
                            si = streetinsider_deeper(tds, true, base_url);
                        }


                        if (si.broker != null && si.tgx > 0 && si.url_next != null)
                        { // worth looking under the hood
                            //  now call the inner page

                            Response.Write(si.broker + " " + si.tgx + " " + si.url_next + "<br>");
                            streetinsider_inner(si.url_next.Replace("amp;", ""), si.tgx, si.broker, ticker);
                            Response.Write("<br><br>");
                        }

                    }

                    count_tr++;
                }
            }
        }
    }

    protected void streetinsider_inner(string url_string, double tgx, string broker, string ticker)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        string base_url = "http://www.streetinsider.com/";

        HtmlWeb hw = new HtmlWeb();
        HtmlDocument ho = hw.Load(url_string);
        HtmlNodeCollection tables_first = ho.DocumentNode.SelectNodes("//table[@class='rating_history']");

        if (tables_first != null)
        {
            foreach (HtmlNode table_first in tables_first)
            { // two of these tables exist for main, one (1) for page within

                int count_tr = 0;
                foreach (var tr in table_first.ChildNodes)
                {

                    if (count_tr > 1)
                    {
                        HtmlNodeCollection tds = tr.SelectNodes("td");
                        streetinsider si = new streetinsider();

                        if (tds != null)
                        {
                            si = streetinsider_deeper(tds, false, base_url);
                        }


                        if (broker != null && tgx > 0 && si.url_summary != null && tgx > 0)
                        { // worth looking under the hood
                            //  get and insert summary
                            try
                            {


                                //if (summary != null) {
                                var action = from temp in db.Actions where temp.fund.ticker == ticker && temp.targetValue == tgx && temp.active && (temp.article1.text == null || temp.article1.text.Trim() == "") && (temp.article1.user.Bloomberg_Broker1.name == broker || temp.article1.user.Bloomberg_Broker1.name.Contains(broker) || temp.article1.user.Bloomberg_Broker1.name.Contains(broker.Replace(" ", "")) || temp.article1.user.Bloomberg_Broker1.name.Contains(broker.Split(' ')[0])) select temp;
                                if (action.Any())
                                {
                                    string summary = get_double_quotes_streetinsider(si.url_summary);

                                    if (!string.IsNullOrEmpty(summary))
                                    {
                                        insert_summary_meat(action.First().actionID, summary, si.url_summary);
                                        Response.Write("<br><font color=green>Added</font><br>");
                                    }
                                }
                                //}

                            }
                            catch
                            {
                                //Response.Write("<font color=red>Fucked up for " + broker + " - " + si.url_summary + "</font><br>");
                            }
                            //Response.Write(broker + " " + tgx + " " + url_next);
                            //Response.Write("<br><br>");
                            break;
                        }


                    }

                    count_tr++;
                }
            }
        }
    }

    protected streetinsider streetinsider_deeper(HtmlNodeCollection tds, bool main, string base_url)
    {
        streetinsider si = new streetinsider();
        int count_td = 0;

        foreach (var td in tds)
        {
            switch (count_td)
            {
                case 1:
                    try
                    {
                        if (main)
                        {
                            si.broker = td.ChildNodes[0].InnerText.Trim();
                            si.url_next = base_url + td.ChildNodes[0].Attributes[1].Value.Trim();
                        }
                    }
                    catch { }
                    break;
                case 4:
                    try
                    {
                        if (main)
                        {
                            si.tgx = Convert.ToDouble(td.InnerText.Substring(0, td.InnerText.IndexOf('(')));
                        }
                    }
                    catch { }
                    break;
                case 7:
                    if (!main)
                    {
                        try
                        {
                            si.url_summary = td.ChildNodes[0].Attributes[0].Value;
                        }
                        catch { }
                    }
                    break;
            }

            count_td++;
        }

        return si;
    }

    

    protected void benzinga_v2(string page,string segment)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        string url_string = "http://www.benzinga.com/" + (!string.IsNullOrEmpty(segment)?segment:"analyst-ratings") + "/analyst-color?page=" + page;
        HtmlWeb hw = new HtmlWeb();
        HtmlDocument ho = hw.Load(url_string);
        HtmlNodeCollection lis = ho.DocumentNode.SelectNodes("//li");

        if (lis != null)
        {
            foreach (HtmlNode li in lis)
            {
                DateTime date = new DateTime();
                double tgx = 0;
                int broker = 0;
                string broker_string = null;
                string analyst_string = null;
                int analyst = 0;
                string ticker = null;
                string url = null;
                string summary = null;
                List<string> extracted = new List<string>();

                HtmlNodeCollection spans = li.SelectNodes("span[@class]");

                if (spans != null)
                {
                    foreach (HtmlNode span in spans)
                    {
                        if (span.Attributes.Count >= 1)
                        {
                            if (span.Attributes.First().Value == "date")
                            {
                                try
                                {
                                    date = Convert.ToDateTime(span.InnerText);
                                }
                                catch { }

                                //Response.Write(date.Date);
                            }

                            if (span.Attributes.First().Value == "tags")
                            {
                                string[] tags = span.InnerText.Split(',');


                                foreach (string t in tags)
                                {
                                    if (!t.Contains("Analyst") && !t.Contains("Price") && !t.Contains("News"))
                                    {
                                        //Response.Write(t + ", ");
                                        extracted.Add(t);
                                    }
                                }
                            }

                            HtmlNode div = li.SelectSingleNode("div");
                            if (div != null)
                            {
                                HtmlNode p = div.SelectSingleNode("p");
                                if (p != null)
                                {
                                    summary = p.InnerText;
                                    //Response.Write(p.InnerText + "<br>");
                                }

                                HtmlNode span_readmore = div.SelectSingleNode("span[@class]");
                                if (span_readmore != null)
                                {
                                    if (span_readmore.Attributes.First().Value == "read-more")
                                    {
                                        //try{
                                        url = span_readmore.ChildNodes[0].Attributes[0].Value;
                                        //Response.Write(url + "<br>");
                                        //}
                                        //catch{}
                                    }
                                }

                            }
                        }
                    }
                }

                if (summary != null)
                {
                    string[] summary_array = summary.Split(' ');

                    for (var i = 1; i < summary_array.Length - 1; i++)
                    {
                        if (summary_array[i] == "to" && summary_array[i - 1].Contains("$") && summary_array[i + 1].Contains("$"))
                        {
                            try
                            {
                                tgx = Convert.ToDouble(summary_array[i + 1].Replace("$", "").Replace(".\n\nIn", ""));
                                //Response.Write(tgx);
                            }
                            catch { }
                        }

                        if (summary_array[i].Contains("price") && summary_array[i + 1].Contains("target") && summary_array[i - 1].Contains("$"))
                        {
                            try
                            {
                                tgx = Convert.ToDouble(summary_array[i - 1].Replace("$", "").Replace(".\n\nIn", ""));
                            }
                            catch { }
                        }

                        if (summary_array[i].Contains("of") && summary_array[i - 1].Contains("target") && summary_array[i + 1].Contains("$"))
                        {
                            try
                            {
                                tgx = Convert.ToDouble(summary_array[i + 1].Replace("$", "").Replace(".\n\nIn", ""));
                            }
                            catch { }
                        }

                        if (summary_array[i].Contains("$") && summary_array[i + 1].Contains("PT"))
                        {
                            try
                            {
                                tgx = Convert.ToDouble(summary_array[i].Replace("$", "").Replace(".\n\nIn", ""));
                            }
                            catch { }
                        }


                    }
                }

                if (extracted.Count > 0)
                {
                    foreach (var element in extracted)
                    {
                        if (element.Length <= 5) // ticker
                        {
                            var tickers = from temp in db.funds where temp.ticker == element.Trim() select temp;

                            if (tickers.Any())
                            {
                                ticker = element.Trim();
                                //Response.Write(element);
                            }
                        }
                        else
                        { // analyst/broker
                            var brokers = from temp in db.Bloomberg_Brokers where temp.name.ToLower().Contains(element.Trim().ToLower()) select temp;

                            if (brokers.Any())
                            {
                                //Response.Write(brokers.First().name);
                                broker = brokers.First().id;
                                broker_string = brokers.First().name;
                            }
                            else
                            {
                                var analysts = from temp in db.users where temp.bloomberg_broker.HasValue && temp.display_name.Contains(element.Trim()) select temp;

                                if (analysts.Any())
                                {
                                    //Response.Write(analysts.First().display_name);
                                    analyst = analysts.First().userID;
                                    analyst_string = analysts.First().display_name;
                                }
                            }
                        }
                    }
                }

                if (url != null && tgx > 0 && (broker > 0 || analyst > 0) && ticker != null)
                {
                    var action = from temp in db.Actions where temp.fund.ticker == ticker && (temp.article1.text == null || temp.article1.text.Trim() == "") && temp.targetValue == tgx && (temp.article1.origin == analyst || temp.article1.user.bloomberg_broker == broker) && temp.active select temp;

                    Response.Write(ticker + " " + tgx + " " + date + " " + broker_string + " " + analyst_string + " " + "<br>");

                    if (action.Any())
                    {
                        Response.Write("<font color=green>" + action.First().startDate + "</font><br>");
                        string summary_text = get_double_quotes_benzinga("http://www.benzinga.com" + url);
                        Response.Write("<font color=gray>" + summary_text + "</font><br>");
                        try
                        {
                            if (summary_text.Length > 0)
                            {
                                insert_summary_meat(action.First().actionID, summary_text, "http://www.benzinga.com" + url);
                                Response.Write("<font color=green>Success</font>");
                            }
                        }
                        catch
                        {
                            Response.Write("<font color=red>Error</font>");
                        }
                    }

                    Response.Write("<br><br>");

                }

            }
        }
    }

    protected void benzinga()
    {
        string url = "http://www.benzinga.com/analyst-ratings/price-target?page=2";
        HtmlWeb hw = new HtmlWeb();
        HtmlDocument ho = hw.Load(url);
        HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//span");


        //benzinga-articles benzinga-articles-mixed
        if (m != null)
        {
            foreach (HtmlNode z in m)
            {
                if (z.Attributes.Count >= 1)
                {
                    DateTime date = new DateTime();

                    if (z.Attributes.First().Value == "date")
                    {
                        date = Convert.ToDateTime(z.InnerText);
                    }


                    if (z.Attributes.First().Value == "tags")
                    {
                        string[] tags = z.InnerText.Split(',');
                        List<string> extracted = new List<string>();

                        foreach (string t in tags)
                        {
                            if (!t.Contains("Analyst") && !t.Contains("Price") && !t.Contains("News"))
                            {
                                Response.Write(t + ", ");
                                extracted.Add(t);
                            }
                        }

                        Response.Write("<br>");



                        //foreach (var thead in z.ChildNodes)
                        //{
                        //}
                    }
                }
            }
        }
    }


    protected void loop_through_tickers()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var tickers = from temp in db.Aggregate_Tickers orderby temp.analysts descending select temp;

        if (tickers.Any())
        {
            int c = 0;
            foreach (var t in tickers)
            {
                //if (c <= 20)
                //{
                    scrape_benzinga_article(t.fund.ticker.Trim());
                    streetinsider_control(t.fund.ticker.Trim());
                //}
                //c++;
            }
        }
    }


    protected void scrape_benzinga_article(string ticker)
    {

        string x = "http://www.benzinga.com/stock/" + ticker + "/ratings";

        HtmlWeb hw = new HtmlWeb();
        HtmlDocument ho = hw.Load(x);
        HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//table");

        if (m != null)
        {
            foreach (HtmlNode z in m)
            {
                if (z.Attributes.Count >= 1)
                {
                    if (z.Attributes.First().Value == "sortable stock-ratings-calendar sticky-enabled")
                    {
                        foreach (var thead in z.ChildNodes)
                        {

                            int count_tr = 0;
                            foreach (var tr in thead.ChildNodes)
                            {

                                if (count_tr > 0)
                                {
                                    int count_td = 0;
                                    DateTime date = new DateTime();
                                    string broker = null;
                                    double tgx = 0;
                                    string url = null;

                                    foreach (var td in tr.ChildNodes)
                                    {
                                        switch (count_td)
                                        {
                                            case 0:
                                                date = Convert.ToDateTime(td.InnerText);
                                                break;
                                            case 1:
                                                broker = td.InnerText.Trim();
                                                break;
                                            case 5:
                                                try { tgx = Convert.ToDouble(td.InnerText); }
                                                catch { }
                                                break;
                                            case 6:
                                                try
                                                {
                                                    url = td.ChildNodes[0].Attributes[0].Value;
                                                }
                                                catch { }
                                                break;
                                        }
                                        count_td++;
                                    }

                                    if (!string.IsNullOrEmpty(broker) && !string.IsNullOrEmpty(url))
                                    {
                                        DataClassesDataContext db = new DataClassesDataContext();
                                        try
                                        {
                                            var action = from temp in db.Actions where temp.fund.ticker == ticker && temp.targetValue == tgx && temp.active && (temp.article1.text == null || temp.article1.text.Trim() == "") && (temp.article1.user.Bloomberg_Broker1.name == broker || temp.article1.user.Bloomberg_Broker1.name.Contains(broker) || temp.article1.user.Bloomberg_Broker1.name.Contains(broker.Replace(" ", "")) || temp.article1.user.Bloomberg_Broker1.name.Contains(broker.Split(' ')[0])) select temp;
                                            if (action.Any())
                                            {
                                                string summary = get_double_quotes_benzinga(url);
                                                if (!string.IsNullOrEmpty(summary))
                                                {
                                                    insert_summary_meat(action.First().actionID, summary, url);
                                                    Response.Write(date + " " + broker + " " + tgx + " " + url + "<br>");
                                                    Response.Write("<font color=gray>" + summary + "</font><br>");
                                                    Response.Write("<font color=green>Date: " + action.First().startDate + "</font>");
                                                    Response.Write("<br><br>");
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        //Response.Write("<font color=red>URL/BROKER NULL for " + ticker + "</font><br>");
                                    }
                                }

                                count_tr++;
                            }
                        }
                    }
                }

            }
        }
        else
        {
            Response.Write("<font color=red>Nothing found for " + ticker + "</font><br>");
        }
    }

    protected string get_double_quotes_streetinsider(string url)
    {
        HtmlWeb hw = new HtmlWeb();
        HtmlDocument ho = hw.Load(url);
        HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//div");

        foreach (HtmlNode z in m)
        {
            if (z.Attributes.Count >= 1)
            {
                // parse out all content
                if (z.Attributes.First().Value == "article_body")
                {
                    foreach (var a in z.ChildNodes)
                    {
                        foreach (Match match in Regex.Matches(a.InnerText, "\"([^\"]*)\""))
                        {
                            if (match.ToString().Length > 50 && !match.ToString().Contains("ad.doubleclick"))
                            {
                                Response.Write(match.ToString().Trim().Substring(1, match.ToString().Length - 2) + " ");
                                return match.ToString().Trim().Substring(1, match.ToString().Length - 2);

                            }
                        }
                    }
                }
            }
        }

        return null;
    } // streetinsider.com

    protected string get_double_quotes_benzinga(string url)
    {
        HtmlWeb hw = new HtmlWeb();
        //HtmlDocument ho = hw.Load("http://www.benzinga.com/analyst-ratings/analyst-color/13/09/3922598/j-p-morgan-reiterates-neutral-on-microsoft-corporation-a");
        HtmlDocument ho = hw.Load(url);
        HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//div");

        foreach (HtmlNode z in m)
        {
            if (z.Attributes.Count >= 1)
            {
                if (z.Attributes.First().Value == "article-content-body")
                {
                    foreach (var a in z.ChildNodes)
                    {
                        foreach (Match match in Regex.Matches(a.InnerText, "“([^\"]*)”"))
                        {
                            //Response.Write(match.ToString().Trim().Replace("”", "").Replace("“", "") + " ");
                            return match.ToString().Trim().Replace("”", "").Replace("“", "");
                        }
                    }
                }
            }
        }

        return null;
    }

    // scrape rationales












    protected void calculate_percentage_with_rationales()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where temp.active select temp;

        if (actions.Any())
        {
            double all = actions.Count();
            double with = actions.Where(b => (b.article1.text == null || b.article1.text.Trim() == "") && b.article1.url != null).Count();

            Response.Write(all + " actions, " + with + " with rationale: " + Math.Round((100 * with / all), 0) + "%");
        }
    }

    protected void remove_extra_actives()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var tickers = (from temp in db.Actions where temp.active select temp).GroupBy(b=>b.ticker);
        if (tickers.Any())
        {
            foreach (var ticker in tickers)
            {
                remvoe_extra_active_by_analyst(ticker.First().fund.ticker);
            }
        }
    }

    protected void remvoe_extra_active_by_analyst(string ticker)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = (from temp in db.Actions where temp.fund.ticker == ticker && temp.active select temp).GroupBy(b => b.article1.origin).Where(b => b.Count()>1);
        if (actions.Any())
        {
            foreach (var action_group in actions)
            {
                int c = 0;
                foreach (var action in action_group.OrderByDescending(b => b.startDate))
                {
                    if (c > 0)
                    {
                        action.active = false;
                        db.SubmitChanges();
                    }

                    c++;
                }
            }
        }
    }

    protected void set_benchmark_values_for_action_monitors()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var ams = from temp in db.ActionMonitors select temp;
        if (ams.Any())
        {
            var values = from temp in db.fund_values where temp.fundID == Constants.sp500_index_fund_id select temp;

            foreach (var am in ams)
            {
                am.benchmark_beg = values.Where(b => b.date <= am.monitorStart).OrderByDescending(b => b.date).First().closeValue;
                am.benchmark_end = values.Where(b => b.date <= am.lastUpdated).OrderByDescending(b => b.date).First().closeValue;
                db.SubmitChanges();
            }
        }
    }

    protected void analyst_performance_first_coverage_date()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var aps = from temp in db.AnalystPerformances select temp;
        if (aps.Any())
        {
            foreach (var ap in aps)
            {
                if (ap.ticker.HasValue)
                {
                    var action = from temp in db.Actions where temp.article1.origin == ap.analyst && temp.ticker == ap.ticker orderby temp.startDate ascending select temp;
                    if (action.Any())
                    {
                        ap.first_action_date = action.First().startDate;
                        db.SubmitChanges();
                    }
                }
                else if (ap.sector.HasValue)
                {
                    var action = from temp in db.Actions where temp.article1.origin == ap.analyst && temp.fund.peer_group == ap.industry orderby temp.startDate ascending select temp;
                    if (action.Any())
                    {
                        ap.first_action_date = action.First().startDate;
                        db.SubmitChanges();
                    }
                }
                else if (ap.industry.HasValue)
                {
                    var action = from temp in db.Actions where temp.article1.origin == ap.analyst && temp.fund.sector == ap.sector orderby temp.startDate ascending select temp;
                    if (action.Any())
                    {
                        ap.first_action_date = action.First().startDate;
                        db.SubmitChanges();
                    }
                }
            }
        }
    }

    protected void error_by_ticker(string ticker,double threshold)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where temp.fund.ticker == ticker && temp.active && temp.targetValue >= threshold select temp;

        //foreach (var a in actions)
        //{
        //    db.Actions.DeleteOnSubmit(a);
        //    db.SubmitChanges();
        //}

        if (actions.Any())
        {
            foreach (var a in actions)
            {
                bool found = false;
                int current = a.actionID;
                int count = 0;

                while (!found)
                {
                    var previous = from temp in db.Actions where temp.Action_Next == current select temp;
                    
                    if (previous.Any())
                    {
                        int previous_id = previous.First().actionID;
                        var child = from temp in db.Actions where temp.actionID == current select temp;
                        

                        previous.First().Action_Next = null;
                        db.SubmitChanges();
                        db.Actions.DeleteOnSubmit(child.First());
                        db.SubmitChanges();

                        var previous_new = from temp in db.Actions where temp.actionID == previous_id select temp;

                        if (previous_new.First().targetValue < threshold)
                        {
                            previous_new.First().active = true;
                            found = true;
                        }

                        db.SubmitChanges();
                        current = previous_id;
                    }
                    else
                    {
                        found = true;
                    }
                }
            }
        }
    }

    protected void find_error()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where temp.ticker == 684 && temp.article1.origin == 1730167 select temp;
        if (actions.Any())
        {
            foreach (var a in actions.OrderByDescending(b => b.targetDate))
            {
                Response.Write(a.targetValue + ", " + a.startDate + "<br>");
            }
            //
        }

    }

    protected void get_erroneous_actions()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where temp.active && temp.Action_Previous.HasValue select temp;

        if (actions.Any())
        {
            foreach (var a in actions)
            {
                var action_previous = from temp in db.Actions where temp.actionID == a.Action_Previous select temp;

                if (action_previous.Any())
                {
                    if (action_previous.First().targetValue > a.targetValue * 2 || action_previous.First().targetValue < 0.5)
                    {
                        Response.Write(a.fund.ticker + ", " + a.article1.user.display_name + ", " + a.actionID + "<br>");
                    }
                }
                
            }
        }

    }

    protected void user_table_details_initiate()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var ams_by_user = (from temp in db.ActionMonitors where temp.active select temp).GroupBy(b=>b.usermon);

        if (ams_by_user.Any())
        {
            foreach (var ams in ams_by_user)
            {
                portfolio_header portfolio = new portfolio_header();
                portfolio = DataBaseLayer.portfolio_stats_guts(ams.First().usermon);
                ams.First().user.positions = portfolio.positions;
                ams.First().user.sectors = portfolio.sectors;
                ams.First().user.dividend = portfolio.dividend;
                ams.First().user.stocks = portfolio.profit + portfolio.invested;
                ams.First().user.stocks_invested = portfolio.invested;
                ams.First().user.stocks_delta = 0;
                ams.First().user.cash_delta = 0;
                ams.First().user.profit_day = 0;
                ams.First().user.dividend_delta = 0;
                ams.First().user.profit = portfolio.profit;
                ams.First().user.upside_target = portfolio.upside / ams.First().user.stocks;
                ams.First().user.date_updated = DateTime.Now;
                db.SubmitChanges();
            }
        }
    }

    protected void scrape_financials(string ticker)
    {
        string url_string = "http://financials.morningstar.com/valuation/valuation-history.action?&t=" + ticker + "&type=price-sales";
        HtmlWeb hw = new HtmlWeb();
        try
        {
            HtmlDocument ho = hw.Load(url_string);
            HtmlNodeCollection trs = ho.DocumentNode.SelectNodes("//tr");

            List<double> pe = new List<double>();
            List<double> ps = new List<double>();
            List<double> pcf = new List<double>();
            List<double> pb = new List<double>();

            if (trs != null)
            {
                int tr_count = 0;
                foreach (HtmlNode tr in trs)
                {
                    HtmlNodeCollection tds = tr.SelectNodes("td");

                    int td_count = 2003;
                    if (tds != null)
                    {
                        foreach (HtmlNode td in tds)
                        {
                            if (td_count <= 2012)
                            {
                                switch (tr_count)
                                {
                                    case 1:
                                        try
                                        {
                                            ps.Add(Convert.ToDouble(td.InnerText));
                                        }
                                        catch
                                        {
                                            ps.Add(0);
                                        }
                                        break;
                                    case 4:
                                        try
                                        {
                                            pcf.Add(Convert.ToDouble(td.InnerText));
                                        }
                                        catch
                                        {
                                            pcf.Add(0);
                                        }
                                        break;
                                    case 7:
                                        try
                                        {
                                            pe.Add(Convert.ToDouble(td.InnerText));
                                        }
                                        catch
                                        {
                                            pe.Add(0);
                                        }
                                        break;
                                    case 10:
                                        try
                                        {
                                            pb.Add(Convert.ToDouble(td.InnerText));
                                        }
                                        catch
                                        {
                                            pb.Add(0);
                                        }

                                        break;
                                }
                            }


                            td_count++;
                        }
                    }

                    Response.Write("<br><br><br>");
                    tr_count++;
                }
            }

            DataClassesDataContext db = new DataClassesDataContext();
            int year = 2003;
            int c = 0;
            foreach (var l in pe)
            {
                Financial f = new Financial();
                f.year = year;
                f.ticker = (from temp in db.funds where temp.ticker == ticker.Trim() select temp.fundID).First();
                f.pe = pe[c];
                f.pb = pb[c];
                f.pcf = pcf[c];
                f.ps = ps[c];

                f.dividend = 0;
                f.market_cap = 0;

                try
                {
                    db.Financials.InsertOnSubmit(f);
                    db.SubmitChanges();
                }
                catch { }

                year++;
                c++;
            }
        }
        catch
        {
            
        }
        
        

    }

    protected void gephi_broker_company() {
        DataClassesDataContext db = new DataClassesDataContext();
        var aps = (from temp in db.AnalystPerformances where temp.user.bloomberg_broker.HasValue select temp).GroupBy(b=>b.user.bloomberg_broker);

        List<Tuple<int, string>> company = new List<Tuple<int, string>>();
        List<Tuple<int, string>> broker = new List<Tuple<int, string>>();
        List<Tuple<int, int>> edges = new List<Tuple<int, int>>();

        foreach (var ap in aps.Where(b=>b.First().user.bloomberg_broker.HasValue)) {
            foreach (var a in ap)
            {
                if (a.user.bloomberg_broker.Value>0)
                {
                    edges.Add(new Tuple<int, int>(a.ticker.Value, a.user.bloomberg_broker.Value));

                    if (!company.Where(b => b.Item1 == a.ticker.Value).Any())
                        company.Add(new Tuple<int, string>(a.ticker.Value, a.fund.name));

                    if (!broker.Where(b => b.Item1 == a.user.bloomberg_broker.Value).Any())
                        broker.Add(new Tuple<int, string>(a.user.bloomberg_broker.Value, a.user.Bloomberg_Broker1.name));
                }
            }
        }

        foreach (var e in edges) {
            Response.Write(e.Item1.ToString() + "," + e.Item2.ToString() + "<br>");
        }

        Response.Write("<br><br><br>Nodes<br><br><br>");

        foreach (var b in broker)
        {
            Response.Write(b.Item1.ToString() + "," + b.Item2.ToString() + "<br>");
        }

        foreach (var b in company)
        {
            Response.Write(b.Item1.ToString() + "," + b.Item2.ToString() + "<br>");
        }

    }

    protected void Add_Exchange()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var funds = from temp in db.funds where temp.ticker.Trim() == "ANX" select temp;
        foreach (var fund in funds)
        {
            string exchange = YahooStockEngine.FetchExchange(fund.ticker.Trim());
            fund.exchange = exchange;
            db.SubmitChanges();
        }
    }
















    protected void fix_sector_industry_extended() {
        DataClassesDataContext db = new DataClassesDataContext();
        var funds = from temp in db.funds where !temp.sector.HasValue || !temp.peer_group.HasValue select temp;

        if (funds.Any()) {
            foreach (var f in funds) {
                int sector = 0;
                int industry = 0;

                try
                {
                    sector = DataBaseLayer.get_sector(db, f.ticker.Trim());
                }
                catch {
                    Response.Write(f.ticker + " <-- sector<br>");
                }

                try
                {
                    industry = DataBaseLayer.get_peer_group(f.ticker.Trim());
                }
                catch {
                    Response.Write(f.ticker + " <-- industry<br>");
                }

                if (sector >0 || industry>0)
                {
                    if (sector > 0)
                        f.sector = sector;

                    if (industry > 0)
                        f.peer_group = industry;

                    db.SubmitChanges();
                }

            }
        }

    }

    protected void fix_sector_industry() {
        DataClassesDataContext db = new DataClassesDataContext();
        var reader = new StreamReader(File.OpenRead(Server.MapPath("../files/fix_sector_industry.csv")));

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(',');


            var ticker = from temp in db.funds where temp.ticker == values[0].Trim() select temp;

            if (ticker.Any()) {
                try
                {
                    ticker.First().sector = (from temp in db.Sectors where temp.sector1 == values[1].Trim() select temp).First().id;
                    ticker.First().peer_group = (from temp in db.Peer_Groups where temp.name == values[2].Trim().Replace("; ",", ") select temp).First().id;
                    db.SubmitChanges();
                }
                catch {
                    Response.Write(values[0] + " " + values[1] + " " + values[2] + "<br>");
                }
            }

        }
    }

    protected void get_all_descriptions() {
        DataClassesDataContext db = new DataClassesDataContext();
        var aas = from temp in db.Aggregate_Tickers where temp.fund.description.Length<10 orderby temp.analysts descending select temp;

        foreach (var a in aas) {
            a.fund.description = company_description(a.fund.ticker.Trim());
            db.SubmitChanges();
        }
    }

    public static string company_description(string ticker) {
        //string x = string.Empty;
        //if (!string.IsNullOrEmpty(Request.QueryString["ticker"]))
        //{
        //    x = Request.QueryString["ticker"];
        //}
        //else {
        //    x = ticker;
        //}

        HtmlWeb hw = new HtmlWeb();
        HtmlDocument ho = hw.Load("http://www.bloomberg.com/quote/" + ticker + ":US/profile");
        HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//p");

        foreach (HtmlNode z in m)
        {
            if (z.Attributes.Count >= 1)
            {
                // parse out all content
                if (z.Attributes.First().Value == "extended_profile")
                {
                    foreach (var a in z.ChildNodes)
                    {
                        return a.InnerText;
                    }
                }
            }
        }

        return null;
    }

    protected void insert_summary(object sender, EventArgs e) {
        //string status = insert_summary_meat(Convert.ToInt32(action.Text), summary.Text,null);
        //message.Text = status;
    }

    protected string insert_summary_meat(int actionid,string summary,string url) {
        DataClassesDataContext db = new DataClassesDataContext();

        try
        {
            var actions = from temp in db.Actions where temp.actionID == actionid select temp;

            if (actions.Any())
            {
                actions.First().article1.summary = summary.Length > 400 ? summary.Substring(0, 400) : summary;
                actions.First().article1.text = summary;
                actions.First().rational = summary;

                if (!string.IsNullOrEmpty(url))
                    actions.First().article1.url = url;
                db.SubmitChanges();

                try
                {
                    Ancillary.send_email("info@invesd.com", "Invesd Rationale", "arziai@gmail.com", "Amir", "Rationale for " + actions.First().fund.ticker + " by " + actions.First().user.Bloomberg_Broker1.name + ", " + actions.First().user.display_name,"<b>" + string.Format("{0:C2}",actions.First().targetValue) + "<br>" + string.Format("{0:MMM d,yy}",actions.First().startDate) + "</b><br>" + summary, false);
                }
                catch { }
                
                return "Done";
            }
            else
            {
                return "Not found";
            }
        }
        catch (Exception ex)
        {
            return "Error" + ex.InnerException + ex.StackTrace;
        }
    }

    protected void stupid_nlp_v2()
    {
        HtmlWeb hw = new HtmlWeb();
        HtmlDocument ho = hw.Load("http://www.streetinsider.com/Analyst+Comments/Google+%28GOOG%29+Target+Raised+to+%241220+at+Deutsche+Bank/8789319.html");
        HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//div");

        foreach (HtmlNode z in m)
        {
            if (z.Attributes.Count >= 1)
            {
                // parse out all content
                if (z.Attributes.First().Value == "article_body")
                {
                    foreach (var a in z.ChildNodes)
                    {
                        if (a.InnerText.Contains("\"") && !a.InnerText.Contains("<script"))
                        {
                            Response.Write(a.InnerText);
                            Response.Write("<br><br>");
                        }
                    }
                }
            }
        }
    }


    protected void text_only() { 
        // solution 1
        //XmlDocument document = new XmlDocument();
        //document.Load("http://www.streetinsider.com/Analyst+PT+Change/Goldman+Sachs+Upgrades+Peabody+Energy+%28BTU%29+to+Buy/8862272.html");
        //string allText = document.InnerText;
        //Response.Write(allText);

        // solution 2

    }

    protected void delete_screwed_up_dates() {
        DataClassesDataContext db = new DataClassesDataContext();
        var xs = from temp in db.fund_values where temp.isLatest.Value && temp.date.Year == 0001  select temp;
        //var x = from temp in db.fund_values where temp.isLatest.Value && temp.date == DateTime.Now.AddDays(-1) select temp;

        if (xs.Any()) {
            foreach (var x in xs)
            {
                db.fund_values.DeleteOnSubmit(x);
                db.SubmitChanges();
                //var screwed_up = from temp in db.fund_values where temp.fundID == xx.fundID && temp.isLatest.Value && temp.date.Year == 0001 select temp;
                //Response.Write(x.Count());

            }
        }
    }
    protected void delete_duplicate_values()
    {
        DataClassesDataContext db = new DataClassesDataContext();
       // var fs = from temp in db.funds select temp;
        var fs = (from temp in db.popularTickers select temp).OrderByDescending(q => q.count);
        //Parallel.ForEach(fs, new ParallelOptions { MaxDegreeOfParallelism = 7 }, f =>        
        foreach (var f in fs)
        {
            var vals = (from temp2 in db.fund_values where temp2.fundID == f.fundID select temp2).GroupBy(aa => aa.date);
            foreach (var v in vals)
            //Parallel.ForEach(vals, new ParallelOptions { MaxDegreeOfParallelism = 2 }, v =>
            {
                if (v.Count() > 1)
                {
                    db.fund_values.DeleteOnSubmit(v.Last());
                    db.SubmitChanges();
                    //db.SubmitChanges();
                }
            }
        }


    }

    protected void find_no_is_latest() {
        DataClassesDataContext db = new DataClassesDataContext();
        //var no_islatest = (from temp in db.fund_values  select temp ).GroupBy(b => b.fundID);

         var fs = (from temp in db.funds select temp);
        
        foreach (var f in fs)
        {
            var vals = from temp2 in db.fund_values where temp2.fundID == f.fundID select temp2;
           
                    try
                    {
                        if (!vals.OrderByDescending(b => b.date).First().isLatest.GetValueOrDefault(false))
                        {
                            vals.OrderByDescending(b => b.date).First().isLatest = true;
                            db.SubmitChanges();
                        }
                    }
                    catch
                    {
                    }
          
        }

    }

    protected void test() {
        DataClassesDataContext db = new DataClassesDataContext();
        var x = from temp in db.Actions where (DateTime.Now - temp.startDate).TotalDays > 380 && temp.active select temp;

        if (x.Any()) {
            foreach (var y in x) {
                y.active = false;
                db.SubmitChanges();
            }
        }
    }

    protected void find_the_problem() {
        DataClassesDataContext db = new DataClassesDataContext();
        var find = from temp in db.Actions where temp.creationTime >= DateTime.Now.AddDays(-180) select temp;

        if (find.Any()) {
            foreach (var a in find.GroupBy(b=>b.ticker)){
                foreach (var b in a.GroupBy(b=>b.article1.origin)){
                    var last = b.Skip(b.Count()-1).First();

                    if (!last.active && !last.matured && !last.expired && !last.breached && last.Action_Next.HasValue){
                        bool success = false;
                        try
                        {
                            last.active = true;
                            last.Action_Next = null;
                            db.SubmitChanges();
                        }
                        catch { }

                        Response.Write(last.fund.ticker + " - " + last.article1.origin + (success?"":" <font style=\"color:red\">FAILED</font>")  + "<br>");
                    }
                }
            }
        }

    }

    protected void get_google_names() {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = (from temp in db.Actions where temp.active && (temp.fund.name.Trim()=="" || temp.fund.name == string.Empty)  select temp).GroupBy(b => b.ticker);

        if (actions.Any()) { 
            foreach (var a in actions) {
                a.First().fund.name = DataBaseLayer.google_finance_name(a.First().fund.ticker.Trim());
                db.SubmitChanges();
            }
        }
    }

    protected void stupid_nlp() {
        HtmlWeb hw = new HtmlWeb();
        HtmlDocument ho = hw.Load("http://www.streetinsider.com/Analyst+PT+Change");
        HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//div");

        foreach (HtmlNode z in m)
        {
            if (z.Attributes.Count >= 1)
            {
                // parse out all content
                if (z.Attributes.First().Value == "news_article")
                {
                    foreach (var x in z.ChildNodes) {
                        if (x.Attributes.Count() > 0)
                        {
                            if (x.Attributes.First().Value == "news_title")
                            {
                                Response.Write("Title ");
                                string title = x.InnerHtml;
                                //Response.Write(x.InnerHtml);
                                //Response.Write("<br>");       
                            }
                        }
                        else {
                            if (x.InnerHtml.Trim().Length > 10) {
                                string body = x.InnerHtml;
                                //Response.Write("Body " + x.InnerHtml);
                                //Response.Write("<br>");       
                            }
                        }
                        
                    }
                }
            }



        }

    }


    protected void ticker_broker() {
        DataClassesDataContext db = new DataClassesDataContext();
        var all = (from temp in db.Actions where temp.active select temp).GroupBy(b=>b.ticker).OrderByDescending(b=>b.Count());

        if (all.Any()) {
            // for each ticker
            Response.Write("<table>");
            foreach (var a in all) {
                string ticker = a.First().fund.ticker.Trim().ToUpper() + " US Equity";

                foreach (var action in a) {
                    if (action.article1.user.bloomberg_broker.HasValue)
                    {
                        Response.Write("<tr><td>" + ticker + "</td><td>" + action.article1.user.Bloomberg_Broker1.broker_code.Trim().ToUpper() + "</td></tr>");
                    }
                }
            }
            Response.Write("</table>");
        }

    }

    //protected void ticker_similar_companies(object sender, EventArgs e) {
    //    get_similar_companies(ticker.Text);
    //}

    //protected void get_similar_companies(string ticker) {
    //    DataClassesDataContext db = new DataClassesDataContext();

    //    var find = from temp in db.funds where temp.ticker == ticker.Trim() select temp;

    //    if (find.Any()){
    //        var ts = from temp in db.Aggregate_Tickers where temp.fund.peer_group == find.First().peer_group && temp.ticker != find.First().fundID select temp;

    //        if (ts.Any()) {
    //            tickers.Text = "";
    //            ts = ts.OrderByDescending(b=>b.analysts).Take(3);
    //            foreach (var t in ts) {
    //                tickers.Text += t.fund.ticker + "- ";
    //            }
    //        }
    //    }
    //}

    protected void honest_stats() {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where temp.active select temp;

        if (actions.Any()) {
            Response.Write("Companies\t\t\t" + actions.GroupBy(b => b.ticker).Count() + "<br>");
            Response.Write("Analysts\t\t\t" + actions.GroupBy(b => b.article1.origin).Count() + "<br>");
            Response.Write("Views\t\t\t" + actions.Count() + "<br>");
            Response.Write("Industries\t\t\t" + actions.GroupBy(b=>b.fund.peer_group).Count() + "<br>");
        }

    }

    protected void kill_expired_matures() {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where temp.matured && temp.active select temp;

        foreach (var a in actions) {
            if ((DateTime.Now - a.startDate).TotalDays > 365)
            {
                a.active = false;
                db.SubmitChanges();
            }
        }
    }

    protected void kill_useless_actives() {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = (from temp in db.Actions where temp.active select temp).GroupBy(b=>b.ticker);

        if (actions.Any()) {
            foreach (var az in actions) {
                foreach (var a in az.GroupBy(b => b.article1.origin))
                {
                    if (a.Count() > 1) {
                        int c = 0;
                        foreach (var z in a.OrderByDescending(b => b.startDate)) {
                            if (c > 0)
                            {
                                z.active = false;
                                db.SubmitChanges();
                            }

                            c++;
                        }
                    }
                }
            }
        }
        
    }

    protected void reset_other_end(bool parallel) {
        DataClassesDataContext dbTop = new DataClassesDataContext();
        var actions = (from temp in dbTop.Actions where temp.lastUpdated != temp.startDate select temp).GroupBy(b=>b.ticker);

        if (actions.Any()) {
            if (parallel)
            {
                Parallel.ForEach(actions, new ParallelOptions { MaxDegreeOfParallelism = 4 }, a =>
                {
                    DataClassesDataContext db = new DataClassesDataContext();
                    var actions_in = from temp in db.Actions where temp.lastUpdated != temp.startDate && temp.ticker == a.First().ticker select temp;

                    foreach (var a_in in actions_in) {
                        AdminBackend.reset_action(a_in, db);
                    }
                });
            }
            else {
                foreach (var a in actions) {
                    //
                }
            }
            
        }
    }

    protected void kill_all_seeking_alpha() {
        DataClassesDataContext db = new DataClassesDataContext();
        var sas = from temp in db.Actions where temp.article1.url.Contains("seeking") select temp;

        if (sas.Any()) {
            foreach (var sa in sas) {
                Response.Write(sa.actionID);
            }
        }
    }

    //protected void find() {
    //    DataClassesDataContext db = new DataClassesDataContext();
    //    var actions = from temp in db.Actions where temp.article1.origin == 32842 && temp.ticker == 793 select temp;

    //    if (actions.Any()) {
    //        foreach (var a in actions) {
    //            Response.Write(a.actionID + "<br>");
    //            }
    //    }

    //}

    protected void walkthrough(bool parallel) {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = (from temp in db.Actions where !temp.matured && !temp.expired select temp).GroupBy(b => b.ticker);

        if (parallel)
        {
            Parallel.ForEach(actions, new ParallelOptions { MaxDegreeOfParallelism = 10 }, a =>
            {
                AdminBackend.update_actions_v2(a.First().ticker,false);
            });
        }
        else {
            foreach (var a in actions) {
                AdminBackend.update_actions_v2(a.First().ticker,false);
            }
        }
    }

    protected void last_ticker() {
        DataClassesDataContext db = new DataClassesDataContext();
        //var a = (from temp in db.Actions where temp.creationTime >= DateTime.Today.AddDays(-2) orderby temp.fund.ticker descending select temp).Take(1);
        var actions = (from temp in db.Actions where temp.creationTime >= DateTime.Now.AddDays(-2) orderby temp.actionID select temp).GroupBy(b => b.ticker);

        if (actions.Any()) {
            foreach (var a in actions)
            {
                Response.Write(a.First().fund.ticker + "<br>");
            }
        }

    }

    protected void create_confidence_metric() {
        DataClassesDataContext db = new DataClassesDataContext();
        var ap_tickers = from temp in db.AnalystPerformances where temp.ticker.HasValue select temp;

        double accuracy = 0;
        double return_abs = 0;
        double return_rel = 0;
        double loss = 0;

        if (ap_tickers.Any()) {
            accuracy = 0; return_rel = 0; return_abs = 0; loss = 0;

            foreach (var ap in ap_tickers) {
                accuracy = ap.accuracy_average > 0 ? ap.accuracy_average : 0;
                return_abs = ap.return_average >= 0.1 ? 1 : ap.return_average <= 0 ? 0 : ap.return_average * 10;
                loss = 1 - ap.actions_be_and_negative / ap.actions;
                // for simplicity ticker is substituted for industry
                double analysts = (from temp in db.AnalystPerformances where temp.ticker == ap.ticker select temp.id).Count();

                if (analysts > 1)
                {
                    return_rel = 1 - ((ap.rank - 1) / (analysts-1));
                }
                else {
                    return_rel = 1;
                }

                ap.confidence = (1-Math.Exp(-ap.actions/3)) *( 0.25 * (accuracy + return_abs + loss + return_rel));
                db.SubmitChanges();
            }
        }

    }

    protected void kill_seeking_alpha() {
        DataClassesDataContext db = new DataClassesDataContext();
        var sa = from temp in db.Actions where !temp.article1.user.bloomberg_broker.HasValue && temp.targetValue==47.67 select temp;

        if (sa.Any()) {
            foreach (var a in sa) {
                a.article1.action = false;
                db.SubmitChanges();
                db.Actions.DeleteOnSubmit(a);
                db.SubmitChanges();
            }
        }

    }

    protected void find_splits() {
        DataClassesDataContext db = new DataClassesDataContext();
        DateTime d = new DateTime(2012,8,1);
        var fs = (from temp in db.fund_values where temp.split!=1 && temp.date>=d select temp).GroupBy(b=>b.fundID);

        foreach (var f in fs) {
            var aas = from temp in db.Actions where temp.ticker == f.First().fundID && temp.matured && temp.active select temp;

            if (aas.Any()) {
                foreach (var aa in aas) {
                    var fvs = from temp in db.fund_values where temp.fundID == aa.ticker && temp.date >= aa.startDate && temp.split != 1 select temp;

                    if (fvs.Any()) { 
                        Response.Write(aa.fund.ticker + " - " + aa.targetValue + " - " + fvs.First().split + "<br>");
                    }
                }
            }
        }
    }

    protected void trytofix2() {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where (!temp.matured || temp.dividend != 0) && temp.article1.user.bloomberg_broker.HasValue select temp;

        if (actions.Any()) {
            foreach (var a in actions) {
                string[] t = a.article1.title.Trim().Split(' ');
                string x = t.Where(b => b.Contains("$")).First();
                try
                {
                    double target = Convert.ToDouble(x.Replace("$", "").Replace(",", ""));
                    a.targetValue = target;
                    db.SubmitChanges();
                }
                catch { }
            }
        }

    }

    protected void fund_pks() {
        DataClassesDataContext db = new DataClassesDataContext();
        var fs = from temp in db.funds where temp.ticker.Contains(".PK") select temp ;

        if (fs.Any()) {
            foreach (var f in fs) {
                f.ticker = f.ticker.Replace(".PK", "");
                db.SubmitChanges();
            }
        }
    }

    protected void try_to_fix() {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where temp.matured && temp.dividend==0 select temp;

        if (actions.Any()) {
            foreach (var a in actions) {
                a.targetValue = a.currentValue;
                db.SubmitChanges();
            }
        }
    }

    protected void fix_splitted_matured() {
        //int split = 3;
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where temp.active && temp.matured && temp.ticker == 1482 select temp;

        if (actions.Any()) {
            foreach (var action in actions) {
                action.currentValue = action.currentValue / 3;
                action.targetValue = action.currentValue;
                action.startValue = (from temp in db.fund_values where temp.fundID == 1482 && temp.date == action.startDate select temp.adjValue).First();
                action.maxValue = action.currentValue;
                db.SubmitChanges();
            }
        }
    }

    protected void complete_actions() {

        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where temp.active select temp;
        //var a = from temp in db.Actions where temp.AnalystPerformance.actions
        
        if (actions.Any()) {
            foreach (var a in actions) {
                var ap = from temp in db.AnalystPerformances where temp.analyst == a.article1.origin && temp.ticker == a.ticker select temp;
                var last = from temp in db.fund_values where temp.fundID == a.ticker && temp.isLatest.Value select temp;

                if (last.Any()) {
                    if (a.targetValue > last.First().closeValue)
                    {
                        a.buysell = true;
                    }
                    else if (a.targetValue < last.First().closeValue) {
                        a.buysell = false;
                    }
                    
                }

                if (ap.Any()) {
                    a.analyst_performance = ap.First().id;

                }

                db.SubmitChanges();
            }
        }
    }

    protected void bloomberg_tickers() {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions select temp;

        foreach (var a in actions.Where(b => b.article1.user.bloomberg_broker.HasValue).GroupBy(b => b.ticker)) {
            Response.Write(a.First().fund.ticker + "<br>");
        }

        //var funds = from temp in db.funds where 
    }

    protected void add_ticker_articles() {
        DataClassesDataContext db = new DataClassesDataContext();

        //var articles = from temp in db.articles where temp.user.bloomberg_broker.HasValue select temp;

        //if (articles.Any()) {
        //    foreach (var a in articles) {
        //        a.premium = true;
        //        db.SubmitChanges();
        //    }
        //}

        var actions = from temp in db.Actions where temp.article1.user.bloomberg_broker.HasValue select temp;

        if (actions.Any()) {
            foreach (var a in actions) {
                ArticleTicker at = new ArticleTicker();
                at.actions = 1;
                at.article = a.article;
                at.ticker = a.ticker;
                db.ArticleTickers.InsertOnSubmit(at);
                db.SubmitChanges();
            }
        }
    }

    protected void rename_funds() {
        DataClassesDataContext db = new DataClassesDataContext();
        string x = " Ltd";
        var fs = from temp in db.funds where temp.name.Contains(x) select temp;

        if (fs.Any()) {
            foreach (var f in fs) {
                f.name = f.name.Replace(x, string.Empty).Trim();
                try
                {
                    db.SubmitChanges();
                }
                catch
                {

                }
            }
        }
    }

    protected void add_data_to_metrics_ticker() {
        DataClassesDataContext db = new DataClassesDataContext();
        var mt = from temp in db.Metrics_Tickers orderby temp.average descending select temp;
        int c = 1;

        foreach (var m in mt) {
            var aps = from temp in db.AnalystPerformances where temp.ticker == m.ticker && temp.rank == 1 select temp;
            
            if (aps.Any())
            {
                m.top_analyst = aps.First().id;
            }
                var actions = from temp in db.Actions where temp.article1.origin == aps.First().analyst && temp.ticker == m.ticker && temp.active select temp;
                if (actions.Any())
                {
                    double price = (from temp in db.fund_values where temp.isLatest.Value && temp.fundID == m.ticker select temp.closeValue.Value).First();
                    m.top_analyst_view = price < actions.First().targetValue;
                }

                var ms = from temp in db.Metrics_Sectors where temp.sector == m.fund.sector select temp;
                if (ms.Any()) {
                    m.metrics_sector = ms.First().id;
                    m.trophy_sector = m.average > ms.First().average;
                }

                var mi = from temp in db.Metrics_Industries where temp.industry == m.fund.peer_group select temp;
                if (mi.Any()) {
                    m.metrics_industry = mi.First().id;
                    m.trophy_industry = m.average > mi.First().average;
                }

                if (c <= 30 && m.average>.2)
                    m.trophy_ticker = true;
                else
                    m.trophy_ticker = false;

                db.SubmitChanges();
                c++;
            }
        }

    protected void sort_unsorted_ranks()
        {
            DataClassesDataContext db = new DataClassesDataContext();

            var ap = (from temp in db.AnalystPerformances where temp.rank == 0 select temp).GroupBy(b => b.ticker);
            int c;

            if (ap.Any())
            {
                foreach (var b in ap)
                {
                    c = 1;
                    foreach (var a in b.OrderByDescending(q => q.return_annualized_sum))
                    {
                        a.rank = c;
                        db.SubmitChanges();
                        c++;
                    }
                }
            }
        }

    protected void fix_return_overall()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where (temp.matured || temp.expired || temp.breached) && temp.@short select temp;

        foreach (var a in actions)
        {
            double div = a.dividend;
            double num = a.@short ? -1 : 1;
            num *= (a.currentValue + div - a.startValue);
            double denom_negative = Math.Abs(a.startValue - a.lowerValue);
            double denom_positive = Math.Abs(a.targetValue - a.startValue);
            a.return_overall = num / a.startValue;
            db.SubmitChanges();
        }
    }

    protected void correct_analyst_names() {
        DataClassesDataContext db = new DataClassesDataContext();

        var u = from temp in db.users where temp.display_name.Contains("  ") select temp;

        foreach (var v in u) {
            v.display_name = v.display_name.Replace("  ", " ");
            v.seekingalpha_name = v.display_name;
            db.SubmitChanges();
        }
    }

    protected void insert_all() { 
        var lines = File.ReadLines(Server.MapPath("~") + "\\files\\all3.csv").Select(b => b.Split(','));
        DataClassesDataContext db = new DataClassesDataContext();
        bool updated=false;
        int ticker_previous=0;
        int broker_previous = 0;
        double previous=0;
        string skip_ticker = "";

        foreach (var a in lines)
        {
            string ticker = a.ElementAt(0);string broker = a.ElementAt(1);DateTime date = Convert.ToDateTime(a.ElementAt(2));Double tgx = Convert.ToDouble(a.ElementAt(3));

            if (skip_ticker != ticker)
            {
                try
                {
                    var t = from temp in db.funds where temp.ticker == ticker select temp; var b = from temp in db.Bloomberg_Brokers where temp.broker_code == broker select temp;

                    if (b.Any() && t.Any())
                    {
                        string company = t.First().name.Trim();
                        if (t.First().fundID == ticker_previous && b.First().id == broker_previous)
                            updated = true;

                        var liaison = from temp in db.Liaisons where temp.broker == b.First().id && temp.ticker == t.First().fundID select temp;

                        if (liaison.Any())
                        {
                            int userid = liaison.First().analyst;
                            var analyst = from temp in db.users where temp.userID == liaison.First().analyst select temp;

                            if (analyst.Any())
                            {

                                //var action_exists = from temp in db.Actions where temp.article1.origin == analyst.First().userID && temp.ticker == t.First().fundID && temp.startDate == date && temp.targetValue == tgx select temp;

                                //if (!action_exists.Any())
                                //{
                                // set and insert article
                                article art = new article();
                                art.title = analyst.First().display_name + (!updated ? " initiated a " + string.Format("{0:c2}", tgx) + " price target for " + company : " revised " + company + "'s price target to " + string.Format("{0:c2}", tgx));
                                art.summary = analyst.First().Bloomberg_Broker1.name + " analyst " + analyst.First().display_name + (!updated ? " initiated a " + string.Format("{0:c2}", tgx) + " price target for " + company + " (" + ticker + ")." : " revised " + company + "'s (" + ticker + ") price target to " + string.Format("{0:c2}", tgx) + " from " + string.Format("{0:c2}", previous) + ".");
                                art.text = art.summary;
                                art.date = date;
                                art.origin = analyst.First().userID;
                                art.rateSum = 0; art.rateCount = 0; art.provider = 1410832; art.type = 3; art.action = false; art.deleted = false; art.is_ticket = false; art.price = 0; art.not_actionable = false; art.premium = false; art.Publish = true; art.actions = 0; art.backdated = true;
                                db.articles.InsertOnSubmit(art);
                                db.SubmitChanges();
                                int articleid = art.idarticle;

                                // action
                                Action action = new Action();
                                DateTime startdate = date;
                                var start = (from temp in db.fund_values where temp.fundID == t.First().fundID && temp.date <= startdate select temp).OrderByDescending(q => q.date).First();
                                DateTime startDateActual = start.date;
                                Double currentValue = start.closeValue.Value;
                                action.creator = 2;
                                action.article = articleid;
                                action.targetDate = startDateActual.AddDays(365);
                                action.targetValue = tgx;
                                action.currentValue = currentValue;
                                action.startDate = startDateActual;
                                action.lastUpdated = startDateActual;
                                action.startValue = currentValue;
                                action.matured = false; action.breached = false; action.expired = false;
                                action.maxValue = currentValue; action.minValue = currentValue;
                                action.actualStartDate = startDateActual;
                                action.dividend = 0;
                                action.TotalReturn = true;
                                action.days_gain = 0;
                                action.beta = 0;
                                action.premium = false;
                                action.active = true;
                                action.is_ticket = false;
                                action.ticker = t.First().fundID;
                                action.deleted = false;
                                action.skin_in_the_game = false;
                                action.price = 0;
                                action.date_feed = startdate;
                                action.return_overall = 0;
                                action.progress = 0;

                                if (action.targetValue >= currentValue)
                                {
                                    action.lowerValue = 0;
                                    action.@short = false;
                                }
                                else
                                {
                                    action.lowerValue = 2 * currentValue;
                                    action.@short = true;
                                }

                                action.creationTime = DateTime.Now;
                                db.Actions.InsertOnSubmit(action);
                                db.SubmitChanges();

                                // need to revise the previous action first

                                if (updated)
                                {
                                    var past_action = (from temp in db.Actions where temp.actionID != action.actionID && temp.ticker == t.First().fundID && temp.article1.origin == analyst.First().userID select temp).OrderByDescending(q => q.targetDate);
                                    if (past_action.Any())
                                    {
                                        var pa = past_action.First();
                                        pa.active = false;
                                        pa.Action_Next = action.actionID;
                                        action.Action_Previous = pa.actionID;
                                        if (pa.Action_Parent.HasValue)
                                            action.Action_Parent = pa.Action_Parent.Value;
                                        else
                                            action.Action_Parent = pa.actionID;

                                        db.SubmitChanges();
                                    }
                                }
                                //}
                            }
                        }

                        ticker_previous = t.First().fundID;
                        broker_previous = b.First().id;
                        previous = tgx;
                        updated = false;

                        //break;

                    }
                    else
                    {
                        Response.Write(a.ToString() + " not found<br>");
                    }
                }
                catch
                {
                    // delete the article
                    var del_article = (from temp in db.articles select temp).OrderByDescending(q => q.idarticle).First();
                    db.articles.DeleteOnSubmit(del_article);
                    db.SubmitChanges();
                    // blacklist the ticker and write it down
                    Response.Write(ticker + "<br>");
                    skip_ticker = ticker;
                    //break;
                }
            }
        }
    }

    protected  fund GetorAddCompany(string ticker) {
        fund theCompany = new fund();
        DataClassesDataContext db = new DataClassesDataContext();
        var comps = from temp in db.funds where temp.ticker == ticker select temp;
        if (comps.Any())
        {
            theCompany = comps.First();
            return theCompany;
        }
        else  //later, add company
        {
            return null;
        }
    }

    protected  user GetorAddAnalyst(string analyst_name, string analyst_bloombergID, int brokerID) {
        user Analyst = new user();
        DataClassesDataContext db = new DataClassesDataContext();
        try
        {
            var anas = from temp in db.users where temp.bloomberg_id == Convert.ToInt32(analyst_bloombergID) select temp;
            if (anas.Any())
            {
                Analyst = anas.First();
                return Analyst;
            }
            else
            {
                Analyst.bloomberg_id = Convert.ToInt32(analyst_bloombergID);
                Analyst.display_name = reconstruct(analyst_name);
                Analyst.seekingalpha_name = Analyst.display_name;
                Analyst.bloomberg_broker = brokerID;
                db.users.InsertOnSubmit(Analyst);
                db.SubmitChanges();
                return Analyst;
            }
        }
        catch {
            return null;
        }
        
    }

    protected  Bloomberg_Broker GetorAddBroker(string broker_code, string broker_name) {
        Bloomberg_Broker theBroker = new Bloomberg_Broker();
        DataClassesDataContext db = new DataClassesDataContext();
        try
        {
            var broks = from temp in db.Bloomberg_Brokers where temp.broker_code == broker_code select temp;
            if (broks.Any())
            {
                theBroker = broks.First();
                return theBroker;
            }
            else
            {
                theBroker.broker_code = broker_code;
                theBroker.name = broker_name;
                db.Bloomberg_Brokers.InsertOnSubmit(theBroker);
                db.SubmitChanges();
                return theBroker;
            }
        }
        catch
        {
            return null;
        }
    }


    

    //public string insert_all_v2(ActionDate input)//string ticker, string analyst_bloombergID, string analyst_name, string broker_code, string broker_name, string inDate, string inPtx)
    //{
    //    string ticker = input.ticker;
    //    string analyst_bloombergID = input.analyst_bloombergID;
    //    string analyst_name = input.analyst_name;
    //    string broker_code = input.broker_code;
    //    string broker_name = input.broker_name;
    //    string inDate = input.inDate;
    //    string inPtx = input.inPtx;

    //    DateTime date;
    //    Double tgx;
    //    DataClassesDataContext db = new DataClassesDataContext();
    //    //string ticker = a.ElementAt(0); string analyst_bloombergID = a.ElementAt(1); string analyst_name = a.ElementAt(2); string broker_code = a.ElementAt(3); string broker_name = a.ElementAt(4);
    //    try
    //    {
    //        date = Convert.ToDateTime(inDate);
    //        tgx = Convert.ToDouble(inPtx);
    //    }
    //    catch
    //    {
    //        return "";
    //    }

    //    fund Company = GetorAddCompany(ticker);
    //    if (Company == null)
    //    {

    //        return ticker + " not found!<br>";
    //    }

    //    Bloomberg_Broker Broker = GetorAddBroker(broker_code, broker_name);
    //    if (Broker == null)
    //    {

    //        return broker_code + " not found or added!<br>";
    //    }

    //    user Analyst = GetorAddAnalyst(analyst_name, analyst_bloombergID, Broker.id);
    //    if (Analyst == null)
    //    {

    //        return analyst_name + " not found or added!<br>";
    //    }

    //    try
    //    {

    //        //if (t.First().fundID == ticker_previous && b.First().id == broker_previous)
    //        //  updated = true;
    //        var action_exists = from temp in db.Actions where temp.article1.origin == Analyst.userID && temp.ticker == Company.fundID && temp.startDate == date && temp.targetValue == tgx select temp;

    //        if (!action_exists.Any())
    //        {
    //            //check previous actions                        
    //            //var past_action = (from temp in db.Actions where temp.ticker == Company.fundID && temp.article1.origin == Analyst.userID select temp).OrderByDescending(q => q.targetDate);
    //            var past_action = (from temp in db.Actions where temp.ticker == Company.fundID && temp.article1.origin == Analyst.userID && temp.startDate<date select temp).OrderByDescending(q => q.targetDate);  // edited by amir: in case we are inserting an action that predates a bunch of other ones
    //            if (past_action.Any())
    //            {
    //                var pa = past_action.First();
    //                if (tgx!=pa.targetValue || (tgx == pa.targetValue && (date - pa.startDate).TotalDays > 60))     // added by amir: if tpx changes there's a new action regardless of the number of days
    //                {
    //                    //create new action
    //                    //article
    //                    article art = new article();
    //                    art.title = " ";
    //                    art.summary = " ";
    //                    art.text = " ";
    //                    art.date = date;
    //                    art.origin = Analyst.userID;
    //                    art.rateSum = 0; art.rateCount = 0; art.provider = 1410832; art.type = 3; art.action = false; art.deleted = false; art.is_ticket = false; art.price = 0; art.not_actionable = false; art.premium = false; art.Publish = true; art.actions = 0; art.backdated = true;
    //                    db.articles.InsertOnSubmit(art);
    //                    db.SubmitChanges();
    //                    int articleid = art.idarticle;

    //                    // action
    //                    Action action = new Action();
    //                    DateTime startdate = date;
    //                    var start = (from temp in db.fund_values where temp.fundID == Company.fundID && temp.date <= startdate select temp).OrderByDescending(q => q.date).First();
    //                    DateTime startDateActual = start.date;
    //                    Double currentValue = start.closeValue.Value;
    //                    action.creator = 2;
    //                    action.article = articleid;
    //                    action.targetDate = startDateActual.AddDays(365);
    //                    action.targetValue = tgx;
    //                    action.currentValue = currentValue;
    //                    action.startDate = startDateActual;
    //                    action.lastUpdated = startDateActual;
    //                    action.startValue = currentValue;
    //                    action.matured = false; action.breached = false; action.expired = false;
    //                    action.maxValue = currentValue; action.minValue = currentValue;
    //                    action.actualStartDate = startDateActual;
    //                    action.dividend = 0;
    //                    action.TotalReturn = true;
    //                    action.days_gain = 0;
    //                    action.beta = 0;
    //                    action.premium = false;
    //                    action.active = true;
    //                    action.is_ticket = false;
    //                    action.ticker = Company.fundID;
    //                    action.deleted = false;
    //                    action.skin_in_the_game = false;
    //                    action.price = 0;
    //                    action.date_feed = startdate;
    //                    action.return_overall = 0;
    //                    action.progress = 0;

    //                    if (action.targetValue >= currentValue)
    //                    {
    //                        action.lowerValue = 0;
    //                        action.@short = false;
    //                    }
    //                    else
    //                    {
    //                        action.lowerValue = 2 * currentValue;
    //                        action.@short = true;
    //                    }

    //                    action.creationTime = DateTime.Now;
    //                    db.Actions.InsertOnSubmit(action);
    //                    db.SubmitChanges();


    //                    pa.active = false;
    //                    pa.Action_Next = action.actionID;
    //                    action.Action_Previous = pa.actionID;
    //                    if (pa.Action_Parent.HasValue)
    //                        action.Action_Parent = pa.Action_Parent.Value;
    //                    else
    //                        action.Action_Parent = pa.actionID;
    //                }

    //                else //Do not create duplicate action within two months
    //                {
    //                    pa.date_feed = date;
    //                }

    //                db.SubmitChanges();
    //            }
    //            else
    //            {
    //                article art = new article();
    //                art.title = " ";
    //                art.summary = " ";
    //                art.text = " ";
    //                art.date = date;
    //                art.origin = Analyst.userID;
    //                art.rateSum = 0; art.rateCount = 0; art.provider = 1410832; art.type = 3; art.action = false; art.deleted = false; art.is_ticket = false; art.price = 0; art.not_actionable = false; art.premium = false; art.Publish = true; art.actions = 0; art.backdated = true;
    //                db.articles.InsertOnSubmit(art);
    //                db.SubmitChanges();
    //                int articleid = art.idarticle;

    //                // action
    //                Action action = new Action();
    //                DateTime startdate = date;
    //                var start = (from temp in db.fund_values where temp.fundID == Company.fundID && temp.date <= startdate select temp).OrderByDescending(q => q.date).First();
    //                DateTime startDateActual = start.date;
    //                Double currentValue = start.closeValue.Value;
    //                action.creator = 2;
    //                action.article = articleid;
    //                action.targetDate = startDateActual.AddDays(365);
    //                action.targetValue = tgx;
    //                action.currentValue = currentValue;
    //                action.startDate = startDateActual;
    //                action.lastUpdated = startDateActual;
    //                action.startValue = currentValue;
    //                action.matured = false; action.breached = false; action.expired = false;
    //                action.maxValue = currentValue; action.minValue = currentValue;
    //                action.actualStartDate = startDateActual;
    //                action.dividend = 0;
    //                action.TotalReturn = true;
    //                action.days_gain = 0;
    //                action.beta = 0;
    //                action.premium = false;
    //                action.active = true;
    //                action.is_ticket = false;
    //                action.ticker = Company.fundID;
    //                action.deleted = false;
    //                action.skin_in_the_game = false;
    //                action.price = 0;
    //                action.date_feed = startdate;
    //                action.return_overall = 0;
    //                action.progress = 0;

    //                if (action.targetValue >= currentValue)
    //                {
    //                    action.lowerValue = 0;
    //                    action.@short = false;
    //                }
    //                else
    //                {
    //                    action.lowerValue = 2 * currentValue;
    //                    action.@short = true;
    //                }

    //                action.creationTime = DateTime.Now;
    //                db.Actions.InsertOnSubmit(action);
    //                db.SubmitChanges();
    //            }
    //        }
    //        else
    //        {
    //            //duplicate
    //        }
    //        //break;
    //        return "";
    //    }

    //    catch (Exception e)
    //    {
    //        return ticker + " not inserted:" + e.Message + "<br>";
    //    }

    //}

    //protected void example()
    //{
    //    ActionDate test = new ActionDate();
    //    Thread t = new Thread(() => insert_all_v2(test));
    //    t.Start();
    //}

    //protected void xls(string filename)
    //{
    //    FileInfo newFile = new FileInfo(Server.MapPath("~") + "\\files\\" + filename);

    //    ExcelPackage pck = new ExcelPackage(newFile);
    //    int worksheet_index = 1;

    //    while (true)
    //    {
    //        var ws = pck.Workbook.Worksheets["Data" + worksheet_index];

    //        if (ws == null)
    //        {
    //            break;
    //        }
    //        else
    //        {
    //            int i = 150;
    //            int j = 1;
    //            int r = 1;
    //            int n = 1;

    //            try
    //            {
    //                while (ws.Cells[i, j].Value != null)
    //                {
    //                    while (ws.Cells[i, j].Value != null)
    //                    {
    //                        try
    //                        {
    //                            string ticker = ws.Cells[i, j + 2].Value.ToString();
    //                            string broker_code = ws.Cells[i, j + 3].Value.ToString();
    //                            string broker_name = ws.Cells[i, j + 4].Value.ToString();
    //                            string analyst_name = ws.Cells[i, j + 5].Value.ToString();
    //                            string analyst_id = ws.Cells[i, j + 6].Value.ToString();

    //                            while (ws.Cells[i, j].Value != null && ws.Cells[i, j].Value.ToString().IndexOf("#") == -1)
    //                            {
    //                                if (ws.Cells[i, j + 1].Value != null)
    //                                {
    //                                    if (ws.Cells[i, j + 1].Value.ToString().IndexOf("#") == -1)
    //                                    {
    //                                        ActionDate data = new ActionDate();
    //                                        data.analyst_bloombergID = analyst_id;
    //                                        data.analyst_name = analyst_name;
    //                                        data.broker_code = broker_code;
    //                                        data.broker_name = broker_name;
    //                                        data.inDate = ws.Cells[i, j].Value.ToString();
    //                                        data.inPtx = ws.Cells[i, j + 1].Value.ToString();
    //                                        // Call the threaded function here


    //                                        //Response.Write(ticker + "\t\t\t" + analyst_id + "\t\t\t" + analyst_name + "\t\t\t" + broker_code + "\t\t\t" + broker_name + "\t\t\t" + ws.Cells[i, j].Value + "\t\t\t" + ws.Cells[i, j + 1].Value);
    //                                        //Response.Write("<br>");
    //                                        //r++;
    //                                    }
    //                                }

    //                                i++;
    //                            }
    //                        }
    //                        catch
    //                        {

    //                        }

    //                        n++;
    //                        i = n * 150;
    //                    }

    //                    n = 1;
    //                    i = 150;
    //                    j = j + 25;
    //                }
    //            }
    //            catch
    //            {
    //                Response.Write("Broke on i=" + i + ", j=" + j + ", n=" + n);
    //            }

    //        }

    //        worksheet_index++;
    //    }
    //}



    protected void get_all_ticker_data() {
        DataClassesDataContext db = new DataClassesDataContext();
        string ticker = "TCK";
        Quote fundQuote = new Quote(ticker.Trim());
        var fund = from temp in db.funds where temp.ticker == ticker select temp;

        if (fund.Any()) {
            var fund_values = from temp in db.fund_values where temp.fundID == fund.First().fundID select temp;

            if (fund_values.Any()) {
                foreach (var f in fund_values)
                {
                    db.fund_values.DeleteOnSubmit(f);
                    db.SubmitChanges();
                }
            }
            DataBaseLayer.insert_fund_data(fundQuote, db, fund.First());
        }

        
    }

    protected void insert_bloomberg_consensus() { 
        var lines = File.ReadLines(Server.MapPath("~") + "\\files\\bloomberg_consensus.csv").Select(b => b.Split(','));
        DataClassesDataContext db = new DataClassesDataContext();

        foreach (var a in lines)
        {
            string ticker = a.ElementAt(0);
            DateTime date = Convert.ToDateTime(a.ElementAt(1));
            Double tgx = Convert.ToDouble(a.ElementAt(2));

            Bloomberg_Consensus b = new Bloomberg_Consensus();
            try
            {
                var t = from temp in db.funds where temp.ticker == ticker select temp;

                if (t.Any()){
                    b.ticker = t.First().fundID;
                    b.date = date;
                    b.price = tgx;
                    db.Bloomberg_Consensus.InsertOnSubmit(b);
                    db.SubmitChanges();
                }
                else{
                    Response.Write("Not found " + ticker + "/" + date + "/" + tgx);    
                }
                
            }
            catch {
                Response.Write("Error in " + ticker + "/" + date + "/" + tgx);
            }
        }
    }

    protected void insert_liaison() { 
        var lines = File.ReadLines(Server.MapPath("~") + "\\files\\liaison2.csv").Select(b => b.Split(','));
        DataClassesDataContext db = new DataClassesDataContext();

        foreach (var a in lines)
        {
            string ticker = a.ElementAt(0);
            string broker = a.ElementAt(1);
            int analyst_id = Convert.ToInt32(a.ElementAt(3));
            int invesd_id = 0;
            int invesd_broker_id = 0;
            int ticker_id = 0;

            var analyst = from temp in db.users where temp.bloomberg_id == analyst_id select temp;

            if (!analyst.Any())
            {
                user aa = new user();
                aa.display_name = reconstruct(a.ElementAt(2));;
                aa.seekingalpha_name = aa.display_name;
                aa.bloomberg_id = analyst_id;
                try
                {
                    aa.bloomberg_broker = (from temp in db.Bloomberg_Brokers where temp.broker_code == broker select temp).First().id;
                }
                catch {
                    Response.Write("Broker " + broker + " not found");
                    break;
                }

                db.users.InsertOnSubmit(aa);
                db.SubmitChanges();
                invesd_id = aa.userID;
                invesd_broker_id = aa.bloomberg_broker.Value;
            }
            else {
                invesd_id = analyst.First().userID;
                invesd_broker_id = analyst.First().bloomberg_broker.Value;
            }

            var tick = from temp in db.funds where temp.ticker == ticker select temp;
            if (tick.Any())
            {
                ticker_id = tick.First().fundID;
            }
            else {
                var tick_pl = from temp in db.funds where temp.ticker == ticker + ".PK" || temp.ticker == ticker + ".OB" || temp.ticker == ticker + ".A" || temp.ticker == ticker + ".B" select temp;

                if (tick_pl.Any())
                    ticker_id = tick_pl.First().fundID;
                
            }


            Liaison l = new Liaison();
            l.ticker = ticker_id;
            l.analyst = invesd_id;
            l.broker = invesd_broker_id;

            var x = from temp in db.Liaisons where temp.ticker == l.ticker && temp.broker == l.broker && temp.analyst == l.analyst select temp;
            if (!x.Any()) {
                db.Liaisons.InsertOnSubmit(l);
                db.SubmitChanges();
            }
        }
    }

    protected void insert_brokers() {
        var lines = File.ReadLines(Server.MapPath("~") + "\\files\\brokers.csv").Select(b => b.Split(';'));
        DataClassesDataContext db = new DataClassesDataContext();

        foreach (var a in lines) {
            string broker_name = a.ElementAt(0);
            string broker_code = a.ElementAt(1);

            var broker = from temp in db.Bloomberg_Brokers where temp.broker_code == broker_code select temp;

            if (!broker.Any()) {
                Bloomberg_Broker b = new Bloomberg_Broker();
                b.broker_code = broker_code;
                b.name = broker_name;
                db.Bloomberg_Brokers.InsertOnSubmit(b);
                db.SubmitChanges();
            }
        }
    }

    protected void list_all_brokers() {
        DataClassesDataContext db = new DataClassesDataContext();

        var a = from temp in db.Bloomberg_Brokers select temp;

        foreach (var b in a) {
            Response.Write(b.broker_code + "<br>");
        }
    }

    protected void list_all_funds() {
        DataClassesDataContext db = new DataClassesDataContext();

        var a = from temp in db.funds select temp;

        foreach (var b in a) {
            Response.Write(b.ticker.ToUpper() + " US Equity<br>");
        }
    }

    protected void active_sectors()
    {
        DataClassesDataContext db = new DataClassesDataContext();

        var aa = (from temp in db.Active_Tickers where temp.fund.sector.HasValue select temp).GroupBy(b => b.fund.sector);

        if (aa.Any())
        {
            foreach (var a in aa)
            {
                //Active_Industry i = new Active_Industry();
                Active_Sector i = new Active_Sector();
                var c = a.GroupBy(b => b.ticker);

                //i.industry = a.First().fund.peer_group.Value;
                i.sector = a.First().fund.sector.Value;

                i.actions_short = a.Sum(b => b.actions_short);
                i.actions_short_top = a.Sum(b => b.actions_short_top);
                i.actions_medium = a.Sum(b => b.actions_medium);
                i.actions_medium_top = a.Sum(b => b.actions_medium_top);
                i.actions_long = a.Sum(b => b.actions_long);
                i.actions_long_top = a.Sum(b => b.actions_long_top);

                i.companies = a.GroupBy(b => b.ticker).Count();
                i.companies_short = a.Where(b => b.actions_short > 0).GroupBy(b => b.ticker).Count();
                i.companies_medium = a.Where(b => b.actions_medium > 0).GroupBy(b => b.ticker).Count();
                i.companies_long = a.Where(b => b.actions_long > 0).GroupBy(b => b.ticker).Count();

                i.industries = a.GroupBy(b => b.fund.peer_group).Count();
                i.industries_short = a.Where(b => b.actions_short > 0).GroupBy(b => b.fund.sector).Count();
                i.industries_medium = a.Where(b => b.actions_medium > 0).GroupBy(b => b.fund.sector).Count();
                i.industries_long = a.Where(b => b.actions_long > 0).GroupBy(b => b.fund.sector).Count();

                i.target_percentage_short = a.Sum(b => b.actions_short) > 0 ? a.Sum(b => b.actions_short * b.target_percentage_short) / a.Sum(b => b.actions_short) : null;
                i.target_percentage_short_top = a.Sum(b => b.actions_short_top) > 0 ? a.Sum(b => b.actions_short_top * b.target_percentage_short_top) / a.Sum(b => b.actions_short_top) : null;
                i.target_percentage_medium = a.Sum(b => b.actions_medium) > 0 ? a.Sum(b => b.actions_medium * b.target_percentage_medium) / a.Sum(b => b.actions_medium) : null;
                i.target_percentage_medium_top = a.Sum(b => b.actions_medium_top) > 0 ? a.Sum(b => b.actions_medium_top * b.target_percentage_medium_top) / a.Sum(b => b.actions_medium_top) : null;
                i.target_percentage_long = a.Sum(b => b.actions_long) > 0 ? a.Sum(b => b.actions_long * b.target_percentage_long) / a.Sum(b => b.actions_long) : null;
                i.target_percentage_long_top = a.Sum(b => b.actions_long_top) > 0 ? a.Sum(b => b.actions_long_top * b.target_percentage_long_top) / a.Sum(b => b.actions_long_top) : null;

                i.bullishness_short = a.Sum(b => b.actions_short) > 0 ? a.Sum(b => b.actions_short * b.bullishness_short) / a.Sum(b => b.actions_short) : null;
                i.bullishness_short_top = a.Sum(b => b.actions_short_top) > 0 ? a.Sum(b => b.actions_short_top * b.bullishness_short_top) / a.Sum(b => b.actions_short_top) : null;
                i.bullishness_medium = a.Sum(b => b.actions_medium) > 0 ? a.Sum(b => b.actions_medium * b.bullishness_medium) / a.Sum(b => b.actions_medium) : null;
                i.bullishness_medium_top = a.Sum(b => b.actions_medium_top) > 0 ? a.Sum(b => b.actions_medium_top * b.bullishness_medium_top) / a.Sum(b => b.actions_medium_top) : null;
                i.bullishness_long = a.Sum(b => b.actions_long) > 0 ? a.Sum(b => b.actions_long * b.bullishness_long) / a.Sum(b => b.actions_long) : null;
                i.bullishness_long_top = a.Sum(b => b.actions_long_top) > 0 ? a.Sum(b => b.actions_long_top * b.bullishness_long_top) / a.Sum(b => b.actions_long_top) : null;

                i.consensus_short = a.Sum(b => b.actions_short) > 0 ? a.Sum(b => b.actions_short * b.consensus_short) / a.Sum(b => b.actions_short) : null;
                i.consensus_short_top = a.Sum(b => b.actions_short_top) > 0 ? a.Sum(b => b.actions_short_top * b.consensus_short) / a.Sum(b => b.actions_short_top) : null;
                i.consensus_medium = a.Sum(b => b.actions_medium) > 0 ? a.Sum(b => b.actions_medium * b.consensus_medium) / a.Sum(b => b.actions_medium) : null;
                i.consensus_medium_top = a.Sum(b => b.actions_medium_top) > 0 ? a.Sum(b => b.actions_medium_top * b.consensus_medium) / a.Sum(b => b.actions_medium_top) : null;
                i.consensus_long = a.Sum(b => b.actions_long) > 0 ? a.Sum(b => b.actions_long * b.consensus_long) / a.Sum(b => b.actions_long) : null;
                i.consensus_long_top = a.Sum(b => b.actions_long_top) > 0 ? a.Sum(b => b.actions_long_top * b.consensus_long) / a.Sum(b => b.actions_long_top) : null;
                i.consensus_overall = a.Sum(b => b.actions_short + b.actions_medium + b.actions_long) > 0 ? a.Sum(b => ((b.actions_short + b.actions_medium + b.actions_long) * b.consensus_overall)) / a.Sum(b => b.actions_short + b.actions_medium + b.actions_long) : 0;
                i.consensus_overall_top = a.Sum(b => b.actions_short_top + b.actions_medium_top + b.actions_long_top) > 0 ? a.Sum(b => ((b.actions_short_top + b.actions_medium_top + b.actions_long_top) * b.consensus_overall_top)) / a.Sum(b => b.actions_short_top + b.actions_medium_top + b.actions_long_top) : null;

                var bb = from temp in db.Actions where temp.fund.sector == i.sector select temp;

                if (bb.Any())
                {
                    List<Tuple<int, int>> top = new List<Tuple<int, int>>();
                    //Dictionary<int, int> top = new Dictionary<int, int>();
                    var ap = from temp in db.AnalystPerformances where temp.fund.sector == i.sector && temp.rank <= 10 && temp.return_average > 0 select temp;
                    if (ap.Any())
                    {
                        foreach (var p in ap)
                        {
                            Tuple<int, int> x = new Tuple<int, int>(p.ticker.Value, p.analyst);
                            //top.Add(p.ticker.Value, p.analyst);
                            top.Add(x);
                        }
                    }

                    // how do we do tops?

                    i.analysts = bb.GroupBy(b => b.article1.origin).Count();
                    //i.analysts_top = 
                    i.analysts_short = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays < 183).GroupBy(b => b.article1.origin).Count();
                    //i.analysts_short_top = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays < 183).Where(b=>top.Any(n=>n.Item1 == b.ticker && n.Item2 == b.article1.origin )).GroupBy(b => b.article1.origin).Count();
                    //i.analysts_short_top = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays < 183).Where(b => b.).GroupBy(b => b.article1.origin).Count();
                    i.analysts_medium = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays >= 183 && (b.targetDate - DateTime.Now).TotalDays <= 548).GroupBy(b => b.article1.origin).Count();
                    //i.analysts_medium_top = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays < 183 && (b.targetDate - DateTime.Now).TotalDays <= 548).Where(b => top.Any(n => n.Item1 == b.ticker && n.Item2 == b.article1.origin)).GroupBy(b => b.article1.origin).Count();
                    i.analysts_long = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays > 548).GroupBy(b => b.article1.origin).Count();
                    //i.analysts_long_top = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays > 548).Where(b => top.Any(n => n.Item1 == b.ticker && n.Item2 == b.article1.origin)).GroupBy(b => b.article1.origin).Count();
                }

                i.date = DateTime.Now;
                db.Active_Sectors.InsertOnSubmit(i);
                db.SubmitChanges();
            }
        }
    }


    protected void active_inudstries() {
        DataClassesDataContext db = new DataClassesDataContext();

        var aa = (from temp in db.Active_Tickers where temp.fund.peer_group.HasValue select temp).GroupBy(b=>b.fund.peer_group);

        if (aa.Any()){
            foreach (var a in aa) {
            Active_Industry i = new Active_Industry();
            var c = a.GroupBy(b => b.ticker);

            i.industry = a.First().fund.peer_group.Value;

            i.actions_short = a.Sum(b => b.actions_short);
            i.actions_short_top = a.Sum(b => b.actions_short_top);
            i.actions_medium = a.Sum(b => b.actions_medium);
            i.actions_medium_top = a.Sum(b => b.actions_medium_top);
            i.actions_long = a.Sum(b => b.actions_long);
            i.actions_long_top = a.Sum(b => b.actions_long_top);

            i.companies = a.GroupBy(b => b.ticker).Count();
            i.companies_short = a.Where(b => b.actions_short > 0).GroupBy(b => b.ticker).Count();
            i.companies_medium = a.Where(b => b.actions_medium > 0).GroupBy(b => b.ticker).Count();
            i.companies_long = a.Where(b => b.actions_long > 0).GroupBy(b => b.ticker).Count();

            i.target_percentage_short = a.Sum(b => b.actions_short)>0? a.Sum(b => b.actions_short * b.target_percentage_short) / a.Sum(b => b.actions_short):null;
            i.target_percentage_short_top = a.Sum(b => b.actions_short_top)>0?a.Sum(b => b.actions_short_top * b.target_percentage_short_top) / a.Sum(b => b.actions_short_top):null;
            i.target_percentage_medium = a.Sum(b => b.actions_medium)>0?a.Sum(b => b.actions_medium * b.target_percentage_medium) / a.Sum(b => b.actions_medium):null;
            i.target_percentage_medium_top = a.Sum(b => b.actions_medium_top)>0?a.Sum(b => b.actions_medium_top * b.target_percentage_medium_top) / a.Sum(b => b.actions_medium_top):null;
            i.target_percentage_long = a.Sum(b => b.actions_long)>0?a.Sum(b => b.actions_long * b.target_percentage_long) / a.Sum(b => b.actions_long):null;
            i.target_percentage_long_top = a.Sum(b => b.actions_long_top)>0?a.Sum(b => b.actions_long_top * b.target_percentage_long_top) / a.Sum(b => b.actions_long_top):null;

            i.bullishness_short = a.Sum(b => b.actions_short)>0? a.Sum(b => b.actions_short * b.bullishness_short) / a.Sum(b => b.actions_short):null;
            i.bullishness_short_top = a.Sum(b => b.actions_short_top)>0?a.Sum(b => b.actions_short_top * b.bullishness_short_top) / a.Sum(b => b.actions_short_top):null;
            i.bullishness_medium = a.Sum(b => b.actions_medium)>0?a.Sum(b => b.actions_medium * b.bullishness_medium) / a.Sum(b => b.actions_medium):null;
            i.bullishness_medium_top = a.Sum(b => b.actions_medium_top)>0?a.Sum(b => b.actions_medium_top * b.bullishness_medium_top) / a.Sum(b => b.actions_medium_top):null;
            i.bullishness_long = a.Sum(b => b.actions_long)>0?a.Sum(b => b.actions_long * b.bullishness_long) / a.Sum(b => b.actions_long):null;
            i.bullishness_long_top = a.Sum(b => b.actions_long_top)>0?a.Sum(b => b.actions_long_top * b.bullishness_long_top) / a.Sum(b => b.actions_long_top):null;

            i.consensus_short = a.Sum(b => b.actions_short)>0? a.Sum(b => b.actions_short * b.consensus_short) / a.Sum(b => b.actions_short):null;
            i.consensus_short_top = a.Sum(b => b.actions_short_top)>0? a.Sum(b => b.actions_short_top * b.consensus_short) / a.Sum(b => b.actions_short_top):null;
            i.consensus_medium = a.Sum(b => b.actions_medium)>0?a.Sum(b => b.actions_medium * b.consensus_medium) / a.Sum(b => b.actions_medium):null;
            i.consensus_medium_top = a.Sum(b => b.actions_medium_top)>0? a.Sum(b => b.actions_medium_top * b.consensus_medium) / a.Sum(b => b.actions_medium_top):null;
            i.consensus_long = a.Sum(b => b.actions_long)>0? a.Sum(b => b.actions_long * b.consensus_long) / a.Sum(b => b.actions_long):null;
            i.consensus_long_top = a.Sum(b => b.actions_long_top)>0? a.Sum(b => b.actions_long_top * b.consensus_long) / a.Sum(b => b.actions_long_top):null;
            i.consensus_overall = a.Sum(b => b.actions_short+b.actions_medium+b.actions_long)>0? a.Sum(b => ((b.actions_short+b.actions_medium+b.actions_long) * b.consensus_overall)) / a.Sum(b => b.actions_short+b.actions_medium+b.actions_long):0;
            i.consensus_overall_top = a.Sum(b => b.actions_short_top + b.actions_medium_top + b.actions_long_top)>0?a.Sum(b => ((b.actions_short_top + b.actions_medium_top + b.actions_long_top) * b.consensus_overall_top)) / a.Sum(b => b.actions_short_top + b.actions_medium_top + b.actions_long_top):null;

            var bb = from temp in db.Actions where temp.fund.peer_group == i.industry select temp;

            if (bb.Any()) {
                List<Tuple<int,int>> top = new List<Tuple<int,int>>();
                //Dictionary<int, int> top = new Dictionary<int, int>();
                var ap = from temp in db.AnalystPerformances where temp.fund.peer_group == i.industry && temp.rank<=10 && temp.return_average>0 select temp;
                if (ap.Any()){
                    foreach (var p in ap){
                        Tuple<int,int> x = new Tuple<int,int>(p.ticker.Value,p.analyst);
                        //top.Add(p.ticker.Value, p.analyst);
                        top.Add(x);
                    }
                }
                
                // how do we do tops?

                i.analysts = bb.GroupBy(b => b.article1.origin).Count();
                //i.analysts_top = 
                i.analysts_short = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays<183).GroupBy(b => b.article1.origin).Count();
                //i.analysts_short_top = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays < 183).Where(b=>top.Any(n=>n.Item1 == b.ticker && n.Item2 == b.article1.origin )).GroupBy(b => b.article1.origin).Count();
                //i.analysts_short_top = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays < 183).Where(b => b.).GroupBy(b => b.article1.origin).Count();
                i.analysts_medium = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays >= 183 && (b.targetDate - DateTime.Now).TotalDays<=548).GroupBy(b => b.article1.origin).Count();
                //i.analysts_medium_top = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays < 183 && (b.targetDate - DateTime.Now).TotalDays <= 548).Where(b => top.Any(n => n.Item1 == b.ticker && n.Item2 == b.article1.origin)).GroupBy(b => b.article1.origin).Count();
                i.analysts_long = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays > 548).GroupBy(b => b.article1.origin).Count();
                //i.analysts_long_top = bb.Where(b => (b.targetDate - DateTime.Now).TotalDays > 548).Where(b => top.Any(n => n.Item1 == b.ticker && n.Item2 == b.article1.origin)).GroupBy(b => b.article1.origin).Count();
            }

            i.date = DateTime.Now;
            db.Active_Industries.InsertOnSubmit(i);
            db.SubmitChanges();
            }
        }
    }

    protected void spit_type_author_action() {
        DataClassesDataContext db = new DataClassesDataContext();

        var aa = from temp in db.articles where temp.provider == 36 && temp.type == 3 select temp;

        foreach (var a in aa) {
            Response.Write(a.type + "\t" + a.origin + "\t" + bib(a.action) + "<br>");
        }
    }

    protected string bib(bool a) {
        if (!a)
            return "0";
        else
            return "1";
    }

    protected void get_ticker_list() {
        DataClassesDataContext db = new DataClassesDataContext();

        var a = (from temp in db.Actions select temp).GroupBy(b => b.ticker);

        foreach (var b in a) {
            Response.Write(b.First().fund.ticker.Trim() + "<br>");
        }
    }

    protected void fix_bloomberg_names() {
        DataClassesDataContext db = new DataClassesDataContext();

        var b = from temp in db.users where temp.bloomberg_id.HasValue select temp;

        foreach (var a in b) {
            string[] x = a.display_name.Split(' ');
            List<string> name = new List<string>();

            foreach (string z in x) {
                if (z.Length > 1) {
                    name.Add(z);
                }
            }

            string name_new = "";
            int c = 0;
            foreach (string n in name) {
                if (c == name.Count)
                {
                    name_new += n;
                }
                else {
                    name_new += n + " ";
                }
            }

            a.display_name = name_new;
            a.seekingalpha_name = name_new;

            db.SubmitChanges();
        }
    }

    protected void fix_qcom() {
        DataClassesDataContext db = new DataClassesDataContext();

        var a = from temp in db.Actions where temp.ticker == 800 && temp.active && temp.article1.user.bloomberg_id.HasValue select temp;

        if (a.Any()) {
            foreach (var x in a) {
                var yy = from temp in db.Actions where temp.ticker == 800 && temp.article1.origin == x.article1.origin && temp.targetDate > x.targetDate select temp;

                if (yy.Any()) {
                    x.active = false;
                    db.SubmitChanges();
                }
            }
        }
    }

    protected void read_and_insert_actions_from_csv() { 
        DataClassesDataContext db = new DataClassesDataContext();

        // initialize for ticker
        string ticker = "TSLA";
        string company = "Tesla";
        int fund = 782;

        var lines = File.ReadLines(Server.MapPath("~") + "\\files\\" + ticker + "_details.csv").Select(b => b.Split(','));
        

        int id = 0;
        double previous = 0;
        bool updated = false;

        int count_tgx = 1;

        foreach (var a in lines)
        {
            int bid = Convert.ToInt32(a.ElementAt(0));

            if (id == bid)
                updated = true;

            var analyst = from temp in db.users where temp.bloomberg_id == bid select temp;

            if (analyst.Any()) {
                article art = new article();
                double tgx = Convert.ToDouble(a.ElementAt(2));
                art.title = analyst.First().display_name + (!updated ? " initiated a " + string.Format("{0:c2}", tgx) + " price target for " + company : " revised " + company + "'s price target to " + string.Format("{0:c2}", tgx));
                art.summary = analyst.First().Bloomberg_Broker1.name + "'s analyst " + analyst.First().display_name + (!updated ? " initiated a " + string.Format("{0:c2}", tgx) + " price target for " + company + " (" + ticker + ")." : " revised " + company + "'s (" + ticker + ") price target to " + string.Format("{0:c2}", tgx) + " from " + string.Format("{0:c2}", previous) + ".");
                art.text = art.summary;
                art.date = Convert.ToDateTime(a.ElementAt(1));
                art.origin = analyst.First().userID;
                art.rateSum = 0; art.rateCount = 0; art.provider = 1410832; art.type = 3; art.action = false; art.deleted = false; art.is_ticket = false; art.price = 0; art.not_actionable = false; art.premium = false; art.Publish = true; art.actions = 0; art.backdated = true;

                db.articles.InsertOnSubmit(art);
                db.SubmitChanges();

                int articleid = art.idarticle;

                Action action = new Action();

                DateTime startdate = Convert.ToDateTime(a.ElementAt(1));
                var start = (from temp in db.fund_values where temp.fundID == fund && temp.date <= startdate select temp).OrderByDescending(b => b.date).First();
                DateTime startDateActual = start.date;
                Double currentValue = start.closeValue.Value;

                action.creator = 2;
                action.article = articleid;
                action.targetDate = startDateActual.AddDays(365);
                action.targetValue = Convert.ToDouble(a.ElementAt(2));
                action.currentValue = currentValue;
                action.startDate = startDateActual;
                action.lastUpdated = startDateActual;
                action.startValue = currentValue;
                action.matured = false; action.breached = false; action.expired = false;
                action.maxValue = currentValue; action.minValue = currentValue;
                action.actualStartDate = startDateActual;
                action.dividend = 0;
                action.TotalReturn = true;
                action.days_gain = 0;
                action.beta = 0;
                action.premium = false;
                action.active = true;
                action.is_ticket = false;
                action.ticker = fund;
                action.deleted = false;
                action.skin_in_the_game = false;
                action.price = 0;
                action.date_feed = startdate;
                action.return_overall = 0;
                action.progress = 0;

                if (action.targetValue >= currentValue)
                {
                    action.lowerValue = 0;
                    action.@short = false;
                }
                else {
                    action.lowerValue = 2 * currentValue;
                    action.@short = true;
                }

                action.creationTime = DateTime.Now;

                action.rational = art.summary;
                action.analystID = art.origin;
                db.Actions.InsertOnSubmit(action);
                db.SubmitChanges();

                // need to revise the previous action first
                
                if (updated){
                    var past_action = (from temp in db.Actions where temp.actionID !=action.actionID  && temp.ticker == fund && temp.article1.origin == analyst.First().userID select temp).OrderByDescending(b => b.targetDate);
                    if (past_action.Any()) {
                        var pa = past_action.First();
                        pa.active = false;
                        pa.Action_Next = action.actionID;
                        action.Action_Previous = pa.actionID;
                        if (pa.Action_Parent.HasValue)
                            action.Action_Parent = pa.Action_Parent.Value;
                        else
                            action.Action_Parent = pa.actionID;

                        db.SubmitChanges();
                    }
                }

                count_tgx++;
            }

            updated = false;
            id = bid;
            previous = Convert.ToDouble(a.ElementAt(2));

            //if (count_tgx == 5)
            //    break;
        }
    }

    protected string reconstruct(string x) {
        string[] y = x.Split(' ');
        string final_product = "";
        int count = y.Count();
        int i = 1;

        foreach (string z in y) {
            
            if (z.Length>1)
                final_product += z[0] + z.Substring(1).ToLower();

            if (count != i && z.Length>1) {
                final_product += " ";
            }
            i++;
        }

        return final_product;
    }

    protected void read_and_insert_csv() {
        DataClassesDataContext db = new DataClassesDataContext();

        //var lines = File.ReadLines(Server.MapPath("~") + "\\files\\qcom.csv").Select(b => b.Split(';'));
        var lines = File.ReadLines(Server.MapPath("~") + "\\files\\tsla_ppl.csv").Select(b => b.Split(';'));

        foreach (var a in lines) {
            string name = reconstruct(a.ElementAt(1));

            var u = from temp in db.users where temp.display_name.ToLower() == name.ToLower() select temp;

            if (u.Any())
            {
                Response.Write("<font color=red>oh oh!" + name + "</font");
            }
            else {
                user analyst = new user();
                analyst.display_name = name;
                analyst.seekingalpha_name = name;

                Response.Write(name);
                var b = from temp in db.Bloomberg_Brokers where temp.broker_code == a.ElementAt(2) select temp;

                if (b.Any())
                {
                    Response.Write(" - " + b.First().id + "<br>");
                    analyst.bloomberg_broker = b.First().id;
                }
                else
                {
                    Bloomberg_Broker bb = new Bloomberg_Broker();
                    bb.name = a.ElementAt(3);
                    bb.broker_code = a.ElementAt(2);
                    db.Bloomberg_Brokers.InsertOnSubmit(bb);
                    db.SubmitChanges();
                    analyst.bloomberg_broker = bb.id;
                }

                analyst.bloomberg_id = Convert.ToInt32(a.ElementAt(0));

                db.users.InsertOnSubmit(analyst);
                db.SubmitChanges();
            }
        }

        //foreach (var a in lines)
        //{
        //    var find = from temp in db.Bloomberg_Brokers where temp.broker_code == a.ElementAt(1) select temp;

        //    if (!find.Any()) {
        //        Bloomberg_Broker b = new Bloomberg_Broker();
        //        b.broker_code = a.ElementAt(1);
        //        b.name = a.ElementAt(0);
        //        db.Bloomberg_Brokers.InsertOnSubmit(b);
        //        db.SubmitChanges();
        //    }
        //}

    }

    protected void active_and_expired() {
        DataClassesDataContext db = new DataClassesDataContext();

        var aa = from temp in db.Actions where temp.active select temp;

        if (aa.Any()) {
            foreach (var a in aa) {
                if (a.targetDate >= DateTime.Now)
                {
                    a.active = false;
                    db.SubmitChanges();
                }
            }
        }
    }

    protected void Active_Tickers() {
        // note: bullishness: what happens to equal cases???

        DataClassesDataContext db = new DataClassesDataContext();

        var active = (from temp in db.Actions where temp.active && temp.ticker!=451 select temp).GroupBy(b => b.ticker);

        foreach (var a in active) {
            var record = from temp in db.Active_Tickers where temp.ticker == a.First().ticker select temp;

            double current_price = (from temp in db.fund_values where temp.fundID == a.First().ticker && temp.isLatest.Value select temp.closeValue).First().Value;

            var tops = from temp in db.AnalystPerformances where temp.ticker == a.First().ticker && temp.rank<=10 && temp.return_average>0 select temp;
            List<int> top = new List<int>();
            foreach (var t in tops){
                top.Add(t.analyst);
            }

            var a_top = a.Where(b => top.Contains(b.article1.origin));

            int ticker = a.First().ticker;

            var active_short = a.Where(b => (b.targetDate-DateTime.Now).TotalDays < 183);
            var active_medium = a.Where(b => (b.targetDate-DateTime.Now).TotalDays >= 183 && (b.targetDate-DateTime.Now).TotalDays < 548);
            var active_long = a.Where(b => (b.targetDate-DateTime.Now).TotalDays >= 548);

            var active_short_top = active_short.Where(b => top.Contains(b.article1.origin));
            var active_medium_top = active_medium.Where(b => top.Contains(b.article1.origin));
            var active_long_top = active_long.Where(b => top.Contains(b.article1.origin));

            int actions_short = active_short.Count();
            int actions_medium = active_medium.Count();
            int actions_long = active_long.Count();
            int actions_short_top = active_short_top.Count();
            int actions_medium_top = active_medium_top.Count();
            int actions_long_top = active_long_top.Count();

            int analysts_short = active_short.GroupBy(b => b.article1.origin).Count();
            int analysts_medium = active_medium.GroupBy(b => b.article1.origin).Count();
            int analysts_long = active_long.GroupBy(b => b.article1.origin).Count();
            int analysts_short_top = active_short_top.GroupBy(b => b.article1.origin).Count();
            int analysts_medium_top = active_medium_top.GroupBy(b => b.article1.origin).Count();
            int analysts_long_top = active_long_top.GroupBy(b => b.article1.origin).Count();

            int analysts = a.GroupBy(b => b.article1.origin).Count();
            int analysts_top = a.GroupBy(b => b.article1.origin).Where(b => top.Contains(b.First().article1.origin)).Count();

            double target_price_short = 0; double target_price_medium = 0; double target_price_long = 0;
            double target_price_short_top = 0; double target_price_medium_top = 0; double target_price_long_top = 0;
            double target_percentage_short = 0; double target_percentage_medium = 0; double target_percentage_long = 0;
            double target_percentage_short_top = 0; double target_percentage_medium_top = 0; double target_percentage_long_top = 0;

            double bullishness_short = 0; double bullishness_medium = 0; double bullishness_long = 0;
            double bullishness_short_top = 0; double bullishness_medium_top = 0; double bullishness_long_top = 0;

            double consensus_short = 0; double consensus_medium = 0; double consensus_long = 0;
            double consensus_short_top = 0; double consensus_medium_top = 0; double consensus_long_top = 0;

            double consensus_overall = 1 - Calculate_STD(a.Select(b => b.targetValue)) / current_price;
            double consensus_overall_top = 0;

            if (a_top.Any()) {
                consensus_overall_top = 1 - Calculate_STD(a_top.Select(b => b.targetValue)) / current_price;
            }

            if (active_short.Any()) {
                target_price_short = active_short.Average(b => b.targetValue);
                target_percentage_short = active_short.Average(b => b.targetValue / current_price - 1);
                bullishness_short = (double)active_short.Where(b => b.targetValue >= current_price).Count() / (double)actions_short;
                consensus_short = Math.Max(0,1-Calculate_STD(active_short.Select(b => b.targetValue))/current_price);

                if (active_short_top.Any()) {
                    target_price_short_top = active_short_top.Average(b => b.targetValue);
                    target_percentage_short_top = active_short_top.Average(b => b.targetValue / current_price - 1);
                    bullishness_short_top = (double)active_short_top.Where(b => b.targetValue >= current_price).Count() / (double)actions_short_top;
                    consensus_short_top = Math.Max(0,1 - Calculate_STD(active_short_top.Select(b => b.targetValue)) / current_price);
                }
            }

            if (active_medium.Any()) {
                target_price_medium = active_medium.Average(b => b.targetValue);
                target_percentage_medium = active_medium.Average(b => b.targetValue / current_price - 1);
                bullishness_medium = (double)active_medium.Where(b => b.targetValue >= current_price).Count() / (double)actions_medium;
                consensus_medium = Math.Max(0,1 - Calculate_STD(active_medium.Select(b => b.targetValue)) / current_price);

                if (active_medium_top.Any())
                {
                    target_price_medium_top = active_medium_top.Average(b => b.targetValue);
                    target_percentage_medium_top = active_medium_top.Average(b => b.targetValue / current_price - 1);
                    bullishness_medium_top = (double)active_medium_top.Where(b => b.targetValue >= current_price).Count() / (double)actions_medium_top;
                    consensus_medium_top = Math.Max(0,1 - Calculate_STD(active_medium_top.Select(b => b.targetValue)) / current_price);
                }
            }

            if (active_long.Any()) {
                target_price_long = active_long.Average(b => b.targetValue);
                target_percentage_long = active_long.Average(b => b.targetValue / current_price - 1);
                bullishness_long = (double)active_long.Where(b => b.targetValue >= current_price).Count() / (double)actions_long;
                consensus_long = Math.Max(0,1 - Calculate_STD(active_long.Select(b => b.targetValue)) / current_price);

                if (active_long_top.Any())
                {
                    target_price_long_top = active_long_top.Average(b => b.targetValue);
                    target_percentage_long_top = active_long_top.Average(b => b.targetValue / current_price - 1);
                    bullishness_long_top = (double)active_long_top.Where(b => b.targetValue >= current_price).Count() / (double)actions_long_top;
                    consensus_long_top = Math.Max(0,1 - Calculate_STD(active_long_top.Select(b => b.targetValue)) / current_price);
                }
            }

            if (record.Any())
            {
                var r = record.First();

                r.analysts = analysts;
                r.analysts_top = analysts_top;
                r.actions_short = actions_short;
                r.actions_medium = actions_medium;
                r.actions_long = actions_long;
                r.actions_short_top = actions_short_top;
                r.actions_medium_top = actions_medium_top;
                r.actions_long_top = actions_long_top;
                r.analysts_short = analysts_short;
                r.analysts_medium = analysts_medium;
                r.analysts_long = analysts_long;
                r.analysts_short_top = analysts_short_top;
                r.analysts_medium_top = analysts_medium_top;
                r.analysts_long_top = analysts_long_top;

                if (active_short.Any())
                {
                    r.target_price_short = target_price_short;
                    r.target_percentage_short = target_percentage_short;
                    r.bullishness_short = bullishness_short;
                    r.consensus_short = consensus_short;

                    if (active_short_top.Any())
                    {
                        r.target_price_short_top = target_price_short_top;
                        r.target_percentage_short_top = target_percentage_short_top;
                        r.bullishness_short_top = bullishness_short_top;
                        r.consensus_short_top = consensus_short_top;
                    }
                }

                if (active_medium.Any())
                {
                    r.target_price_medium = target_price_medium;
                    r.target_percentage_medium = target_percentage_medium;
                    r.bullishness_medium = bullishness_medium;
                    r.consensus_medium = consensus_medium;

                    if (active_medium_top.Any())
                    {
                        r.target_price_medium_top = target_price_medium_top;
                        r.target_percentage_medium_top = target_percentage_medium_top;
                        r.bullishness_medium_top = bullishness_medium_top;
                        r.consensus_medium_top = consensus_medium_top;
                    }
                }

                if (active_long.Any())
                {
                    r.target_price_long = target_price_long;
                    r.target_percentage_long = target_percentage_long;
                    r.bullishness_long = bullishness_long;
                    r.consensus_long = consensus_long;

                    if (active_long_top.Any())
                    {
                        r.target_price_long_top = target_price_long_top;
                        r.target_percentage_long_top = target_percentage_long_top;
                        r.bullishness_long_top = bullishness_long_top;
                        r.consensus_long_top = consensus_long_top;
                    }
                }

                r.consensus_overall = consensus_overall;
                if (a_top.Any())
                    r.consensus_overall_top = consensus_overall_top;

            }
            else {
                Active_Ticker at = new Active_Ticker();

                at.ticker = ticker;
                at.analysts = analysts;
                at.analysts_top = analysts_top;
                at.actions_short = actions_short;
                at.actions_short_top = actions_short_top;
                at.actions_medium = actions_medium;
                at.actions_medium_top = actions_medium_top;
                at.actions_long = actions_long;
                at.actions_long_top = actions_long_top;
                at.analysts_short = analysts_short;
                at.analysts_medium = analysts_medium;
                at.analysts_long = analysts_long;
                at.analysts_short_top = analysts_short_top;
                at.analysts_medium_top = analysts_medium_top;
                at.analysts_long_top = analysts_long_top;

                if (active_short.Any()) {
                    at.target_price_short = target_price_short;
                    at.target_percentage_short = target_percentage_short;
                    at.bullishness_short = bullishness_short;
                    at.consensus_short = consensus_short;

                    if (active_short_top.Any()) {
                        at.target_price_short_top = target_price_short_top;
                        at.target_percentage_short_top = target_percentage_short_top;
                        at.bullishness_short_top = at.bullishness_short_top;
                        at.consensus_short_top = consensus_short_top;
                    }
                }

                if (active_medium.Any())
                {
                    at.target_price_medium = target_price_medium;
                    at.target_percentage_medium = target_percentage_medium;
                    at.bullishness_medium = bullishness_medium;
                    at.consensus_medium = consensus_medium;

                    if (active_medium_top.Any())
                    {
                        at.target_price_medium_top = target_price_medium_top;
                        at.target_percentage_medium_top = target_percentage_medium_top;
                        at.bullishness_medium_top = at.bullishness_medium_top;
                        at.consensus_medium_top = consensus_medium_top;
                    }
                }

                if (active_long.Any())
                {
                    at.target_price_long = target_price_long;
                    at.target_percentage_long = target_percentage_long;
                    at.bullishness_long = bullishness_long;
                    at.consensus_long = consensus_long;

                    if (active_long_top.Any())
                    {
                        at.target_price_long_top = target_price_long_top;
                        at.target_percentage_long_top = target_percentage_long_top;
                        at.bullishness_long_top = at.bullishness_long_top;
                        at.consensus_long_top = consensus_long_top;
                    }
                }

                at.consensus_overall = consensus_overall;
                if (a_top.Any())
                    at.consensus_overall_top = consensus_overall_top;

                db.Active_Tickers.InsertOnSubmit(at);
            }

            db.SubmitChanges();

        }
    }

    protected void TrackRecord_Tickers()
    {
        DataClassesDataContext db = new DataClassesDataContext();

        var aps = (from temp in db.AnalystPerformances where temp.ticker.HasValue select temp).GroupBy(b => b.ticker);
        
        foreach (var ap in aps) {
            int ticker = ap.First().ticker.Value;
            int actions = ap.Sum(b => b.actions);
            double avg = ap.Sum(b => b.actions * b.return_average) / ap.Sum(b => b.actions);
            double alpha = ap.Sum(b => b.actions * b.alpha_average) / ap.Sum(b => b.actions);
            double accuracy = ap.Sum(b => b.actions * b.accuracy_average) / ap.Sum(b => b.actions);
            int analysts = ap.GroupBy(b => b.analyst).Count();

            // filter out top analysts
            double avg_top = 0;
            double alpha_top = 0;
            int actions_top = 0;
            double accuracy_top = 0;
            int analysts_top = 0;

            var tops = ap.Where(b => b.rank <= 10 && b.return_average > 0);

            if (tops.Any()) {
                actions_top = tops.Sum(b => b.actions);
                avg_top = tops.Sum(b => b.actions * b.return_average) / tops.Sum(b => b.actions);
                alpha_top = tops.Sum(b => b.actions * b.alpha_average) / tops.Sum(b => b.actions);
                accuracy_top = tops.Sum(b => b.actions * b.accuracy_average) / tops.Sum(b => b.actions);
                analysts_top = tops.GroupBy(b => b.analyst).Count();
            }
            
            var agg = from temp in db.TrackRecord_Tickers where temp.ticker == ap.First().ticker select temp;
            if (agg.Any())
            {
                agg.First().ticker = ticker;
                agg.First().avg = avg;
                agg.First().alpha = alpha;
                agg.First().actions = actions;
                agg.First().accuracy = accuracy;
                agg.First().analysts = analysts;

                if (tops.Any())
                {
                    agg.First().actions_top = actions_top;
                    agg.First().avg_top = avg_top;
                    agg.First().alpha_top = alpha_top;
                    agg.First().accuracy_top = accuracy_top;
                    agg.First().analysts_top = analysts_top;
                }
            }
            else
            {
                TrackRecord_Ticker a = new TrackRecord_Ticker();

                a.ticker = ticker;
                a.avg = avg;
                a.alpha = alpha;
                a.actions = actions;
                a.accuracy = accuracy;
                a.analysts = analysts;

                if (tops.Any())
                {
                    a.actions_top = actions_top;
                    a.avg_top = avg_top;
                    a.alpha_top = alpha_top;
                    a.accuracy_top = accuracy_top;
                    a.analysts_top = analysts_top;
                }
                db.TrackRecord_Tickers.InsertOnSubmit(a);
            }
            
            db.SubmitChanges();
        }
    }

    protected void calculate_universal_metrics() {
        DataClassesDataContext db = new DataClassesDataContext();
        var aps = from temp in db.AnalystPerformances select temp;
        var m = from temp in db.Metrics_Masters select temp;

        try
        {
            var x =  aps.Where(b=>b.ticker.HasValue).GroupBy(b=>b.ticker);
            double ret_max = 0;
            double ret_min = 0;
            double alpha_max = 0;
            double alpha_min = 0;
            int analyst_max = 1;
            int analyst_min = 1;
            int history_max = 1;
            int history_min = 1;

            double interim_ret_avg;
            double interim_alpha_avg;
            int interim_analyst_count;
            int interim_actions_count;

            foreach (var y in x){
                if (y.Count() > 5)
                {
                    interim_ret_avg = y.Average(b => b.return_average);
                    interim_alpha_avg = y.Average(b => b.alpha_average);

                    ret_max = Math.Max(ret_max, interim_ret_avg);
                    ret_min = Math.Min(ret_min, interim_ret_avg);
                    alpha_max = Math.Max(alpha_max, interim_alpha_avg);
                    alpha_min = Math.Min(alpha_min, interim_alpha_avg);
                }
            }

            m.Where(b => b.id == 5).First().val_float = ret_max;
            m.Where(b => b.id == 6).First().val_float = ret_min;
            m.Where(b => b.id == 7).First().val_float = alpha_max;
            m.Where(b => b.id == 8).First().val_float = alpha_min;

            var actions = from temp in db.Actions select temp;
            var aas = actions.Where(b => b.active).GroupBy(b => b.ticker);
            foreach (var aa in aas) {
                interim_analyst_count = aa.GroupBy(b => b.article1.origin).Count();

                analyst_max = Math.Max(analyst_max, interim_analyst_count);
                analyst_min = Math.Min(analyst_min, interim_analyst_count);
            }

            var ahs = actions.Where(b => !b.active).GroupBy(b => b.ticker);
            foreach (var aa in ahs) {
                interim_actions_count = aa.Count();

                history_max = Math.Max(history_max, interim_actions_count);
                history_min = Math.Min(history_min, interim_actions_count);

            }

            m.Where(b => b.id == 9).First().val_int = analyst_max;
            m.Where(b => b.id == 10).First().val_int = analyst_min;
            m.Where(b => b.id == 11).First().val_int = history_max;
            m.Where(b => b.id == 12).First().val_int = history_min;

            db.SubmitChanges();

        }
        catch { 
        
        }
    }

    protected void z() {
        DataClassesDataContext db = new DataClassesDataContext();

        var a = (from temp in db.Actions where !temp.fund.peer_group.HasValue && !temp.fund.sector.HasValue select temp).GroupBy(b=>b.ticker);

        foreach (var b in a) {
            if (b.First().fund.sector.HasValue && b.First().fund.peer_group.HasValue)
                Response.Write(b.First().fund.Sector1.sector1 + " - " + b.First().fund.Peer_Group1.name + "<br>");
            else
                Response.Write("<font style=\"color:Red\">" + b.First().ticker + "- " + b.First().fund.name + "</font><br>");
        }
    }

    protected void y() {
        DataClassesDataContext db = new DataClassesDataContext();
        var a = from temp in db.funds where !temp.sector.HasValue && temp.peer_group.HasValue select temp;

        foreach (var b in a) {
            try
            {
                b.sector = b.Peer_Group1.sector;
                db.SubmitChanges();
            }
            catch {
                Response.Write("OOps!<BR>");
            }
        }
    }

    protected void x() {
        DataClassesDataContext db = new DataClassesDataContext();
        int x = 0;

        while (x < 100)
        {
            var fs = from temp in db.funds where !temp.peer_group.HasValue select temp;
            int peer = 0;

            foreach (var f in fs)
            {
                peer = DataBaseLayer.get_peer_group(f.ticker.Trim());

                if (peer != 0)
                {
                    f.peer_group = peer;
                    try
                    {
                        db.SubmitChanges();
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            x++;
        }

        //var a = from temp in db.Peer_Groups select temp;
        //foreach (var b in a) {
        //    b.name = HttpUtility.HtmlDecode(b.name);
        //    db.SubmitChanges();

        //HtmlWeb hw = new HtmlWeb();
        //HtmlDocument ho = hw.Load("http://finance.yahoo.com/q/in?ql=1&s=AAPL");
        //HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//td");

        ////for (int i = 0; i < 40; i++) {
        ////    Response.Write(i + "- " + m[i].InnerHtml + "<br>");
        ////}

        //Response.Write(m[13].InnerText);

    }

    protected void fill_peer_groups() {
        DataClassesDataContext db = new DataClassesDataContext();

        var a = from temp in db.Sectors select temp;

        if (a.Any()) {
            foreach (var b in a) {
                string[] x = b.industry.Split(',');

                foreach (string y in x) {
                    var pp = from temp in db.Peer_Groups where temp.name == y.Trim() select temp;

                    if (!pp.Any() && !string.IsNullOrEmpty(y.Trim())) {
                        Peer_Group p = new Peer_Group();
                        p.sector = b.id;
                        p.name = y.Trim();
                        db.Peer_Groups.InsertOnSubmit(p);
                        db.SubmitChanges();
                    }
                }
            }
        }
    }

    protected void find_no_sectors()
    {
        int c = 1;
        DataClassesDataContext db = new DataClassesDataContext(); {
            var a = (from temp in db.Actions where temp.active && !temp.fund.sector.HasValue select temp).GroupBy(b=>b.ticker);

            foreach (var b in a) {
                Response.Write(c + "- " + b.First().fund.name + " (" + b.First().fund.ticker + ")<br>");
                c++;
            }
        }
    }

    protected void calculate_sector_metrics() {
        // short 99     183
        // medium 107   
        // long 98      548

        DataClassesDataContext db = new DataClassesDataContext();

        for (int i = 1; i < 9; i++) {
            var z = from temp in db.Actions where temp.fund.sector == i && temp.active select temp;

            var x = z.Where(b => Math.Abs((b.targetDate - DateTime.Now).TotalDays) < 183);
            calculate_and_insert_metric_sector(x, db, i, 99);

            x = z.Where(b => Math.Abs((b.targetDate - DateTime.Now).TotalDays) >= 183 && Math.Abs((b.targetDate - DateTime.Now).TotalDays) < 548);
            calculate_and_insert_metric_sector(x, db, i, 107);

            x = z.Where(b => Math.Abs((b.targetDate - DateTime.Now).TotalDays) >= 548);
            calculate_and_insert_metric_sector(x, db, i, 98);
        }
    }

    protected void calculate_industry_metrics() {
        DataClassesDataContext db = new DataClassesDataContext();

        var industries = from temp in db.Peer_Groups select temp;

        if (industries.Any()) {
            foreach (var i in industries) {
                var z = from temp in db.Actions where temp.fund.peer_group == i.id && temp.active select temp;

                if (z.Any()) {
                    var x = z.Where(b => Math.Abs((b.targetDate - DateTime.Now).TotalDays) >= 183 && Math.Abs((b.targetDate - DateTime.Now).TotalDays) < 548);
                    calculate_and_insert_metric_industry(x, db, i.id);
                }

            }
        }
    }

    protected void calculate_ticker_metrics()
    {
        // short 99     183
        // medium 107   
        // long 98      548

        DataClassesDataContext db = new DataClassesDataContext();

        var fs = (from temp in db.Actions where temp.active select temp).GroupBy(b=>b.ticker);

        foreach (var f in fs)
        {
            //var z = from temp in db.Actions where temp.fund.sector == i && temp.active select temp;

            var x = f.Where(b => Math.Abs((b.targetDate - DateTime.Now).TotalDays) < 183);
            calculate_and_insert_metric_ticker(x, db, f.First().ticker, 99);

            x = f.Where(b => Math.Abs((b.targetDate - DateTime.Now).TotalDays) >= 183 && Math.Abs((b.targetDate - DateTime.Now).TotalDays) < 548);
            calculate_and_insert_metric_ticker(x, db, f.First().ticker, 107);

            x = f.Where(b => Math.Abs((b.targetDate - DateTime.Now).TotalDays) >= 548);
            calculate_and_insert_metric_ticker(x, db, f.First().ticker, 98);
        }
    }

    protected void calculate_and_insert_metric_industry(IEnumerable<Action> x, DataClassesDataContext db, int industry)
    {
        int bullish = 0;

        if (x.Any())
        {

            Metrics_Industry m = new Metrics_Industry();
            foreach (var y in x)
            {
                var price = from temp in db.fund_values where temp.fundID == y.ticker && temp.isLatest.Value select temp;

                if (price.Any())
                {
                    double current = price.First().closeValue.Value;
                    double diff = y.targetValue - current;
                    if (diff > 0)
                        bullish++;

                    m.average += diff / current;
                }
            }

            m.bullishness = bullish / (double)x.Count();
            m.actions = x.Count();
            m.average /= x.Count();
            m.industry = industry;
            m.date = DateTime.Now;
            db.Metrics_Industries.InsertOnSubmit(m);
            db.SubmitChanges();
        }
    }

    protected void calculate_and_insert_metric_sector(IEnumerable<Action> x,DataClassesDataContext db,int sector,int horizon){

        int rank_cutoff = 10;

        bool atop = false;
        double average_top = 0;
        int top_bullish = 0;
        int top_total = 0;
        int bullish = 0;

        if (x.Any()) {

            Metrics_Sector m = new Metrics_Sector();
            foreach (var y in x)
            {
                var price = from temp in db.fund_values where temp.fundID == y.ticker && temp.isLatest.Value select temp;

                if (price.Any()) {
                    double current = price.First().closeValue.Value;
                    double diff = y.targetValue - current;
                    if (diff > 0)
                        bullish++;

                    m.average += diff / current;

                    var p = from temp in db.AnalystPerformances where temp.horizon == 0 && temp.analyst == y.article1.origin && temp.rank <= rank_cutoff && temp.return_average > 0 select temp;

                    if (sector > 8)
                    {
                        p = p.Where(b => b.ticker == sector);
                    }
                    else
                    {
                        p = p.Where(b => b.sector == sector);
                    }


                    if (p.Any())
                    {
                        atop = true;

                        average_top += diff / current;
                        top_total++;
                        if (diff > 0)
                            top_bullish++;
                    }
                }
            }

            if (atop == true) {
                m.bullishness_top = (double)top_bullish / (double)top_total;
                m.average_top = average_top / (double)top_total;
                m.actions_top = top_total;
            }

            m.bullishness = bullish / (double)x.Count();
            m.actions = x.Count();
            m.date = DateTime.Now;
            m.average /= x.Count();
            m.sector = sector;
            m.horizon = horizon;
            db.Metrics_Sectors.InsertOnSubmit(m);
            db.SubmitChanges();
        }
    }

    protected void calculate_and_insert_metric_ticker(IEnumerable<Action> x, DataClassesDataContext db, int ticker, int horizon)
    {

        int rank_cutoff = 10;

        int bullish = 0;
        bool atop = false;
        double average_top = 0;
        int top_bullish = 0;
        int top_total = 0;

        if (x.Any())
        {

            Metrics_Ticker m = new Metrics_Ticker();
            foreach (var y in x)
            {
                var price = from temp in db.fund_values where temp.fundID == y.ticker && temp.isLatest.Value select temp;

                if (price.Any()) {
                    double current = price.First().closeValue.Value;
                    double diff = y.targetValue - current;
                    if (diff > 0)
                        bullish++;

                    m.average += diff / current;

                    var p = from temp in db.AnalystPerformances where temp.horizon == 0 && temp.analyst == y.article1.origin && temp.rank <= rank_cutoff && temp.return_average > 0 select temp;

                    if (ticker > 8)
                    {
                        p = p.Where(b => b.ticker == ticker);
                    }
                    else
                    {
                        p = p.Where(b => b.sector == ticker);
                    }


                    if (p.Any())
                    {
                        atop = true;

                        average_top += diff / current;
                        top_total++;
                        if (diff > 0)
                            top_bullish++;
                    }
                }
            }

            if (atop == true)
            {
                //m.bullishness_top = (double)top_bullish / (double)top_total;
                //m.average_top = average_top / (double)top_total;
                //m.actions_top = top_total;
            }

            //m = x.Count();
            m.date = DateTime.Now;
            m.average /= x.Count();
            m.bullishness = bullish / (double)x.Count();
            m.ticker = ticker;
            m.horizon = horizon;
            db.Metrics_Tickers.InsertOnSubmit(m);
            db.SubmitChanges();
        }
    }

    protected void show_action_grouped_by_ticker_distribution()
    {
        DataClassesDataContext db = new DataClassesDataContext(); {
            var a = (from temp in db.Actions where temp.active == true select temp).GroupBy(b=>b.ticker).OrderByDescending(b=>b.Count());
            int c=1;

            foreach (var b in a) {
                Response.Write(c + "- " + b.First().fund.ticker + "(" + b.Count() + ")<br>");
                c++;
            }
        }
    }

    protected void insert_fund_values_of_existing_funds() {
        string ticker = "YCC";

        Quote fundQuote = new Quote(ticker);
        DataClassesDataContext db = new DataClassesDataContext();
        fund newFund = new fund();
        newFund = (from temp in db.funds where temp.ticker == ticker select temp).First();

        DataBaseLayer.insert_fund_data(fundQuote, db, newFund);
    }

    protected void some_analyst_tests() {
        DataClassesDataContext db = new DataClassesDataContext();
        string z = "";

        var a = (from temp in db.Actions where temp.ticker == 699 && temp.expired == false && temp.active==true select temp).OrderBy(b=>b.targetDate);
        foreach (var b in a) {
            z="";

            var ap = from temp in db.AnalystPerformances where temp.ticker == b.ticker && temp.analyst == b.article1.origin select temp;

            if (ap.Any()) {
                z = "(" + ap.First().rank + " - " + ap.First().return_average + " - " + ap.First().alpha_average + " - " + ap.First().accuracy_average + ")";
            }

            string x = string.Format("{0:M/d/yyyy}", b.targetDate);
            string y = b.targetValue.ToString();
            Response.Write(y + z + "<br>");
            //Response.Write(z + "<br>");
        }


        //var a = (from temp in db.Actions where temp.matured == false && temp.expired == false && temp.breached == false select temp).GroupBy(b => b.ticker).OrderByDescending(c => c.Count());

        //foreach (var b in a) {
        //    Response.Write(b.Count() + "- " + b.First().fund.ticker + "<br>");
        //}
    }

    protected void action_screw_up() {
        if (!string.IsNullOrEmpty(Request.QueryString["action"]) && !string.IsNullOrEmpty(Request.QueryString["ticker"]))
        {
            DataClassesDataContext db = new DataClassesDataContext();
            try
            {
                var a = from temp in db.Actions where temp.actionID == Convert.ToInt32(Request.QueryString["action"]) select temp;
                a.First().ticker = Convert.ToInt32(Request.QueryString["ticker"]);
                db.SubmitChanges();
                Response.Write("ok");

            }
            catch
            {
                Response.Write("oh oh");
            }
        }
        else
        {
            guess_action_tickers();
        }
    }

    protected void guess_action_tickers()
    {
        int c = 0;
        string x = "";

        DataClassesDataContext db = new DataClassesDataContext();
        {
            var actions = from temp in db.Actions where temp.ticker==785 select temp;

            foreach (var a in actions) {
                //var ats = from temp in db.ArticleTickers where temp.article == a.article && temp.actions > 0 select temp;

                //if (ats.Count() == 1)
                //{
                //    a.ticker = ats.First().ticker;
                //    db.SubmitChanges();
                //    Response.Write(a.article1.title + " set to " + ats.First().fund.ticker + " <br>");
                //}
                //else if (ats.Count() == 0)
                //{
                //    Response.Write(a.article1.title + "<br>");
                //}


                var fvs = from temp in db.fund_values where temp.date == a.startDate && temp.closeValue == a.startValue select temp;
                c = fvs.Count();

                if (c == 0)
                {
                    Response.Write(a.actionID + " not found<br>");
                }
                else if (c == 1)
                {
                    //a.ticker = fvs.First().fundID;
                    //db.SubmitChanges();
                }
                else
                {
                    x = "";

                    foreach (var f in fvs)
                    {
                        x += "<a href=\"?action=" + a.actionID + "&ticker=" + f.fundID + "\">" + f.fund.ticker + "</a> -";
                    }

                    Response.Write(a.actionID + " multiple - <a href=\"" + a.article1.url + "\" target=\"_blank\">" + a.article1.title + "</a> " + x + "<br>");
                }

            }
        }
    }

    protected void reset_all_actions() {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions select temp;

        foreach (var a in actions) {
            a.dividend = 0;
            a.currentValue = a.startValue;
            a.matured = false;
            a.breached = false;
            a.maxValue = a.startValue;
            a.minValue = a.maxValue;
            a.date_feed = a.startDate;
            a.lastUpdated = a.startDate;
            db.SubmitChanges();
        }
    }

    protected void add_main_subscription() {
        DataClassesDataContext db = new DataClassesDataContext();
        var aps = from temp in db.AnalystPerformances where temp.horizon == 0 && !temp.sector.HasValue && !temp.ticker.HasValue select temp;

        foreach (var ap in aps) {
            var ass = from temp in db.Subscriptions where temp.analyst == ap.analyst select temp;

            if (!ass.Any()) {
                Subscription s = new Subscription();
                s.analyst = ap.analyst;
                s.name = "Main";
                s.description = "Main subscription";
                s.premium = false;
                s.article_count = 0;
                s.region = 5;
                s.style = 11;
                s.assetclass = 47;
                s.analysis = 1;
                s.horizon = 83;
                s.price = 0;
                db.Subscriptions.InsertOnSubmit(s);
                db.SubmitChanges();


            }

        }
    }

    protected void count_no_logos() {
        DataClassesDataContext db = new DataClassesDataContext();
        var t = (from temp in db.Actions select temp).GroupBy(b=>b.ticker);
        int c = 1;

        if (t.Any()) {
            foreach (var tt in t) {
                string[] y = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath("~") + "\\images\\logo\\", tt.First().fund.ticker.Trim() + ".*");
                if (!y.Any())
                {
                    Response.Write(c + "- " + tt.First().fund.ticker.Trim() + "<br>");
                    c++;
                }
            }
            
        }

    }

    protected void count_analysts_with_actions() {
        DataClassesDataContext db = new DataClassesDataContext();
        int a = (from temp in db.AnalystPerformances select temp).GroupBy(b => b.analyst).Count();
        Response.Write(a);
    }

    protected void find_new_analysts() {
        DataClassesDataContext db = new DataClassesDataContext();
        var analysts = (from temp in db.articles where temp.type == 1 && temp.action == false select temp).GroupBy(b => b.origin).OrderByDescending(b => b.Count()) ;
        //string x = "NOT RANKED";

        if (analysts.Any()) {
            

            foreach (var a in analysts) {
                
                var r = from temp in db.AnalystPerformances where temp.analyst == a.First().origin select temp;
                if (!r.Any()) {
                    foreach (var b in a)
                    {
                        b.type = 2;
                    }
                    db.SubmitChanges();
                    //Response.Write(c + "- " + a.Count() + "- " + a.First().user.display_name + " - " + a.First().origin + " - " + x + "<br>");
                    //Response.Write(c + "- " + a.Count() + "- " + a.First().user.display_name + " - " + a.First().origin + "<br>");
                    //c++;
                }
                    

                
            }
        }
    }

    protected void find_analysts(int actions,DateTime date) {
        DataClassesDataContext db = new DataClassesDataContext();
        var analysts = from temp in db.AnalystPerformances where temp.horizon == 0 && !temp.sector.HasValue && !temp.ticker.HasValue && temp.actions>actions && temp.user.articles.Where(b=>b.date>date).Any() select temp;
        analysts = analysts.OrderBy(b => b.rank);

        if (analysts.Any())
        {
            foreach (var a in analysts) {
                Response.Write(a.rank + " - " + a.analyst + "- " + a.user.display_name + "<br>");
            }
        }


    }

    protected void apology(string email,string name,string url) {
        MailMessage newMsg = new MailMessage();
        newMsg.From = new MailAddress("support@invesd.com", "Invesd");

        // add analyst name to the email - personal touch
        newMsg.To.Add(new MailAddress(email, name));
        newMsg.Bcc.Add(new MailAddress("analyst@invesd.com", "Invesd"));
        newMsg.Subject = "You were almost there";
        newMsg.IsBodyHtml = true;

        // body
        newMsg.Body = @"<html>
                <table style=""margin-right:auto;margin-left:auto;"">
                    <tr>
                        <td style=""width:550px;"">
                            <img src=""http://invesd.com/images/invesd_logo.png"" width=""130"" height=""32"" style=""width:130px;height:32px"" alt=""Invesd"">
                        </td>
                    </tr>
                    <tr>
                        <td style=""font-size:15px;font-weight:100;font-family:helvetica;background-color:#1b1b1b;color:#cccccc;letter-spacing:1px"">
                            &nbsp;Don't just invest!
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p style=""font-family:helvetica;color:#000000;font-size:10pt;margin:10px 0 0 0"">Hello " + name.Trim() + @",</p>
                            <p style=""font-family:helvetica;color:#000000;font-size:10pt;margin:10px 0 0 0"">Thank you for visiting Invesd.</p>
                            <p style=""font-family:helvetica;color:#000000;font-size:10pt;margin:10px 0 0 0"">In order to unlock your Invesd account please <a style=""color:#0088cc;font-family:helvetica"" href=""" + url + @""">click here</a>. If you experience any difficulties with account activation please contact us at <a style=""color:#0088cc;font-family:helvetica"" href=""mailto:support@invesd.com"">support@invesd.com</a>.</p>
                            <p style=""font-family:helvetica;color:#000000;font-size:10pt;margin:10px 0 0 0"">Happy investing,<br />Invesd</p>
                        </td>
                    </tr>
                </table>
            </html>";
        try
        {
            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.UseDefaultCredentials = true;
                smtp.Send(newMsg);
            }
        }
        catch
        {
            Gripe g = new Gripe();
            g.message = "couldn't send email to " + name + " " + email;
            g.date = DateTime.Now;
            g.userid = 2;
            DataClassesDataContext db = new DataClassesDataContext();
            db.Gripes.InsertOnSubmit(g);
            db.SubmitChanges();
        }
    }

    private double Calculate_STD(IEnumerable<double> values)
    {
        double ret = 0;

        if (values.Count() > 1)
        {
            double avg = values.Average();
            double sum = values.Sum(b => Math.Pow(b - avg, 2));
            ret = Math.Sqrt(sum / (values.Count() - 1));
        }

        return ret;
    }
  
    //protected void testGoogle_Click(object sender, EventArgs e)
    //{
    //    double? tmp = AdminBackend.testGoogle(ticker.Text);
    //    if (tmp == null)
    //    {
    //        Response.Write("No, Stock price is not going to get updated");
    //    }
    //    else
    //    {
    //        Response.Write("Yes, Stock close price is going to be updated to:" + AdminBackend.testGoogle(ticker.Text));
    //    }
    //}
}

//public class ActionDate
//{    
//    public string ticker;
//    public string analyst_bloombergID;
//    public string analyst_name;
//    public string broker_code;
//    public string broker_name;
//    public string inDate;
//    public string inPtx;
//}

public class streetinsider {
    public string broker { get; set; }
    public double tgx { get; set; }
    public string url_next { get; set; }
    public string url_summary { get; set; }
}

