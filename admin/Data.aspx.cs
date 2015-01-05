using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using OfficeOpenXml;
using System.Threading;
using System.Threading.Tasks;

public partial class admin_Data : System.Web.UI.Page
{
    //protected void Page_Load(object sender, EventArgs e)
    //{
    //    xls("entire_v4_04.xlsm");
    //}

    //protected void xls(string filename)
    //{
    //    FileInfo newFile = new FileInfo(Server.MapPath("~") + "\\files\\" + filename);

    //    ExcelPackage pck = new ExcelPackage(newFile);
    //    int worksheet_index1 = 24;
    //    List<int> indices = new List<int>();
    //    while (true)
    //    {
    //        var ws = pck.Workbook.Worksheets["Data" + worksheet_index1];

    //        if (ws == null)
    //        {
    //            break;
    //        }
    //        else
    //        {
    //            indices.Add(worksheet_index1);
    //            worksheet_index1++;
    //        }
    //    }

    //    Parallel.ForEach(indices, worksheet_index =>
    //            {
    //                var ws = pck.Workbook.Worksheets["Data" + worksheet_index];

    //                int i = 300;
    //                int j = 1;
    //                int n = 1;

    //                try
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
    //                                        data.ticker = ticker;
    //                                        data.analyst_bloombergID = analyst_id;
    //                                        data.analyst_name = analyst_name;
    //                                        data.broker_code = broker_code;
    //                                        data.broker_name = broker_name;
    //                                        data.inDate = ws.Cells[i, j].Value.ToString();
    //                                        data.inPtx = ws.Cells[i, j + 1].Value.ToString();
    //                                        // Call the threaded function here

    //                                        // thread it
    //                                        //Thread t = new Thread(() => insert_all_v2(data));
    //                                        //t.Start();
    //                                        insert_all_v2(data);

    //                                        n++;
    //                                        Response.Write(ticker + "\t\t\t" + analyst_id + "\t\t\t" + analyst_name + "\t\t\t" + broker_code + "\t\t\t" + broker_name + "\t\t\t" + ws.Cells[i, j].Value + "\t\t\t" + ws.Cells[i, j + 1].Value);
    //                                        Response.Write("<br>");
    //                                    }
    //                                }

    //                                i++;
    //                            }
    //                        }
    //                        catch
    //                        {

    //                        }

    //                        i = 300;
    //                        j = j + 8;
    //                    }

    //                    Response.Write("<br>" + n);
    //                }
    //                catch
    //                {
    //                    Response.Write("<br>Broke on i=" + i + ", j=" + j + ", n=" + n);
    //                }



    //            });
    //}

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
    //            var past_action = (from temp in db.Actions where temp.ticker == Company.fundID && temp.article1.origin == Analyst.userID && temp.startDate < date select temp).OrderByDescending(q => q.targetDate);  // edited by amir: in case we are inserting an action that predates a bunch of other ones
    //            if (past_action.Any())
    //            {
    //                var pa = past_action.First();
    //                if (tgx != pa.targetValue || (tgx == pa.targetValue && (date - pa.startDate).TotalDays > 60))     // added by amir: if tpx changes there's a new action regardless of the number of days
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

    //protected fund GetorAddCompany(string ticker)
    //{
    //    fund theCompany = new fund();
    //    DataClassesDataContext db = new DataClassesDataContext();
    //    var comps = from temp in db.funds where temp.ticker == ticker select temp;
    //    if (comps.Any())
    //    {
    //        theCompany = comps.First();
    //        return theCompany;
    //    }
    //    else  //later, add company
    //    {
    //        return null;
    //    }
    //}

    //protected user GetorAddAnalyst(string analyst_name, string analyst_bloombergID, int brokerID)
    //{
    //    user Analyst = new user();
    //    DataClassesDataContext db = new DataClassesDataContext();
    //    try
    //    {
    //        var anas = from temp in db.users where temp.bloomberg_id == Convert.ToInt32(analyst_bloombergID) select temp;
    //        if (anas.Any())
    //        {
    //            Analyst = anas.First();
    //            return Analyst;
    //        }
    //        else
    //        {
    //            Analyst.bloomberg_id = Convert.ToInt32(analyst_bloombergID);
    //            Analyst.display_name = reconstruct(analyst_name);
    //            Analyst.seekingalpha_name = Analyst.display_name;
    //            Analyst.bloomberg_broker = brokerID;
    //            db.users.InsertOnSubmit(Analyst);
    //            db.SubmitChanges();
    //            return Analyst;
    //        }
    //    }
    //    catch
    //    {
    //        return null;
    //    }

    //}

    //protected Bloomberg_Broker GetorAddBroker(string broker_code, string broker_name)
    //{
    //    Bloomberg_Broker theBroker = new Bloomberg_Broker();
    //    DataClassesDataContext db = new DataClassesDataContext();
    //    try
    //    {
    //        var broks = from temp in db.Bloomberg_Brokers where temp.broker_code == broker_code select temp;
    //        if (broks.Any())
    //        {
    //            theBroker = broks.First();
    //            return theBroker;
    //        }
    //        else
    //        {
    //            theBroker.broker_code = broker_code;
    //            theBroker.name = broker_name;
    //            db.Bloomberg_Brokers.InsertOnSubmit(theBroker);
    //            db.SubmitChanges();
    //            return theBroker;
    //        }
    //    }
    //    catch
    //    {
    //        return null;
    //    }
    //}

    //protected string reconstruct(string x)
    //{
    //    string[] y = x.Split(' ');
    //    string final_product = "";
    //    int count = y.Count();
    //    int i = 1;

    //    foreach (string z in y)
    //    {

    //        if (z.Length > 1)
    //            final_product += z[0] + z.Substring(1).ToLower();

    //        if (count != i && z.Length > 1)
    //        {
    //            final_product += " ";
    //        }
    //        i++;
    //    }

    //    return final_product;
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