using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;

public partial class Calculate_Rankings : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        //clear_rankings(db);
        // bem only
        calculate_all_bem(db);

        // interim
        // 1w, 1m, 6m, 1y, all-time
        //AuthorRanking(0, 0, false, false, DateTime.Now.AddDays(-7), 7);
        //AuthorRanking(0, 0, false, false, DateTime.Now.AddDays(-30), 30);
        //AuthorRanking(0, 0, false, false, DateTime.Now.AddDays(-183), 183);
        //AuthorRanking(0, 0, false, false, DateTime.Now.AddDays(-365), 365);
        //AuthorRanking(0, 0, false, false, DateTime.Now.AddDays(-730), 730);
        //AuthorRanking(0, 0, false, false, DateTime.Now.AddDays(-1825), 1825);
        // all-time???

        // all active in a given period
        //AuthorRanking(0, 0, false, true, DateTime.Now.AddDays(-7), 7);
    }

    protected void calculate_all_bem(DataClassesDataContext db)
    {
            // BEM by sector
            //var sectors = (from temp in db.Actions where (temp.matured == true || temp.breached == true || temp.expired == true) select temp).GroupBy(b => b.fund.sector).OrderByDescending(b => b.Count());
            //foreach (var s in sectors)
            //{
            //    // ATTENTION make sure tickers have sectors
            //    if (s.First().fund.sector.HasValue)
            //        AuthorRanking(0, s.First().fund.sector.Value, true, false, DateTime.MinValue, 0);
            //}

            //// BEM by ticker
            var tickers = (from temp in db.Actions where (temp.matured == true || temp.breached == true || temp.expired == true) select temp).GroupBy(b => b.ticker).OrderByDescending(b => b.Count());
            //var tickers = (from temp in db.Actions where temp.ticker==785 && (temp.matured == true || temp.breached == true || temp.expired == true) select temp).GroupBy(b => b.ticker).OrderByDescending(b => b.Count());
            foreach (var t in tickers)
            {
                AuthorRanking(t.First().ticker, 0, true, false, DateTime.MinValue, 0);
            }

            // BEM all
            //AuthorRanking(0,0,true,false,DateTime.MinValue,0);
    }

    protected void clear_rankings(DataClassesDataContext db)
    {
        db.AnalystPerformances.DeleteAllOnSubmit(db.AnalystPerformances);
        db.SubmitChanges();
    }

    protected void AuthorRanking(int ticker,int sector,bool bem,bool active,DateTime start,int horizon)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            var step1 = from temp in db.Actions select temp;

            // REMOVE
            // REMOVE
            // REMOVE
            //step1 = step1.Where(b => b.article1.origin == 11248 || b.article1.origin == 10333);
            //step1 = step1.Where(b => b.article1.origin == 11248);

            // no tickets
            step1 = step1.Where(b => b.is_ticket == false);
            // filter all-time BEM or horizon-based interim actions
            if (bem == true)
                // Breached/Expired/Matured ==> Filter all-time BEM actions
                step1 = step1.Where(b => b.matured == true || b.breached == true || b.expired == true);
            else if (active == false)
                // Filter B/M and LU>start or !B/M and TD>start
                step1 = step1.Where(b => b.lastUpdated > start);
            else
                step1 = step1.Where(b => b.lastUpdated > start && b.matured == false && b.expired == false && b.breached == false);

            // filter based on ticker or sector
            if (ticker != 0 && sector == 0)
                step1 = step1.Where(b => b.ticker == ticker);
            else if (ticker == 0 && sector != 0)
            {
                step1 = step1.Where(b => b.fund.sector == sector);
            }

            var actions_by_author = step1.GroupBy(b=>b.article1.user.userID);

            // database variables
            int analyst;
            int actions_active_positive;
            int actions_active_negative;
            double return_average;
            double return_min;
            double return_max;
            double return_annualized_average;
            double return_annualized_min;
            double return_annualized_max;
            double return_annualized_sum;
            double alpha_average;
            double alpha_min;
            double alpha_max;
            double accuracy_average;
            double accuracy_min;
            double accuracy_max;
            double horizon_average;
            double horizon_min;
            double horizon_max;
            int top1;
            int top2;
            int top3;
            int bottom1;
            int bottom2;
            int bottom3;
            double top1_return;
            double top2_return;
            double top3_return;
            double bottom1_return;
            double bottom2_return;
            double bottom3_return;
            double remaining_average;
            double remaining_min;
            double remaining_max;
            int actions_matured;
            int actions_be_and_negative;
            int actions_e_and_positive;
            int horizon_short;
            int horizon_medium;
            int horizon_long;
            double progress_return;
            double progress_time;
            
            // intermediary variables
            double benchmark_dividend;
            double benchmark_lastupdated;
            double benchmark_start;
            int benchmark_fund_id;
            double beta;

            // ranking array
            List<Tuple<int, double>> rank_later = new List<Tuple<int, double>>();

            foreach (var abas in actions_by_author)
            {
                //Thread t = new Thread();
                //t.Start();

                // Arrays
                List<double> alpha_array = new List<double>();
                List<double> return_array = new List<double>();
                List<double> return_annualized_array = new List<double>();
                List<double> accuracy_array = new List<double>();
                List<double> horizon_array = new List<double>();
                List<double> remaining_array = new List<double>();
                List<double> beta_array = new List<double>();
                List<double> target_array = new List<double>();

                List<Tuple<int, double>> action_return = new List<Tuple<int, double>>();
                

                // database variables
                analyst = abas.First().article1.origin;
                actions_active_positive = 0;
                actions_active_negative = 0;
                top1 = 0;
                top2 = 0;
                top3 = 0;
                bottom1 = 0;
                bottom2 = 0;
                bottom3 = 0;
                remaining_average = 0;
                remaining_min = 0;
                remaining_max = 0;
                actions_matured = 0;
                actions_be_and_negative = 0;
                actions_e_and_positive = 0;
                top1_return = 0;
                top2_return = 0;
                top3_return = 0;
                bottom1_return = 0;
                bottom2_return = 0;
                bottom3_return = 0;
                horizon_short = 0;
                horizon_medium = 0;
                horizon_long = 0;
                progress_return = 0;
                progress_time = 0;

                // intermediary variables
                benchmark_fund_id = 922;
                beta = 0;

                foreach (var aba in abas)
                {
                    DateTime begin = start > aba.startDate ? start : aba.startDate;
                    double start_value = aba.startValue;
                    double dividend = aba.dividend;

                    if (aba.matured == true)
                        actions_matured++;

                    if (start > aba.startDate)
                    {
                        // ATTENTION
                        // these need to be adjusted for split(s), if any
                        start_value = (from temp in db.fund_values where temp.fundID == aba.ticker && temp.date >= begin orderby temp.date ascending select temp.closeValue.Value).Take(1).First();
                        dividend = (from temp in db.fund_values where temp.fundID == aba.ticker && temp.date >= begin && temp.date<=aba.lastUpdated select temp.dividend).Sum().Value;
                    }

                    if (aba.TotalReturn == false)
                        dividend = 0;

                    // r, ra, target, h (in days), remaining days
                    return_array.Add(((aba.currentValue + dividend - start_value) / start_value)*(aba.@short?-1:1));
                    return_annualized_array.Add(return_array.Last() * (365 / (aba.targetDate - aba.startDate).TotalDays));
                    target_array.Add(Math.Abs((aba.targetValue - aba.startValue)) / aba.startValue); // total return = false will mean that we can have >100% of target %
                    horizon_array.Add((aba.targetDate - aba.startDate).TotalDays);
                    if (bem == false)
                        remaining_array.Add(Math.Max(0,(aba.targetDate - aba.lastUpdated).TotalDays));

                    // action return array
                    Tuple<int, double> item;
                    item = new Tuple<int, double>(aba.actionID, return_array.Last());
                    action_return.Add(item);

                    // get benchmark return data
                    benchmark_dividend = 0;
                    benchmark_start = 0;
                    var bench_div = (from temp in db.fund_values where temp.fundID == benchmark_fund_id select temp).Where(b => b.date >= begin && b.date <= aba.lastUpdated);
                    if (bench_div.Any())
                        benchmark_dividend = bench_div.Sum(b => b.dividend.Value);
                    benchmark_lastupdated = (from temp in db.fund_values where temp.fundID == benchmark_fund_id && temp.date <= aba.lastUpdated && temp.date >= aba.lastUpdated.AddDays(-10) select temp).OrderByDescending(b => b.date).First().closeValue.Value;
                    var bench_start = from temp in db.fund_values where temp.fundID == benchmark_fund_id && temp.date >= begin && temp.date <= begin.AddDays(10) select temp;
                    if (bench_start.Any())
                        benchmark_start = bench_start.OrderBy(b=>b.date).First().closeValue.Value;

                    // calculate beta, alpha
                    beta = (benchmark_lastupdated + benchmark_dividend - benchmark_start) / benchmark_start;
                    beta_array.Add(beta);
                    alpha_array.Add(return_array.Last() - beta);
                    //if (bem == true)
                    //{
                    //    aba.beta = alpha_array.Last();
                    //    db.SubmitChanges();
                    //}
                    
                    // accuracy
                    if ((aba.@short == false && aba.currentValue + aba.dividend >= start_value) || (aba.@short == true && aba.currentValue + aba.dividend <= start_value))
                    {
                        if (Math.Abs((aba.currentValue + dividend - start_value) / (aba.targetValue - start_value)) > 1)
                        {
                            int amir = 0;
                        }
                        accuracy_array.Add(Math.Abs((aba.currentValue + dividend - start_value) / (aba.targetValue - start_value)));
                        if (aba.expired == false && aba.breached == false && aba.matured == false)
                            actions_active_positive++;
                        if (aba.expired == true)
                            actions_e_and_positive++;
                    }
                    else
                    {
                        accuracy_array.Add(-1 * Math.Abs((aba.currentValue + dividend - start_value) / (start_value - aba.lowerValue)));
                        if (aba.expired == false && aba.breached == false && aba.matured == false)
                            actions_active_negative++;
                        if (aba.breached == true || aba.expired == true)
                            actions_be_and_negative++;
                    }
                }

                return_average = return_array.Average();
                return_min = return_array.Min();
                return_max = return_array.Max();
                return_annualized_average = return_annualized_array.Average();
                return_annualized_min = return_annualized_array.Min();
                return_annualized_max = return_annualized_array.Max();
                return_annualized_sum = return_annualized_array.Sum();
                alpha_average = alpha_array.Average();
                alpha_min = alpha_array.Min();
                alpha_max = alpha_array.Max();
                accuracy_average = accuracy_array.Average();
                accuracy_min = accuracy_array.Min();
                accuracy_max = accuracy_array.Max();
                horizon_average = horizon_array.Average();
                horizon_min = horizon_array.Min();
                horizon_max = horizon_array.Max();

                // horizon composition
                foreach (var x in horizon_array)
                {
                    if (x < 183)
                        horizon_short++;
                    else if (x >= 183 && x < 548)
                        horizon_medium++;
                    else
                        horizon_long++;
                }

                int c = 0;
                if (bem == false)
                {
                    // return progress
                    List<double> pr = new List<double>();
                    foreach (var x in return_array)
                    {
                        pr.Add(x / target_array[c]);
                        c++;
                    }
                    progress_return = pr.Average();

                    // time progress (past vs. remaining)
                    c = 0;
                    List<double> pt = new List<double>();
                    foreach (var x in remaining_array)
                    {
                        if (x == 0)
                            pt.Add(1);
                        else
                            pt.Add(horizon / (horizon + x));
                    }
                    progress_time = pt.Average();

                    // remaining
                    remaining_average = remaining_array.Average();
                    remaining_min = remaining_array.Min();
                    remaining_max = remaining_array.Max();
                }

                // find top 3
                c=1;

                foreach (var x in action_return.Where(x=>x.Item2>0).OrderByDescending(x => x.Item2))
                {
                    switch (c)
                    {
                        case 1:
                            top1 = x.Item1;
                            top1_return = x.Item2;
                            break;
                        case 2:
                            top2 = x.Item1;
                            top2_return = x.Item2;
                            break;
                        case 3:
                            top3 = x.Item1;
                            top3_return = x.Item2;
                            break;
                        default:
                            break;
                    }

                    c++;
                    if (c == 4)
                        break;
                }

                // find bottom 3
                c = 1;
                foreach (var x in action_return.Where(x=>x.Item2<0).OrderBy(x => x.Item2))
                {
                    switch (c)
                    {
                        case 1:
                            bottom1 = x.Item1;
                            bottom1_return = x.Item2;
                            break;
                        case 2:
                            bottom2 = x.Item1;
                            bottom2_return = x.Item2;
                            break;
                        case 3:
                            bottom3 = x.Item1;
                            bottom3_return = x.Item2;
                            break;
                        default:
                            break;
                    }

                    c++;
                    if (c == 4)
                        break;
                }

                // prepare ranking array
                Tuple<int,double> item2;
                item2 = new Tuple<int, double>(analyst, return_annualized_sum);
                rank_later.Add(item2);

                // insert into the database
                
                // OR - find and replace
                var record = from temp in db.AnalystPerformances where temp.analyst == analyst && temp.horizon == horizon && temp.active_only == active select temp;
                if (ticker == 0)
                    record = record.Where(b => !b.ticker.HasValue);
                else
                    record = record.Where(b => b.ticker == ticker);

                if (sector == 0)
                    record = record.Where(b => !b.sector.HasValue);
                else
                    record = record.Where(b => b.sector == sector);

                if (record.Any())
                {
                    record.First().actions = abas.Count();
                    record.First().actions_active_positive = actions_active_positive;
                    record.First().actions_active_negative = actions_active_negative;
                    record.First().return_average = return_average;
                    record.First().return_min = return_min;
                    record.First().return_max = return_max;
                    record.First().return_annualized_average = return_annualized_average;
                    record.First().return_annualized_min = return_annualized_min;
                    record.First().return_annualized_max = return_annualized_max;
                    record.First().return_annualized_sum = return_annualized_sum;
                    record.First().alpha_average = alpha_average;
                    record.First().alpha_min = alpha_min;
                    record.First().alpha_max = alpha_max;
                    record.First().accuracy_average = accuracy_average;
                    record.First().accuracy_min = accuracy_min;
                    record.First().accuracy_max = accuracy_max;
                    record.First().horizon_average = horizon_average;
                    record.First().horizon_min = horizon_min;
                    record.First().horizon_max = horizon_max;
                    if (top1 != 0)
                        record.First().top1 = top1;
                    if (top2 != 0)
                        record.First().top2 = top2;
                    if (top3 != 0)
                        record.First().top3 = top3;
                    if (bottom1 != 0)
                        record.First().bottom1 = bottom1;
                    if (bottom2 != 0)
                        record.First().bottom2 = bottom2;
                    if (bottom3 != 0)
                        record.First().bottom3 = bottom3;
                    record.First().date = DateTime.Now;
                    record.First().rank = 0;
                    record.First().horizon = horizon;
                    record.First().remaining_average = remaining_average;
                    record.First().remaining_min = remaining_min;
                    record.First().remaining_max = remaining_max;
                    record.First().active_only = active;
                    record.First().actions_matured = actions_matured;
                    record.First().actions_be_and_negative = actions_be_and_negative;
                    record.First().actions_e_and_positive = actions_e_and_positive;
                    record.First().target_average = target_array.Average();
                    record.First().target_min = target_array.Min();
                    record.First().target_max = target_array.Max();
                    record.First().top1_return = top1_return;
                    record.First().top2_return = top2_return;
                    record.First().top3_return = top3_return;
                    record.First().bottom1_return = bottom1_return;
                    record.First().bottom2_return = bottom2_return;
                    record.First().bottom3_return = bottom3_return;

                    record.First().horizon_short = horizon_short;
                    record.First().horizon_medium = horizon_medium;
                    record.First().horizon_long = horizon_long;
                    record.First().progress_return = progress_return;
                    record.First().progress_time = progress_time;

                    record.First().date = DateTime.Now;

                    try
                    {
                        db.SubmitChanges();
                    }
                    catch { }

                }
                else
                {
                    AnalystPerformance analyst_performance = new AnalystPerformance();
                    analyst_performance.analyst = analyst;

                    if (sector != 0)
                        analyst_performance.sector = sector;
                    if (ticker != 0)
                        analyst_performance.ticker = ticker;

                    analyst_performance.actions = abas.Count();
                    analyst_performance.actions_active_positive = actions_active_positive;
                    analyst_performance.actions_active_negative = actions_active_negative;
                    analyst_performance.return_average = return_average;
                    analyst_performance.return_min = return_min;
                    analyst_performance.return_max = return_max;
                    analyst_performance.return_annualized_average = return_annualized_average;
                    analyst_performance.return_annualized_min = return_annualized_min;
                    analyst_performance.return_annualized_max = return_annualized_max;
                    analyst_performance.return_annualized_sum = return_annualized_sum;
                    analyst_performance.alpha_average = alpha_average;
                    analyst_performance.alpha_min = alpha_min;
                    analyst_performance.alpha_max = alpha_max;
                    analyst_performance.accuracy_average = accuracy_average;
                    analyst_performance.accuracy_min = accuracy_min;
                    analyst_performance.accuracy_max = accuracy_max;
                    analyst_performance.horizon_average = horizon_average;
                    analyst_performance.horizon_min = horizon_min;
                    analyst_performance.horizon_max = horizon_max;
                    if (top1 != 0)
                        analyst_performance.top1 = top1;
                    if (top2 != 0)
                        analyst_performance.top2 = top2;
                    if (top3 != 0)
                        analyst_performance.top3 = top3;
                    if (bottom1 != 0)
                        analyst_performance.bottom1 = bottom1;
                    if (bottom2 != 0)
                        analyst_performance.bottom2 = bottom2;
                    if (bottom3 != 0)
                        analyst_performance.bottom3 = bottom3;
                    analyst_performance.date = DateTime.Now;
                    analyst_performance.rank = 0;
                    analyst_performance.horizon = horizon;
                    analyst_performance.remaining_average = remaining_average;
                    analyst_performance.remaining_min = remaining_min;
                    analyst_performance.remaining_max = remaining_max;
                    analyst_performance.active_only = active;
                    analyst_performance.actions_matured = actions_matured;
                    analyst_performance.actions_be_and_negative = actions_be_and_negative;
                    analyst_performance.actions_e_and_positive = actions_e_and_positive;
                    analyst_performance.target_average = target_array.Average();
                    analyst_performance.target_min = target_array.Min();
                    analyst_performance.target_max = target_array.Max();
                    analyst_performance.top1_return = top1_return;
                    analyst_performance.top2_return = top2_return;
                    analyst_performance.top3_return = top3_return;
                    analyst_performance.bottom1_return = bottom1_return;
                    analyst_performance.bottom2_return = bottom2_return;
                    analyst_performance.bottom3_return = bottom3_return;

                    analyst_performance.horizon_short = horizon_short;
                    analyst_performance.horizon_medium = horizon_medium;
                    analyst_performance.horizon_long = horizon_long;
                    analyst_performance.progress_return = progress_return;
                    analyst_performance.progress_time = progress_time;

                    try
                    {
                        db.AnalystPerformances.InsertOnSubmit(analyst_performance);
                        db.SubmitChanges();
                    }
                    catch { }
                }
            }

            // update ranking
            int count = 1;
            foreach (var x in rank_later.OrderByDescending(b => b.Item2))
            { 
                if (sector!=0)
                {
                    var item = from temp in db.AnalystPerformances where temp.analyst == x.Item1 && temp.sector == sector && temp.date.Date == DateTime.Now.Date && temp.horizon == horizon && temp.active_only == active select temp;
                    item.First().rank = count;
                    db.SubmitChanges();
                    count++;
                }
                else if (ticker != 0)
                {
                    try
                    {
                        var item = from temp in db.AnalystPerformances where temp.analyst == x.Item1 && temp.ticker == ticker && temp.date.Date == DateTime.Now.Date && temp.horizon == horizon && temp.active_only == active select temp;
                        item.First().rank = count;
                        db.SubmitChanges();
                        count++;
                    }
                    catch { }
                }
                else
                {
                    var item = from temp in db.AnalystPerformances where temp.analyst == x.Item1 && !temp.sector.HasValue && !temp.ticker.HasValue && temp.date.Date == DateTime.Now.Date && temp.horizon == horizon && temp.active_only == active select temp;
                    item.First().rank = count;
                    db.SubmitChanges();
                    count++;
                }
            }
        }
    }

    protected void updateAnalyst_TickerPerformanceByAction(int inTicker, int inAnalyst, double inReturn, double in_Accuracy, string eventType , DateTime inDate)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        try
        {
            
            //var _inAction = from temp in db.Actions where temp.actionID == inActionID select temp;
            //Action inAction = (Action)_inAction.First();
            AnalystPerformance newPerformance = new AnalystPerformance();
            //used fields
            newPerformance.return_average = inReturn;
            newPerformance.accuracy_average = in_Accuracy;
            newPerformance.actions = 1;
            newPerformance.actions_matured = 0;
            newPerformance.actions_e_and_positive = 0;
            newPerformance.actions_be_and_negative = 0;
            newPerformance.date = inDate;

            //not used fields
            newPerformance.actions_active_negative = 0;
            newPerformance.actions_active_positive = 0;
            newPerformance.alpha_average = 0;
            newPerformance.alpha_max = 0;
            newPerformance.alpha_min = 0;
            newPerformance.horizon = 0;
            newPerformance.actions_active_positive = 0;
            newPerformance.actions_active_negative = 0;
            newPerformance.return_min = 0;
            newPerformance.return_max = 0;
            newPerformance.return_annualized_max = 0;
            newPerformance.return_annualized_average = 0;
            newPerformance.return_annualized_min = 0;
            newPerformance.return_annualized_sum = 0;
            newPerformance.accuracy_max = 0;
            newPerformance.accuracy_min = 0;
            newPerformance.target_average = 0;
            newPerformance.target_min = 0;
            newPerformance.target_max = 0;
            newPerformance.horizon_average = 0;
            newPerformance.horizon_min = 0;
            newPerformance.horizon_max = 0;
            newPerformance.rank = 0;
            newPerformance.remaining_average = 0;
            newPerformance.remaining_min = 0;
            newPerformance.remaining_max = 0;
            newPerformance.active_only = false;



            switch (eventType)
            {
                case "m":
                    newPerformance.actions_matured = 1;
                    break;
                case "e":
                    if (inReturn > 0)
                        newPerformance.actions_e_and_positive = 1;
                    break;
                case "b":
                    newPerformance.actions_be_and_negative = 1;
                    break;
            }

            //Find already existing performance 
            var performance_exists = from temp in db.AnalystPerformances where temp.ticker == inTicker && temp.analyst == inAnalyst select temp;

            if (performance_exists.Any()) ////Update analyst performance
            {
                AnalystPerformance oldPerformance = performance_exists.First();
                oldPerformance.return_average = ((oldPerformance.return_average * oldPerformance.actions) + newPerformance.return_average) / (oldPerformance.actions + 1);
                oldPerformance.accuracy_average = ((oldPerformance.accuracy_average * oldPerformance.actions) + newPerformance.accuracy_average) / (oldPerformance.actions + 1);
                oldPerformance.actions++;
                oldPerformance.actions_be_and_negative += newPerformance.actions_be_and_negative;
                oldPerformance.actions_e_and_positive += newPerformance.actions_e_and_positive;
                oldPerformance.actions_matured += newPerformance.actions_matured;
                oldPerformance.date = newPerformance.date;
                db.SubmitChanges();
            }

            else  ////Add new analyst performance record
            {
                newPerformance.analyst = inAnalyst;
                newPerformance.ticker = inTicker;
                db.AnalystPerformances.InsertOnSubmit(newPerformance);
            }
        }
        catch (Exception e)
        {

        }
    }
    //private void database_insertion(DataClassesDataContext db, List<Tuple<string, double, double, double, double, double, double, Tuple<double, double, int, int, int, int, int>>> x, string ticker, int sector_or_ticker)
    //{
    //    int cc = 1;

    //    foreach (var b in x.OrderByDescending(b => b.Item4))
    //    {
    //        // overall
    //        if (string.IsNullOrEmpty(ticker) && sector_or_ticker == 0)
    //        {
    //            var a = from temp in db.users where temp.userID == b.Rest.Item5 select temp;
    //            a.First().performance = b.Item2;
    //            a.First().accuracy = b.Item3;
    //            a.First().rank = cc;
    //            a.First().alpha = b.Item6;
    //            a.First().beta = b.Item5;
    //            a.First().performance_min = b.Rest.Item1;
    //            a.First().performance_max = b.Rest.Item2;

    //            try
    //            {
    //                db.SubmitChanges();
    //            }
    //            catch { }
    //        }
    //        // ticker based
    //        else if (!string.IsNullOrEmpty(ticker) && sector_or_ticker == 1)
    //        {
    //            var uf = from temp in db.user_funds where temp.author == b.Rest.Item5 && temp.fund == b.Rest.Item7 select temp;

    //            if (uf.Count() > 0)
    //            {
    //                uf.First().performance = b.Item2;
    //                uf.First().accuracy = b.Item3;
    //                uf.First().rank = cc;
    //                uf.First().alpha = b.Item6;
    //                uf.First().beta = b.Item5;
    //                uf.First().performance_min = b.Rest.Item1;
    //                uf.First().performance_max = b.Rest.Item2;

    //                try
    //                {
    //                    db.SubmitChanges();

    //                }
    //                catch
    //                {
    //                    Response.Write("<font color=red>Error inserting " + uf.First().author + "/" + uf.First().fund + "</font>");
    //                }

    //            }
    //            else
    //            {
    //                user_fund ufo = new user_fund();

    //                ufo.author = b.Rest.Item5;
    //                ufo.fund = b.Rest.Item7;
    //                ufo.performance = b.Item2;
    //                ufo.accuracy = b.Item3;
    //                ufo.rank = cc;
    //                ufo.alpha = b.Item6;
    //                ufo.beta = b.Item5;
    //                ufo.performance_min = b.Rest.Item1;
    //                ufo.performance_max = b.Rest.Item2;

    //                try
    //                {

    //                    db.user_funds.InsertOnSubmit(ufo);
    //                    db.SubmitChanges();

    //                }
    //                catch
    //                {
    //                    Response.Write("<font color=red>Error inserting " + ufo.author + "/" + ufo.fund + "</font>");
    //                }

    //            }


    //        }
    //        // sector based
    //        else if (!string.IsNullOrEmpty(ticker) && sector_or_ticker == 2)
    //        {
    //            var us = from temp in db.user_sectors where temp.user.userID == b.Rest.Item5 && temp.sector == b.Rest.Item6 select temp;

    //            if (us.Count() > 0)
    //            {
    //                us.First().performance = b.Item2;
    //                us.First().accuracy = b.Item3;
    //                us.First().rank = cc;
    //                us.First().alpha = b.Item6;
    //                us.First().beta = b.Item5;
    //                us.First().performance_min = b.Rest.Item1;
    //                us.First().performance_max = b.Rest.Item2;

    //                try
    //                {

    //                    db.SubmitChanges();

    //                }
    //                catch
    //                {
    //                    Response.Write("<font color=red>Error inserting " + us.First().author + "/" + us.First().sector.Value + "</font>");
    //                }
    //            }
    //            else
    //            {
    //                user_sector uso = new user_sector();

    //                uso.author = b.Rest.Item5;
    //                uso.sector = b.Rest.Item6;
    //                uso.performance = b.Item2;
    //                uso.accuracy = b.Item3;
    //                uso.rank = cc;
    //                uso.alpha = b.Item6;
    //                uso.beta = b.Item5;
    //                uso.performance_min = b.Rest.Item1;
    //                uso.performance_max = b.Rest.Item2;

    //                try
    //                {

    //                    db.user_sectors.InsertOnSubmit(uso);
    //                    db.SubmitChanges();


    //                }
    //                catch
    //                {
    //                    Response.Write("<font color=red>Error inserting " + uso.author + "/" + uso.sector + "</font>");
    //                }

    //            }
    //        }
    //        cc++;
    //    }
    //}

    private void display_on_screen(List<Tuple<string, double, double, double, double, double, double, Tuple<double, double, int, int, int, int, int>>> x)
    {
        Response.Write("<table><tr><td><b>#</b></td><td><b>Author</b></td><td><b>Performance</b></td><td><b>Accuracy</b></td><td><b>Cum. Performance</b></td><td><b>Beta</b></td><td><b>Alpha</b></td><td><b>Cum. alpha</b></td><td><b>Min</b></td><td><b>Max</b></td><td><b>+</b></td><td><b>Count</b></td></tr>");
        int c = 1;
        foreach (var a in x.OrderByDescending(b => b.Item4))
        {

            Response.Write("<tr><td>");
            Response.Write(c);
            Response.Write("</td><td>");
            Response.Write(a.Item1);
            Response.Write("</td><td>");
            Response.Write(string.Format("{0:p2}", a.Item2));
            Response.Write("</td><td>");
            Response.Write(string.Format("{0:p2}", a.Item3));
            Response.Write("</td><td>");
            Response.Write(string.Format("{0:p2}", a.Item4));
            Response.Write("</td><td>");
            Response.Write(string.Format("{0:p2}", a.Item5));
            Response.Write("</td><td>");
            Response.Write(string.Format("{0:p2}", a.Item6));
            Response.Write("</td><td>");
            Response.Write(string.Format("{0:p2}", a.Item7));
            Response.Write("</td><td>");
            Response.Write(string.Format("{0:p2}", a.Rest.Item1));
            Response.Write("</td><td>");
            Response.Write(string.Format("{0:p2}", a.Rest.Item2));
            Response.Write("</td><td>");
            Response.Write(string.Format("{0:n0}", a.Rest.Item3));
            Response.Write("</td><td>");
            Response.Write(string.Format("{0:n0}", a.Rest.Item4));
            Response.Write("</td></tr>");
            c++;
        }
        Response.Write("</table>");
    }
}