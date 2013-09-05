window.app = window.app || {};
window.report = window.report || {};
window.report.repOverviewPiecharts = {};
window.report.repPieCharts = {};
window.app.piedata = {};
window.app.yesnopiedata = {};
window.app.overviewPieData = {};
window.app.tags = [];
window.app.displayReportsForRatingQ = function (questionId, qType) {
   var chartId = "#pieChart_div" + questionId;
   var chartElem = $(chartId);
   var loadingIndicatorId = "#loadingIndicator" + questionId;   
   var loadingIndicator = $(loadingIndicatorId);

   var pieChart = window.report.repPieCharts[questionId];
   if (pieChart == undefined) {
      pieChart = new google.visualization.PieChart($(chartId)[0]);
      google.visualization.events.addListener(pieChart, 'select', function (e) {
         var sel = pieChart.getSelection();
         var selectedValue = window.app.piedata[questionId].getValue(sel[0].row, 0);
         var url = "/Answer/GetMessagesWithOnePredefinedAnswer?questionId=" + questionId + "&answer=" + selectedValue;
         var win = window.open(url, "_blank");
         win.focus();
      });
   }
   
   var graphsContainer = $("#graphsContainer" + questionId);
   var candidateHeight = graphsContainer.outerHeight();
   candidateHeight = candidateHeight != 0 ? candidateHeight + "px" : "410px";
   loadingIndicator.height(candidateHeight);
  
   graphsContainer.height(candidateHeight);
   loadingIndicator.width(chartElem.outerWidth());
   loadingIndicator.css("line-height", candidateHeight);
   loadingIndicator.show();   
   
   $.ajax({
      data: {
         questionId: questionId,
         iIntervalStart: window.app.dateHelper.transformStartDate(window.app.startDate),
         iIntervalEnd: window.app.dateHelper.transformEndDate(window.app.endDate),
         tags: window.app.tags
      },
      traditional: true,
      url: "/Reports/GetSurveyQuestionResults",
      dataType: "json",
      async: true,
      success: function (jsonData) {        
         var options = {
            backgroundColor: '#F5F8FA',
            sliceVisibilityThreshold: 0,
            'width': 'auto',
            'height': 350,
             animation: {
               duration: 1000,
               easing: 'out'
            }
         };
         var piedata = new google.visualization.DataTable(jsonData.pie);
         window.app.piedata[questionId] = piedata;
         pieChart.draw(piedata, options);
         
         var tablechart = new google.visualization.Table(document.getElementById('tableChart_div' + questionId));
         tabledata = new google.visualization.DataTable(jsonData.table);
         var tableOptions = {
            backgroundColor: '#F5F8FA',
            sliceVisibilityThreshold: 0,
            cssClassNames: { tableCell: "tCell" }
         };
         loadingIndicator.hide();
         tablechart.draw(tabledata, tableOptions);
         if (qType == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_RATING) {
            window.app.displayReportForRatingAdditionalInfo(questionId);
         }
      }
   });
   //for rating question we should also show an word cloud for Additional Info
};

window.app.displayReportForRatingAdditionalInfo = function (questionId) {
   $.ajax({
      data: {
         questionId: questionId,
         iIntervalStart: window.app.dateHelper.transformStartDate(window.app.startDate),
         iIntervalEnd: window.app.dateHelper.transformEndDate(window.app.endDate),
         checkAdditionalInfo: true,
         tags: window.app.tags
      },
      url: "/Reports/GetWordCloud",
      dataType: 'html',
      traditional: true,
      async: true,
      cache: false,
      success: function (wordCloudData) {
         $("#textCloudSectionAdditionalInfo" + questionId).replaceWith(wordCloudData);
      }
   });
};
window.app.displayReportsForFreeTextQ = function (questionId) {
   $.ajax({
      data: {
         questionId: questionId,
         iIntervalStart: window.app.dateHelper.transformStartDate(window.app.startDate),
         iIntervalEnd: window.app.dateHelper.transformEndDate(window.app.endDate),
         checkAdditionalInfo: false,
         tags: window.app.tags
      },
      url: "/Reports/GetWordCloud",
      dataType: 'html',
      traditional: true,
      async: true,
      cache: false,
      success: function (wordCloudData) {
         $("#textCloudSection" + questionId).replaceWith(wordCloudData);
      }
   });
};

window.app.displayReportOverview = function (surveyPlanId) {
   //DA we should avoid recreating the chart as this is a costly operation
   var chartId = "#pieOverviewChart_div" + surveyPlanId;
   var chartElem = $(chartId);
   var loadingIndicatorId = "#loadingOverviewIndicator" + surveyPlanId;
   var loadingIndicator = $(loadingIndicatorId);
   var pieChart = window.report.repOverviewPiecharts[surveyPlanId];
   if (pieChart == undefined) {
      pieChart = new google.visualization.PieChart(chartElem[0]);
      google.visualization.events.addListener(pieChart, 'select', function (e) {
         var sel = pieChart.getSelection();
         var selectedValue = window.app.overviewPieData[surveyPlanId].getValue(sel[0].row, 0);
         var url = "/Answer/GetCustomerWhichAnsweredXQuestions?surveyId=" + surveyPlanId + "&nrOfAnsweredQuestions=" + selectedValue;
         var win = window.open(url, "_blank");
         win.focus();
      });
   }
   

   var graphsContainer = $("#graphsOverviewContainer" + surveyPlanId);
   var candidateHeight = graphsContainer.outerHeight();
   candidateHeight = candidateHeight != 0 ? candidateHeight + "px" : "360px";
   loadingIndicator.height(candidateHeight);

   graphsContainer.height(candidateHeight);
   loadingIndicator.width(chartElem.outerWidth());
   loadingIndicator.css("line-height", candidateHeight);
   loadingIndicator.show();   

   $.ajax({
      data: {
         surveyPlanId: surveyPlanId,
         iIntervalStart: window.app.dateHelper.transformStartDate(window.app.startDate),
         iIntervalEnd: window.app.dateHelper.transformEndDate(window.app.endDate),
         tags: window.app.tags
      },
      url: "/Reports/GetSurveyOverview",
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
         var piedata = new google.visualization.DataTable(jsonData.pie);
         window.app.overviewPieData[surveyPlanId] = piedata;         
         loadingIndicator.hide();
         pieChart.draw(piedata, options);
        
         var tablechart = new google.visualization.Table(document.getElementById('tableOverviewChart_div' + surveyPlanId));
         tabledata = new google.visualization.DataTable(jsonData.table);
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
   $('input[qid]').each(function (index) {
      if ($(this).attr('qtype') === "Rating" || $(this).attr('qtype') === "YesNo" || $(this).attr('qtype') === "SelectOneFromMany" ) {
         window.app.displayReportsForRatingQ($(this).attr('qid'), $(this).attr('qtype'));
      } else if ($(this).attr('qtype') === "FreeText") {
         window.app.displayReportsForFreeTextQ($(this).attr('qid'));
      }
   });
}
$(document).ready(function () {
   window.filterArea.initialize();
   $("#refreshReport").click(function () {
      window.app.runrunrun();
   });
 
   window.app.runrunrun();
});