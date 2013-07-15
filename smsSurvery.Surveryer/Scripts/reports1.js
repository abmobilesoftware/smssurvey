window.app = window.app || {};
window.app.piedata = {};
window.app.overviewPieData = {};
window.app.tags = [];
window.app.displayComparisonReportsForRatingQ = function (questionId) {
   $.ajax({
      data: {
         questionId: questionId,
         tags: window.app.tags
      },
      traditional: true,
      url: "/Reports/GetTagComparisonReportForRatingQuestion",
      dataType: "json",
      async: true,
      success: function (jsonData) {
         var options = {
            backgroundColor: '#F5F8FA',
            sliceVisibilityThreshold: 0,
            'width': 'auto',            
            vAxis: { title: "Percentage of answers" },
            hAxis: { title: "" },
            seriesType: "bars"            
         };
         
         var barchart = new google.visualization.ComboChart(document.getElementById('barChart_div1'));
         var bardata = new google.visualization.DataTable(jsonData);
         barchart.draw(bardata, options);
              
      }
   });
};

window.app.runrunrun = function () {
   window.app.displayComparisonReportsForRatingQ("1");
   
}
$(document).ready(function () {
   window.app.runrunrun();
});