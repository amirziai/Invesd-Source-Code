using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_Gripe : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        bool users = !string.IsNullOrEmpty(Request.QueryString["users"]);

        if (!string.IsNullOrEmpty(Request.QueryString["delete"]))
        {
            try
            {
                DataClassesDataContext db = new DataClassesDataContext();
                var delete = from temp in db.Gripes where temp.id == Convert.ToInt32(Request.QueryString["delete"]) select temp;

                if (delete.Any()) {
                    db.Gripes.DeleteOnSubmit(delete.First());
                    db.SubmitChanges();
                }
            }
            catch {
                DataBaseLayer.gripe(DataBaseLayer.GetCheckUser(), "admin/Gripe.aspx- Couldn't delete gripe ID " + Request.QueryString["delete"]);
            }
        }

        if (!string.IsNullOrEmpty(Request.QueryString["records"]) && !string.IsNullOrEmpty(Request.QueryString["skipped"]))
        {
            try
            {
                gripe_table(Convert.ToInt32(Request.QueryString["records"]), Convert.ToInt32(Request.QueryString["skipped"]),users);
            }
            catch
            {
                gripe_table(20, 0,users);
            }
        }
        else
        {
            gripe_table(20, 0,users);
        }
    }

    protected void gripe_table(int records,int skipped,bool users) {
        DataClassesDataContext db = new DataClassesDataContext();
        var gripes = from temp in db.Gripes where !temp.message.Contains("(Google") && !temp.message.Contains("Changed/exists") select temp;
        if (users)
            gripes = gripes.Where(b => b.userid != 1 && b.userid != 2);
        
        gripes = gripes.OrderByDescending(b=>b.date).Skip(records*skipped).Take(records);
        
        if (gripes.Any()) {
            foreach (var g in gripes) {
                System.Web.UI.HtmlControls.HtmlTableRow row = new System.Web.UI.HtmlControls.HtmlTableRow();
                System.Web.UI.HtmlControls.HtmlTableCell c1 = new System.Web.UI.HtmlControls.HtmlTableCell();
                System.Web.UI.HtmlControls.HtmlTableCell c2 = new System.Web.UI.HtmlControls.HtmlTableCell();
                System.Web.UI.HtmlControls.HtmlTableCell c3 = new System.Web.UI.HtmlControls.HtmlTableCell();
                System.Web.UI.HtmlControls.HtmlTableCell c4 = new System.Web.UI.HtmlControls.HtmlTableCell();

                c1.InnerText = g.user.display_name;
                c2.InnerHtml = Ancillary.string_cutter(g.message,75,false,"top");
                c3.InnerText = string.Format("{0:MMM d,yy hh:mm tt}", g.date);
                c4.InnerHtml = "<a class=\"urls\" href=\"Gripe.aspx?delete=" + g.id + "\"><i class=\"icon-trash\"></i></a>";

                row.Cells.Add(c1);
                row.Cells.Add(c2);
                row.Cells.Add(c3);
                row.Cells.Add(c4);

                tbl.Rows.Add(row);

            }
        }

        if (skipped == 0)
        {
            hyp_previous.Enabled = false;
        }
        else
        {
            hyp_previous.Enabled = true;
            hyp_previous.NavigateUrl = "Gripe.aspx?records=" + records.ToString() + "&skipped=" + (skipped-1).ToString();
        }

        hyp_next.NavigateUrl = "Gripe.aspx?records=" + records.ToString() + "&skipped=" + (skipped+1).ToString();

    }
}