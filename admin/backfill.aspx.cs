using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_backfill : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

       // Response.Write(backfill_actionmonitor_scores());
    }

    public static string backfill_actionmonitor_scores(){
        DataClassesDataContext db = new DataClassesDataContext();
        var ams = from temp in db.ActionMonitors where temp.active == false select temp;
        string outp = "";
        foreach (var am in ams)
        {
            outp += AdminBackend.updateAnalystPerformanceByActionMonitorS("m", am.lastUpdated, am);
        }
        return outp;
    }
}