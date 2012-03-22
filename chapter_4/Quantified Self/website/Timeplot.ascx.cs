using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Timeplot : System.Web.UI.UserControl
{
    public List<TimeSeries> Plots = new List<TimeSeries>();

    public string GetDataSource()
    {
        string s = "[";
        foreach (TimeSeries t in Plots)
        {
            s += string.Format("{{ data: {0}, label: \"{1}\"}}",
                    t.ToJSON(), t.SeriesName.ToString());
            s += ",";
        }
        s += "]";
        return s;
    }

    protected void Page_Load(object sender, EventArgs e)
    {

    }
}

public class TimeSeries
{
    public String SeriesName;
    public List<TimeSeriesValues> SeriesValue;

    public TimeSeries(string name)
    {
        SeriesName = name;
        SeriesValue = new List<TimeSeriesValues>();
    }

    public string ToJSON()
    {
        string o = string.Format("[");
        foreach (TimeSeriesValues t in SeriesValue)
        {
            o += t.ToJSON();
            o += ",";
        }
        o += "]";

        return o;
    }

    public class TimeSeriesValues
    {
        public DateTime Time;
        public Double Value;

        public TimeSeriesValues(DateTime when, Double value)
        {
            Time = when;
            Value = value;
        }

        public string ToJSON()
        {
            TimeSpan span = new TimeSpan(DateTime.Parse("1/1/1970").Ticks);
            DateTime time = Time.Subtract(span);

            return String.Format("[{0}, \"{1}\"]",
                ((long)(time.Ticks / 10000)).ToString(),
                Value.ToString());
        }
    }


}