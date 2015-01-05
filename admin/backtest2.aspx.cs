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
        //clear_backtest_table();
        //backtest_calculations();
        //calculate_ranking_history_all();
    }

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

    //protected void backtest_calculations() {
    //    DataClassesDataContext db = new DataClassesDataContext();

    //    var allx = (from temp in db.AnalystPerformances select temp);
    //    // delete later on
    //    allx = allx.Where(b => b.ticker == 461);
    //    var all = allx.GroupBy(b => b.ticker);
    //    double bloomberg = 0;
    //    double invesd = 0;

    //    if (all.Any()) {
    //        foreach (var t in all)
    //        {
    //            var tgx_history = from temp in db.Bloomberg_Consensus where temp.ticker == t.First().ticker select temp;

    //            foreach (var tgx in tgx_history) {
    //                for (int method = 1; method <= 10; method++) {
    //                    bloomberg = tgx.price;
    //                    invesd = construct_invesd(tgx.ticker, tgx.date,db,method);
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
    //                    ap_actual = ap_actual.Where(b => b.return_sum > 0 && b.rank <= 10).OrderBy(b => b.rank).Take(3);
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
    //                    ap_actual = ap_actual.OrderBy(b => b.rank);
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
    //        else {
    //            tgx = -1000;
    //        }
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
            
    //        if ((is_long && ticker_values.Max(b=>b.highValue)>=target) || (!is_long && ticker_values.Min(b=>b.lowValue<=target)))
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
    //                return (close_value / start_value - 1)*(is_long?1:-1);
    //            }
    //        }

    //    }

    //    return 0;
    //}

    
}