window.app = window.app || {};
window.report = window.report || {};

window.app.displayReportOverview = function (surveyTemplateId) {
   //DA we should avoid recreating the chart as this is a costly operation   
   var chartId = '#tableOverviewChart_div' + surveyTemplateId;
   var chartElem = $(chartId);

   var loadingIndicatorId = "#loadingOverviewIndicator" + surveyTemplateId;
   var loadingIndicator = $(loadingIndicatorId);
   
   var graphsContainer = $("#graphsOverviewContainer" + surveyTemplateId);
   var candidateHeight = graphsContainer.outerHeight();
   candidateHeight = candidateHeight != 0 ? candidateHeight + "px" : "360px";
   loadingIndicator.height(candidateHeight);

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
         var tablechart = new google.visualization.Table(chartElem[0]);
         tabledata = new google.visualization.DataTable(jsonData);
         var tableOptions = {
            backgroundColor: '#F5F8FA',
            sliceVisibilityThreshold: 0,
            cssClassNames: { tableCell: "tCell" }
         };
         tablechart.draw(tabledata, tableOptions);
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