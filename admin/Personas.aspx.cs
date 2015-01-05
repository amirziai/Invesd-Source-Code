using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class admin_Personas : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        x();
    }

    public void x()
    {
        DataClassesDataContext db = new DataClassesDataContext();
        

    }
}