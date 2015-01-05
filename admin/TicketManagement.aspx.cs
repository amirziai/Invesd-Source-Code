using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class TicketManagement : System.Web.UI.Page
{
    // shows all created tickets and allows approving or rejecting requested mofications
    // TicketManagement_Edit handles the actual database manipulation
    protected void Page_Load(object sender, EventArgs e)
    {
        DataClassesDataContext db = new DataClassesDataContext();
        {
            var article_ticket = from temp in db.articles where temp.is_ticket == true select temp;

            if (article_ticket.Any())
            {
                foreach (var at in article_ticket)
                {
                    var original = from temp in db.articles where temp.idarticle == at.ticketOrArticleNumber select temp;
                    int found = 0;

                    if (original.Any())
                    {
                        Table t = new Table();
                        t.BorderWidth = 1;
                        t.BorderColor = System.Drawing.Color.Green;
                        TableRow tr = new TableRow();
                        TableRow tr2 = new TableRow();

                        if (at.url != original.First().url)
                        {
                            TableCell td = new TableCell();
                            TableCell td2 = new TableCell();
                            td.Text = "URL";
                            td.ToolTip = at.url;
                            td.ForeColor = System.Drawing.Color.Green;
                            td2.Text = "URL";
                            td2.ToolTip = original.First().url;
                            tr.Cells.Add(td);
                            tr2.Cells.Add(td2);
                            found = 1;
                        }

                        if (at.title != original.First().title)
                        {
                            TableCell td = new TableCell();
                            TableCell td2 = new TableCell();
                            td.Text = shorten(at.title,25);
                            td.ToolTip = at.title;
                            td.ForeColor = System.Drawing.Color.Green;
                            td2.Text = shorten(original.First().title,25);
                            td2.ToolTip = original.First().title;
                            tr.Cells.Add(td);
                            tr2.Cells.Add(td2);
                            found = 1;
                        }

                        if (at.text != original.First().text)
                        {
                            TableCell td = new TableCell();
                            TableCell td2 = new TableCell();
                            td.Text = shorten(at.text,25);
                            td.ToolTip = at.text;
                            td.ForeColor = System.Drawing.Color.Green;
                            td2.Text = shorten(original.First().text,25);
                            td2.ToolTip = original.First().text;
                            tr.Cells.Add(td);
                            tr2.Cells.Add(td2);
                            found = 1;
                        }

                        if (at.date != original.First().date)
                        {
                            TableCell td = new TableCell();
                            TableCell td2 = new TableCell();
                            td.Text = string.Format("{0:MMM dd, yy}",at.date);
                            td.ForeColor = System.Drawing.Color.Green;
                            td2.Text = string.Format("{0:MMM dd, yy}", original.First().date); 
                            tr.Cells.Add(td);
                            tr2.Cells.Add(td2);
                            found = 1;
                        }

                        if (found == 1)
                        {
                            TableCell td = new TableCell();
                            HyperLink hl = new HyperLink();
                            hl.NavigateUrl = at.url;
                            hl.Text = "Link";
                            TableCell td2 = new TableCell();
                            HyperLink hl2 = new HyperLink();
                            hl2.NavigateUrl = original.First().url;
                            hl2.Text = "Link";
                            td.Controls.Add(hl);
                            td2.Controls.Add(hl2);
                            tr.Cells.Add(td);
                            tr2.Cells.Add(td2);

                            Button btn = new Button();
                            btn.Text = "Approve";
                            TableCell td_last = new TableCell();
                            td_last.Controls.Add(btn);
                            Button btn2 = new Button();
                            btn2.Text = "Reject";
                            TableCell td_last2 = new TableCell();
                            td_last2.Controls.Add(btn2);
                            tr.Cells.Add(td_last);
                            tr2.Cells.Add(td_last2);

                            t.Controls.Add(tr);
                            t.Controls.Add(tr2);
                            div_main.Controls.Add(t);
                        }


                    }
                }
            }
        }
    }

    protected string shorten(string x, int length)
    {
        if (x.Length > length)
            return x.Substring(0, length) + "...";
        else
            return x;
    }
}