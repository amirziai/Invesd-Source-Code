using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Text.RegularExpressions;

public partial class admin_Consensus : System.Web.UI.Page
{
    public bool display = false;
    public double price_universal = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        calculate_consensus_history();
    }

    protected void calculate_consensus_history()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var ticker = from temp in db.funds where temp.ticker == Request.QueryString["ticker"] select temp;
        if (ticker.Any())
        {
            var ticker_first = ticker.First();
            logo.ImageUrl = DataBaseLayer.get_logo_smart(Request.QueryString["ticker"]);
            ticker_info.Text = "<b>" + ticker_first.name + "</b><br>" + ticker_first.Sector1.sector1 + "<br>" + ticker_first.Peer_Group1.name + "<br><small style=\"color:gray\">" + Ancillary.string_cutter(ticker_first.description,125,false,null) + "</small>";

            Dictionary<int, double> confidence_exists = calculate_confidence(db, ticker.First()); // constant
            
            int count = 0;
            int no_of_months = 1;
            int no_of_days = 30;

            try {
                if (!string.IsNullOrEmpty(Request.QueryString["months"]))
                    no_of_months = Convert.ToInt32( Request.QueryString["months"] );
            }
            catch { }
            try
            {
                if (!string.IsNullOrEmpty(Request.QueryString["days"]))
                    no_of_days = Convert.ToInt32(Request.QueryString["days"]);
            }
            catch { }

            List<consensus> consensus_time_series = new List<consensus>();
            List<Tuple<DateTime, consensus>> consensus_date_value = new List<Tuple<DateTime, consensus>>();

            for (DateTime i = DateTime.Now.AddDays(-(no_of_months - 1)*30); i <= DateTime.Now; i = i.AddDays(no_of_days))
            {
                count++;
                if (i.AddDays(no_of_days)>DateTime.Now )
                    display = true;

                DateTime ref_date = i;
                List<action> actions_query = query_for_consensus(db,ticker.First(), ref_date);
                consensus cons = new consensus();
                double? price = null;
                if (actions_query.Any())
                {
                    cons = consensus_calculator(actions_query, ref_date, confidence_exists, db, ticker.First());
                    consensus_time_series.Add(cons);
                    consensus_date_value.Add(new Tuple<DateTime, consensus>(i, cons));
                }
                
                price = last_price(db,ticker.First(),ref_date);

                tabulate_consensus(cons, ref_date, price);
                plot_consensus(cons, ref_date, price);

                if (display)
                {
                    plot_consensus_changes(consensus_date_value);
                }
                
            }

            //peers(ticker.First());
        }
    }

    // main calculator
    protected consensus consensus_calculator(List<action> actions_query, DateTime ref_date, Dictionary<int, double> confidence_exists,DataClassesDataContext db,fund ticker)
    {
        consensus cons = new consensus();
        //List<List<action>> actions_analyst_group = actions_query.GroupBy(b => b.analyst).Select(b => b.ToList()).OrderByDescending(b => b.OrderByDescending(c => c.started).First().started).ToList();

        if (actions_query.Count() >= 3 || confidence_exists.Sum(b => b.Value) >= 1) // need at least 3 estimates OR a sum confidence of at least 100% (high bar) to create a total consensus
        {
            Tuple<double, double> mu_sigma = calculate_range_metrics(actions_query, db, confidence_exists, ticker);
            List<PT_weights_item> PT_weights_list = calculate_PT_weights_list(actions_query, confidence_exists, mu_sigma.Item1, mu_sigma.Item2, ref_date, ticker);
            cons = calculate_consensus(PT_weights_list);


            // some outputs for last consensus
            if (display)
            {
                plot_pie_chart(PT_weights_list, cons);
                plot_confidence_distribution(PT_weights_list);
                tabulate_consensus_now(cons);
                scatter_plot(PT_weights_list, mu_sigma.Item1,mu_sigma.Item2);
                info.Text += "Current price: " + Math.Round(price_universal,2);
            }
        }
        
        return cons;
    }




    // query (ticker, ref date)
    protected List<action> query_for_consensus(DataClassesDataContext db, fund ticker,DateTime ref_date)
    {
        int days_recency = 120;
        int days_horizon = 365;

        var query_raw = db.Actions.Where(b => b.ticker == ticker.fundID
            && (ref_date - b.date_feed).TotalDays <= days_recency
            && ( ref_date >= b.date_feed || ref_date >= b.startDate)
            && b.targetDate > ref_date
            && (b.targetDate - ref_date).TotalDays <= days_horizon  ).Select(b =>new action { started = b.startDate, ID = b.actionID, created = b.date_feed, target = b.targetValue, targetdate = b.user.bloomberg_broker.HasValue ? b.date_feed.AddDays(365).Date : b.targetDate.Date, analyst = b.analystID.Value, name = b.user.display_name, broker = b.user.bloomberg_broker.HasValue ? b.user.Bloomberg_Broker1.name : "Invesd", rationale = b.rational, wall_st = b.user.bloomberg_broker.HasValue, estimate = true }).Concat(db.ActionMonitors.Where(b => b.ticker == ticker.fundID && b.monitorEnd > ref_date && (b.monitorEnd - ref_date).TotalDays <= 365 && ref_date >= b.investment_date).Select(c => new action { started = c.investment_date.AddDays(0), ID = c.ID, created = c.investment_date, target = c.targetValue, targetdate = c.monitorEnd.Date, analyst = c.usermon, name = c.user.display_name, broker = "Invesd", rationale = c.rationale, wall_st = false, estimate = false }));

        List<action> output = new List<action>();
        foreach (var query_analyst in query_raw.GroupBy(b=>b.analyst))
        {
            if (query_analyst.Count() == 1)
                output.Add(query_analyst.First());
            else
            {
                var query_analyst_estimates = query_analyst.Where(b=>b.estimate);
                var query_analyst_positions = query_analyst.Where(b=>!b.estimate);
                
                // estimates only
                if (query_analyst_estimates.Any() && !query_analyst_positions.Any())
                {
                    output.Add(query_analyst_estimates.OrderByDescending(b => b.created).First());
                }
                else if (!query_analyst_estimates.Any() && query_analyst_positions.Any())
                { // position only
                    output.Add(query_analyst_positions.OrderByDescending(b => b.created).First());
                }
                else
                { // both
                    var query_analyst_estimates_recent = query_analyst_estimates.OrderByDescending(b => b.created).First();
                    var query_analyst_positions_recent = query_analyst_positions.OrderByDescending(b => b.created).First();

                    double difference = (query_analyst_estimates_recent.created - query_analyst_positions_recent.created).TotalDays;
                    int days_threshold = 30; // number of days which estimate trumps position (logic: probably were created at the same time or around the same time and estimate is more intuitive in terms of PT)

                    if (Math.Abs(difference) <= days_threshold)
                    {
                        output.Add( query_analyst_estimates_recent ) ;
                    }
                    else
                    {
                        if (difference >= 0)
                            output.Add(query_analyst_estimates_recent);
                        else
                            output.Add(query_analyst_positions_recent);
                    }
                }
                

                
            }
        }

        return output.OrderByDescending(b=>b.created).ToList();
    }

    // get last price
    protected double? last_price(DataClassesDataContext db, fund ticker,DateTime ref_date)
    {
        try
        {
            return (from temp in db.fund_values where temp.fundID == ticker.fundID && temp.date <= ref_date select temp).OrderByDescending(b => b.date).First().adjValue;
        }
        catch
        {
            return null;
        }
    }

    // ancillary calculators
    protected List<PT_weights_item> calculate_PT_weights_list(List<action> actions_analyst_group, Dictionary<int, double> confidence_exists, double mu, double sigma, DateTime ref_date,fund ticker)
    {
        List<PT_weights_item> PT_weights_list = new List<PT_weights_item>();
        
        foreach (var actions_analyst in actions_analyst_group)
        {
            double confidence = 0;
            confidence_exists.TryGetValue(actions_analyst.analyst, out confidence);

            action item = actions_analyst;
            //if (actions_analyst.Where(b => b.estimate).Any())
            //{
            //    item = actions_analyst.Where(b => b.estimate).OrderByDescending(b => b.created).First();
            //}
            //else
            //{
            //    item = actions_analyst.OrderByDescending(b => b.created).First();
            //}

            PT_weights_item pt_item = new PT_weights_item();

            
            pt_item.analystID = item.analyst;
            pt_item.name = item.name;
            pt_item.broker = item.broker;
            pt_item.confidence = confidence;
            pt_item.recency = recency_multiplier(item.created, ref_date);
            pt_item.range = range_multiplier(mu, sigma, item.target);
            pt_item.expiration = expiration_multiplier(item.targetdate, ref_date);
            pt_item.target = item.target;
            pt_item.wall_st = item.wall_st;
            pt_item.created = item.started;
            pt_item.reiterated = item.created;
            pt_item.targetdate = item.targetdate;
            pt_item.rationale = item.rationale;
            pt_item.targetdate = item.targetdate;
            pt_item.estimate = item.estimate;
            //pt_item.performance = 

            PT_weights_list.Add(pt_item);
        }

        if (display)
            tabulate_last_consensus_constituents(PT_weights_list,ticker);
        
        return PT_weights_list;
    }

    protected consensus calculate_consensus(List<PT_weights_item> PT_weights_list)
    {
        consensus cons = new consensus();
        if (PT_weights_list.Any())
        {
            if (PT_weights_list.Count() >= 3 || PT_weights_list.Sum(b => b.confidence) >= 1)
            {
                cons.total_flat = PT_weights_list.Sum(b => b.recency * b.range * b.expiration * b.target) / PT_weights_list.Sum(b => b.recency * b.range * b.expiration);
                cons.total_flat_analysts = PT_weights_list.Where(b => b.recency * b.range * b.expiration > 0).Count();
                cons.total_flat_analysts_ws = PT_weights_list.Where(b => b.recency * b.range * b.expiration > 0 && b.wall_st).Count();

                if (PT_weights_list.Where(b => b.confidence > 0).Count() >= 3 || PT_weights_list.Sum(b => b.confidence) > 0)
                {
                    cons.total_weighted = PT_weights_list.Sum(b => b.confidence * b.recency * b.range * b.expiration * b.target) / PT_weights_list.Sum(b => b.confidence * b.recency * b.range * b.expiration);
                    cons.total_weighted_analysts = PT_weights_list.Where(b => b.confidence * b.recency * b.range * b.expiration > 0).Count();
                    cons.total_weighted_analysts_ws = PT_weights_list.Where(b => b.confidence * b.recency * b.range * b.expiration > 0 && b.wall_st).Count();
                }
                
            }
            
            var PT_weights_list_WS = PT_weights_list.Where(b => b.wall_st);
            if (PT_weights_list_WS.Any())
            {
                if (PT_weights_list_WS.Count() >= 3 || PT_weights_list_WS.Sum(b => b.confidence) >= 1)
                {
                    cons.ws_flat = PT_weights_list_WS.Sum(b => b.recency * b.range * b.expiration * b.target) / PT_weights_list_WS.Sum(b => b.recency * b.range * b.expiration);
                    cons.ws_flat_analysts = PT_weights_list_WS.Where(b => b.recency * b.range * b.expiration > 0).Count();

                    if (PT_weights_list_WS.Where(b => b.confidence > 0).Count() >= 3 || PT_weights_list_WS.Sum(b => b.confidence) > 0)
                    {
                        cons.ws_weighted = PT_weights_list_WS.Sum(b => b.confidence * b.recency * b.range * b.expiration * b.target) / PT_weights_list_WS.Sum(b => b.confidence * b.recency * b.range * b.expiration);
                        cons.ws_weighted_analysts = PT_weights_list_WS.Where(b => b.confidence * b.recency * b.range * b.expiration > 0).Count();
                    }
                }
            }
            
            var PT_weights_list_crowd = PT_weights_list.Where(b => !b.wall_st);
            if (PT_weights_list_crowd.Any())
            {
                if (PT_weights_list_crowd.Count() >= 3 || PT_weights_list_crowd.Sum(b => b.confidence) >= 1)
                {
                    
                    cons.crowd_flat = PT_weights_list_crowd.Sum(b => b.recency * b.range * b.expiration * b.target) / PT_weights_list_crowd.Sum(b => b.recency * b.range * b.expiration);
                    cons.crowd_flat_analysts = PT_weights_list_crowd.Where(b => b.recency * b.range * b.expiration > 0).Count();

                    if (PT_weights_list_crowd.Where(b=>b.confidence>0).Count()>=3 ||  PT_weights_list_crowd.Sum(b => b.confidence) > 1)
                    {
                        cons.crowd_weighted = PT_weights_list_crowd.Sum(b => b.confidence * b.recency * b.range * b.expiration * b.target) / PT_weights_list_crowd.Sum(b => b.confidence * b.recency * b.range * b.expiration);
                        cons.crowd_weighted_analysts = PT_weights_list_crowd.Where(b => b.confidence * b.recency * b.range * b.expiration > 0).Count();
                    }
                }
            }
            
        }

        return cons;
    }

    protected Tuple<double,double> calculate_range_metrics(List<action> actions_analyst_group, DataClassesDataContext db, Dictionary<int, double> confidence_exists,fund ticker)
    {
        List<Tuple<double, double>> confidence_PT = new List<Tuple<double, double>>();

        var current = (from temp in db.fund_values where temp.fundID == ticker.fundID select temp).OrderByDescending(b => b.date);
        if (current.Any())
        {
            price_universal = current.First().adjValue;
        }

        double mu = 0;
        double sigma = 0;
        foreach (var actions_analyst in actions_analyst_group) // aggregate values
        {
            double confidence = 0;
            double value = 0;
            confidence_exists.TryGetValue(actions_analyst.analyst, out confidence);
            value = actions_analyst.target; // target
            confidence_PT.Add(new Tuple<double, double>(confidence, value));
        }
        if (confidence_PT.Count >= 3 && confidence_PT.Sum(b => b.Item1) >= 1) // giant assumptions
        {
            double sum_confidence = confidence_PT.Sum(b => b.Item1);
            mu = confidence_PT.Sum(b => b.Item1 * b.Item2);
            mu /= sum_confidence;
            sigma = Math.Sqrt(confidence_PT.Sum(b => ((b.Item1 / sum_confidence) * Math.Pow(b.Item2 - mu, 2))));
        }
        else
        {
            mu = price_universal;
            sigma = mu * (ticker.std_1yr_return.HasValue ? ticker.std_1yr_return.Value : 0);
        }
        if (mu == 0)
        {
            Response.Write("<font color=red>FATAL ERROR</font><BR>");
        }
        if (sigma == 0)
        {
            sigma = mu / 2;
            info.Text += "<font color=red>NO SIGMA, ASSMUING Sigma = Avg / 2</font><br>";
        }
        
        if (display)
        {
            generic_push_to_javascript("range_upper", (mu + 2 * sigma).ToString());
            generic_push_to_javascript("range_lower", (mu - 2 * sigma).ToString());
            generic_push_to_javascript("current_price", (price_universal).ToString());
            info.Text += "Average: " + Math.Round(mu, 2) + ", STD: " + Math.Round(sigma,2) + " ";
            info.Text += "<font color=gray>Range: " + Math.Round(mu - 2 * sigma, 2) + "-" + Math.Round(mu + 2 * sigma, 2) + "</font><br>";
        }
        
        return new Tuple<double, double>(mu,sigma);
    }

    protected Dictionary<int, double> calculate_confidence(DataClassesDataContext db, fund ticker)
    {
        // constant AP assumption

        Dictionary<int, double> confidence_exists = new Dictionary<int, double>(); // hold confidence for analysts with confidence>0
        var ap = from temp in db.AnalystPerformances where temp.confidence.HasValue && temp.ticker == ticker.fundID select temp; // used to filter actions

        if (ap.Any()) // build confidence_exists dictionary of <analyst,confidence>
        {
            foreach (var a in ap)
            {
                if (a.confidence > 0)
                    confidence_exists.Add(a.analyst, a.confidence.Value);
            }
        }

        return confidence_exists;
    }

    protected void peers(fund ticker)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        if (ticker.peer_group.HasValue){
            var peers = from temp in db.funds where temp.peer_group == ticker.peer_group && temp.fundID!=ticker.fundID select temp;
            if (peers.Any())
            {
                List<Tuple<DateTime, fund, consensus, double?>> results = new List<Tuple<DateTime, fund, consensus, double?>>();

                int c = 0;
                foreach (var p in peers)
                {
                    c++;
                    l_peers.Text += c + "- " + p.name + " (" + p.ticker.Trim() + ")<br>";
                    Dictionary<int, double> confidence_exists = calculate_confidence(db, p); // constant

                    for (DateTime i=DateTime.Now.Date.AddYears(-1);i<=DateTime.Now.Date;i = i.AddMonths(4) ){
                        DateTime ref_date = i;
                        double? price = null;
                        price = last_price(db, p, ref_date);
                        l_peers.Text += string.Format("{0:MMM d, yy}", i) + p.ticker + " " + price  + "<br>";
                        List<action> actions_query = query_for_consensus(db, p, i);
                        consensus cons = new consensus();
                        

                        if (actions_query.Any())
                            cons = consensus_calculator(actions_query, ref_date, confidence_exists, db, p);
                        
                        

                        results.Add(new Tuple<DateTime, fund, consensus, double?>(ref_date, p, cons, price));
                    }
                }

                plot_sector(results);
            }
        }
        
    }



    // multipliers
    protected double recency_multiplier(DateTime created,DateTime reference)
    {
        double days = (reference - created).TotalDays;
        if (days < 60)
        {
            return 1;
        }
        else if (days >= 60 && days < 120)
        {
            int decay_factor = 20;
            return Math.Exp(-(days - 60) / decay_factor);
        }
        else
        {
            return 0;
        }
    }

    protected double range_multiplier(double avg, double std, double value)
    {
        if (value >= avg - 2 * std && value <= avg + 2 * std)
        {
            return 1;
        }
        else if (value <= (avg - 5 * std) || value >= (avg + 5 * std))
        {
            return 0;
        }
        else
        {
            double decay_factor = std / Math.Log(2,2);
            if (value > avg)
                return Math.Exp(-(value - (avg + 2 * std)) / decay_factor);
            else
                return Math.Exp(-((avg - 2 * std) - value ) / decay_factor);
        }
    }

    protected double expiration_multiplier(DateTime target_date,DateTime reference)
    {
        double days = (target_date - reference).TotalDays;

        if (days >= 30)
            return 1;
        else if (days <= 0)  // fail safe
            return 0;
        else
        {
            int decay_factor = 14; // 10 / ln(2)
            return Math.Exp(-days / decay_factor);
        }

    }

    protected double? calculate_ratio(double? num,double? den)
    {
        double cap = 1e2;

        if (num.HasValue && den.HasValue)
        {
            if (den != 0)
            {
                if ( (num/den)>=0 )
                    return Math.Min(num.Value / den.Value, cap);
                else
                    return Math.Max(num.Value / den.Value, -cap);
            }
                
            else
                return null;
        }
        else
            return null;
    }

    protected string calculate_ratio_to_string(double? input)
    {
        if (input.HasValue)
            return input.ToString();
        else
            return "null";
    }


    // presentation
    protected void tabulate_consensus(consensus cons, DateTime date, double? price)
    {
        TableRow tr = new TableRow();
        TableCell date_td = new TableCell();
        TableCell total_f_td = new TableCell();
        TableCell total_w_td = new TableCell();
        TableCell ws_f_td = new TableCell();
        TableCell ws_w_td = new TableCell();
        TableCell crowd_f_td = new TableCell();
        TableCell crowd_w_td = new TableCell();
        TableCell price_td = new TableCell();

        date_td.Text = string.Format("{0:MMM d, yy}", date);
        total_f_td.Text = (cons.total_flat>=0?Math.Round(cons.total_flat,2).ToString():"") + "<font style=\"color:gray;font-size:small\"> (" + cons.total_flat_analysts + ")</font>";
        total_w_td.Text = (cons.total_weighted >= 0 ? Math.Round(cons.total_weighted, 2).ToString() : "") + "<font style=\"color:gray;font-size:small\"> (" + cons.total_weighted_analysts + ")</font>";
        ws_f_td.Text = (cons.ws_flat >= 0 ? Math.Round(cons.ws_flat, 2).ToString() : "") + "<font style=\"color:gray;font-size:small\"> (" + cons.ws_flat_analysts + ")</font>";
        ws_w_td.Text = (cons.ws_weighted >= 0 ? Math.Round(cons.ws_weighted, 2).ToString() : "") + "<font style=\"color:gray;font-size:small\"> (" + cons.ws_weighted_analysts + ")</font>";
        crowd_f_td.Text = (cons.crowd_flat >= 0 ? Math.Round(cons.crowd_flat, 2).ToString() : "") + "<font style=\"color:gray;font-size:small\"> (" + cons.crowd_flat_analysts + ")</font>";
        crowd_w_td.Text = (cons.crowd_weighted >= 0 ? Math.Round(cons.crowd_weighted, 2).ToString() : "") + "<font style=\"color:gray;font-size:small\"> (" + cons.crowd_weighted_analysts + ")</font>";
        price_td.Text = Math.Round(price.HasValue?price.Value:0, 2).ToString();

        tr.Cells.Add(date_td);
        tr.Cells.Add(total_f_td);
        tr.Cells.Add(total_w_td);
        tr.Cells.Add(ws_f_td);
        tr.Cells.Add(ws_w_td);
        tr.Cells.Add(crowd_f_td);
        tr.Cells.Add(crowd_w_td);
        tr.Cells.Add(price_td);

        tbl.Rows.Add(tr);

        if (display)
        {
            //info.Text += "Contributors: Wall St: " + cons.ws_flat_analysts + ", Wall St weighted: " + cons.ws_weighted_analysts + " , Crowd: "  + cons.crowd_flat_analysts + " , Crowd weighted: " + cons.crowd_weighted_analysts;
        }
    }

    protected void tabulate_trends(List<Tuple<DateTime, List<double?>>> x,bool second)
    {
        // filter
        var x_1w = x.Where(b => b.Item1 >= DateTime.Now.AddDays(-7));
        var x_1m = x.Where(b => b.Item1 >= DateTime.Now.AddDays(-30));
        var x_3m = x.Where(b => b.Item1 >= DateTime.Now.AddDays(-90));
        var x_6m = x.Where(b => b.Item1 >= DateTime.Now.AddDays(-180));
        var x_12m = x.Where(b => b.Item1 >= DateTime.Now.AddDays(-365));

        ContentPlaceHolder body = (ContentPlaceHolder)this.Master.FindControl("body");
        TableCell td = (TableCell)body.FindControl("trends_total_changes" + (second?"_2nd":"") + "_1w");

        double? output = null;
        if (x_1w.Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0)
        {
            output = x_1w.Where(b => b.Item2.ElementAt(0).HasValue).Average(b => b.Item2.ElementAt(0).Value);
            generic_push_to_javascript_declared_array("trends_total" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_total" + (second ? "_2nd" : ""), "null");
        }
            

        td.Text = output.HasValue? Math.Round(100 * output.Value,1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value>=0 ? System.Drawing.Color.Green : System.Drawing.Color.Red ) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_total_changes" + (second ? "_2nd" : "") + "_1m");
        output = null;
        if (x_1m.Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0 && x_1m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0)
        {
            output = x_1m.Where(b => b.Item2.ElementAt(0).HasValue).Average(b => b.Item2.ElementAt(0).Value);
            generic_push_to_javascript_declared_array("trends_total" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_total" + (second ? "_2nd" : ""), "null");
        }
            

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_total_changes" + (second ? "_2nd" : "") + "_3m");
        output = null;
        if (x_3m.Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0) {
            output = x_3m.Where(b => b.Item2.ElementAt(0).HasValue).Average(b => b.Item2.ElementAt(0).Value);
            generic_push_to_javascript_declared_array("trends_total" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_total" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_total_changes" + (second ? "_2nd" : "") + "_6m");
        output = null;
        if (x_6m.Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0) {
            output = x_6m.Where(b => b.Item2.ElementAt(0).HasValue).Average(b => b.Item2.ElementAt(0).Value);
            generic_push_to_javascript_declared_array("trends_total" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_total" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_total_changes" + (second ? "_2nd" : "") + "_12m");
        output = null;
        if (x_12m.Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-180)).Where(b => b.Item2.ElementAt(0).HasValue).Count() > 0) {
            output = x_12m.Where(b => b.Item2.ElementAt(0).HasValue).Average(b => b.Item2.ElementAt(0).Value);
            generic_push_to_javascript_declared_array("trends_total" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_total" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        // total weighted
        td = (TableCell)body.FindControl("trends_total_w_changes" + (second ? "_2nd" : "") + "_1w");
        output = null;
        if (x_1w.Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0) {
            output = x_12m.Where(b => b.Item2.ElementAt(1).HasValue).Average(b => b.Item2.ElementAt(1).Value);
            generic_push_to_javascript_declared_array("trends_total_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_total_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_total_w_changes" + (second ? "_2nd" : "") + "_1m");
        output = null;
        if (x_1m.Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0 && x_1m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0 ) {
            output = x_1m.Where(b => b.Item2.ElementAt(1).HasValue).Average(b => b.Item2.ElementAt(1).Value);
            generic_push_to_javascript_declared_array("trends_total_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_total_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_total_w_changes" + (second ? "_2nd" : "") + "_3m");
        output = null;
        if (x_3m.Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0)
        { 
            output = x_3m.Where(b => b.Item2.ElementAt(1).HasValue).Average(b => b.Item2.ElementAt(1).Value);
            generic_push_to_javascript_declared_array("trends_total_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_total_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_total_w_changes" + (second ? "_2nd" : "") + "_6m");
        output = null;
        if (x_6m.Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0)
        {
            output = x_6m.Where(b => b.Item2.ElementAt(1).HasValue).Average(b => b.Item2.ElementAt(1).Value);
            generic_push_to_javascript_declared_array("trends_total_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_total_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_total_w_changes" + (second ? "_2nd" : "") + "_12m");
        output = null;
        if (x_12m.Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-180)).Where(b => b.Item2.ElementAt(1).HasValue).Count() > 0)
        {
            output = x_12m.Where(b => b.Item2.ElementAt(1).HasValue).Average(b => b.Item2.ElementAt(1).Value);
            generic_push_to_javascript_declared_array("trends_total_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_total_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        // WS
        td = (TableCell)body.FindControl("trends_ws_changes" + (second ? "_2nd" : "") + "_1w");
        output = null;
        if (x_1w.Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0) {
            output = x_1w.Where(b => b.Item2.ElementAt(2).HasValue).Average(b => b.Item2.ElementAt(2).Value);
            generic_push_to_javascript_declared_array("trends_ws" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_ws" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_ws_changes" + (second ? "_2nd" : "") + "_1m");
        output = null;
        if (x_1m.Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0 && x_1m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0)
        {
            output = x_1m.Where(b => b.Item2.ElementAt(2).HasValue).Average(b => b.Item2.ElementAt(2).Value);
            generic_push_to_javascript_declared_array("trends_ws" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_ws" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_ws_changes" + (second ? "_2nd" : "") + "_3m");
        output = null;
        if (x_3m.Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0)
        {
            output = x_3m.Where(b => b.Item2.ElementAt(2).HasValue).Average(b => b.Item2.ElementAt(2).Value);
            generic_push_to_javascript_declared_array("trends_ws" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_ws" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_ws_changes" + (second ? "_2nd" : "") + "_6m");
        output = null;
        if (x_6m.Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0)
        {
            output = x_6m.Where(b => b.Item2.ElementAt(2).HasValue).Average(b => b.Item2.ElementAt(2).Value);
            generic_push_to_javascript_declared_array("trends_ws" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_ws" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_ws_changes" + (second ? "_2nd" : "") + "_12m");
        output = null;
        if (x_12m.Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-180)).Where(b => b.Item2.ElementAt(2).HasValue).Count() > 0)
        {
            output = x_12m.Where(b => b.Item2.ElementAt(2).HasValue).Average(b => b.Item2.ElementAt(2).Value);
            generic_push_to_javascript_declared_array("trends_ws" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_ws" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        // WS weighted
        td = (TableCell)body.FindControl("trends_ws_w_changes" + (second ? "_2nd" : "") + "_1w");
        output = null;
        if (x_1w.Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0) {
            output = x_1w.Where(b => b.Item2.ElementAt(3).HasValue).Average(b => b.Item2.ElementAt(3).Value);
            generic_push_to_javascript_declared_array("trends_ws_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_ws_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_ws_w_changes" + (second ? "_2nd" : "") + "_1m");
        output = null;
        if (x_1m.Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0 && x_1m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0)
        {
            output = x_1m.Where(b => b.Item2.ElementAt(3).HasValue).Average(b => b.Item2.ElementAt(3).Value);
            generic_push_to_javascript_declared_array("trends_ws_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_ws_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_ws_w_changes" + (second ? "_2nd" : "") + "_3m");
        output = null;
        if (x_3m.Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0)
        {
            output = x_3m.Where(b => b.Item2.ElementAt(3).HasValue).Average(b => b.Item2.ElementAt(3).Value);
            generic_push_to_javascript_declared_array("trends_ws_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_ws_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_ws_w_changes" + (second ? "_2nd" : "") + "_6m");
        output = null;
        if (x_6m.Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0)
        {
            output = x_6m.Where(b => b.Item2.ElementAt(3).HasValue).Average(b => b.Item2.ElementAt(3).Value);
            generic_push_to_javascript_declared_array("trends_ws_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_ws_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_ws_w_changes" + (second ? "_2nd" : "") + "_12m");
        output = null;
        if (x_12m.Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-180)).Where(b => b.Item2.ElementAt(3).HasValue).Count() > 0)
        {
            output = x_12m.Where(b => b.Item2.ElementAt(3).HasValue).Average(b => b.Item2.ElementAt(3).Value);
            generic_push_to_javascript_declared_array("trends_ws_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_ws_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        // Crowd
        td = (TableCell)body.FindControl("trends_crowd_changes" + (second ? "_2nd" : "") + "_1w");
        output = null;
        if (x_1w.Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0) {
            output = x_1w.Where(b => b.Item2.ElementAt(4).HasValue).Average(b => b.Item2.ElementAt(4).Value);
            generic_push_to_javascript_declared_array("trends_crowd" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_crowd" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_crowd_changes" + (second ? "_2nd" : "") + "_1m");
        output = null;
        if (x_1m.Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0 && x_1m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0)
        {
            output = x_1m.Where(b => b.Item2.ElementAt(4).HasValue).Average(b => b.Item2.ElementAt(4).Value);
            generic_push_to_javascript_declared_array("trends_crowd" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_crowd" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_crowd_changes" + (second ? "_2nd" : "") + "_3m");
        output = null;
        if (x_3m.Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0)
        {
            output = x_3m.Where(b => b.Item2.ElementAt(4).HasValue).Average(b => b.Item2.ElementAt(4).Value);
            generic_push_to_javascript_declared_array("trends_crowd" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_crowd" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_crowd_changes" + (second ? "_2nd" : "") + "_6m");
        output = null;
        if (x_6m.Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0)
        { 
            output = x_6m.Where(b => b.Item2.ElementAt(4).HasValue).Average(b => b.Item2.ElementAt(4).Value);
            generic_push_to_javascript_declared_array("trends_crowd" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_crowd" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_crowd_changes" + (second ? "_2nd" : "") + "_12m");
        output = null;
        if (x_12m.Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-180)).Where(b => b.Item2.ElementAt(4).HasValue).Count() > 0)
        {
            output = x_12m.Where(b => b.Item2.ElementAt(4).HasValue).Average(b => b.Item2.ElementAt(4).Value);
            generic_push_to_javascript_declared_array("trends_crowd" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_crowd" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        // Crowd W
        td = (TableCell)body.FindControl("trends_crowd_w_changes" + (second ? "_2nd" : "") + "_1w");
        output = null;
        if (x_1w.Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0) {
            output = x_1w.Where(b => b.Item2.ElementAt(5).HasValue).Average(b => b.Item2.ElementAt(5).Value);
            generic_push_to_javascript_declared_array("trends_crowd_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_crowd_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_crowd_w_changes" + (second ? "_2nd" : "") + "_1m");
        output = null;
        if (x_1m.Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0 && x_1m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0)
        {
            output = x_1m.Where(b => b.Item2.ElementAt(5).HasValue).Average(b => b.Item2.ElementAt(5).Value);
            generic_push_to_javascript_declared_array("trends_crowd_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_crowd_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_crowd_w_changes" + (second ? "_2nd" : "") + "_3m");
        output = null;
        if (x_3m.Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0 && x_3m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0)
        {
            output = x_3m.Where(b => b.Item2.ElementAt(5).HasValue).Average(b => b.Item2.ElementAt(5).Value);
            generic_push_to_javascript_declared_array("trends_crowd_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_crowd_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_crowd_w_changes" + (second ? "_2nd" : "") + "_6m");
        output = null;
        if (x_6m.Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0 && x_6m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0)
        {
            output = x_6m.Where(b => b.Item2.ElementAt(5).HasValue).Average(b => b.Item2.ElementAt(5).Value);
            generic_push_to_javascript_declared_array("trends_crowd_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_crowd_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;

        td = (TableCell)body.FindControl("trends_crowd_w_changes" + (second ? "_2nd" : "") + "_12m");
        output = null;
        if (x_12m.Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-7)).Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-30)).Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-90)).Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0 && x_12m.Where(b => b.Item1 < DateTime.Now.AddDays(-180)).Where(b => b.Item2.ElementAt(5).HasValue).Count() > 0)
        {
            output = x_12m.Where(b => b.Item2.ElementAt(5).HasValue).Average(b => b.Item2.ElementAt(5).Value);
            generic_push_to_javascript_declared_array("trends_crowd_w" + (second ? "_2nd" : ""), output.Value.ToString());
        }
        else
        {
            generic_push_to_javascript_declared_array("trends_crowd_w" + (second ? "_2nd" : ""), "null");
        }

        td.Text = output.HasValue ? Math.Round(100 * output.Value, 1) + "%" : "";
        td.ForeColor = output.HasValue ? (output.Value >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent;
    }

    protected void tabulate_consensus_now(consensus cons)
    {
        total_flat_pt.Text = cons.total_flat>=0? Math.Round(cons.total_flat,2).ToString() : "";
        total_weighted_pt.Text = cons.total_weighted>=0? Math.Round(cons.total_weighted, 2).ToString() : "";
        ws_flat_pt.Text = cons.ws_flat>=0? Math.Round(cons.ws_flat,2).ToString() : "";
        ws_weighted_pt.Text = cons.ws_weighted>=0? Math.Round(cons.ws_weighted,2).ToString() : "";
        crowd_flat_pt.Text = cons.crowd_flat>=0 ? Math.Round(cons.crowd_flat,2).ToString() : "";
        crowd_weighted_pt.Text = cons.crowd_weighted>=0 ? Math.Round(cons.crowd_weighted,2).ToString() : "";

        total_flat_upside.Text = (price_universal>0 && cons.total_flat>=0) ? Math.Round( 100 * (cons.total_flat / price_universal - 1), 1) + "%" : "";
        total_flat_upside.ForeColor = (price_universal > 0 && cons.total_flat >= 0) ? cons.total_flat >= price_universal ? System.Drawing.Color.Green : System.Drawing.Color.Red : System.Drawing.Color.Transparent;
        total_weighted_upside.Text = (price_universal > 0 && cons.total_weighted >= 0) ? Math.Round(100 * (cons.total_weighted / price_universal - 1), 1) + "%" : "";
        total_weighted_upside.ForeColor = (price_universal > 0 && cons.total_weighted >= 0) ? cons.total_weighted >= price_universal ? System.Drawing.Color.Green : System.Drawing.Color.Red : System.Drawing.Color.Transparent;
        ws_flat_upside.Text = (price_universal > 0 && cons.ws_flat >= 0) ? Math.Round(100 * (cons.ws_flat / price_universal - 1), 1) + "%" : "";
        ws_flat_upside.ForeColor = (price_universal > 0 && cons.ws_flat >= 0) ? cons.ws_flat >= price_universal ? System.Drawing.Color.Green : System.Drawing.Color.Red : System.Drawing.Color.Transparent;
        ws_weighted_upside.Text = (price_universal > 0 && cons.ws_weighted >= 0) ? Math.Round(100 * (cons.ws_weighted / price_universal - 1), 1) + "%" : "";
        ws_weighted_upside.ForeColor = (price_universal > 0 && cons.ws_weighted >= 0) ? cons.ws_weighted >= price_universal ? System.Drawing.Color.Green : System.Drawing.Color.Red : System.Drawing.Color.Transparent;
        crowd_flat_upside.Text = (price_universal > 0 && cons.crowd_flat >= 0) ? Math.Round(100 * (cons.crowd_flat / price_universal - 1), 1) + "%" : "";
        crowd_flat_upside.ForeColor = (price_universal > 0 && cons.crowd_flat >= 0) ? cons.crowd_flat >= price_universal ? System.Drawing.Color.Green : System.Drawing.Color.Red : System.Drawing.Color.Transparent;
        crowd_weighted_upside.Text = (price_universal > 0 && cons.crowd_weighted >= 0) ? Math.Round(100 * (cons.crowd_weighted / price_universal - 1), 1) + "%" : "";
        crowd_weighted_upside.ForeColor = (price_universal > 0 && cons.crowd_weighted >= 0) ? cons.crowd_weighted >= price_universal ? System.Drawing.Color.Green : System.Drawing.Color.Red : System.Drawing.Color.Transparent;

        total_flat_analysts.Text = cons.total_flat_analysts.ToString();
        total_weighted_analysts.Text = cons.total_weighted_analysts.ToString();
        ws_flat_analysts.Text = cons.ws_flat_analysts.ToString();
        ws_weighted_analysts.Text = cons.ws_weighted_analysts.ToString();
        crowd_flat_analysts.Text = cons.crowd_flat_analysts.ToString();
        crowd_weighted_analysts.Text = cons.crowd_weighted_analysts.ToString();

        total_flat_analysts_viz.Text = visualize_coverage_of_analysts(cons.total_flat_analysts);
        total_weighted_analysts_viz.Text = visualize_coverage_of_analysts(cons.total_weighted_analysts);

        ws_flat_analysts_viz.Text = visualize_coverage_of_analysts(cons.ws_flat_analysts);
        ws_weighted_analysts_viz.Text = visualize_coverage_of_analysts(cons.ws_weighted_analysts);

        crowd_flat_analysts_viz.Text = visualize_coverage_of_analysts(cons.crowd_flat_analysts);
        crowd_weighted_analysts_viz.Text = visualize_coverage_of_analysts(cons.crowd_weighted_analysts);
    }

    protected void tabulate_last_consensus_constituents(List<PT_weights_item> PT_weights_list,fund ticker)
    {
        DateTime ref_date = DateTime.Now;
        DataClassesDataContext db = new DataClassesDataContext();
        double? price = null;
        try
        {
            price = (from temp in db.fund_values where temp.fundID == ticker.fundID && temp.date <= ref_date select temp).OrderByDescending(b => b.date).First().adjValue;
        }
        catch { }

        double sum_flat = PT_weights_list.Sum(item => item.recency * item.range * item.expiration);
        double sum_weighted = PT_weights_list.Sum(item => item.confidence * item.recency * item.range * item.expiration);

        Dictionary<int, int> analyst_industry_rank = new Dictionary<int, int>();

        if (ticker.peer_group.HasValue){
            var aps_industry = from temp in db.AnalystPerformances where temp.industry == ticker.peer_group select temp;
            if (aps_industry.Any()) {
                double all_analysts = aps_industry.Count();

                foreach (PT_weights_item item in PT_weights_list){
                    var aps_industry_analyst = aps_industry.Where(b => b.analyst == item.analystID);
                    if (aps_industry_analyst.Any()){
                        analyst_industry_rank.Add(item.analystID, aps_industry_analyst.First().rank);
                    }
                }
            }
        }

        foreach (PT_weights_item item in PT_weights_list)
        {
            TableRow tr = new TableRow();
            tr.BackColor = item.wall_st ? System.Drawing.Color.Transparent : System.Drawing.Color.PaleGreen;

            TableCell td1 = new TableCell(); td1.Font.Size = 9; td1.Text = Ancillary.string_cutter(item.name,15,false,null); tr.Cells.Add(td1);
            TableCell td2 = new TableCell(); td2.Font.Size = 9; td2.Text = Ancillary.string_cutter(item.broker,15,false,null); tr.Cells.Add(td2);
            TableCell td3 = new TableCell(); td3.Font.Size = 9; td3.Text = Math.Round(item.confidence, 2).ToString(); tr.Cells.Add(td3);
            TableCell td4 = new TableCell(); td4.Font.Size = 9; td4.Text = Math.Round(item.recency, 2).ToString(); tr.Cells.Add(td4);
            TableCell td5 = new TableCell(); td5.Font.Size = 9; td5.Text = Math.Round(item.range, 2).ToString(); tr.Cells.Add(td5);
            TableCell td6 = new TableCell(); td6.Font.Size = 9; td6.Text = Math.Round(item.expiration, 2).ToString(); tr.Cells.Add(td6);
            TableCell td7 = new TableCell(); td7.Font.Size = 9; td7.Text = Math.Round(item.target, 2).ToString(); tr.Cells.Add(td7);
            TableCell td8 = new TableCell(); td8.Font.Size = 9; td8.Text = Math.Round(item.recency * item.range * item.expiration, 2).ToString(); tr.Cells.Add(td8);
            TableCell td9 = new TableCell(); td9.Font.Size = 9; td9.Text = Math.Round(item.confidence * item.recency * item.range * item.expiration, 2).ToString(); tr.Cells.Add(td9);
            TableCell td10 = new TableCell(); td10.Font.Size = 9; td10.Text = Math.Round(100 * item.recency * item.range * item.expiration / sum_flat, 1) + "%"; tr.Cells.Add(td10);
            TableCell td11 = new TableCell(); td11.Font.Size = 9; td11.Text = Math.Round(100 * item.confidence * item.recency * item.range * item.expiration / sum_weighted, 1) + "%"; tr.Cells.Add(td11);

            tbl_today.Rows.Add(tr);

            // company table row
            double confidence = 0; double ret = 0; double accuracy = 0; double win = 0; int relative = 0; double estimates = 0; DateTime since = DateTime.Now;
            var analyst_confidence = from temp in db.AnalystPerformances where temp.analyst == item.analystID && temp.ticker == ticker.fundID select temp;
            if (analyst_confidence.Any())
            {
                var ap = analyst_confidence.First();
                confidence = ap.confidence.HasValue ? ap.confidence.Value : 0;
                ret = ap.return_average;
                accuracy = ap.accuracy_average;
                win = Math.Max(Math.Min((double)(ap.actions_matured + ap.actions_e_and_positive) / (double)ap.actions, 1), 0);
                relative = 0;
                analyst_industry_rank.TryGetValue(item.analystID, out relative);
                estimates = ap.actions;
                if (ap.first_action_date.HasValue)
                    since = ap.first_action_date.Value;
            }

            TableRow tr_c = new TableRow();
            tr_c.BackColor = item.wall_st ? System.Drawing.Color.Transparent : System.Drawing.Color.PaleGreen;

            TableCell tdc_1 = new TableCell(); tdc_1.Font.Size = 8;  tdc_1.Text = (PT_weights_list.OrderByDescending(b=>b.confidence).Select(b=>b.analystID).ToList().IndexOf(item.analystID) + 1).ToString(); tr_c.Cells.Add(tdc_1);
            TableCell tdc_2 = new TableCell(); tdc_2.Font.Size = 8; tdc_2.Text = Ancillary.string_cutter( item.name,10,false,null) + "<br><font color=gray>" + Ancillary.string_cutter(item.broker,10,false,null) + "</font>"; tr_c.Cells.Add(tdc_2);
            TableCell tdc_3 = new TableCell(); tdc_3.Font.Size = 8; tdc_3.Text = Math.Round(100 * confidence) + "%"; tr_c.Cells.Add(tdc_3);
            TableCell tdc_4 = new TableCell(); tdc_4.Font.Size = 8; tdc_4.Text = Math.Round(100 * ret, 1) + "%"; tr_c.Cells.Add(tdc_4);
            TableCell tdc_5 = new TableCell(); tdc_5.Font.Size = 8; tdc_5.Text = Math.Round(100 * accuracy, 0) + "%"; tr_c.Cells.Add(tdc_5);
            TableCell tdc_6 = new TableCell(); tdc_6.Font.Size = 8; tdc_6.Text = Math.Round(100 * win, 0) + "%"; tr_c.Cells.Add(tdc_6);
            TableCell tdc_7 = new TableCell(); tdc_7.Font.Size = 8; tdc_7.Text = relative>0?relative.ToString():""; tr_c.Cells.Add(tdc_7);
            TableCell tdc_8 = new TableCell(); tdc_8.Font.Size = 8; tdc_8.Text = estimates.ToString(); tr_c.Cells.Add(tdc_8);
            TableCell tdc_9 = new TableCell(); tdc_9.Font.Size = 8; tdc_9.Text = item.target.ToString(); tr_c.Cells.Add(tdc_9);
            TableCell tdc_10 = new TableCell(); tdc_10.Font.Size = 8; tdc_10.Text = price.HasValue ? Math.Round(100 * (item.target / price.Value - 1), 1) + "%" : ""; tdc_10.ForeColor = price.HasValue ? (item.target >= price.Value ? System.Drawing.Color.Green : System.Drawing.Color.Red) : System.Drawing.Color.Transparent; tr_c.Cells.Add(tdc_10);
            TableCell tdc_11 = new TableCell(); tdc_11.Font.Size = 8; tdc_11.Text = Math.Round((item.targetdate - ref_date ).TotalDays,0).ToString() ; tr_c.Cells.Add(tdc_11);
            TableCell tdc_12 = new TableCell(); tdc_12.Font.Size = 8; tdc_12.Text = Math.Round((ref_date - (item.estimate?item.created:item.reiterated)).TotalDays, 0).ToString(); tr_c.Cells.Add(tdc_12);
            TableCell tdc_13 = new TableCell(); tdc_13.Font.Size = 8; tdc_13.Text = Math.Round((ref_date - item.reiterated).TotalDays, 0).ToString(); tr_c.Cells.Add(tdc_13);
            TableCell tdc_14 = new TableCell(); tdc_14.Font.Size = 8; tdc_14.Text = Ancillary.string_cutter(item.rationale,30,false,null); tr_c.Cells.Add(tdc_14);
            TableCell tdc_15 = new TableCell(); tdc_15.Font.Size = 8; tdc_15.Text = ""; tr_c.Cells.Add(tdc_15);
            TableCell tdc_16 = new TableCell(); tdc_16.Font.Size = 8; tdc_16.Text = since<=ref_date?Math.Round((ref_date - since).TotalDays/30, 0).ToString() : ""; tr_c.Cells.Add(tdc_16);
            
            tbl_co.Rows.Add(tr_c);
        }
    }

    protected void plot_consensus_changes(List<Tuple<DateTime,consensus>> cons)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");

        double previous1 = -1;
        double previous2 = -1;
        double previous3 = -1;
        double previous4 = -1;
        double previous5 = -1;
        double previous6 = -1;

        List<Tuple<DateTime, List<string>>> list_changes = new List<Tuple<DateTime, List<string>>>();
        List<Tuple<DateTime, List<double?>>> trend = new List<Tuple<DateTime, List<double?>>>();

        int count = 0;
        foreach (Tuple<DateTime,consensus> c in cons)
        {
            string v1 = "null"; string v2 = "null"; string v3 = "null"; string v4 = "null"; string v5 = "null"; string v6 = "null";
            if (count > 0)
            {
                List<double?> val = new List<double?>();

                if (previous1 > 0 && c.Item2.total_flat > 0){
                    val.Add(c.Item2.total_flat / previous1 - 1);
                    v1 = val.Last().Value.ToString();
                }
                else{
                    val.Add(null);
                    v1 = "null";
                }

                if (previous2 > 0 && c.Item2.total_weighted > 0){
                    val.Add(c.Item2.total_weighted / previous2 - 1);
                    v2 = val.Last().Value.ToString();
                }
                else{
                    val.Add(null);
                    v2 = "null";
                }

                if (previous3 > 0 && c.Item2.ws_flat > 0){
                    val.Add(c.Item2.ws_flat / previous3 - 1);
                    v3 = val.Last().Value.ToString();
                }
                else{
                    val.Add(null);
                    v3 = "null";
                }

                if (previous4 > 0 && c.Item2.ws_weighted > 0){
                    val.Add(c.Item2.ws_weighted / previous4 - 1);
                    v4 = val.Last().Value.ToString();
                }
                else{
                    val.Add(null);
                    v4 = "null";
                }

                if (previous5 > 0 && c.Item2.crowd_flat > 0){
                    val.Add(c.Item2.crowd_flat / previous5 - 1);
                    v5 = val.Last().Value.ToString();
                }
                else{
                    val.Add(null);
                    v5 = "null";
                }

                if (previous6 > 0 && c.Item2.crowd_weighted > 0){
                    val.Add(c.Item2.crowd_weighted / previous6 - 1);
                    v6 = val.Last().Value.ToString();
                }
                else{
                    val.Add(null);
                    v6 = "null";
                }

                trend.Add(new Tuple<DateTime,List<double?>>(c.Item1,val));
            }

            List<string> changes = new List<string>();  
            changes.Add(v1); changes.Add(v2); changes.Add(v3); changes.Add(v4); changes.Add(v5); changes.Add(v6);
            list_changes.Add(new Tuple<DateTime,List<string>>(c.Item1,changes));

            sb.Append("total_flat_changes.push(" + v1 + ");");
            sb.Append("total_weighted_changes.push(" + v2 + ");");
            sb.Append("ws_flat_changes.push(" + v3 + ");");
            sb.Append("ws_weighted_changes.push(" + v4 + ");");
            sb.Append("crowd_flat_changes.push(" + v5 + ");");
            sb.Append("crowd_weighted_changes.push(" + v6 + ");");

            previous1 = c.Item2.total_flat;
            previous2 = c.Item2.total_weighted;
            previous3 = c.Item2.ws_flat;
            previous4 = c.Item2.ws_weighted;
            previous5 = c.Item2.crowd_flat;
            previous6 = c.Item2.crowd_weighted;

            count++;
        }

        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), "consensus_changes", sb.ToString());

        plot_consensus_changes_2nd(list_changes);
        tabulate_trends(trend,false);
    }

    protected void plot_consensus_changes_2nd(List<Tuple<DateTime,List<string>>> list_changes)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");

        double? previous1 = null;
        double? previous2 = null;
        double? previous3 = null;
        double? previous4 = null;
        double? previous5 = null;
        double? previous6 = null;

        int count = 0;
        List<Tuple<DateTime, List<double?>>> trend = new List<Tuple<DateTime, List<double?>>>();

        foreach (Tuple<DateTime,List<string>> changes in list_changes)
        {
            string v1 = "null"; string v2 = "null"; string v3 = "null"; string v4 = "null"; string v5 = "null"; string v6 = "null";

            if (count > 1)
            {
                List<double?> val = new List<double?>();

                try
                {
                    if (previous1.HasValue && changes.Item2[0] != "null" && previous1 != 0){
                        val.Add(calculate_ratio(Convert.ToDouble(changes.Item2[0]), previous1));
                        v1 = calculate_ratio_to_string(val.Last());
                    }
                    else{
                        val.Add(null);
                        v1 = "null";
                    }
                }
                catch
                {
                    val.Add(null);
                    v1 = "null";
                }
                try
                {
                    if (previous2.HasValue && changes.Item2[1] != "null" && previous2 != 0)
                    {
                        val.Add(calculate_ratio(Convert.ToDouble(changes.Item2[1]), previous2));
                        v2 = calculate_ratio_to_string(val.Last());
                    }
                    else{
                        val.Add(null);
                        v2 = "null";
                    }
                }
                catch { 
                    val.Add(null);
                    v2 = "null";
                }
                try
                {
                    if (previous3.HasValue && changes.Item2[2] != "null" && previous3 != 0)
                    {
                        val.Add(calculate_ratio(Convert.ToDouble(changes.Item2[2]), previous3));
                        v3 = calculate_ratio_to_string(val.Last());
                    }
                    else
                    {
                        val.Add(null);
                        v3 = "null";
                    }
                }
                catch {
                    val.Add(null);
                    v3 = "null";
                }
                try
                {
                    if (previous4.HasValue && changes.Item2[3] != "null" && previous4 != 0)
                    {
                        val.Add(calculate_ratio(Convert.ToDouble(changes.Item2[3]), previous4));
                        v4 = calculate_ratio_to_string(val.Last());
                    }
                    else
                    {
                        val.Add(null);
                        v4 = "null";
                    }
                }
                catch {
                    val.Add(null);
                    v4 = "null";
                }
                try
                {
                    if (previous5.HasValue && changes.Item2[4] != "null" && previous5 != 0)
                    {
                        val.Add(calculate_ratio(Convert.ToDouble(changes.Item2[4]), previous5));
                        v5 = calculate_ratio_to_string(val.Last());
                    }
                    else
                    {
                        val.Add(null);
                        v5 = "null";
                    }
                }
                catch {
                    val.Add(null);
                    v5 = "null";
                }
                try
                {
                    if (previous6.HasValue && changes.Item2[5] != "null" && previous6 != 0)
                    {
                        val.Add(calculate_ratio(Convert.ToDouble(changes.Item2[5]), previous6));
                        v6 = calculate_ratio_to_string(val.Last());
                    }
                    else
                    {
                        val.Add(null);
                        v6 = "null";
                    }
                }
                catch {
                    val.Add(null);
                    v6 = "null";
                }

                trend.Add(new Tuple<DateTime, List<double?>>( changes.Item1 , val ));
            }

            sb.Append("total_flat_changes_2nd.push(" + v1 + ");");
            sb.Append("total_weighted_changes_2nd.push(" + v2 + ");");
            sb.Append("ws_flat_changes_2nd.push(" + v3 + ");");
            sb.Append("ws_weighted_changes_2nd.push(" + v4 + ");");
            sb.Append("crowd_flat_changes_2nd.push(" + v5 + ");");
            sb.Append("crowd_weighted_changes_2nd.push(" + v6 + ");");

            try
            {
                if (changes.Item2[0] != "null")
                    previous1 = Convert.ToDouble(changes.Item2[0]);
                else
                    previous1 = null;
            }
            catch { previous1 = null; }
            try
            {
                if (changes.Item2[1] != "null")
                    previous2 = Convert.ToDouble(changes.Item2[1]);
                else
                    previous2 = null;
            }
            catch { previous2 = null; }
            try
            {
                if (changes.Item2[2] != "null")
                    previous3 = Convert.ToDouble(changes.Item2[2]);
                else
                    previous3 = null;
            }
            catch { previous3 = null; }
            try
            {
                if (changes.Item2[3] != "null")
                    previous4 = Convert.ToDouble(changes.Item2[3]);
                else
                    previous4 = null;
            }
            catch { previous4 = null; }
            try
            {
                if (changes.Item2[4] != "null")
                    previous5 = Convert.ToDouble(changes.Item2[4]);
                else
                    previous5 = null;
            }
            catch { previous5 = null; }
            try
            {
                if (changes.Item2[5] != "null")
                    previous6 = Convert.ToDouble(changes.Item2[5]);
                else
                    previous6 = null;
            }
            catch { previous6 = null; }
            

            count++;
        }

        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), "consensus_changes_2nd", sb.ToString());

        tabulate_trends(trend, true);
    }

    protected void plot_consensus(consensus cons, DateTime date, double? price)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");
        sb.Append("categories.push('" + date.Month + "/" + date.Day + "');");
        sb.Append("price.push(" + price + ");");
        
        sb.Append("total_flat.push(" + ((cons.total_flat<0) ? "null" : cons.total_flat.ToString()) + ");");
        sb.Append("total_weighted.push(" + ((cons.total_weighted<0) ? "null" : cons.total_weighted.ToString()) + ");");
        sb.Append("ws_flat.push(" + ((cons.ws_flat<0) ? "null" : cons.ws_flat.ToString()) + ");");
        sb.Append("ws_weighted.push(" + ((cons.ws_weighted<0) ? "null" : cons.ws_weighted.ToString()) + ");");
        
        sb.Append("crowd_flat.push(" + ((cons.crowd_flat<0) ? "null" : cons.crowd_flat.ToString()) + ");");
        sb.Append("crowd_weighted.push(" + ((cons.crowd_weighted<0) ? "null" : cons.crowd_weighted.ToString()) + ");");
        sb.Append("count_ws = " + cons.total_flat_analysts_ws + ";");
        sb.Append("count_crowd = " + (cons.total_flat_analysts - cons.total_flat_analysts_ws) + ";");
        sb.Append("count_ws_weighted = " + cons.total_weighted_analysts_ws + ";");
        sb.Append("count_crowd_weighted = " + (cons.total_weighted_analysts - cons.total_weighted_analysts_ws) + ";");
        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), string.Format("{0:MMddyy}", date), sb.ToString());
    }

    protected void plot_pie_chart(List<PT_weights_item> PT_weights_list,consensus cons)
    {
        // plot consensus weight contribution
        //double sum_flat_ws = cons.ws_flat>=0 ? PT_weights_list.Where(b=>b.wall_st).Sum(item => item.recency * item.range * item.expiration) : 0;
        //double sum_flat_crowd = cons.crowd_flat >= 0 ? PT_weights_list.Where(b => !b.wall_st).Sum(item => item.recency * item.range * item.expiration) : 0;
        //double sum_flat = sum_flat_ws + sum_flat_crowd;
        double sum_flat = PT_weights_list.Sum(item => item.recency * item.range * item.expiration);

        //double sum_weighted_ws = cons.ws_weighted >= 0 ? PT_weights_list.Where(b => b.wall_st).Sum(item => item.confidence * item.recency * item.range * item.expiration) : 0;
        //double sum_weighted_crowd = cons.crowd_weighted >= 0 ? PT_weights_list.Where(b => !b.wall_st).Sum(item => item.confidence * item.recency * item.range * item.expiration) : 0;
        //double sum_weighted = sum_weighted_ws + sum_weighted_crowd;
        double sum_weighted = PT_weights_list.Sum(item => item.confidence * item.recency * item.range * item.expiration);

        //double consensus_weight_contribution_ws_flat = cons.ws_flat>=0 ? PT_weights_list.Where(b => b.wall_st).Sum(item => item.recency * item.range * item.expiration) / sum_flat : 0;
        //double consensus_weight_contribution_ws_weighted = cons.ws_weighted>=0 ? PT_weights_list.Where(b => b.wall_st).Sum(item => item.confidence * item.recency * item.range * item.expiration) / sum_weighted : 0;
        //double consensus_weight_contribution_crowd_flat = cons.crowd_flat>=0 ? PT_weights_list.Where(b => !b.wall_st).Sum(item => item.recency * item.range * item.expiration) / sum_flat : 0;
        //double consensus_weight_contribution_crowd_weighted = cons.crowd_weighted>=0 ? PT_weights_list.Where(b => !b.wall_st).Sum(item => item.confidence * item.recency * item.range * item.expiration) / sum_weighted : 0;
        double consensus_weight_contribution_ws_flat = PT_weights_list.Where(b => b.wall_st).Sum(item => item.recency * item.range * item.expiration) / sum_flat;
        double consensus_weight_contribution_ws_weighted = PT_weights_list.Where(b => b.wall_st).Sum(item => item.confidence * item.recency * item.range * item.expiration) / sum_weighted;
        double consensus_weight_contribution_crowd_flat = PT_weights_list.Where(b => !b.wall_st).Sum(item => item.recency * item.range * item.expiration) / sum_flat;
        double consensus_weight_contribution_crowd_weighted = PT_weights_list.Where(b => !b.wall_st).Sum(item => item.confidence * item.recency * item.range * item.expiration) / sum_weighted;

        if (cons.total_flat >= 0)
        {
            generic_push_to_javascript("consensus_weight_contribution_ws_flat", (Math.Round(100 * consensus_weight_contribution_ws_flat, 2)).ToString());
            generic_push_to_javascript("consensus_weight_contribution_ws_weighted", (Math.Round(100 * consensus_weight_contribution_ws_weighted, 2)).ToString());
            generic_push_to_javascript("consensus_weight_contribution_crowd_flat", (Math.Round(100 * consensus_weight_contribution_crowd_flat, 2)).ToString());
            generic_push_to_javascript("consensus_weight_contribution_crowd_weighted", (Math.Round(100 * consensus_weight_contribution_crowd_weighted, 2)).ToString());
        }
    }

    protected void plot_confidence_distribution(List<PT_weights_item> PT_weights_list)
    {
        string sort = "";
        try
        {
            sort = Request.QueryString["sort"];
        }
        catch { }

        var x = PT_weights_list.OrderByDescending(b => b.reiterated);
        if (sort == "confidence")
        {
            x = PT_weights_list.OrderByDescending(b => b.confidence);
        }

        generic_push_to_javascript_list("analysts", x.Select(b => "'" + b.name + " " + b.broker + "'").ToList());
        generic_push_to_javascript_list_value_color("confidence", x.Select(b => b.confidence.ToString()).ToList(), x.Select(b => b.wall_st ? "0088cc" : "62c462").ToList());
        generic_push_to_javascript_list_value_color("range", x.Select(b => b.range!=1?b.range.ToString():"0" ).ToList(), x.Select(b => b.wall_st ? "0088cc" : "62c462").ToList());
        generic_push_to_javascript_list_value_color("recency", x.Select(b => b.recency != 1 ? (b.recency==0?"0.01":b.recency.ToString()) : "0").ToList(), x.Select(b => b.wall_st ? "0088cc" : "62c462").ToList());
        generic_push_to_javascript_list_value_color("expiration", x.Select(b => b.expiration != 1 ? b.expiration.ToString() : "0").ToList(), x.Select(b => b.wall_st ? "0088cc" : "62c462").ToList());

        double sum_flat = PT_weights_list.Sum(b=>b.recency * b.range * b.expiration);
        double sum_weighted = PT_weights_list.Sum(b=>b.confidence * b.recency * b.range * b.expiration);
        generic_push_to_javascript_list_value_color("contribution_flat", x.Select(item => (item.recency * item.range * item.expiration / sum_flat).ToString() ).ToList(), x.Select(b => b.wall_st ? "0088cc" : "62c462").ToList());
        generic_push_to_javascript_list_value_color("contribution_weighted", x.Select(item => (item.confidence * item.recency * item.range * item.expiration / sum_weighted).ToString()).ToList(), x.Select(b => b.wall_st ? "0088cc" : "62c462").ToList());


    }

    protected string visualize_coverage_of_analysts(int analysts)
    {
        string on = "<i class=\"icon-user\" style=\"color:black\"></i>";
        string off = "<i class=\"icon-user\" style=\"color:silver\"></i>";

        if (analysts == 0)
            return off + off + off + off;
        else if (analysts <= 2)
            return on + off + off + off;
        else if (analysts <= 10)
            return on + on + off + off;
        else if (analysts <= 20)
            return on + on + on + off;
        else
            return on + on + on + on;
    }

    protected void scatter_plot(List<PT_weights_item> ps,double mu,double sigma)
    {
        DateTime start = ps.OrderBy(b=>b.targetdate).First().targetdate ;
        DateTime end = ps.OrderByDescending(b=>b.targetdate).First().targetdate;

        List<scatter_plot_point> output_ws = new List<scatter_plot_point>();
        List<scatter_plot_point> output_crowd = new List<scatter_plot_point>();

        var ps_ordered = ps.OrderBy(b => b.targetdate);
        int count_failsafe = 0;
        int count_ps = ps.Count();

        for (DateTime i = start; i <= end; i = i.AddDays(1) )
        {
            bool anything = false;
            while (count_failsafe < count_ps && ps_ordered.ElementAt(count_failsafe).targetdate == i)
            {
                //if (!anything)
                //    anything = true;

                PT_weights_item item = ps_ordered.ElementAt(count_failsafe);

                if (item.target <= (mu + 5 * sigma) && item.target >= (mu - 5 * sigma)) {
                    if (item.wall_st)
                    {
                        scatter_plot_point p = new scatter_plot_point();
                        p.x = DataBaseLayer.UnixTicks(item.reiterated.AddDays(365));
                        p.y = item.target;
                        p.fillColor = (item.target >= price_universal ? "98, 196, 98," : "238, 95, 91,") + Ancillary.bubble_opacity(item.confidence, 2);
                        p.name = item.name + ", " + item.broker;

                        output_ws.Add(p);
                    }
                    else
                    {
                        scatter_plot_point p = new scatter_plot_point();
                        p.x = DataBaseLayer.UnixTicks(item.targetdate);
                        p.y = item.target;
                        p.fillColor = (item.target >= price_universal ? "98, 196, 98," : "238, 95, 91,") + Ancillary.bubble_opacity(item.confidence, 2);
                        p.name = item.name + ", " + item.broker;

                        output_crowd.Add(p);
                    }
                }
                else
                {
                    scatterplot_notes.Text += "Removed " + item.target + " PT by " + item.name + "<br>";
                }
                
                

                count_failsafe++;
            }

            //if (!anything) // add empty data
            //{

            //}
        }

        push_to_javascript_scatter_point("scatter_ws", output_ws);
        push_to_javascript_scatter_point("scatter_crowd", output_crowd);
    }

    protected void plot_sector(List<Tuple<DateTime, fund, consensus, double?>> results)
    {
        foreach (var results_grouped_by_date in results.GroupBy(b=>b.Item1.Date).OrderBy(b=>b.First().Item1))
        {
            DateTime this_date = results_grouped_by_date.First().Item1;

            var filtered = results_grouped_by_date.Where(b=>b.Item4.HasValue && b.Item3.total_flat>=0);
            double p = filtered.Sum(b => b.Item4.Value) / filtered.Count(); generic_push_to_javascript_declared_array("sector_p", p.ToString());
            double pt = filtered.Sum(b => b.Item3.total_flat) / filtered.Count(); generic_push_to_javascript_declared_array("sector_pt", pt.ToString());
            double upside = filtered.Sum(b => (b.Item3.total_flat / b.Item4.Value - 1)) / filtered.Count(); generic_push_to_javascript_declared_array("sector_u", upside.ToString());

            var filtered_4q = filtered.OrderByDescending(b => b.Item3.total_flat / b.Item4.Value).Take( (int)( filtered.Count() * .25 ));
            var filtered_1q = filtered.OrderBy(b => b.Item3.total_flat / b.Item4.Value).Take((int)(filtered.Count() * .25));

            l_peers.Text += "<b>4Q</b><br>";
            foreach (var r in filtered_4q)
            {
                l_peers.Text += r.Item2.ticker + "<br>";
            }

            l_peers.Text += "<br><b>1Q</b><br>";
            foreach (var r in filtered_1q)
            {
                l_peers.Text += r.Item2.ticker + "<br>";
            }

            generic_push_to_javascript_declared_array("sector_u_4q", (filtered_4q.Sum(b => (b.Item3.total_flat / b.Item4.Value - 1)) / filtered_4q.Count()).ToString());
            generic_push_to_javascript_declared_array("sector_u_1q", (filtered_1q.Sum(b => (b.Item3.total_flat / b.Item4.Value - 1)) / filtered_1q.Count()).ToString());


            //l_peers.Text += string.Format("{0:MM/dd/yy}", this_date) + ", " + Math.Round(100 * upside,1) + "% (" + filtered.Count() + " companies)<br>";
            //foreach (var r in filtered){
            //    l_peers.Text += "<font color=silver>" + r.Item2.ticker + "</font><br>";
            //}
            l_peers.Text += "<br>";
            //l_peers.Text += results_grouped_by_date.Count() + "<br>";
        }
    }



    // ancillary JS outputs
    private Random rnd = new Random();
    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    protected string random_string_generator()
    {
        int size = 10;
        char[] buffer = new char[size];

        for (int i = 0; i < size; i++)
        {
            buffer[i] = chars[rnd.Next(chars.Length)];
        }

        return new string(buffer);
    }

    protected void generic_push_to_javascript_declared_array(string var_name,string var_value){
        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");
        sb.Append(var_name + ".push(" + var_value + ");");
        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), random_string_generator() + var_name + "_from_cs" + var_value, sb.ToString());
    }
    protected void generic_push_to_javascript(string var_name,string var_value)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");
        sb.Append(var_name + " = " + var_value + ";");
        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), string.Format("{0:MMddyy}", var_name + "_from_cs"), sb.ToString());
    }
    protected void generic_push_to_javascript_list(string var_name, List<string> var_values)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");
        foreach (string v in var_values)
            sb.Append(var_name + ".push(" + v + ");");

        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), string.Format("{0:MMddyy}", var_name + "_from_cs"), sb.ToString());
    }
    protected void generic_push_to_javascript_list_value_color(string var_name, List<string> var_values,List<string> var_color)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");
        for (int i = 0; i < var_values.Count();i++ )
            sb.Append(var_name + ".push({y:" + var_values[i] + ",color:'#" + var_color[i]  + "'});");

        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), string.Format("{0:MMddyy}", var_name + "_from_cs"), sb.ToString());
    }
    protected void push_to_javascript_scatter_point(string name,List<scatter_plot_point> p)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<script>");
        for (int i = 0; i < p.Count(); i++)
            sb.Append(name + ".push({x:" + p[i].x + ",y:" + p[i].y + ",fillColor:'rgba(" + p[i].fillColor + ")',name:'" + p[i].name + "'});");

        sb.Append("</script>");
        ClientScript.RegisterStartupScript(this.GetType(), name + "_from_cs", sb.ToString());
    }
}



