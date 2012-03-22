<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Timeplot.ascx.cs" Inherits="Timeplot" %>

<script language="javascript" type="text/javascript" src="http://people.iola.dk/olau/flot/jquery.js"></script>
<script language="javascript" type="text/javascript" src="http://people.iola.dk/olau/flot/jquery.flot.js"></script>
<script type="text/javascript">
$(function() {

        var data = <%= GetDataSource()%>;
        var plot = $.plot($("#timeplot"), data, {
                series : { lines: {show: true }, points: {show: true} },
                grid: {hoverable: true, clickable: true},
                xaxis: {mode: "time"}
            } 
        );

       function showTooltip(x, y, contents) {
        $('<div id="tooltip">' + contents + '</div>').css( {
            position: 'absolute',
            display: 'none',
            top: y + 5,
            left: x + 5,
            border: '1px solid #fdd',
            padding: '2px',
            'background-color': '#fee',
            opacity: 0.80
        }).appendTo("body").fadeIn(200);
       }

       var previousPoint = null;
       $("#timeplot").bind("plothover", function (event, pos, item) {
        $("#x").text(pos.x.toFixed(2));
        $("#y").text(pos.y.toFixed(2));
        if (item) {
                if (previousPoint != item.dataIndex) {
                    previousPoint = item.dataIndex;
                    
                    $("#tooltip").remove();
                    var x = item.datapoint[0].toFixed(2),
                        y = item.datapoint[1].toFixed(2);
                    
                    showTooltip(item.pageX, item.pageY,
                                item.series.label + " of " + new Date(parseInt(x))
                                + " = " + y);
                }
            }
            else {
                $("#tooltip").remove();
                previousPoint = null;            
            }
      });
      
});
</script>

<div id="timeplot" style="width:600px;height:300px;"/>