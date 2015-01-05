using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_backtest : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {   
        //backtest_v4_main();
        //random_stocks();
        //attach_financials_and_aggregate_tickers_histories();
    }

    protected void attach_financials_and_aggregate_tickers_histories()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var bs = from temp in db.Backtest_by_Positions where temp.horizon == 365 select temp;

        foreach (var b in bs)
        {
            var at = from temp in db.Aggregate_Tickers_Histories_v2s where temp.ticker == b.ticker && temp.date <= b.date && temp.date >= b.date.AddDays(-60) select temp;

            if (at.Any())
            {
                b.aggregate_ticker = at.OrderByDescending(c => c.date).First().id;
            }

            var financials = from temp in db.Financials where temp.ticker == b.ticker && temp.year == b.date.Year select temp;

            if (financials.Any())
            {
                b.financials = financials.First().id;
            }

            db.SubmitChanges();

        }
    }

    protected void backtest_v4_main()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        DateTime date = new DateTime(2005, 1, 1);
        while (date <= DateTime.Now.AddDays(-365))
        {
            Random rnd = new Random();
            date = date.AddDays(rnd.Next(1, 7));

            if (date <= DateTime.Now.AddDays(-365 * 3))
                backtest_v4(date, date.AddDays(365 * 3));

            if (date <= DateTime.Now.AddDays(-365 * 2))
                backtest_v4(date, date.AddDays(365 * 2));

            backtest_v4(date, date.AddDays(365));
        }
    }


    protected void backtest_v4(DateTime start, DateTime end)
    {
        DataClassesDataContext db = new DataClassesDataContext();

        double initial = 1e5;
        int analysts = 5;
        double sum_confidence_min = 3;
        int max_number_of_positions = 10;


        var positions_by_ticker = (from temp in db.Aggregate_Tickers_Histories_v2s where temp.analysts >= analysts && temp.confidence_sum >= sum_confidence_min && temp.consensus_percentage > 0 && temp.standard_deviation_percentage > 0 && temp.standard_deviation_flat > 0 && temp.date <= start && temp.date >= start.AddDays(-60) group temp by temp.ticker into g select g.OrderByDescending(b => b.date).First());
        positions_by_ticker = positions_by_ticker.OrderByDescending(b => b.consensus_percentage / b.standard_deviation_percentage).Take(max_number_of_positions);

        if (positions_by_ticker.Any())
        {
            int c = 1;
            foreach (var p in positions_by_ticker)
            {
                Backtest_by_Position back = new Backtest_by_Position();
                back.creation_time = DateTime.Now;
                back.date = start;
                back.ticker = p.ticker;
                back.rank = c;

                // spy
                var spy = from temp in db.fund_values where temp.fundID == 922 && temp.date >= start && temp.date <= end select temp;
                if (spy.Any())
                {
                    double spy_first = spy.OrderBy(b => b.date).First().adjValue;
                    double spy_last = spy.OrderByDescending(b => b.date).First().adjValue;
                    double dividend = spy.Sum(b => b.dividend.Value);
                    back.val_spy = initial * ((spy_last + dividend) / spy_first);
                }

                // position
                var fund_values = from temp in db.fund_values where temp.fundID == p.ticker && temp.date >= start && temp.date <= end select temp;
                double start_price = fund_values.OrderBy(b => b.date).First().closeValue.Value;
                double end_price = fund_values.OrderByDescending(b => b.date).First().closeValue.Value;
                double target_price = p.consensus;
                double terminal_price = 0;
                double dividend_value = 0;


                if (fund_values.Any())
                {
                    foreach (var f in fund_values.OrderBy(b => b.date))
                    {
                        terminal_price = f.closeValue.Value;
                        dividend_value += f.dividend.Value;
                        if (f.closeValue >= target_price)
                            break;
                    }
                }

                back.val_position = initial * ((terminal_price + dividend_value) / start_price);
                back.val_position_uncapped = initial * ((end_price + dividend_value) / start_price);
                back.split_or_dividend = (fund_values.Where(b => b.split.Value != 1).Any() || fund_values.Where(b => b.dividend.Value > 0).Any());
                back.horizon = (int)(end - start).TotalDays;
                back.method = "sharpe";

                db.Backtest_by_Positions.InsertOnSubmit(back);
                db.SubmitChanges();

                c++;
            }
        }

    }





    protected void random_stocks()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var tbs = from temp in db.Backtest_by_Positions where temp.horizon == 365 orderby temp.date ascending select temp;

        var funds = (from temp in db.fund_values select temp).GroupBy(b => b.fundID).Where(b => b.Count() >= 3472);
        int count = funds.Count();
        Random r = new Random();

        if (tbs.Any())
        {
            foreach (var tb in tbs)
            {
                bool x = false;
                while (!x)
                {
                    tb.ticker_random = funds.Skip(r.Next(0, count - 1)).First().First().fundID;
                    var fund_values = from temp in db.fund_values where temp.fundID == tb.ticker_random where temp.date >= tb.date && temp.date <= tb.date.AddDays(365) select temp;

                    if (fund_values.Any() && !fund_values.Where(b => b.split != 1).Any() && fund_values.Count()>=240)
                    {
                        double start = fund_values.OrderBy(b => b.date).First().closeValue.Value;
                        double end = fund_values.OrderByDescending(b => b.date).First().closeValue.Value;
                        double dividend = fund_values.Sum(b => b.dividend.Value);
                        tb.val_random = 1e5 * ((end + dividend) / start);
                        db.SubmitChanges();
                        x = true;
                    }
                }
            }
        }

        

        //for (int i=1;i<=6345;i++){
            
        //    date = date.AddDays(gen.Next(range));
            
        //    var fund = from temp in db.funds where temp.fundID == fundid select temp;
        //    var fund_values = from temp in db.fund_values where temp.fundID == fundid where temp.date >= date && temp.date <= date.AddDays(365) select temp;
        //    double start = fund_values.OrderBy(b => b.date).First().closeValue.Value;
        //    double end = fund_values.OrderByDescending(b => b.date).First().closeValue.Value;
        //    double dividend = fund_values.Sum(b => b.dividend.Value);
            
        //}
        

    }

    protected void backtest() // start backtesting now
    {
        DataClassesDataContext db = new DataClassesDataContext();
        var aggs = (from temp in db.Aggregate_Tickers_Histories_v2s select temp).GroupBy(b => b.date).OrderBy(b => b.First().date);

        foreach (var agg in aggs)
        {
            backtest_guts(1, 10, "Sharpe, Target capped", agg.First().date, 5, 3, db);
        }

    }
    
    protected void backtest_v3(int positions,int min_analysts,double min_sum_confidence,int years)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        DateTime date = new DateTime(2007,1,1);
        while (date <= DateTime.Now.AddDays(-365*years))
        {
            Random rnd = new Random();
            date = date.AddDays(rnd.Next(1, 7));
            backtest_guts(years, positions, "Sharpe, Target capped", date, min_analysts, min_sum_confidence, db);
        }
    }

    protected void backtest_guts(int horizon, int positions, string method,DateTime date,int analysts,double sum_confidence_min,DataClassesDataContext db)
    {
        Backtest_Sharpe back = new Backtest_Sharpe();
        back.creation_time = DateTime.Now;
        back.date = date;
        back.horizon = 1;
        back.method = method;
        back.positions = positions;
        
        double initial = 1e5;
        double spy_value = 0;
        double portfolio_value = 0;
        string tickers = string.Empty;

        // SPY Performance
        var spy = from temp in db.fund_values where temp.fundID == 922 && temp.date >= date && temp.date <= date.AddDays(365 * horizon) select temp;
        if (spy.Any())
        {
            double spy_first = spy.OrderBy(b => b.date).First().adjValue;
            double spy_last = spy.OrderByDescending(b => b.date).First().adjValue;
            double dividend = spy.Sum(b => b.dividend.Value);
            spy_value = initial * ((spy_last + dividend) / spy_first);
        }

        // Portfolio Performance

        var positions_by_ticker = (from temp in db.Aggregate_Tickers_Histories_v2s where temp.analysts >= analysts && temp.confidence_sum >= sum_confidence_min && temp.consensus_percentage > 0 && temp.standard_deviation_percentage>0 && temp.standard_deviation_flat>0 && temp.date <= date && temp.date >= date.AddDays(-60) group temp by temp.ticker into g select g.OrderByDescending(b => b.date).First());

        if (method == "Sharpe, Target capped")
        {
            positions_by_ticker = positions_by_ticker.OrderByDescending(b => b.consensus_percentage / b.standard_deviation_percentage).Take(positions);
        }
        else if (method == "Consensus, Target Capped"){
            positions_by_ticker = positions_by_ticker.OrderByDescending(b => b.consensus_percentage).Take(positions);
        }
        else if (method == "Sharpe * sum_confidence, Target capped")
        {
            positions_by_ticker = positions_by_ticker.OrderByDescending(b => b.confidence_sum * b.consensus_percentage / b.standard_deviation_percentage).Take(positions);
        }
        else if (method == "Sharpe Flat, Target Capped")
        {
            positions_by_ticker = positions_by_ticker.OrderByDescending(b => b.consensus_flat / b.standard_deviation_flat).Take(positions);
        }

        //var positions_by_ticker = (from temp in db.Aggregate_Tickers_Histories_v2s where temp.analysts>=analysts && temp.confidence_sum>=sum_confidence_min && temp.consensus_percentage>0 && temp.date <= date && temp.date >= date.AddDays(-60) group temp by temp.ticker into g select g.OrderByDescending(b => b.date).First()).OrderByDescending(b => b.confidence_sum * b.consensus_percentage / b.standard_deviation_percentage).Take(positions);
        if (positions_by_ticker.Any())
        {
            foreach (var p in positions_by_ticker)
            {
                double terminal_price = 0;
                double dividend = 0;
                tickers += p.ticker + ",";

                var fund_values = from temp in db.fund_values where temp.fundID == p.ticker && temp.date >= date && temp.date <= date.AddDays(365 * horizon) select temp;
                //double start_price = fund_values.OrderBy(b => b.date).First().adjValue;
                //double target_price = start_price * (p.consensus/start_price);
                double start_price = fund_values.OrderBy(b => b.date).First().closeValue.Value;
                //double target_price = start_price * (p.consensus / start_price);
                double target_price = p.consensus;

                if (fund_values.Any())
                {

                    foreach (var f in fund_values.OrderBy(b => b.date))
                    {
                        //terminal_price = f.adjValue;
                        terminal_price = f.closeValue.Value;
                        dividend += f.dividend.Value;
                        if (f.closeValue >= target_price)
                            break;
                    }


                }

                portfolio_value += (initial / positions) * ((terminal_price + dividend) / start_price);
            }
        }


        back.val_spy = spy_value;
        back.val_portfolio = portfolio_value;
        back.tickers = tickers;

        db.Backtest_Sharpes.InsertOnSubmit(back);
        db.SubmitChanges();
    }

    protected void build_aggregate_tickers_v2_per_industry(int industry) {
        DataClassesDataContext db = new DataClassesDataContext();
        var actions = from temp in db.Actions where temp.fund.peer_group == industry select temp;

        List<ticker_list_of_analyst_TP> list = new List<ticker_list_of_analyst_TP>();
        List<analyst_industry_sum> list_industry = new List<analyst_industry_sum>();
        
        if (actions.Any()) {
            int index = 0;
            int index_analyst = 0;
            int index_industry = 0;
            int index_industry_ticker = 0;

            foreach (var a in actions.OrderBy(b=>b.startDate)) {
                
                // keep a list of analyst target prices
                if (!list.Where(b => b.ticker == a.ticker).Any()) // add the ticker
                {
                    ticker_list_of_analyst_TP new_ticker = new ticker_list_of_analyst_TP();
                    new_ticker.ticker = a.ticker;
                    analyst_TP tp = new analyst_TP();
                    tp.analyst = a.article1.origin;
                    tp.date = a.startDate;
                    tp.target = a.targetValue;
                    tp.confidence = 0;
                    new_ticker.list_of_TPs = new List<analyst_TP>();
                    new_ticker.list_of_TPs.Add(tp);
                    list.Add(new_ticker);
                    index = list.Count() - 1;
                    index_analyst = 0;
                }
                else {
                    index = list.FindIndex(b => b.ticker == a.ticker);

                    if (!list[index].list_of_TPs.Where(b => b.analyst == a.article1.origin).Any()) // add the analyst
                    {
                        analyst_TP tp = new analyst_TP();
                        tp.analyst = a.article1.origin;
                        tp.date = a.startDate;
                        tp.target = a.targetValue;
                        tp.confidence = 0;
                        //list[index].list_of_TPs = new List<analyst_TP>();
                        list[index].list_of_TPs.Add(tp);
                        index_analyst = list[index].list_of_TPs.Count() - 1;
                    }
                    else // update analyst TP startdate & targetvalue
                    {
                        index_analyst = list[index].list_of_TPs.FindIndex(b => b.analyst == a.article1.origin);

                        list[index].list_of_TPs[index_analyst].date = a.startDate;
                        list[index].list_of_TPs[index_analyst].target = a.targetValue;
                    }
                }

                // find analyst performance history
                var ap_history = (from temp in db.AnalystPerformance_Histories where temp.ticker == a.ticker && temp.analyst == a.article1.origin && temp.date <= a.startDate select temp).OrderByDescending(b => b.date);
                int ap_history_id = 0;
                double action_scalar = 0;
                double accuracy = 0;
                double abs_ret = 0;
                double win_ratio = 0;
                double rel_ret = 0;

                if (ap_history.Any())
                {
                    action_scalar = (1 - Math.Exp(-(double)ap_history.First().actions / (double)3));
                    accuracy = Math.Max(Math.Min(ap_history.First().accuracy_average, 1), 0);
                    abs_ret = 10 * Math.Max(Math.Min(ap_history.First().return_average, 0.1), 0);
                    win_ratio = Math.Max(Math.Min((double)(ap_history.First().actions_matured + ap_history.First().actions_e_and_positive) / (double)ap_history.First().actions, 1), 0);
                }

                // keep aggrgate return sum for each analyst
                // accompanied by the last ID of AnalystPerformance (failsafe against duplicates)
                
                if (!list_industry.Where(b => b.analyst == a.article1.origin).Any()) // add analyst and first ticker
                {
                    analyst_industry_sum industry_sum = new analyst_industry_sum();
                    industry_sum.analyst = a.article1.origin;
                    analyst_industry_sum_details industry_sum_details = new analyst_industry_sum_details();
                    industry_sum_details.ap_history_last_id = ap_history_id;
                    industry_sum_details.ticker = a.ticker;
                    industry_sum_details.sum = abs_ret;
                    industry_sum.details = new List<analyst_industry_sum_details>();
                    industry_sum.details.Add(industry_sum_details);
                    list_industry.Add(industry_sum);
                    index_industry = list_industry.Count() - 1;
                }
                else // analyst exists in list
                {
                    index_industry = list_industry.FindIndex(b => b.analyst == a.article1.origin);

                    if (!list_industry[index_industry].details.Where(b => b.ticker == a.ticker).Any()) // if analyst exists but ticker does not
                    {
                        analyst_industry_sum_details industry_sum_details = new analyst_industry_sum_details();
                        industry_sum_details.ap_history_last_id = ap_history_id;
                        industry_sum_details.ticker = a.ticker;
                        industry_sum_details.sum = abs_ret;
                        //list_industry[index_industry].details = new List<analyst_industry_sum_details>();
                        list_industry[index_industry].details.Add(industry_sum_details);
                        index_industry_ticker = list_industry[index_industry].details.Count() - 1;
                    }
                    else // ticker exists
                    {
                        index_industry_ticker = list_industry[index_industry].details.FindIndex(b => b.ticker == a.ticker);

                        if (list_industry[index_industry].details[index_industry_ticker].ap_history_last_id != ap_history_id) // new data point, update ID and add abs_ret to sum
                        {
                            list_industry[index_industry].details[index_industry_ticker].ap_history_last_id = ap_history_id;
                            list_industry[index_industry].details[index_industry_ticker].sum += abs_ret;
                        }

                    }
                }

                // calculate rel_return based on list_industry
                // IF there's history
                if (ap_history_id>0){
                    var list_industry_by_analyst = list_industry.GroupBy(b => b.analyst);
                    double all_analysts = list_industry_by_analyst.Count();
                    double rank = 1;
                    foreach (var all_a in list_industry_by_analyst.OrderByDescending(b=>b.First().details.Sum(c=>c.sum)))
                    {
                        if (all_a.First().analyst == a.article1.origin)
                            break;

                        rank++;
                    }

                    rel_ret = all_analysts == 1 ? 0.5 : Math.Max(Math.Min(1 - (rank - 1) / (all_analysts - 1), 1), 0);
                }
                
                
                
                // build confidence for analyst
                list[index].list_of_TPs[index_analyst].confidence = action_scalar * 0.25 * (accuracy + abs_ret + win_ratio + rel_ret);

                // clear target prices older than a year (MAKE SURE THIS WORKS WELL!!!)
                //list[index].list_of_TPs = list[index].list_of_TPs.Where(b => b.date >= a.startDate.AddDays(-365)).ToList();
                var list_last_year = list[index].list_of_TPs.Where(b => b.date >= a.startDate.AddDays(-365)).ToList();

                // now build an AT Histories v2 entry
                Aggregate_Tickers_Histories_v2 at = new Aggregate_Tickers_Histories_v2();

                at.date = a.startDate;
                at.ticker = a.ticker;
                //at.analysts_flat = list.Where(b => b.ticker == at.ticker).First().list_of_TPs.GroupBy(b => b.analyst).Count();
                at.analysts_flat = list_last_year.GroupBy(b => b.analyst).Count();
                //at.analysts = list.Where(b => b.ticker == at.ticker).First().list_of_TPs.Where(b=>b.confidence>0).GroupBy(b => b.analyst).Count();
                at.analysts = list_last_year.Where(b => b.confidence > 0).GroupBy(b => b.analyst).Count();
                at.bullishness_flat = 0;
                at.bullishness = 0;
                at.confidence_sum = list_last_year.Sum(b => b.confidence);
                at.consensus_flat = list_last_year.Average(b => b.target);
                at.standard_deviation_flat = Ancillary.standard_deviation(list_last_year.Select(b => b.target), true);
                at.standard_deviation_flat_percentage = 0;
                at.top_analyst_target = 0;
                at.top_analyst_target_percentage = 0;
                at.consensus = 0;

                if (at.confidence_sum > 0)
                {
                    at.consensus = list_last_year.Sum(b => b.confidence * b.target) / at.confidence_sum;
                }
                    
                if (at.confidence_sum>0)
                {
                    double num = 0;
                    foreach (var l in list_last_year)
                    {
                        num += (l.confidence / at.confidence_sum) * Math.Pow(l.target - at.consensus, 2);
                    }

                    at.standard_deviation = Math.Sqrt(num);
                }
                

                try
                {
                    double last = (from temp in db.fund_values where temp.fundID == a.ticker && temp.isLatest.Value select temp.closeValue).First().Value;
                    at.standard_deviation_flat_percentage = at.standard_deviation_flat / last;
                    at.consensus_percentage = at.consensus / last;
                    at.standard_deviation_percentage = at.standard_deviation / last;
                }
                catch { }

                db.Aggregate_Tickers_Histories_v2s.InsertOnSubmit(at);
                db.SubmitChanges();

            }
        }

    }

    protected void backtest_v2()
    {
        //DataClassesDataContext db = new DataClassesDataContext();

        //for (int i = 1; i <= 2000; i++)
        //{
        //    DateTime date = new DateTime(2007, 1, 1);
        //    Random gen = new Random();
        //    int range = (DateTime.Today.AddDays(-365) - date).Days;
        //    date = date.AddDays(gen.Next(range));

        //    backtest_guts(1, 10, "Sharpe, Target capped", date, 5, 3, db);
        //    //backtest_guts(1, 20, "Sharpe, Target capped", date, 5, 3, db);
        //    //backtest_guts(1, 30, "Sharpe, Target capped", date, 5, 3, db);

        //    //backtest_guts(1, 10, "Consensus, Target Capped", date, 5, 3, db);
        //    //backtest_guts(1, 20, "Consensus, Target Capped", date, 5, 3, db);
        //    //backtest_guts(1, 30, "Consensus, Target Capped", date, 5, 3, db);

        //    //backtest_guts(1, 10, "Sharpe * sum_confidence, Target capped", date, 5, 3, db);
        //    //backtest_guts(1, 20, "Sharpe * sum_confidence, Target capped", date, 5, 3, db);
        //    //backtest_guts(1, 30, "Sharpe * sum_confidence, Target capped", date, 5, 3, db);

        //    //backtest_guts(1, 10, "Sharpe Flat, Target Capped", date, 5, 3, db);
        //    //backtest_guts(1, 20, "Sharpe Flat, Target Capped", date, 5, 3, db);
        //    //backtest_guts(1, 30, "Sharpe Flat, Target Capped", date, 5, 3, db);
        //}
    }

    private static void update_spy_adjusted_value()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        Quote quote = new Quote("SPY");
        DateTime Since2005 = new DateTime(2000, 1, 1);
        string[] histData = YahooStockEngine.FetchSince(quote, Since2005);
        int i = -1;
        foreach (string dateReturn in histData.Reverse<string>())
        {
            i++;
            string[] rowReturn = dateReturn.Split(',');
            if (rowReturn[0].Equals("Date") || rowReturn[0].Equals(""))
                continue;
            var value = from temp in db.fund_values where temp.date == Convert.ToDateTime(rowReturn[0]) && temp.fund.ticker == quote.Symbol select temp;
            if (value.Any())
            {
                fund_value curData = (fund_value)value.First();

                curData.adjValue = Convert.ToDouble(rowReturn[6]);
                db.SubmitChanges();
            }
        }

        db.SubmitChanges();
    }

    protected void calculations_for_a_date() {
        DataClassesDataContext db = new DataClassesDataContext();
        DateTime date = new DateTime(2007,1,1);
        //DateTime date = new DateTime(2009, 11, 4);
        int number_of_years = 1;

        Random gen = new Random();
        int range = (DateTime.Today.AddDays(-365) - date).Days;

        date = date.AddDays(gen.Next(range));

        var exists = from temp in db.Backtest_Sharpes where temp.date == date select temp;

        if (!exists.Any())
        {
            List<Tuple<int, double, double>> list = top_picks_for_a_given_date(date, 10, 5);

            //List<Tuple<int, double, double>> list = new List<Tuple<int, double, double>>();

            //list.Add(new Tuple<int,double,double>(820, 7.1,0));
            //list.Add(new Tuple<int,double,double>(3460, 3.4,0));
            //list.Add(new Tuple<int,double,double>(1192, 1.6,0));
            //list.Add(new Tuple<int,double,double>(8409, 1.75,0));
            //list.Add(new Tuple<int,double,double>(575, 0.42,0));
            //list.Add(new Tuple<int,double,double>(2953, 1.53,0));
            //list.Add(new Tuple<int,double,double>(2910, .56, 0));
            //list.Add(new Tuple<int,double,double>(471, 1.12, 0));
            //list.Add(new Tuple<int,double,double>(9798, 2.9, 0));
            //list.Add(new Tuple<int,double,double>(577, 0.56, 0));

            foreach (var a in list)
            {
                Response.Write((from temp in db.funds where temp.fundID == a.Item1 select temp).First().ticker + "- " + a.Item2 + "- " + a.Item3 + "<br>");
            }

            if (list.Any())
            {

                double initial = 1e5;
                int positions = list.Count();
                double portfolio_value = 0;
                double spy_value = 0;
                var spy = from temp in db.fund_values where temp.fundID == 922 && temp.date >= date && temp.date <= date.AddDays(365 * number_of_years) select temp;

                if (spy.Any())
                {
                    double spy_first = spy.OrderBy(b => b.date).First().closeValue.Value;
                    double spy_last = spy.OrderByDescending(b => b.date).First().closeValue.Value;
                    double dividend = spy.Sum(b => b.dividend.Value);
                    spy_value = initial * ((spy_last + dividend) / spy_first);

                }

                foreach (var l in list)
                {
                    //bool is_short = l.Item2 > 0;
                    double terminal_price = 0;
                    double dividend = 0;

                    var fund_values = from temp in db.fund_values where temp.fundID == l.Item1 && temp.date >= date && temp.date <= date.AddDays(365 * number_of_years) select temp;
                    double start_price = fund_values.OrderBy(b => b.date).First().closeValue.Value;
                    double target_price = start_price * (1 + l.Item2);

                    if (fund_values.Any())
                    {

                        foreach (var f in fund_values.OrderBy(b => b.date))
                        {
                            terminal_price = f.closeValue.Value;

                            dividend += f.dividend.Value;

                            if (f.closeValue >= target_price)
                                break;
                        }


                    }

                    portfolio_value += (initial / positions) * ((terminal_price + dividend) / start_price);
                }

                Backtest_Sharpe s = new Backtest_Sharpe();
                s.date = date;
                s.val_portfolio = portfolio_value;
                s.val_spy = spy_value;

                int c = 1;
                foreach (var a in list)
                {
                    s.tickers += a.Item1 + (c>=list.Count()?"":",");
                    c++;
                }

                db.Backtest_Sharpes.InsertOnSubmit(s);
                db.SubmitChanges();

                Response.Write("Date" + date + " - Portfolio: " + portfolio_value + " - SPY: " + spy_value);
            }
        }

        

        

    }

    protected List<Tuple<int, double, double>> top_picks_for_a_given_date(DateTime date, int number_of_positions, int threshold)
    {
        DataClassesDataContext db = new DataClassesDataContext();

        List<Tuple<int, double, double>> list = new List<Tuple<int, double, double>>();

        var exists = from temp in db.Aggregate_Tickers_Histories where temp.date == date select temp;

        if (exists.Any()) { 
            foreach (var a in exists.OrderByDescending(b=>b.consensus/b.stdev).Take(number_of_positions)){
                list.Add(new Tuple<int,double,double>(a.ticker,a.consensus,a.stdev));
            }
        }
        else
        {
            var actions = (from temp in db.Actions where temp.startDate <= date.AddDays(-365) select temp).GroupBy(b => b.ticker).Where(b => b.Count() >= threshold);

            if (actions.Any())
            {

                foreach (var a_ticker in actions)
                { // for each ticker

                    int ticker = a_ticker.First().ticker;

                    try
                    {
                        double price = (from temp in db.fund_values where temp.date <= date && temp.fundID == ticker orderby temp.date descending select temp).First().closeValue.Value;

                        var q = from n in a_ticker group n by n.article1.origin into g select new { ticker = g.Key, target = g.Max(b => b.startValue) };

                        if (q.Count() >= threshold)
                        {
                            double consensus = q.Average(b => b.target) / price - 1;
                            double standard_deviation = Ancillary.standard_deviation(q.Select(b => b.target), false) / price;

                            list.Add(new Tuple<int, double, double>(ticker, consensus, standard_deviation));
                            Aggregate_Tickers_History h = new Aggregate_Tickers_History();
                            h.ticker = ticker;
                            h.analysts = q.Count();
                            h.consensus = consensus;
                            h.date = date;
                            h.stdev = standard_deviation;
                            db.Aggregate_Tickers_Histories.InsertOnSubmit(h);
                            db.SubmitChanges();
                        }
                            

                    }
                    catch { }

                }
            }
        }

        number_of_positions = Math.Min(number_of_positions, list.Count());
        return list.OrderByDescending(b => b.Item2 / b.Item3).Take(number_of_positions).ToList();
    }

    protected void step2_part3_calculate_confidence() {
        DataClassesDataContext db = new DataClassesDataContext();
        var aps = from temp in db.AnalystPerformance_Histories where temp.ticker.HasValue orderby temp.date ascending select temp;

        //if (aps.Any())
        //{
        //    foreach (var a in aps)
        //    {
        //        // confidence
        //        double action_scalar = (1 - Math.Exp(-(double)a.actions / (double)3));
        //        double accuracy = Math.Max(Math.Min(a.accuracy_average, 1), 0);
        //        double abs_ret = 10 * Math.Max(Math.Min(a.return_average, 0.1), 0);
        //        double win_ratio = Math.Max(Math.Min((double)(a.actions_matured + a.actions_e_and_positive) / (double)a.actions, 1), 0);
        //        double rel_ret = 0;

        //        // look for relative return data
        //        var aps_industry = from temp in db.AnalystPerformance_Histories where temp.industry == a.fund.peer_group select temp;
        //        if (aps_industry.Any())
        //        {
        //            var aps_industry_analyst = aps_industry.Where(b => b.analyst == a.analyst);
        //            if (aps_industry_analyst.Any())
        //            {
        //                double all_analysts = aps_industry.Count();
        //                rel_ret = all_analysts == 1 ? 0.5 : Math.Max(Math.Min(1 - (aps_industry_analyst.First().rank - 1) / (all_analysts - 1), 1), 0);
        //            }
        //        }

        //        a.confidence = action_scalar * 0.25 * (accuracy + abs_ret + win_ratio + rel_ret);

        //        db.SubmitChanges();
        //    }
        //}
    }

    protected void step2_part2_calculate_ranking_within_industry() {
        DataClassesDataContext db = new DataClassesDataContext();


        // set the rankings within that industry
        //var get_industries = from temp in db.AnalystPerformances where temp.industry == ap_industry.First().fund.peer_group select temp;

        //if (get_industries.Any())
        //{
        //    int c = 0;
        //    foreach (var i in get_industries.OrderByDescending(b => b.return_annualized_sum))
        //    {
        //        c++;
        //        i.rank = c;
        //        db.SubmitChanges();
        //    }
        //}
    }

    protected void step2_part1_calculate_industry_return_sum_by_analyst() {
        DataClassesDataContext db = new DataClassesDataContext();
        var aps_by_industry = (from temp in db.AnalystPerformance_Histories where temp.fund.peer_group.HasValue && temp.ticker.HasValue select temp).GroupBy(b => b.fund.peer_group.Value);

        if (aps_by_industry.Any()) {
            foreach (var ap_by_industry in aps_by_industry) {
                foreach (var ap_by_analyst in ap_by_industry.GroupBy(b => b.analyst)) {

                    double sum = 0;

                    foreach (var ap in ap_by_analyst.OrderBy(b=>b.date)) {  // 

                        sum += ap.return_annualized_sum;

                        AnalystPerformance_History h = new AnalystPerformance_History();

                        h.analyst = ap.analyst;
                        h.industry = ap.fund.peer_group.Value;
                        h.return_annualized_sum = sum;
                        h.date = ap.date;

                        h.accuracy_average = 0;
                        h.actions = 0;
                        h.actions_be_and_negative = 0;
                        h.actions_e_and_positive = 0;
                        h.actions_matured = 0;
                        h.return_average = 0;

                        var ap_industry_exists = from temp in db.AnalystPerformance_Histories where temp.analyst == h.analyst && temp.industry == h.industry && temp.date == h.date select temp;

                        if (ap_industry_exists.Any())
                        {
                            ap_industry_exists.First().return_annualized_sum = h.return_annualized_sum;
                        }
                        else
                        {
                            db.AnalystPerformance_Histories.InsertOnSubmit(h);
                        }

                        db.SubmitChanges();

                    }
                }
            }
        }

    }

    protected void step1_analystperformance_historical() {
        DataClassesDataContext db = new DataClassesDataContext();
        var aps = (from temp in db.AnalystPerformances select temp).GroupBy(b=>b.ticker);

        if (aps.Any()) {
            foreach (var ap_ticker in aps) {
                var actions = from temp in db.Actions where temp.ticker == ap_ticker.First().ticker orderby temp.lastUpdated ascending select temp;

                if (actions.Any()) {
                    foreach (var a in actions) {
                        if (a.expired || a.matured || a.breached) {
                            string bem = a.expired ? "e" : a.matured ? "m" : "b";
                            Return_Accuracy_Output ret_acc = AdminBackend.calculate_absolute_return_accuracy(a,bem);

                            AnalystPerformance_History newPerformance = new AnalystPerformance_History();
                            newPerformance.return_average = ret_acc.abs_return;
                            newPerformance.accuracy_average = ret_acc.accuracy;
                            newPerformance.actions = 1;
                            newPerformance.actions_matured = 0;
                            newPerformance.actions_e_and_positive = 0;
                            newPerformance.actions_be_and_negative = 0;
                            newPerformance.return_annualized_sum = newPerformance.return_average;
                            newPerformance.date = a.lastUpdated;
                            newPerformance.analyst = a.article1.origin;
                            newPerformance.ticker = a.ticker;

                            switch (bem) { 
                                case "m":
                                    newPerformance.actions_matured = 1;
                                    break;
                                case "e":
                                    if (ret_acc.abs_return > 0)
                                        newPerformance.actions_e_and_positive = 1;
                                    else
                                        newPerformance.actions_be_and_negative = 1;
                                    break;
                                case "b":
                                    newPerformance.actions_be_and_negative = 1;
                                    break;
                            }

                            var ap_exists = from temp in db.AnalystPerformance_Histories where temp.ticker == a.ticker && temp.analyst == a.article1.origin select temp;

                            if (ap_exists.Any())
                            {
                                AnalystPerformance_History oldPerformance = ap_exists.OrderByDescending(b => b.date).First();

                                newPerformance.return_average = ((oldPerformance.return_average * oldPerformance.actions) + newPerformance.return_average) / (oldPerformance.actions + 1);
                                newPerformance.accuracy_average = ((oldPerformance.accuracy_average * oldPerformance.actions) + newPerformance.accuracy_average) / (oldPerformance.actions + 1);
                                newPerformance.actions = oldPerformance.actions + 1;
                                newPerformance.actions_be_and_negative = oldPerformance.actions_be_and_negative + newPerformance.actions_be_and_negative;
                                newPerformance.actions_e_and_positive = oldPerformance.actions_e_and_positive + newPerformance.actions_e_and_positive;
                                newPerformance.actions_matured = oldPerformance.actions_matured + newPerformance.actions_matured;
                                newPerformance.return_annualized_sum = oldPerformance.return_annualized_sum + newPerformance.return_annualized_sum;

                                if (oldPerformance.date == a.lastUpdated)
                                {
                                    oldPerformance.return_average = newPerformance.return_average;
                                    oldPerformance.accuracy_average = newPerformance.accuracy_average;
                                    oldPerformance.actions = newPerformance.actions;
                                    oldPerformance.actions_be_and_negative = newPerformance.actions_be_and_negative;
                                    oldPerformance.actions_e_and_positive = newPerformance.actions_e_and_positive;
                                    oldPerformance.actions_matured = newPerformance.actions_matured;
                                }
                                else {
                                    db.AnalystPerformance_Histories.InsertOnSubmit(newPerformance);
                                }
                            }
                            else {
                                db.AnalystPerformance_Histories.InsertOnSubmit(newPerformance);
                            }

                            db.SubmitChanges();

                            //try
                            //{
                            //    db.SubmitChanges();
                            //}
                            //catch {
                            //    Response.Write("Broke on action " + a.actionID);
                            //    break;
                            //}
                        }
                    }
                }
            }
        }

    }

    

    //protected void unknown() {
    //    DataClassesDataContext db = new DataClassesDataContext();
    //    var history = (from temp in db.Bloomberg_Consensus where temp.ticker != 450 select temp).GroupBy(b => b.ticker).OrderBy(b => b.Count());

    //    foreach (var h in history)
    //    {
    //        backtest_calculation_standalone(h.First().ticker);
    //    }
    //}

    //protected void clear_backtest_table() {
    //    DataClassesDataContext db = new DataClassesDataContext();
    //    var backtest = from temp in db.Backtests select temp;

    //    foreach (var b in backtest) {
    //        db.Backtests.DeleteOnSubmit(b);
    //        db.SubmitChanges();
    //    }
    //}

    //protected void calculate_ranking_history_all() {
    //    DataClassesDataContext db = new DataClassesDataContext();
    //    var all = (from temp in db.AnalystPerformances where temp.ticker.HasValue select temp).GroupBy(b => b.ticker);

    //    if (all.Any()) {
    //        foreach (var a in all) {
    //            calculate_ranking_history(a.First().ticker.Value, db);
    //        }
    //    }
    //}

    //protected void calculate_ranking_history(int ticker,DataClassesDataContext db) {

    //    List<Tuple<int, int, double, double>> analystperformance_history = new List<Tuple<int, int, double, double>>();

    //    var actions = (from temp in db.Actions where temp.ticker == ticker where temp.matured || temp.breached || temp.expired select temp).OrderBy(b=>b.lastUpdated);

    //    if (actions.Any()) {
    //        foreach (var action in actions) { 
    //            // create an entry @ Ranking_History
    //            // insert date, calculate rankings
    //            // 

    //            var history_for_that_date = from temp in db.AnalystPerformance_Histories where temp.date == action.lastUpdated && temp.ticker == ticker select temp;

    //            if (history_for_that_date.Any())
    //            {
    //                foreach (var h in history_for_that_date) {
    //                    db.AnalystPerformance_Histories.DeleteOnSubmit(h);
    //                    db.SubmitChanges();
    //                }
    //            }

    //            var ap = analystperformance_history.Where(b => b.Item2 == action.article1.user.userID);

    //            if (ap.Any())
    //            {
    //                Tuple<int, int, double, double> item;
    //                item = new Tuple<int, int, double, double>(ap.First().Item1 + 1, ap.First().Item2, (double)(ap.First().Item1 * ap.First().Item3 + action.progress) / (double)(ap.First().Item1+1), action.return_overall+ap.First().Item4);
    //                analystperformance_history.Remove(ap.First());
    //                analystperformance_history.Add(item);
    //            }
    //            else {
    //                Tuple<int, int, double, double> item;
    //                item = new Tuple<int, int, double, double>(1, action.article1.user.userID, action.progress, action.return_overall);
    //                analystperformance_history.Add(item);
    //            }

    //            int c=1;
    //            foreach (var aps in analystperformance_history.OrderByDescending(b=>b.Item4)) {
    //                AnalystPerformance_History h = new AnalystPerformance_History();
    //                h.date = action.lastUpdated;
    //                h.analyst = aps.Item2;
    //                h.rank = c;
    //                h.return_sum = aps.Item4;
    //                h.ticker = ticker;
    //                h.accuracy_average = aps.Item3;
    //                db.AnalystPerformance_Histories.InsertOnSubmit(h);
    //                db.SubmitChanges();

    //                c++;
    //            }

    //        }
    //    }
    //}

    //protected void backtest_calculation_standalone(int ticker) {
    //    DataClassesDataContext db = new DataClassesDataContext();

    //    var tgx_history = from temp in db.Bloomberg_Consensus where temp.ticker == ticker select temp;
    //    double bloomberg = 0;
    //    double invesd = 0;

    //    if (tgx_history.Any()) {
    //        calculate_ranking_history(ticker, db);

    //        foreach (var tgx in tgx_history)
    //        {
    //            for (int method = 3; method <= 10; method+=4)
    //            {
    //                bloomberg = tgx.price;
    //                invesd = construct_invesd(tgx.ticker, tgx.date, db, method);
    //                if (invesd == -1000)
    //                    invesd = bloomberg;
    //                Backtest b = new Backtest();
    //                b.date = tgx.date;
    //                b.ticker = tgx.ticker;
    //                b.accuracy_bloomberg = calc_accuracy(b.ticker, b.date, bloomberg, db);
    //                b.accuracy_invesd = calc_accuracy(b.ticker, b.date, invesd, db);
    //                b.method = method;
    //                db.Backtests.InsertOnSubmit(b);
    //                db.SubmitChanges();
    //            }
    //        }
    //    }
    //}

    //protected void backtest_calculations() {
    //    DataClassesDataContext db = new DataClassesDataContext();

    //    var allx = (from temp in db.AnalystPerformances select temp);
    //    // delete later on
    //    //allx = allx.Where(b => b.ticker == 450);
    //    var all = allx.GroupBy(b => b.ticker);
    //    double bloomberg = 0;
    //    double invesd = 0;

    //    if (all.Any()) {
    //        foreach (var t in all)
    //        {
    //            var tgx_history = from temp in db.Bloomberg_Consensus where temp.ticker == t.First().ticker select temp;

    //            foreach (var tgx in tgx_history) {
    //                for (int method = 1; method <= 1; method++) {
    //                    bloomberg = tgx.price;
    //                    invesd = construct_invesd(tgx.ticker, tgx.date,db,method);
    //                    if (invesd==-1000)
    //                        invesd= bloomberg;
    //                    Backtest b = new Backtest();
    //                    b.date = tgx.date;
    //                    b.ticker = tgx.ticker;
    //                    b.accuracy_bloomberg = calc_accuracy(b.ticker, b.date, bloomberg, db);
    //                    b.accuracy_invesd = calc_accuracy(b.ticker, b.date, invesd, db);
    //                    b.method = method;
    //                    db.Backtests.InsertOnSubmit(b);
    //                    db.SubmitChanges();
    //                }
    //            }
    //        }
    //    }
        
    //}

    //protected double construct_invesd(int ticker, DateTime date,DataClassesDataContext db,int method) {

    //    var ap = from temp in db.AnalystPerformance_Histories where temp.ticker == ticker && temp.date<=date select temp;
    //    DateTime d;

    //    double tgx = 0;
    //    if (ap.Any()) {
    //        d = ap.OrderByDescending(b => b.date).First().date;

    //        var ap_actual = from temp in db.AnalystPerformance_Histories where temp.ticker == ticker && temp.date == d select temp;            
    //        int count = 1;

    //        if (ap_actual.Any())
    //        {
    //            switch (method)
    //            {
    //                case 1:
    //                    ap_actual = ap_actual.Where(b => b.return_sum > 0 && b.rank <= 10).OrderBy(b => b.rank).Take(10);
    //                    tgx = 0;
    //                    count = 1;
    //                    if (ap_actual.Any())
    //                    {
    //                        foreach (var aa in ap_actual)
    //                        {
    //                            var actions = from temp in db.Actions where temp.ticker == ticker && temp.article1.origin == aa.analyst && temp.startDate <= date && temp.targetDate >= date select temp;

    //                            if (actions.Any())
    //                            {
    //                                if (count == 1)
    //                                {
    //                                    tgx = actions.OrderByDescending(b => b.startDate).First().targetValue;
    //                                }
    //                                else
    //                                {
    //                                    tgx = (double)((count - 1) * tgx + actions.First().targetValue) / (double)count;
    //                                }
    //                            }

    //                            count++;
    //                        }
    //                    }
    //                    else {
    //                        tgx = -1000;
    //                    }
    //                    break;
    //                case 2:
    //                    ap_actual = ap_actual.Where(b => b.return_sum > 0 && b.rank <= 10).OrderBy(b => b.rank).Take(5);
    //                    tgx = 0;
    //                    count = 1;
    //                    foreach (var aa in ap_actual)
    //                    {
    //                        var actions = from temp in db.Actions where temp.ticker == ticker && temp.article1.origin == aa.analyst && temp.startDate <= date && temp.targetDate <= date select temp;

    //                        if (actions.Any())
    //                        {
    //                            if (count == 1)
    //                            {
    //                                tgx = actions.OrderByDescending(b => b.startDate).First().targetValue;
    //                            }
    //                            else
    //                            {
    //                                tgx = (double)((count - 1) * tgx + actions.First().targetValue) / (double)count;
    //                            }
    //                        }
    //                    }
    //                    break;
    //                case 3:
    //                    ap_actual = ap_actual.Where(b => b.return_sum > 0 && b.rank <= 3).OrderBy(b => b.rank).Take(10);
    //                    tgx = 0;
    //                    count = 1;
    //                    if (ap_actual.Any())
    //                    {
    //                        foreach (var aa in ap_actual)
    //                        {
    //                            var actions = from temp in db.Actions where temp.ticker == ticker && temp.article1.origin == aa.analyst && temp.startDate <= date && temp.targetDate >= date select temp;

    //                            if (actions.Any())
    //                            {
    //                                if (count == 1)
    //                                {
    //                                    tgx = actions.OrderByDescending(b => b.startDate).First().targetValue;
    //                                }
    //                                else
    //                                {
    //                                    tgx = (double)((count - 1) * tgx + actions.First().targetValue) / (double)count;
    //                                }
    //                            }

    //                            count++;
    //                        }
    //                    }
    //                    else {
    //                        tgx = -1000;
    //                    }
    //                    break;
    //                case 4:
    //                    ap_actual = ap_actual.Where(b => b.rank <= 10).OrderBy(b => b.rank).Take(10);
    //                    tgx = 0;
    //                    count = 1;
    //                    foreach (var aa in ap_actual)
    //                    {
    //                        var actions = from temp in db.Actions where temp.ticker == ticker && temp.article1.origin == aa.analyst && temp.startDate <= date && temp.targetDate <= date select temp;

    //                        if (actions.Any())
    //                        {
    //                            if (count == 1)
    //                            {
    //                                tgx = actions.OrderByDescending(b => b.startDate).First().targetValue;
    //                            }
    //                            else
    //                            {
    //                                tgx = (double)((count - 1) * tgx + actions.First().targetValue) / (double)count;
    //                            }
    //                        }
    //                    }
    //                    break;
    //                case 5:
    //                    ap_actual = ap_actual.Where(b => b.rank <= 10).OrderBy(b => b.rank).Take(5);
    //                    tgx = 0;
    //                    count = 1;
    //                    foreach (var aa in ap_actual)
    //                    {
    //                        var actions = from temp in db.Actions where temp.ticker == ticker && temp.article1.origin == aa.analyst && temp.startDate <= date && temp.targetDate <= date select temp;

    //                        if (actions.Any())
    //                        {
    //                            if (count == 1)
    //                            {
    //                                tgx = actions.OrderByDescending(b => b.startDate).First().targetValue;
    //                            }
    //                            else
    //                            {
    //                                tgx = (double)((count - 1) * tgx + actions.First().targetValue) / (double)count;
    //                            }
    //                        }
    //                    }
    //                    break;
    //                case 6:
    //                    ap_actual = ap_actual.Where(b => b.rank <= 10).OrderBy(b => b.rank).Take(3);
    //                    tgx = 0;
    //                    count = 1;
    //                    foreach (var aa in ap_actual)
    //                    {
    //                        var actions = from temp in db.Actions where temp.ticker == ticker && temp.article1.origin == aa.analyst && temp.startDate <= date && temp.targetDate <= date select temp;

    //                        if (actions.Any())
    //                        {
    //                            if (count == 1)
    //                            {
    //                                tgx = actions.OrderByDescending(b => b.startDate).First().targetValue;
    //                            }
    //                            else
    //                            {
    //                                tgx = (double)((count - 1) * tgx + actions.First().targetValue) / (double)count;
    //                            }
    //                        }
    //                    }
    //                    break;
    //                case 7:
    //                    ap_actual = ap_actual.OrderBy(b => b.rank).Take(10);
    //                    tgx = 0;
    //                    count = 1;
    //                    if (ap_actual.Any())
    //                    {
    //                        foreach (var aa in ap_actual)
    //                        {
    //                            var actions = from temp in db.Actions where temp.ticker == ticker && temp.article1.origin == aa.analyst && temp.startDate <= date && temp.targetDate >= date select temp;

    //                            if (actions.Any())
    //                            {
    //                                tgx += actions.OrderByDescending(b => b.startDate).First().targetValue * (double)(1 / aa.rank);
    //                            }

    //                            count++;
    //                        }
    //                        tgx /= (double)ap_actual.Sum(b => (1 / b.rank));
    //                    }
    //                    else {
    //                        tgx = -1000;
    //                    }
    //                    break;
    //                case 8:
    //                    ap_actual = ap_actual.Where(b => b.rank <= 10).OrderBy(b => b.rank);
    //                    tgx = 0;
    //                    count = 1;
    //                    foreach (var aa in ap_actual)
    //                    {
    //                        var actions = from temp in db.Actions where temp.ticker == ticker && temp.article1.origin == aa.analyst && temp.startDate <= date && temp.targetDate <= date select temp;

    //                        if (actions.Any())
    //                        {
    //                            if (count == 1)
    //                            {
    //                                tgx += actions.OrderByDescending(b => b.startDate).First().targetValue * (double)(1 / aa.rank);
    //                            }
    //                            else
    //                            {
    //                                tgx = (double)((count - 1) * tgx + actions.First().targetValue) / (double)count;
    //                            }
    //                        }
    //                    }

    //                    tgx /= (double)ap_actual.Sum(b => (1 / b.rank));
    //                    break;
    //                case 9:
    //                    ap_actual = ap_actual.Where(b => b.rank <= 5).OrderBy(b => b.rank);
    //                    tgx = 0;
    //                    count = 1;
    //                    foreach (var aa in ap_actual)
    //                    {
    //                        var actions = from temp in db.Actions where temp.ticker == ticker && temp.article1.origin == aa.analyst && temp.startDate <= date && temp.targetDate <= date select temp;

    //                        if (actions.Any())
    //                        {
    //                            if (count == 1)
    //                            {
    //                                tgx += actions.OrderByDescending(b => b.startDate).First().targetValue * (double)(1 / aa.rank);
    //                            }
    //                            else
    //                            {
    //                                tgx = (double)((count - 1) * tgx + actions.First().targetValue) / (double)count;
    //                            }
    //                        }
    //                    }

    //                    tgx /= (double)ap_actual.Sum(b => (1 / b.rank));
    //                    break;
    //                case 10:
    //                    ap_actual = ap_actual.Where(b => b.rank <= 3).OrderBy(b => b.rank);
    //                    tgx = 0;
    //                    count = 1;
    //                    foreach (var aa in ap_actual)
    //                    {
    //                        var actions = from temp in db.Actions where temp.ticker == ticker && temp.article1.origin == aa.analyst && temp.startDate <= date && temp.targetDate <= date select temp;

    //                        if (actions.Any())
    //                        {
    //                            if (count == 1)
    //                            {
    //                                tgx += actions.OrderByDescending(b => b.startDate).First().targetValue * (double)(1 / aa.rank);
    //                            }
    //                            else
    //                            {
    //                                tgx = (double)((count - 1) * tgx + actions.First().targetValue) / (double)count;
    //                            }
    //                        }
    //                    }

    //                    tgx /= (double)ap_actual.Sum(b => (1 / b.rank));
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        tgx = -1000;
    //    }

    //    return tgx;
    //}

    //protected double calc_accuracy(int ticker,DateTime start, double target,DataClassesDataContext db) {

    //    var ticker_values = from temp in db.fund_values where temp.fundID == ticker && temp.date >= start && temp.date <= start.AddDays(365) select temp;
    //    bool is_long = true;
    //    double close_value = 0;

    //    if (ticker_values.Any()) {
    //        double start_value = ticker_values.OrderBy(b => b.date).First().closeValue.Value;
    //        is_long = target >= start_value;

    //        if ((is_long && ticker_values.Max(b => b.highValue) >= target) || (!is_long && ticker_values.Min(b => b.lowValue) <= target))
    //        {
    //            return 1;
    //        }
    //        else{
    //            close_value = ticker_values.OrderByDescending(b => b.date).First().closeValue.Value;

    //            if ((close_value >= start_value && is_long) || (!is_long && close_value<=start_value))
    //            {
    //                return (close_value - start_value) / (target - start_value);
    //            }
    //            else {
    //                return Math.Max(-1,(close_value / start_value - 1)*(is_long?1:-1));
    //            }
    //        }

    //    }

    //    return 0;
    //}

    
}

public class ticker_list_of_analyst_TP
{
    public int ticker { get; set; }
    public List<analyst_TP> list_of_TPs { get; set; }
}

public class analyst_TP
{
    public int analyst { get; set; }
    public double confidence { get; set; }
    public DateTime date { get; set; }
    public double target { get; set; }
}

public class analyst_industry_sum
{
    public int analyst { get; set; }
    public List<analyst_industry_sum_details> details { get; set; }
}

public class analyst_industry_sum_details
{
    public int ticker { get; set; }
    public int ap_history_last_id { get; set; } // this makes sure that no return is being summed more than once
    public double sum { get; set; }
}

//protected class tickers
//{
//    public int ticker { get; set; }
//    public List<int> analyst { get; set; }
//    public List<double> TP { get; set; }
//}

