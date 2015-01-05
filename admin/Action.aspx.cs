using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Money = System.Double;
using Rate = System.Double;
using HtmlAgilityPack;

public partial class Update_Actions : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        // action filters
        if (Request.QueryString["action"] == "update")
        {
            update_actions();
        }

        if (Request.QueryString["am"] == "go") {
            update_action_monitors();
        }

        if (Request.QueryString["sector"] == "update")
            get_sector();

        string s = null;
        switch (filter.SelectedIndex)
        {
            case 0:
                s = null;
                break;
            case 1:
                s = "breached==true";
                break;
            case 2:
                s = "expired==true";
                break;
            case 3:
                s = "matured==true";
                break;
            case 4:
                s = "(matured==false and expired==false and breached==false)";
                break;
            case 5:
                s = "(matured==true or expired=true or breached=true)";
                break;
            default:
                s = null;
                break;
        }

        if (!string.IsNullOrEmpty(tb_div.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                Convert.ToDouble(tb_div.Text);
                s += "dividend>=" + tb_div.Text;
            }
            catch { }

        }

        if (!string.IsNullOrEmpty(tb_ticker.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                s += "fund_actions.max(fund1.ticker).Contains(\"" + tb_ticker.Text + "\")";
            }
            catch { }
        }

        if (!string.IsNullOrEmpty(tb_author.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                s += "(article1.user.firstname + article1.user.lastname).Contains(\"" + tb_author.Text.Trim() + "\")";
            }
            catch { }
        }
        if (!string.IsNullOrEmpty(tb_irr_r_lower.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                s += "irr_realized>=" + tb_irr_r_lower.Text;
            }
            catch { }
        }

        if (!string.IsNullOrEmpty(tb_irr_r_upper.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                s += "irr_realized<=" + tb_irr_r_upper.Text;
            }
            catch { }
        }

        if (!string.IsNullOrEmpty(tb_irr_t_lower.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                s += "irr_target>=" + tb_irr_t_lower.Text;
            }
            catch { }
        }

        if (!string.IsNullOrEmpty(tb_irr_t_upper.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                s += "irr_target<=" + tb_irr_t_upper.Text;
            }
            catch { }
        }

        if (!string.IsNullOrEmpty(tb_ret_r_lower.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                s += "return_realized>=" + tb_ret_r_lower.Text;
            }
            catch { }
        }

        if (!string.IsNullOrEmpty(tb_ret_r_upper.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                s += "return_realized<=" + tb_ret_r_upper.Text;
            }
            catch { }
        }

        if (!string.IsNullOrEmpty(tb_ret_t_lower.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                s += "return_target>=" + tb_ret_t_lower.Text;
            }
            catch { }
        }

        if (!string.IsNullOrEmpty(tb_ret_t_upper.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                s += "return_target<=" + tb_ret_t_upper.Text;
            }
            catch { }
        }

        if (!string.IsNullOrEmpty(tb_id.Text))
        {
            if (!string.IsNullOrEmpty(s))
            {
                s += " and ";
            }

            try
            {
                s += "actionid==" + tb_id.Text;
            }
            catch { }
        }

        // Bind actions to gridview 2
        GridView2.PageSize = Convert.ToInt32(filter_pages.SelectedValue);
        LinqDataSource1.Where = s;
        GridView2.DataBind();
    }
    protected void uv(object sender, EventArgs e)
    {
        update_actions();
    }

    //protected void 

    

    protected void update_actions()
    {
        GridView2.Enabled = false;

        // counters
        int ok = 0;
        int suc = 0;
        int fai = 0;

        DataClassesDataContext db = new DataClassesDataContext();
        {
            // variable initialization
            string changes = "";
            DateTime CBetweenDate = DateTime.Now;
            DateTime RightNow = DateTime.Now;
            DateTime CEndDate = DateTime.Now;
            DateTime lastUpdated = DateTime.Now;
            bool TotalReturn = false;
            double Cmax = 0;
            double Cmin = 0;
            double CtargetValue = 0;
            double ClowerValue = 0;
            double CstartValue = 0;
            double DividendBucket = 0;
            double CcurrentValue = 0;
            bool matured = false;
            bool breached = false;
            bool expired = false;
            double previousDividends = 0;
            bool is_short = false;
            double keep_current = 0;
            double days_gain = 0;
            int benchmark_fund_id = 922;

            // new version ==> b==0 & e==0 & a=0 (not breached, not expired, and active)
            var actions = from temp in db.Actions where temp.breached == false && temp.expired == false select temp;
            // previous version
            //var actions = from temp in db.Actions where temp.matured == false && temp.breached == false && temp.expired == false select temp;
            //actions = actions.Where(b => b.@short == true);

            // initialize grid view
            DataTable table = new DataTable();
            DataColumn col1 = new DataColumn("Action");
            DataColumn col2 = new DataColumn("Initial");
            DataColumn col3 = new DataColumn("Target");
            DataColumn col4 = new DataColumn("Current");
            DataColumn col5 = new DataColumn("Lower");
            DataColumn col6 = new DataColumn("Start");
            DataColumn col7 = new DataColumn("End");
            DataColumn col8 = new DataColumn("Updated");
            DataColumn col9 = new DataColumn("Dividend");
            DataColumn col10 = new DataColumn("Breached");
            DataColumn col11 = new DataColumn("Expired");
            DataColumn col12 = new DataColumn("Matured");
            DataColumn col13 = new DataColumn("Status");
            DataColumn col14 = new DataColumn("Long/Short");

            table.Columns.Add(col1);
            table.Columns.Add(col2);
            table.Columns.Add(col3);
            table.Columns.Add(col4);
            table.Columns.Add(col5);
            table.Columns.Add(col6);
            table.Columns.Add(col7);
            table.Columns.Add(col8);
            table.Columns.Add(col9);
            table.Columns.Add(col10);
            table.Columns.Add(col11);
            table.Columns.Add(col12);
            table.Columns.Add(col13);
            table.Columns.Add(col14);

            if (actions.Any())
            {
                foreach (var action in actions)
                {
                    // update variables
                    Cmax = action.maxValue;
                    Cmin = action.minValue;
                    CtargetValue = action.targetValue;
                    ClowerValue = action.lowerValue;
                    CstartValue = action.startValue;
                    previousDividends = action.dividend;
                    CEndDate = action.targetDate;
                    TotalReturn = action.TotalReturn;
                    breached = action.breached;
                    matured = action.matured;
                    expired = action.expired;
                    DividendBucket = 0;
                    is_short = action.@short;
                    keep_current = action.currentValue;

                    if (matured == true)
                    {
                        if (CEndDate <= RightNow)
                            expired = true;
                    }

                    var prices = from temp in db.fund_values where temp.fundID == action.ticker && temp.date > action.lastUpdated && temp.date <= RightNow orderby temp.date ascending select temp;

                    if (prices.Any() && matured==false)
                    {
                        foreach (var price in prices)
                        {
                            // walk through
                            CBetweenDate = price.date;
                            CcurrentValue = price.closeValue.Value;

                            days_gain = CcurrentValue / keep_current - 1;

                            // stock split
                            if (price.split.Value != 1)
                            {
                                Cmax = Cmax * (1 / (price.split.Value));
                                Cmin = Cmin * (1 / (price.split.Value));
                                CtargetValue = CtargetValue * (1 / (price.split.Value));
                                ClowerValue = ClowerValue * (1 / (price.split.Value));
                                CstartValue = CstartValue * (1 / (price.split.Value));
                                insert_notification(action.actionID, "Split", action.active, CBetweenDate, false, action.article1.user.userID); //news feed
                            }

                            // dividend
                            if (price.dividend.Value != 0)
                            {
                                DividendBucket = DividendBucket + price.dividend.Value;
                                insert_notification(action.actionID, "Dividend", action.active, CBetweenDate, false, action.article1.user.userID); //news feed
                            }

                            // Long
                            if (is_short == false)
                            {
                                // Dividend helps reaching the target for long poisitions
                                if (TotalReturn == true)
                                {
                                    Cmax = Math.Max(Cmax, Math.Min(price.highValue.Value + DividendBucket, CtargetValue));
                                    Cmin = Math.Min(Cmin, Math.Max(price.lowValue.Value + DividendBucket, ClowerValue));
                                }
                                else
                                {
                                    Cmax = Math.Max(Cmax, Math.Min(price.highValue.Value, CtargetValue));
                                    Cmin = Math.Min(Cmin, Math.Max(price.lowValue.Value, ClowerValue));
                                }
                            }
                            else
                            {
                                // Dividend is a negative cashflow in the case of short positions and doesn't help in
                                // a total return context
                                if (TotalReturn == true)
                                {
                                    Cmax = Math.Min(Cmax, Math.Max(price.lowValue.Value + DividendBucket, CtargetValue));
                                    Cmin = Math.Max(Cmin, Math.Min(price.highValue.Value + DividendBucket, ClowerValue));
                                }
                                else
                                {
                                    Cmax = Math.Min(Cmax, Math.Max(price.lowValue.Value, CtargetValue));
                                    Cmin = Math.Max(Cmin, Math.Min(price.highValue.Value, ClowerValue));
                                }
                            }

                            CcurrentValue = price.closeValue.Value;
                            lastUpdated = CBetweenDate;

                            // matured?
                            // Long
                            if (is_short == false)
                            {
                                if (TotalReturn == true)
                                {
                                    if (price.highValue.Value + DividendBucket + previousDividends >= CtargetValue)
                                    {
                                        Cmax = CtargetValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.highValue.Value >= CtargetValue)
                                    {
                                        Cmax = CtargetValue;
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                            }
                            // Short
                            else
                            {
                                if (TotalReturn == true)
                                {
                                    if (price.lowValue.Value + DividendBucket + previousDividends <= CtargetValue)
                                    {
                                        Cmax = CtargetValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.lowValue.Value <= CtargetValue)
                                    {
                                        Cmax = CtargetValue;
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                            }

                            // breached?
                            // Long
                            if (is_short == false)
                            {
                                if (TotalReturn == true)
                                {
                                    if (price.lowValue.Value + DividendBucket + previousDividends <= ClowerValue)
                                    {
                                        Cmin = ClowerValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.lowValue.Value <= ClowerValue)
                                    {
                                        Cmin = ClowerValue;
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                            }
                            // Short
                            else
                            {
                                if (TotalReturn == true)
                                {
                                    if (price.highValue.Value + DividendBucket + previousDividends >= ClowerValue)
                                    {
                                        Cmin = ClowerValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed
                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.highValue.Value >= ClowerValue)
                                    {
                                        Cmin = ClowerValue;
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed
                                        break;
                                    }
                                }
                            }

                            // expired?
                            if (CBetweenDate >= CEndDate)
                            {
                                expired = true;
                                insert_notification(action.actionID, "Expired", action.active, lastUpdated, true, action.article1.user.userID); // news feed
                                break;
                            }
                        }

                        // if there exists any other action whereby (1) B & E == true (2) article author is the same author (3) ticker is the same ticker (4) is active
                        // when B/E is true then
                        if (breached == true || expired == true)
                        {
                            var other_actions = from temp in db.Actions where temp.active == true && (temp.breached == false && temp.expired == false) && temp.article1.origin == action.article1.origin && temp.ticker == action.ticker select temp;

                            if (other_actions.Any())
                            {
                                action.active = false;
                            }
                        }

                        // update database
                        // add to gridview
                        action.targetValue = CtargetValue;
                        action.currentValue = CcurrentValue;
                        action.lastUpdated = lastUpdated;
                        action.startValue = CstartValue;
                        action.matured = matured;
                        action.expired = expired;
                        action.breached = breached;
                        if (matured || expired || breached)
                        {
                            //action.date_feed = lastUpdated;
                            changes += action.actionID + ";";
                        }
                        action.lowerValue = ClowerValue;
                        action.maxValue = Cmax;
                        action.minValue = Cmin;
                        action.dividend = DividendBucket + previousDividends;
                        // day's gain
                        action.days_gain = days_gain;

                        // alpha
                        var benchmark = from temp in db.fund_values where temp.fundID == benchmark_fund_id && temp.date >= action.startDate && temp.date <= action.lastUpdated select temp;

                        if (benchmark.Any())
                        {
                            double dividend_sum = benchmark.Sum(b => b.dividend.Value);
                            double start = benchmark.OrderBy(b => b.date).First().closeValue.Value;
                            double end = benchmark.OrderByDescending(b => b.date).First().closeValue.Value;

                            action.beta = (end + dividend_sum) / start - 1;
                        }

                        action.progress = calculate_overall_return(action.TotalReturn, action.dividend, action.@short, action.startValue, action.currentValue, action.lowerValue, action.targetValue)[0];
                        action.return_overall = calculate_overall_return(action.TotalReturn, action.dividend, action.@short, action.startValue, action.currentValue, action.lowerValue, action.targetValue)[1];

                        // add to gridview & update database
                        DataRow row = table.NewRow();

                        row[col1] = action.actionID;
                        row[col2] = CstartValue;
                        row[col3] = CtargetValue;
                        row[col4] = CcurrentValue;
                        row[col5] = ClowerValue;
                        row[col6] = action.startDate;
                        row[col7] = CEndDate;
                        row[col8] = lastUpdated;
                        row[col9] = action.dividend;
                        row[col10] = breached;
                        row[col11] = expired;
                        row[col12] = matured;
                        row[col14] = is_short;

                        try
                        {
                            db.SubmitChanges();
                            //IRR_calculator_single(action, db);
                            row[col13] = "Success";
                            suc++;
                        }
                        catch
                        {
                            row[col13] = "Failed";
                            fai++;
                        }

                        table.Rows.Add(row);
                    }
                    else if (prices.Any() && matured == true) {
                        foreach (var price in prices) {
                            // stock split
                            if (price.split.Value != 1)
                            {
                                action.maxValue = Cmax * (1 / (price.split.Value));
                                action.minValue = Cmin * (1 / (price.split.Value));
                                action.targetValue = CtargetValue * (1 / (price.split.Value));
                                action.lowerValue = ClowerValue * (1 / (price.split.Value));
                                action.startValue = CstartValue * (1 / (price.split.Value));
                                insert_notification(action.actionID, "Split", action.active, CBetweenDate, false, action.article1.user.userID); //news feed
                                db.SubmitChanges();
                            }
                        }
                    }
                    else
                    {
                        ok++;
                    }

                }

                // Fill gridview
                GridView1.DataSource = null;
                GridView1.DataSource = table;
                GridView1.DataBind();

                Gripe g = new Gripe();
                g.message = changes;
                g.date = DateTime.Now;
                g.userid = 2;
                db.Gripes.InsertOnSubmit(g);
                db.SubmitChanges();
            }
            else
            {
                l_update.Text = "All actions are up to date";
            }
        }

        l_update.Text = "Successfully updated: " + Convert.ToString(suc) + ", already up-to-date: " + Convert.ToString(ok) + ", failed: " + Convert.ToString(fai) + " actions";
        l_reset.Text = "";
    }

    protected void update_action_monitors() {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            // variable initialization
            //string changes = "";
            DateTime CBetweenDate = DateTime.Now;
            DateTime RightNow = DateTime.Now;
            DateTime CEndDate = DateTime.Now;
            DateTime lastUpdated = DateTime.Now;
            bool TotalReturn = false;
            double Cmax = 0;
            double Cmin = 0;
            double CtargetValue = 0;
            double ClowerValue = 0;
            double CstartValue = 0;
            double DividendBucket = 0;
            double CcurrentValue = 0;
            bool matured = false;
            bool breached = false;
            bool expired = false;
            double previousDividends = 0;
            bool is_short = false;
            double keep_current = 0;
            double days_gain = 0;
            int benchmark_fund_id = 922;

            // new version ==> b==0 & e==0 & a=0 (not breached, not expired, and active)
            var actions = from temp in db.ActionMonitors where !temp.breached && !temp.expired && !temp.matured select temp;
            // previous version
            //var actions = from temp in db.Actions where temp.matured == false && temp.breached == false && temp.expired == false select temp;
            //actions = actions.Where(b => b.@short == true);

            // initialize grid view
            DataTable table = new DataTable();
            DataColumn col1 = new DataColumn("Action");
            DataColumn col2 = new DataColumn("Initial");
            DataColumn col3 = new DataColumn("Target");
            DataColumn col4 = new DataColumn("Current");
            DataColumn col5 = new DataColumn("Lower");
            DataColumn col6 = new DataColumn("Start");
            DataColumn col7 = new DataColumn("End");
            DataColumn col8 = new DataColumn("Updated");
            DataColumn col9 = new DataColumn("Dividend");
            DataColumn col10 = new DataColumn("Breached");
            DataColumn col11 = new DataColumn("Expired");
            DataColumn col12 = new DataColumn("Matured");
            DataColumn col13 = new DataColumn("Status");
            DataColumn col14 = new DataColumn("Long/Short");

            table.Columns.Add(col1);
            table.Columns.Add(col2);
            table.Columns.Add(col3);
            table.Columns.Add(col4);
            table.Columns.Add(col5);
            table.Columns.Add(col6);
            table.Columns.Add(col7);
            table.Columns.Add(col8);
            table.Columns.Add(col9);
            table.Columns.Add(col10);
            table.Columns.Add(col11);
            table.Columns.Add(col12);
            table.Columns.Add(col13);
            table.Columns.Add(col14);

            if (actions.Any())
            {
                foreach (var action in actions)
                {
                    // update variables
                    Cmax = action.maxValue;
                    Cmin = action.minValue;
                    CtargetValue = action.targetValue;
                    ClowerValue = action.lowerValue;
                    CstartValue = action.monitorInitialValue;
                    previousDividends = action.cashDividend;
                    CEndDate = action.monitorEnd;
                    TotalReturn = action.TotalReturn;
                    breached = action.breached;
                    matured = action.matured;
                    expired = action.expired;
                    DividendBucket = 0;
                    is_short = action.monitorInitialValue>action.targetValue;
                    keep_current = action.currentValue;

                    if (matured == true)
                    {
                        if (CEndDate <= RightNow)
                        {

                            expired = true;
                        }
                    }

                    var prices = from temp in db.fund_values where temp.fundID == action.ticker && temp.date > action.lastUpdated && temp.date <= RightNow orderby temp.date ascending select temp;

                    if (prices.Any() && matured == false)
                    {
                        foreach (var price in prices)
                        {
                            // walk through
                            CBetweenDate = price.date;
                            CcurrentValue = price.closeValue.Value;

                            days_gain = CcurrentValue / keep_current - 1;

                            // stock split
                            //if (price.split.Value != 1)
                            //{
                            //    Cmax = Cmax * (1 / (price.split.Value));
                            //    Cmin = Cmin * (1 / (price.split.Value));
                            //    CtargetValue = CtargetValue * (1 / (price.split.Value));
                            //    ClowerValue = ClowerValue * (1 / (price.split.Value));
                            //    CstartValue = CstartValue * (1 / (price.split.Value));
                            //    //insert_notification(action.actionID, "Split", action.active, CBetweenDate, false, action.article1.user.userID); //news feed
                            //}

                            // dividend
                            if (price.dividend.Value != 0)
                            {
                                DividendBucket = DividendBucket + price.dividend.Value;
                                var user = from temp in db.users where temp.userID == action.usermon select temp;

                                if (user.Any()) {
                                    user.First().cash += (is_short?-1:1) * price.dividend.Value * action.units;
                                }
                                
                                //insert_notification(action.actionID, "Dividend", action.active, CBetweenDate, false, action.article1.user.userID); //news feed
                            }

                            // Long
                            if (!is_short)
                            {
                                // Dividend helps reaching the target for long poisitions
                                if (TotalReturn)
                                {
                                    Cmax = Math.Max(Cmax, Math.Min(price.highValue.Value + DividendBucket, CtargetValue));
                                    Cmin = Math.Min(Cmin, Math.Max(price.lowValue.Value + DividendBucket, ClowerValue));
                                }
                                else
                                {
                                    Cmax = Math.Max(Cmax, Math.Min(price.highValue.Value, CtargetValue));
                                    Cmin = Math.Min(Cmin, Math.Max(price.lowValue.Value, ClowerValue));
                                }
                            }
                            else
                            {
                                // Dividend is a negative cashflow in the case of short positions and doesn't help in
                                // a total return context
                                if (TotalReturn)
                                {
                                    Cmax = Math.Min(Cmax, Math.Max(price.lowValue.Value + DividendBucket, CtargetValue));
                                    Cmin = Math.Max(Cmin, Math.Min(price.highValue.Value + DividendBucket, ClowerValue));
                                }
                                else
                                {
                                    Cmax = Math.Min(Cmax, Math.Max(price.lowValue.Value, CtargetValue));
                                    Cmin = Math.Max(Cmin, Math.Min(price.highValue.Value, ClowerValue));
                                }
                            }

                            CcurrentValue = price.closeValue.Value;
                            lastUpdated = CBetweenDate;

                            // matured?
                            // Long
                            if (!is_short)
                            {
                                if (TotalReturn)
                                {
                                    if (price.highValue.Value + DividendBucket + previousDividends >= CtargetValue)
                                    {
                                        Cmax = CtargetValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        //insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.highValue.Value >= CtargetValue)
                                    {
                                        Cmax = CtargetValue;
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        //insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                            }
                            // Short
                            else
                            {
                                if (TotalReturn)
                                {
                                    if (price.lowValue.Value + DividendBucket + previousDividends <= CtargetValue)
                                    {
                                        Cmax = CtargetValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        //insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.lowValue.Value <= CtargetValue)
                                    {
                                        Cmax = CtargetValue;
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        //insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                            }

                            // breached?
                            // Long
                            if (!is_short)
                            {
                                if (TotalReturn)
                                {
                                    if (price.lowValue.Value + DividendBucket + previousDividends <= ClowerValue)
                                    {
                                        Cmin = ClowerValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        //insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.lowValue.Value <= ClowerValue)
                                    {
                                        Cmin = ClowerValue;
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        //insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                            }
                            // Short
                            else
                            {
                                if (TotalReturn)
                                {
                                    if (price.highValue.Value + DividendBucket + previousDividends >= ClowerValue)
                                    {
                                        Cmin = ClowerValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        //insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed
                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.highValue.Value >= ClowerValue)
                                    {
                                        Cmin = ClowerValue;
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        //insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed
                                        break;
                                    }
                                }
                            }

                            // expired?
                            if (CBetweenDate >= CEndDate)
                            {
                                expired = true;
                                //insert_notification(action.actionID, "Expired", action.active, lastUpdated, true, action.article1.user.userID); // news feed
                                break;
                            }
                        }

                        // if there exists any other action whereby (1) B & E == true (2) article author is the same author (3) ticker is the same ticker (4) is active
                        // when B/E is true then
                        //if (breached == true || expired == true)
                        //{
                            //var other_actions = from temp in db.Actions where temp.active == true && (temp.breached == false && temp.expired == false) && temp.article1.origin == action.article1.origin && temp.ticker == action.ticker select temp;

                            //if (other_actions.Any())
                            //{
                            //    //action.active = false;
                            //}
                        //}

                        // update database
                        // add to gridview
                        action.targetValue = CtargetValue;
                        action.currentValue = CcurrentValue;
                        action.lastUpdated = lastUpdated;
                        action.monitorInitialValue = CstartValue;
                        action.matured = matured;
                        action.expired = expired;
                        action.breached = breached;
                        //if (matured || expired || breached)
                        //{
                        //    //action.date_feed = lastUpdated;
                        //    //changes += action.actionID + ";";
                        //}
                        action.lowerValue = ClowerValue;
                        action.maxValue = Cmax;
                        action.minValue = Cmin;
                        action.cashDividend = DividendBucket + previousDividends;
                        // day's gain
                        //action.days_gain = days_gain;

                        // alpha
                        var benchmark = from temp in db.fund_values where temp.fundID == benchmark_fund_id && temp.date >= action.monitorStart && temp.date <= action.lastUpdated select temp;

                        if (benchmark.Any())
                        {
                            double dividend_sum = benchmark.Sum(b => b.dividend.Value);
                            double start = benchmark.OrderBy(b => b.date).First().closeValue.Value;
                            double end = benchmark.OrderByDescending(b => b.date).First().closeValue.Value;

                            //action.beta = (end + dividend_sum) / start - 1;
                        }

                        action.Progress = calculate_overall_return(action.TotalReturn, action.cashDividend, action.targetValue<action.monitorInitialValue, action.monitorInitialValue, action.currentValue, action.lowerValue, action.targetValue)[0];
                        //action.return_overall = calculate_overall_return(action.TotalReturn, action.dividend, action.@short, action.startValue, action.currentValue, action.lowerValue, action.targetValue)[1];

                        // add to gridview & update database
                        DataRow row = table.NewRow();

                        //row[col1] = action.actionID;
                        row[col2] = CstartValue;
                        row[col3] = CtargetValue;
                        row[col4] = CcurrentValue;
                        row[col5] = ClowerValue;
                        row[col6] = action.monitorStart;
                        row[col7] = CEndDate;
                        row[col8] = lastUpdated;
                        row[col9] = action.cashDividend;
                        row[col10] = breached;
                        row[col11] = expired;
                        row[col12] = matured;
                        row[col14] = is_short;

                        try
                        {
                            db.SubmitChanges();
                            //IRR_calculator_single(action, db);
                            row[col13] = "Success";
                            //suc++;
                        }
                        catch
                        {
                            row[col13] = "Failed";
                            //fai++;
                        }

                        table.Rows.Add(row);
                    }
                    //else if (prices.Any() && matured)
                    //{
                    //    //foreach (var price in prices)
                    //    //{
                    //    //    // stock split
                    //    //    //if (price.split.Value != 1)
                    //    //    //{
                    //    //    //    action.maxValue = Cmax * (1 / (price.split.Value));
                    //    //    //    action.minValue = Cmin * (1 / (price.split.Value));
                    //    //    //    action.targetValue = CtargetValue * (1 / (price.split.Value));
                    //    //    //    action.lowerValue = ClowerValue * (1 / (price.split.Value));
                    //    //    //    action.monitorInitialValue = CstartValue * (1 / (price.split.Value));
                    //    //    //    //insert_notification(action.actionID, "Split", action.active, CBetweenDate, false, action.article1.user.userID); //news feed
                    //    //    //    db.SubmitChanges();
                    //    //    //}
                    //    //}
                    //}
                    //else
                    //{
                    //    //ok++;
                    //}

                }

                // Fill gridview
                GridView1.DataSource = null;
                GridView1.DataSource = table;
                GridView1.DataBind();

                //Gripe g = new Gripe();
                //g.message = changes;
                //g.date = DateTime.Now;
                //g.userid = 2;
                //db.Gripes.InsertOnSubmit(g);
                //db.SubmitChanges();
            }
            else
            {
                l_update.Text = "All actions are up to date";
            }
        }
    }

    protected void update_actions_new()
    {
        GridView2.Enabled = false;

        // counters
        int ok = 0;
        int suc = 0;
        int fai = 0;

        DataClassesDataContext db = new DataClassesDataContext();
        {
            // variable initialization
            string changes = "";
            DateTime CBetweenDate = DateTime.Now;
            DateTime RightNow = DateTime.Now;
            DateTime CEndDate = DateTime.Now;
            DateTime lastUpdated = DateTime.Now;
            bool TotalReturn = false;
            double Cmax = 0;
            double Cmin = 0;
            double CtargetValue = 0;
            double ClowerValue = 0;
            double CstartValue = 0;
            double DividendBucket = 0;
            double CcurrentValue = 0;
            bool matured = false;
            bool breached = false;
            bool expired = false;
            double previousDividends = 0;
            bool is_short = false;
            double keep_current = 0;
            double days_gain = 0;
            int benchmark_fund_id = 922;

            // new version ==> b==0 & e==0 & a=0 (not breached, not expired, and active)
            var actions = from temp in db.Actions where temp.breached == false && temp.expired == false select temp;
            // previous version
            //var actions = from temp in db.Actions where temp.matured == false && temp.breached == false && temp.expired == false select temp;
            //actions = actions.Where(b => b.@short == true);

            // initialize grid view
            DataTable table = new DataTable();
            DataColumn col1 = new DataColumn("Action");
            DataColumn col2 = new DataColumn("Initial");
            DataColumn col3 = new DataColumn("Target");
            DataColumn col4 = new DataColumn("Current");
            DataColumn col5 = new DataColumn("Lower");
            DataColumn col6 = new DataColumn("Start");
            DataColumn col7 = new DataColumn("End");
            DataColumn col8 = new DataColumn("Updated");
            DataColumn col9 = new DataColumn("Dividend");
            DataColumn col10 = new DataColumn("Breached");
            DataColumn col11 = new DataColumn("Expired");
            DataColumn col12 = new DataColumn("Matured");
            DataColumn col13 = new DataColumn("Status");
            DataColumn col14 = new DataColumn("Long/Short");

            table.Columns.Add(col1);
            table.Columns.Add(col2);
            table.Columns.Add(col3);
            table.Columns.Add(col4);
            table.Columns.Add(col5);
            table.Columns.Add(col6);
            table.Columns.Add(col7);
            table.Columns.Add(col8);
            table.Columns.Add(col9);
            table.Columns.Add(col10);
            table.Columns.Add(col11);
            table.Columns.Add(col12);
            table.Columns.Add(col13);
            table.Columns.Add(col14);

            if (actions.Any())
            {
                foreach (var action in actions)
                {
                    // update variables
                    Cmax = action.maxValue;
                    Cmin = action.minValue;
                    CtargetValue = action.targetValue;
                    ClowerValue = action.lowerValue;
                    CstartValue = action.startValue;
                    previousDividends = action.dividend;
                    CEndDate = action.targetDate;
                    TotalReturn = action.TotalReturn;
                    breached = action.breached;
                    matured = action.matured;
                    expired = action.expired;
                    DividendBucket = 0;
                    is_short = action.@short;
                    keep_current = action.currentValue;

                    if (matured == true)
                    {
                        if (CEndDate <= RightNow)
                            expired = true;
                    }

                    var prices = from temp in db.fund_values where temp.fundID == action.ticker && temp.date > action.lastUpdated && temp.date <= RightNow orderby temp.date ascending select temp;

                    if (prices.Any() && matured == false)
                    {
                        foreach (var price in prices)
                        {
                            // walk through
                            CBetweenDate = price.date;
                            CcurrentValue = price.closeValue.Value;

                            days_gain = CcurrentValue / keep_current - 1;

                            // stock split
                            if (price.split.Value != 1)
                            {
                                Cmax = Cmax * (1 / (price.split.Value));
                                Cmin = Cmin * (1 / (price.split.Value));
                                CtargetValue = CtargetValue * (1 / (price.split.Value));
                                ClowerValue = ClowerValue * (1 / (price.split.Value));
                                CstartValue = CstartValue * (1 / (price.split.Value));
                                insert_notification(action.actionID, "Split", action.active, CBetweenDate, false, action.article1.user.userID); //news feed
                            }

                            // dividend
                            if (price.dividend.Value != 0)
                            {
                                DividendBucket = DividendBucket + price.dividend.Value;
                                insert_notification(action.actionID, "Dividend", action.active, CBetweenDate, false, action.article1.user.userID); //news feed
                            }

                            // Long
                            if (is_short == false)
                            {
                                // Dividend helps reaching the target for long poisitions
                                if (TotalReturn == true)
                                {
                                    Cmax = Math.Max(Cmax, Math.Min(price.highValue.Value + DividendBucket, CtargetValue));
                                    Cmin = Math.Min(Cmin, Math.Max(price.lowValue.Value + DividendBucket, ClowerValue));
                                }
                                else
                                {
                                    Cmax = Math.Max(Cmax, Math.Min(price.highValue.Value, CtargetValue));
                                    Cmin = Math.Min(Cmin, Math.Max(price.lowValue.Value, ClowerValue));
                                }
                            }
                            else
                            {
                                // Dividend is a negative cashflow in the case of short positions and doesn't help in
                                // a total return context
                                if (TotalReturn == true)
                                {
                                    Cmax = Math.Min(Cmax, Math.Max(price.lowValue.Value + DividendBucket, CtargetValue));
                                    Cmin = Math.Max(Cmin, Math.Min(price.highValue.Value + DividendBucket, ClowerValue));
                                }
                                else
                                {
                                    Cmax = Math.Min(Cmax, Math.Max(price.lowValue.Value, CtargetValue));
                                    Cmin = Math.Max(Cmin, Math.Min(price.highValue.Value, ClowerValue));
                                }
                            }

                            CcurrentValue = price.closeValue.Value;
                            lastUpdated = CBetweenDate;

                            // matured?
                            // Long
                            if (is_short == false)
                            {
                                if (TotalReturn == true)
                                {
                                    if (price.highValue.Value + DividendBucket + previousDividends >= CtargetValue)
                                    {
                                        Cmax = CtargetValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.highValue.Value >= CtargetValue)
                                    {
                                        Cmax = CtargetValue;
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                            }
                            // Short
                            else
                            {
                                if (TotalReturn == true)
                                {
                                    if (price.lowValue.Value + DividendBucket + previousDividends <= CtargetValue)
                                    {
                                        Cmax = CtargetValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.lowValue.Value <= CtargetValue)
                                    {
                                        Cmax = CtargetValue;
                                        CcurrentValue = Cmax;
                                        matured = true;
                                        insert_notification(action.actionID, "Matured", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                            }

                            // breached?
                            // Long
                            if (is_short == false)
                            {
                                if (TotalReturn == true)
                                {
                                    if (price.lowValue.Value + DividendBucket + previousDividends <= ClowerValue)
                                    {
                                        Cmin = ClowerValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.lowValue.Value <= ClowerValue)
                                    {
                                        Cmin = ClowerValue;
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed

                                        if (CBetweenDate >= CEndDate)
                                            expired = true;

                                        break;
                                    }
                                }
                            }
                            // Short
                            else
                            {
                                if (TotalReturn == true)
                                {
                                    if (price.highValue.Value + DividendBucket + previousDividends >= ClowerValue)
                                    {
                                        Cmin = ClowerValue - (DividendBucket + previousDividends);
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed
                                        break;
                                    }
                                }
                                else
                                {
                                    if (price.highValue.Value >= ClowerValue)
                                    {
                                        Cmin = ClowerValue;
                                        CcurrentValue = Cmin;
                                        breached = true;
                                        insert_notification(action.actionID, "Breached", action.active, lastUpdated, true, action.article1.user.userID); // news feed
                                        break;
                                    }
                                }
                            }

                            // expired?
                            if (CBetweenDate >= CEndDate)
                            {
                                expired = true;
                                insert_notification(action.actionID, "Expired", action.active, lastUpdated, true, action.article1.user.userID); // news feed
                                break;
                            }
                        }

                        // if there exists any other action whereby (1) B & E == true (2) article author is the same author (3) ticker is the same ticker (4) is active
                        // when B/E is true then
                        if (breached == true || expired == true)
                        {
                            var other_actions = from temp in db.Actions where temp.active == true && (temp.breached == false && temp.expired == false) && temp.article1.origin == action.article1.origin && temp.ticker == action.ticker select temp;

                            if (other_actions.Any())
                            {
                                action.active = false;
                            }
                        }

                        // update database
                        // add to gridview
                        action.targetValue = CtargetValue;
                        action.currentValue = CcurrentValue;
                        action.lastUpdated = lastUpdated;
                        action.startValue = CstartValue;
                        action.matured = matured;
                        action.expired = expired;
                        action.breached = breached;
                        if (matured || expired || breached)
                        {
                            action.date_feed = lastUpdated;
                            changes += action.actionID + ";";
                        }
                        action.lowerValue = ClowerValue;
                        action.maxValue = Cmax;
                        action.minValue = Cmin;
                        action.dividend = DividendBucket + previousDividends;
                        // day's gain
                        action.days_gain = days_gain;

                        // alpha
                        var benchmark = from temp in db.fund_values where temp.fundID == benchmark_fund_id && temp.date >= action.startDate && temp.date <= action.lastUpdated select temp;

                        if (benchmark.Any())
                        {
                            double dividend_sum = benchmark.Sum(b => b.dividend.Value);
                            double start = benchmark.OrderBy(b => b.date).First().closeValue.Value;
                            double end = benchmark.OrderByDescending(b => b.date).First().closeValue.Value;

                            action.beta = (end + dividend_sum) / start - 1;
                        }

                        action.progress = calculate_overall_return(action.TotalReturn, action.dividend, action.@short, action.startValue, action.currentValue, action.lowerValue, action.targetValue)[0];
                        action.return_overall = calculate_overall_return(action.TotalReturn, action.dividend, action.@short, action.startValue, action.currentValue, action.lowerValue, action.targetValue)[1];

                        // add to gridview & update database
                        DataRow row = table.NewRow();

                        row[col1] = action.actionID;
                        row[col2] = CstartValue;
                        row[col3] = CtargetValue;
                        row[col4] = CcurrentValue;
                        row[col5] = ClowerValue;
                        row[col6] = action.startDate;
                        row[col7] = CEndDate;
                        row[col8] = lastUpdated;
                        row[col9] = action.dividend;
                        row[col10] = breached;
                        row[col11] = expired;
                        row[col12] = matured;
                        row[col14] = is_short;

                        try
                        {
                            db.SubmitChanges();
                            //IRR_calculator_single(action, db);
                            row[col13] = "Success";
                            suc++;
                        }
                        catch
                        {
                            row[col13] = "Failed";
                            fai++;
                        }

                        table.Rows.Add(row);
                    }
                    else
                    {
                        ok++;
                    }

                }

                // Fill gridview
                GridView1.DataSource = null;
                GridView1.DataSource = table;
                GridView1.DataBind();

                Gripe g = new Gripe();
                g.message = changes;
                g.date = DateTime.Now;
                g.userid = 2;
                db.Gripes.InsertOnSubmit(g);
                db.SubmitChanges();
            }
            else
            {
                l_update.Text = "All actions are up to date";
            }
        }

        l_update.Text = "Successfully updated: " + Convert.ToString(suc) + ", already up-to-date: " + Convert.ToString(ok) + ", failed: " + Convert.ToString(fai) + " actions";
        l_reset.Text = "";
    }


    protected void ra(object sender, EventArgs e)
    {
        // counters
        int suc = 0;
        int fai = 0;
        DataClassesDataContext db = new DataClassesDataContext();
        {
            // delete all dividends
            //db.dividends.DeleteAllOnSubmit(db.dividends);
            //db.SubmitChanges();

            var x = from temp in db.Actions select temp;
            foreach (var a in x)
            {
                // update variables
                a.currentValue = a.startValue;
                a.maxValue = a.startValue;
                a.minValue = a.startValue;
                a.lastUpdated = a.startDate;
                a.matured = false;
                a.breached = false;
                a.expired = false;
                a.dividend = 0;

                // update database
                try
                {
                    db.SubmitChanges();
                    suc++;
                }
                catch
                {
                    fai++;
                }
            }
            l_reset.Text = "Successfully resetted " + Convert.ToString(suc) + " actions and failed " + Convert.ToString(fai);
            l_update.Text = "";
        }

        // reset gridview
        GridView1.DataSource = null;
        GridView1.DataBind();
    }

    protected void linq_selected(object sender, LinqDataSourceStatusEventArgs e)
    {
        rowshere.Text = string.Format("{0:n0}", e.TotalRowCount);
    }
    protected void IRR_calculator()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            var actions = from temp in db.Actions select temp;

            foreach (var action in actions)
            {
                IRR_calculator_single(action,db);
            }


        }
    }
    protected void IRR_calculator_single(Action action,DataClassesDataContext db)
    {
        IEnumerable<CashFlow> cf_r = new CashFlow[]
                {
                    new CashFlow(Convert.ToDouble(-action.startValue),action.startDate),
                };

        if (action.dividend > 0)
        {
            //var dx = from temp in db.dividends where temp.action == action.actionID orderby temp.date ascending select temp;
            //foreach (var d in dx)
            //{
            //    // add the dividend CF
            //    cf_r = cf_r.Concat(new[] { new CashFlow(Convert.ToDouble(d.dividend1), d.date) });
            //}
        }
        cf_r = cf_r.Concat(new[] { new CashFlow(Convert.ToDouble(action.currentValue), action.lastUpdated) });

        IEnumerable<CashFlow> cf_t = new CashFlow[]
                {
                    new CashFlow(Convert.ToDouble(-action.startValue),action.startDate),
                    new CashFlow(Convert.ToDouble(action.targetValue),action.targetDate)
                };


        var irr_r = Algorithms.CalculateXIRR(cf_r, 0.0001, 100);
        var irr_t = Algorithms.CalculateXIRR(cf_t, 0.0001, 100);

        Response.Write("IRR for action " + action.actionID + " is " + Convert.ToString(Math.Round(irr_r.Value * 100, 4)) + "/" + Convert.ToString(Math.Round(irr_t.Value * 100, 4)) + "<br>");

        //action.irr_realized = Math.Round(irr_r.Value * 100, 4);
        //action.irr_target = Math.Round(irr_t.Value * 100, 4);

        try
        {
            db.SubmitChanges();
        }
        catch
        {
            Response.Write("Error updating the action " + action.actionID + "<br>");
        }
    }
    protected void gvdb(object sender, EventArgs e)
    {
        //double avg=0;
        //double avg2 = 0;
        //double avgn = 0;
        //double avgp = 0;
        //double avgp_t = 0;
        //double avgn_t = 0;

        //double avg_r = 0;
        //double avg2_r = 0;
        //double avgn_r = 0;
        //double avgp_r = 0;
        //double avgp_t_r = 0;
        //double avgn_t_r = 0;


        //int c = 0; int cneg = 0; int cpos = 0;
        //int cneg_r = 0; int cpos_r = 0;

        //foreach (GridViewRow row in GridView2.Rows)
        //{

        //    // irr
        //    if (Convert.ToDouble(row.Cells[20].Text.Replace("%", "").Replace(",", "")) < 0)
        //    {
        //        avgn += Convert.ToDouble(row.Cells[20].Text.Replace("%", "").Replace(",", ""));
        //        avgn_t += Convert.ToDouble(row.Cells[21].Text.Replace("%", "").Replace(",", ""));
        //        cneg++;
        //    }
        //    else if (Convert.ToDouble(row.Cells[20].Text.Replace("%", "").Replace(",", "")) > 0)
        //    {
        //        avgp += Convert.ToDouble(row.Cells[20].Text.Replace("%", "").Replace(",", ""));
        //        avgp_t += Convert.ToDouble(row.Cells[21].Text.Replace("%", "").Replace(",", ""));
        //        cpos++;
        //    }

        //    //return

        //    if (Convert.ToDouble(row.Cells[22].Text.Replace("%", "").Replace(",", "")) < 0)
        //    {
        //        avgn_r += Convert.ToDouble(row.Cells[22].Text.Replace("%", "").Replace(",", ""));
        //        avgn_t_r += Convert.ToDouble(row.Cells[23].Text.Replace("%", "").Replace(",", ""));
        //        cneg_r++;
        //    }
        //    else if (Convert.ToDouble(row.Cells[22].Text.Replace("%", "").Replace(",", "")) > 0)
        //    {
        //        avgp_r += Convert.ToDouble(row.Cells[22].Text.Replace("%", "").Replace(",", ""));
        //        avgp_t_r += Convert.ToDouble(row.Cells[23].Text.Replace("%", "").Replace(",", ""));
        //        cpos_r++;
        //    }



        //    avg += Convert.ToDouble(row.Cells[20].Text.Replace("%", "").Replace(",", ""));
        //    avg2 += Convert.ToDouble(row.Cells[21].Text.Replace("%", "").Replace(",", ""));
        //    c++;

        //    avg_r += Convert.ToDouble(row.Cells[22].Text.Replace("%", "").Replace(",", ""));
        //    avg2_r += Convert.ToDouble(row.Cells[23].Text.Replace("%", "").Replace(",", ""));

        //}

        //s_count.Text = Convert.ToString(c);

        //s_realized.Text = Convert.ToString(Math.Round(avg/c,0)) + "%";
        //s_return_realized_total.Text = Convert.ToString(Math.Round(avg_r / c, 0)) + "%";

        //s_avg_losers.Text = Convert.ToString(Math.Round(avgn / cneg, 0)) + "%";
        //s_return_realized_negative.Text = Convert.ToString(Math.Round(avgn_r / cneg_r, 0)) + "%";

        //s_avg_winners.Text = Convert.ToString(Math.Round(avgp / cpos, 0)) + "%";
        //s_return_realized_positive.Text = Convert.ToString(Math.Round(avgp_r / cpos_r, 0)) + "%";

        //s_target_neg.Text = Convert.ToString(Math.Round(avgn_t / cneg, 0)) + "%";
        //s_return_target_negative.Text = Convert.ToString(Math.Round(avgn_t_r / cneg_r, 0)) + "%";

        //s_target_pos.Text = Convert.ToString(Math.Round(avgp_t / cpos, 0)) + "%";
        //s_return_target_positive.Text = Convert.ToString(Math.Round(avgp_t_r / cpos_r, 0)) + "%";

        //s_target.Text = Convert.ToString(Math.Round(avg2/c,0)) + "%";
        //s_return_target_total.Text = Convert.ToString(Math.Round(avg2_r / c, 0)) + "%";

        //s_ratio.Text = Convert.ToString(Math.Round(avg / avg2, 2));
        //s_return_ratio_total.Text = Convert.ToString(Math.Round(avg_r / avg2_r, 2));

        //s_losers.Text = Convert.ToString(cneg_r) + " (" + Convert.ToString(Math.Round(100*((double)cneg_r / (double)c),0)) + "%)";
        //s_winners.Text = Convert.ToString(cpos_r) + " (" + Convert.ToString(Math.Round(100 *((double)cpos_r / (double)c), 0)) + "%)";

        //s_ratio_n.Text = Convert.ToString(Math.Round(avgn / avgn_t, 2));
        //s_return_ratio_negative.Text = Convert.ToString(Math.Round(avgn_r / avgn_t_r, 2));

        //s_ratio_p.Text = Convert.ToString(Math.Round(avgp / avgp_t, 2));
        //s_return_ratio_positive.Text = Convert.ToString(Math.Round(avgp_r / avgp_t_r, 2));



    }
    protected void xxx()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            var action = from temp in db.Actions select temp;

            foreach (var a in action)
            {

                double realized;
                realized = Math.Round(100 * (((a.currentValue + a.dividend) / a.startValue - 1) * (1 / ((double)((a.lastUpdated - a.startDate).Days) / 365))), 2);
                //Response.Write((double)((a.lastUpdated.Value - a.startDate).Days)/365);   
                if (!double.IsNaN(realized))
                {
                    //a.return_realized = realized;
                }
                else
                {
                    //a.return_realized = 0;
                }
                //a.return_target = Math.Round(100 * (((a.targetValue) / a.startValue - 1) * (1 / ((double)((a.targetDate - a.startDate).Days) / 365))), 2);
                try
                {
                    db.SubmitChanges();
                }
                catch
                {
                    Response.Write("Failed for " + a.actionID + "<br>");
                }
                //Response.Write(a.actionID + ": " + Math.Round(100*(((a.currentValue+a.dividend.Value) / a.startValue - 1) * (1 / ((double)((a.lastUpdated.Value - a.startDate).Days) / 365))),2) + "%<br>");

                //Response.Write((a.currentValue / a.startValue - 1) * (1 / (((a.lastUpdated.Value - a.startDate).Days / 365))));
            }
            
        }
    }
    protected void dividend_yield(object sender, EventArgs e)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            var fs = from temp in db.funds select temp;
            foreach (var f in fs)
            {
                if (f.fund_values.Max(b => b.dividend) == 0 || f.fundID==605 || f.fundID==710 || f.fundID==608 || f.fundID==767 || f.fundID==593 || f.fundID==802)
                {
                    try
                    {
                        f.dividend_yield = 0;
                        db.SubmitChanges();
                    }
                    catch
                    {
                        Response.Write(f.ticker + " failed<br>");
                    }
                }
                else
                {
                    double div = f.fund_values.Where(b => b.dividend.Value > 0).OrderByDescending(b => b.date).First().dividend.Value;
                    double p = f.fund_values.OrderByDescending(b => b.date).First().closeValue.Value;
                    f.dividend_yield = 4 * div / p;
                    try
                    {
                        db.SubmitChanges();
                        Response.Write("Dividend yield for " + f.ticker + " is " + (div / p)*400 + "<br>");
                    }
                    catch
                    {
                        Response.Write(f.ticker + " failed<br>");
                    }
                    

                }
            }
        }
    }
    protected void dividend_yahoo(object sender, EventArgs e)
    { 
        DataClassesDataContext db = new DataClassesDataContext();
        {
            var funds = from temp in db.funds select temp;
            double div=0;
            

            if (funds.Count()>0)
            {
                foreach (var f in funds)
                {
                    HtmlWeb hw = new HtmlWeb();
                    HtmlDocument ho = hw.Load("http://finance.yahoo.com/q?ql=1&s=" + f.ticker.Trim());
                    HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//td");

                    foreach (HtmlNode z in m)
                    {
                        if (z.InnerText.IndexOf("(") != -1 && z.InnerText.IndexOf(")") != -1)
                        {
                            try
                            {
                                if (z.InnerHtml.Split(' ').First() == "N/A")
                                {
                                    div = 0;
                                    f.dividend_yield = 0;
                                    db.SubmitChanges();
                                }
                                else
                                {
                                    div = Convert.ToDouble(z.InnerHtml.Split(' ').First());
                                    f.dividend_yield = div / f.fund_values.Where(b=>b.isLatest==true).First().closeValue.Value;
                                    db.SubmitChanges();
                                    Response.Write(f.ticker + " dividend yield is " + f.dividend_yield * 100 + "%<br>");
                                }
                            }
                            catch
                            {
                                Response.Write(f.ticker + " was not updated <br>");
                            }
                        }
                    }
                }
            }
        }
    }

    protected void get_sector()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            //var funds = from temp in db.funds where !temp.sector.HasValue select temp;
            var funds = (from temp in db.Actions where !temp.fund.sector.HasValue select temp).GroupBy(b => b.ticker);

            foreach (var fund in funds)
            {
                HtmlWeb hw = new HtmlWeb();
                HtmlDocument ho = hw.Load("http://finance.yahoo.com/q/in?ql=1&s=" + fund.First().fund.ticker.Trim());
                HtmlNodeCollection m = ho.DocumentNode.SelectNodes("//td");
                string peer = "";
                string sector = "";

                try
                {
                    peer = HttpUtility.HtmlDecode(m[21].InnerText.Trim());
                    sector = HttpUtility.HtmlDecode(m[13].InnerText.Trim());
                    int sectorid = DataBaseLayer.yahoo_conversion_table(sector);
                    var peer_group = from temp in db.Peer_Groups where temp.name == peer select temp;
                    var f = from temp in db.funds where temp.fundID == fund.First().ticker select temp;
                            
                    if (peer_group.Any())
                    {
                        if (f.Any()) {
                            Response.Write("Updated " + f.First().ticker + "<br>");
                            f.First().sector = peer_group.First().sector;
                            db.SubmitChanges();
                        }
                        else
                        {
                            Response.Write("OOps!<BR>");
                        }
                    }
                    else if (sectorid != 0)
                    {
                        Peer_Group p = new Peer_Group();
                        p.sector = sectorid;
                        p.name = peer;
                        db.Peer_Groups.InsertOnSubmit(p);
                        db.SubmitChanges();
                        f.First().sector = p.sector;
                        db.SubmitChanges();
                        Response.Write(peer + " was added<br>");
                    }
                    else {
                        Response.Write(peer + " not found<br>");
                    }
                }
                catch (Exception e) {
                    Response.Write(peer + " - " + e.Message + "<br>");
                }
            }
        }
    }
    protected void insert_notification(int actionid,string type,bool active,DateTime lastupdated,bool unique,int userid)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            if (active == true)
            {
                if (unique == true)
                {
                    var find_comments = from temp in db.Comments where temp.action == actionid && temp.type == type select temp;

                    if (find_comments.Count() > 0)
                    {
                        foreach (var comment in find_comments)
                        {
                            db.Comments.DeleteOnSubmit(comment);
                        }
                    }
                }

                Comment comment_obj = new Comment();
                comment_obj.type = type;
                comment_obj.action = actionid;
                comment_obj.date = lastupdated;
                comment_obj.user = userid;
                comment_obj.recap = false;
                try
                {
                    db.Comments.InsertOnSubmit(comment_obj);
                    db.SubmitChanges();
                }
                catch
                {
                    Response.Write("<font color=red>Unable to insert " + type + " notification for " + actionid + " into the database</font><br>");
                }
            }            
        }
    }

    // identifies
    private void rectify_all_function_states()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            var all_actions = from temp in db.Actions where temp.active == true select temp;

            foreach (var action in all_actions.Where(b => b.matured == true && b.targetDate <= DateTime.Now && b.expired == false))
            {
                action.expired = true;

                try
                {
                    db.SubmitChanges();
                }
                catch
                {
                    Response.Write("<font color=red>Unable to update action " + action.actionID + "</font><br>");
                }
            }

            var all_actions_grouped_by_analyst = all_actions.GroupBy(b => b.article1.origin);

            foreach (var action_group_analyst in all_actions_grouped_by_analyst)
            {

                var all_actions_grouped = action_group_analyst.GroupBy(b => b.fund_actions.First().fund);
                //var all_actions_grouped = all_actions.GroupBy(b => b.fund_actions.First().fund);

                foreach (var action_group in all_actions_grouped)
                {
                    // if there are more than one action for that ticker by that analyst
                    int action_count = action_group.Count();
                    if (action_count > 1)
                    {
                        // if there's at least one type (2) (paused- active=1 B/E=1)
                        int action_type2_count = action_group.Where(b => b.active == true && (b.expired == true || b.breached == true)).Count();
                        if (action_type2_count > 0)
                        {
                            // if there's at least one type (3) or (4) ==> (play (a=1,b&e&m=0) -or- a=1,m=1,b&e=0)
                            if (action_group.Where(b => b.active == true && b.expired == false && b.breached == false).Count() > 0)
                            {
                                // set all type (2)s to (1) which is active=0
                                foreach (var action in action_group.Where(b => b.active == true && (b.expired == true || b.breached == true)))
                                {
                                    action.active = false;
                                    try
                                    {
                                        db.SubmitChanges();
                                    }
                                    catch
                                    { Response.Write("<font color=red>Unable to update action " + action.actionID + "</font><br>"); }
                                }
                            }
                            // if only type (2) actions exist
                            else if (action_count == action_type2_count)
                            {
                                int count = 1;
                                foreach (var action in action_group.OrderByDescending(b => b.targetDate))
                                {
                                    if (count > 1)
                                    {
                                        action.active = false;
                                        try
                                        {
                                            db.SubmitChanges();
                                        }
                                        catch
                                        { Response.Write("<font color=red>Unable to update action " + action.actionID + "</font><br>"); }

                                    }
                                    count++;
                                }
                            }
                        }
                    }

                    //foreach (var action in action_group)
                    //{ 

                    //}

                }

            }

            }

            
    }

    private double[] calculate_overall_return(bool totalreturn, double dividend, bool is_short, double start, double current, double lower,double target)
    {
        double[] x = new double[2];

        double div = totalreturn ? dividend : 0;
        double num = is_short ? -1 : 1;
        num *= (current + div - start);
        double denom_negative = Math.Abs(start - lower);
        double denom_positive = Math.Abs(target - start);

        if (num > 0)
            x[0] = num / denom_positive;
        else
            x[0] = num / denom_negative;

        x[1] = num / start;
        //x[1] *= is_short?-1:1;

        return x;
    }
}

// IRR calculator
public struct Pair<T, Z>
{

    public Pair(T first, Z second) { First = first; Second = second; }

    public readonly T First;

    public readonly Z Second;

}
public class CashFlow
{

    public CashFlow(Money amount, DateTime date) { Amount = amount; Date = date; }

    public CashFlow(Money p, DateTime? nullable)
    {
        // TODO: Complete member initialization
        this.p = p;
        this.nullable = nullable;
    }

    public readonly Money Amount;
    public readonly DateTime Date;
    private Money p;
    private DateTime? nullable;
}
public struct AlgorithmResult<TKindOfResult, TValue>
{

    public AlgorithmResult(TKindOfResult kind, TValue value)
    {

        Kind = kind;
        Value = value;
    }

    public readonly TKindOfResult Kind;
    public readonly TValue Value;
}
public enum ApproximateResultKind
{
    ApproximateSolution,
    ExactSolution,
    NoSolutionWithinTolerance
}
public static class Algorithms
{

    internal static Money CalculateXNPV(IEnumerable<CashFlow> cfs, Rate r)
    {

        //if (r <= -1)
        //    r = -0.99999999; // Very funky ... Better check what an IRR <= -100% means

        return (from cf in cfs
                let startDate = cfs.OrderBy(cf1 => cf1.Date).First().Date
                select cf.Amount / (double)Math.Pow(1 + r, (cf.Date - startDate).Days / 365.0)).Sum();
    }

    internal static Pair<Rate, Rate> FindBrackets(Func<IEnumerable<CashFlow>, Rate, Money> func, IEnumerable<CashFlow> cfs)
    {

        // Abracadabra magic numbers ...
        const int maxIter = 100;
        const Rate bracketStep = 0.5;
        const Rate guess = 100;

        Rate leftBracket = guess - bracketStep;
        Rate rightBracket = guess + bracketStep;
        var iter = 0;

        while (func(cfs, leftBracket) * func(cfs, rightBracket) > 0 && iter++ < maxIter)
        {

            leftBracket -= bracketStep;
            rightBracket += bracketStep;
        }

        if (iter >= maxIter)
            return new Pair<double, double>(0, 0);

        return new Pair<Rate, Rate>(leftBracket, rightBracket);
    }

    // From "Applied Numerical Analyis" by Gerald
    internal static AlgorithmResult<ApproximateResultKind, Rate> Bisection(Func<Rate, Money> func, Pair<Rate, Rate> brackets, Rate tol, int maxIters)
    {

        int iter = 1;

        Money f3 = 0;
        Rate x3 = 0;
        Rate x1 = brackets.First;
        Rate x2 = brackets.Second;

        do
        {
            var f1 = func(x1);
            var f2 = func(x2);

            if (f1 == 0 && f2 == 0)
                return new AlgorithmResult<ApproximateResultKind, Rate>(ApproximateResultKind.NoSolutionWithinTolerance, x1);

            if (f1 * f2 > 0)
                throw new ArgumentException("x1 x2 values don't bracket a root");

            x3 = (x1 + x2) / 2;
            f3 = func(x3);

            if (f3 * f1 < 0)
                x2 = x3;
            else
                x1 = x3;

            iter++;

        } while (Math.Abs(x1 - x2) / 2 > tol && f3 != 0 && iter < maxIters);

        if (f3 == 0)
            return new AlgorithmResult<ApproximateResultKind, Rate>(ApproximateResultKind.ExactSolution, x3);

        if (Math.Abs(x1 - x2) / 2 < tol)
            return new AlgorithmResult<ApproximateResultKind, Rate>(ApproximateResultKind.ApproximateSolution, x3);

        if (iter > maxIters)
            return new AlgorithmResult<ApproximateResultKind, Rate>(ApproximateResultKind.NoSolutionWithinTolerance, x3);

        throw new Exception("It should never get here");
    }

    public static AlgorithmResult<ApproximateResultKind, Rate> CalculateXIRR(IEnumerable<CashFlow> cfs, Rate tolerance, int maxIters)
    {

        var brackets = FindBrackets(CalculateXNPV, cfs);

        if (brackets.First == brackets.Second)
            return new AlgorithmResult<ApproximateResultKind, double>(ApproximateResultKind.NoSolutionWithinTolerance, brackets.First);

        return Bisection(r => CalculateXNPV(cfs, r), brackets, tolerance, maxIters);
    }


}