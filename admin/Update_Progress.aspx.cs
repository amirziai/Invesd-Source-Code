using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_Update_Progress : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        get_data();
    }

    protected void get_data() {
        DataClassesDataContext db = new DataClassesDataContext();
        int islatest = (from temp in db.fund_values where temp.isLatest.Value && temp.date >= DateTime.Now.AddDays(-2) select temp).Count();
        int today = (from temp in db.fund_values where temp.isLatest.Value && temp.date >= DateTime.Now.AddDays(-1) select temp).Count();
        double progress = Math.Round(100 * ((double)today / (double)islatest), 0);

        progress_fundvalues.Attributes.Add("style", "width:" + progress + "%");
        l_fundvalues.InnerText = string.Format("{0:n0}", today) + " / " + string.Format("{0:n0}", islatest);
        inside_progress.InnerText = progress + "%";

    }
}