public class consensus
{
    public double total_flat{get;set;}
    public int total_flat_analysts { get; set; }
    public int total_flat_analysts_ws { get; set; }
    public double total_weighted { get; set; }
    public int total_weighted_analysts { get; set; }
    public int total_weighted_analysts_ws { get; set; }
    public double ws_flat { get; set; }
    public int ws_flat_analysts { get; set; }
    public double ws_weighted { get; set; }
    public int ws_weighted_analysts { get; set; }
    public double crowd_flat { get; set; }
    public int crowd_flat_analysts { get; set; }
    public double crowd_weighted { get; set; }
    public int crowd_weighted_analysts { get; set; }

    public consensus()
    {
        total_flat = -1;
        total_weighted = -1;
        ws_flat = -1;
        ws_weighted = -1;
        crowd_flat = -1;
        crowd_weighted = -1;
    }
}

public class PT_weights_item
{
    public int analystID { get; set; }
    public string name { get; set; }
    public string broker { get; set; }
    public double confidence { get; set; }
    public double recency { get; set; }
    public double range { get; set; }
    public double expiration { get; set; }
    public double target { get; set; }
    public bool wall_st { get; set; }

    public DateTime created { get; set; }
    public DateTime reiterated { get; set; }
    public DateTime targetdate { get; set; }
    public string rationale { get; set; }
    public double performance { get; set; }
    public bool estimate { get; set; }
}

public class scatter_plot_point
{
    public double x { get; set; }
    public double y { get; set; }
    public string fillColor { get; set; }
    public string name { get; set; }
}