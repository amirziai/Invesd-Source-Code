using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


public partial class admin_testing_stockUpdates : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //YahooStockEngine.FetchSince(new Quote("GOOG"), new DateTime(2012,1,1));
        /*fund fundm = new fund();
        fundm.ticker = "TWTR";
        fund_value fundValue = new fund_value();
        fundValue.closeValue = 50;
        fundValue.date = new DateTime(2014,1,21);
        AdminBackend.CrossCheckYahooGoogle(fundm, fundValue);*/
        //AdminBackend.updateAllDB_Click();
        /*
               DataClassesDataContext db = new DataClassesDataContext();
                var funds = from temp in db.funds where temp.exchange == null select temp;
                foreach (var fund in funds)
                {
                    string exchange = YahooStockEngine.FetchExchange(fund.ticker.Trim());
                    fund.exchange = exchange;
                    db.SubmitChanges();
                }

                */
        /* DataClassesDataContext db = new DataClassesDataContext();
         var funds = (from temp in db.fund_values where temp.isLatest.Value select temp.fundID).Distinct();
         List<int> fundswithlatest = funds.ToList();
        
         var funddNoIsLatest = from temp in db.funds select temp;
         int count = 0;
         foreach (var fund in funddNoIsLatest)
         {
             if (!fundswithlatest.Contains<int>(fund.fundID))
             {
                 try
                 {
                     var fundValue = (from temp in db.fund_values where temp.fundID == fund.fundID select temp);
                     var lastFundValue = fundValue.First();
                     foreach (var value in fundValue)
                     {
                         if (DateTime.Compare(value.date, lastFundValue.date) > 0)
                             lastFundValue = value;
                     }
                     lastFundValue.isLatest = true;
                     db.SubmitChanges();
                 }
                 catch { count++;

                 try
                 {
                     var lilili = from temp in db.Liaisons where temp.ticker == fund.fundID select temp;
                     foreach (var lili in lilili)
                     {
                         db.Liaisons.DeleteOnSubmit(lili);
                         db.SubmitChanges();
                     }

                     var ararar = from temp in db.ArticleTickers where temp.ticker == fund.fundID select temp;
                     foreach (var arar in ararar)
                     {
                         db.ArticleTickers.DeleteOnSubmit(arar);
                         db.SubmitChanges();
                     }

                     var blblbl = from temp in db.Bloomberg_Consensus where temp.ticker == fund.fundID select temp;
                     foreach (var blbl in blblbl)
                     {
                         db.Bloomberg_Consensus.DeleteOnSubmit(blbl);
                         db.SubmitChanges();
                     }

                     db.funds.DeleteOnSubmit(fund);
                    db.SubmitChanges();
                 }
                 catch { }
                
                 }
             }
         }
         */
        // AdminBackend.update_actions_for_split(1045, 0.1);

        //    AdminBackend.reFetchAdjustedValues("TCTZF");

        ////Updating spevific actions for SVF
        //int ticker = 469;
        //double ratio = (double)2.0;

        //DataClassesDataContext db = new DataClassesDataContext();
        ////   DateTime RightNow = DateTime.Now;

        //var actions = from temp in db.Actions where temp.ticker == ticker select temp;

        //if (actions.Any())
        //{
        //    foreach (var action in actions)
        //    {
        //        action.targetValue *= ratio;
        //        action.startValue *= ratio;
        //        action.currentValue *= ratio;
        //        action.lowerValue *= ratio;
        //        action.maxValue *= ratio;
        //        action.minValue *= ratio;
        //        db.SubmitChanges();
        //    }
        //}
        
        ////

        //DataClassesDataContext db = new DataClassesDataContext();
        //var ams = (from temp in db.ActionMonitors where temp.matured || temp.expired select temp).ToList() ;
        //foreach (ActionMonitor am in ams)
        //{
        //    var lastSplits = (from temp in db.fund_values where temp.fundID == am.ticker && temp.split != 1 select temp).OrderByDescending(b => b.date);
        //    if (lastSplits.Any())
        //    {

        //        var lastSplit = lastSplits.First();
        //        if (am.monitorStart < lastSplit.date)
        //        {
        //            Response.Write(am.fund.ticker + "," + am.ticker + "," + am.ID.ToString() + "\t\n");
        //        }
        //    }
        //}

        try
        {
            AdminBackend.update_All_Actions_v2(false);
            
            Ancillary.email_amir("6pm report, 3/8 actions complete", "Good", "Report");
        }
        catch (Exception eeee)
        {

            Ancillary.email_amir("6pm report, 3/8 actions error", eeee.Message + "<br><br>" + eeee.StackTrace, "Error");
        }

        //Connect all actions to its user
        //DataClassesDataContext db = new DataClassesDataContext();
        //var allActions = from temp in db.Actions select temp;
        //foreach (var a in allActions.OrderByDescending(b => b.actualStartDate))
        //{
        //    try
        //    {
        //        a.analystID = a.article1.origin;
                
        //    }
        //    catch
        //    {

        //    }
        //    try
        //    {
        //        a.rational = a.article1.summary;
        //    }
        //    catch
        //    {

        //    }
        //    db.SubmitChanges();
        //}
    }
}