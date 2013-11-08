window.app = window.app || {};
window.report = window.report || {};

window.report.repOverviewPiecharts = {};
window.app.piedata = {};

window.app.recalculateHeight = function (surveyTemplateId) {
   var graphsContainer = $("#graphsOverviewContainer" + surveyTemplateId);
   var graphContainerHeight = graphsContainer.outerHeight();
   //if less than 650, use 650
   var candidateHeight = graphContainerHeight != 0 ? (graphContainerHeight < 650 ? 650 : graphContainerHeight + "px") : 0;
   candidateHeight = candidateHeight != 0 ? candidateHeight + "px" : "650";
   return candidateHeight;
}

window.app.displayReportOverview = function (surveyTemplateId) {
   //DA we should avoid recreating the chart as this is a costly operation   
   var chartId = '#tableOverviewChart_div' + surveyTemplateId;
   var chartElem = $(chartId);

   var pieChartId = "#pieOverviewChart_div" + surveyTemplateId;
   var pieChartElem = $(pieChartId);
   var pieChart = window.report.repOverviewPiecharts[surveyTemplateId];
   if (pieChart == undefined) {
      pieChart = new google.visualization.PieChart(pieChartElem[0]);
   }

   var candidateHeight = window.app.recalculateHeight(surveyTemplateId);
   var loadingIndicatorId = "#loadingOverviewIndicator" + surveyTemplateId;
   var loadingIndicator = $(loadingIndicatorId);
   loadingIndicator.height(candidateHeight);
   var graphsContainer = $("#graphsOverviewContainer" + surveyTemplateId);
   graphsContainer.height(candidateHeight);
   loadingIndicator.width(chartElem.outerWidth());
   loadingIndicator.css("line-height", candidateHeight);
   loadingIndicator.show();

   $.ajax({
      data: {
         surveyTemplateId: surveyTemplateId,
         iIntervalStart: window.app.dateHelper.transformStartDate(window.app.startDate),
         iIntervalEnd: window.app.dateHelper.transformEndDate(window.app.endDate)       
      },
      url: "/Reports/GetSourceOverview",
      traditional: true,
      dataType: "json",
      async: true,
      cache: false,
      success: function (jsonData) {
         var options = {
            backgroundColor: '#F5F8FA',
            sliceVisibilityThreshold: 0,
            'width': 'auto',
            'height': 300,
            'title': "Percentage of completion (out of 100)"
         };
         loadingIndicator.hide();
         var piedata = new google.visualization.DataTable(jsonData.pie);
         window.app.piedata[surveyTemplateId] = piedata;
         pieChart.draw(piedata, options);

         var tablechart = new google.visualization.Table(chartElem[0]);
         tabledata = new google.visualization.DataTable(jsonData.table);
         var tableOptions = {
            backgroundColor: '#F5F8FA',
            sliceVisibilityThreshold: 0,
            cssClassNames: { tableCell: "tCell" }
         };
         tablechart.draw(tabledata, tableOptions);

         //recalculate the height       
         var newHeight = window.app.recalculateHeight(surveyTemplateId)
         loadingIndicator.height(newHeight);
         var graphsContainer = $("#graphsOverviewContainer" + surveyTemplateId);
         graphsContainer.height(newHeight);
         loadingIndicator.width(chartElem.outerWidth());
         loadingIndicator.css("line-height", newHeight);
         
      }
   });
};

window.app.runrunrun = function () {
   window.app.displayReportOverview($("#surveyId").text());
   
}

$(document).ready(function () {
   window.filterArea.initialize(true);
   $("#refreshReport").click(function () {
      window.app.runrunrun();
   });

   window.app.runrunrun();
});