window.app = window.app || {};
window.report = window.report || {};
window.report.repOverviewPiecharts = {};
window.report.repPieCharts = {};
window.app.piedata = {};
window.app.bardata = {};
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
         if (qType == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_RATING
            || qType == SurveyUtilities.Utilities.CONSTANTS_QUESTION.TYPE_NUMERIC) {
            window.app.displayReportForRatingAdditionalInfo(questionId);
         }
      }
   });
   //for rating question we should also show an word cloud for Additional Info
};

window.app.displayReportForManyFromManyQ = function (questionId) {
   var chartId = '#barChart_div' + questionId;
   var chartElem = $(chartId);
   var barchart = new google.visualization.ComboChart(chartElem[0]);
   google.visualization.events.addListener(barchart, 'select', function (e) {
      var sel = barchart.getSelection();
      var selectedValue = window.app.bardata[questionId].getColumnId(sel[0].column);
      var url = "/Answer/GetManyFromManyResultsWithOnePredefinedAnswer?questionId=" + questionId + "&answer=" + selectedValue;
      var win = window.open(url, "_blank");
      win.focus();
   });
   //show the loading indicator in the space of the chart
   var loadingIndicatorId = "#loadingIndicator" + questionId;
   var loadingIndicator = $(loadingIndicatorId);
   var candidateHeight = chartElem.outerHeight();
   candidateHeight = candidateHeight != 0 ? candidateHeight + "px" : "200px";
   loadingIndicator.height(candidateHeight);
   var graphsContainer = $("#graphsContainer" + questionId);
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
      url: "/Reports/GetReportForManyFromManyQuestion",
      dataType: "json",
      async: true,
      success: function (jsonData) {
         var options = {
            backgroundColor: '#F5F8FA',
            'width': 'auto',
            vAxis: {
               title: "Percentage from total answers",
               format: '##,##%',
               baseline: 0
            },
            hAxis: { title: "" },
            seriesType: "bars",
            animation: {
               duration: 2000,
               easing: 'out'
            }
         };
         var bardata = new google.visualization.DataTable(jsonData);
         window.app.bardata[questionId] = bardata;
         loadingIndicator.hide();
         barchart.draw(bardata, options);
      }
   });
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

window.app.exportRawDataToExcel = function (surveyTemplateId) {
   var intervalStart = window.app.dateHelper.transformStartDate(window.app.startDate);
   var iIntervalEnd = window.app.dateHelper.transformEndDate(window.app.endDate);
   window.location = encodeURI("/Reports/GetActivityReport?surveyTemplateId=" + surveyTemplateId + "&iIntervalStart=" + intervalStart + "&iIntervalEnd=" + iIntervalEnd);
}

window.app.displayReportOverview = function (surveyTemplateId) {
   //DA we should avoid recreating the chart as this is a costly operation
   var chartId = "#pieOverviewChart_div" + surveyTemplateId;
   var chartElem = $(chartId);
   var loadingIndicatorId = "#loadingOverviewIndicator" + surveyTemplateId;
   var loadingIndicator = $(loadingIndicatorId);
   var pieChart = window.report.repOverviewPiecharts[surveyTemplateId];
   if (pieChart == undefined) {
      pieChart = new google.visualization.PieChart(chartElem[0]);
      google.visualization.events.addListener(pieChart, 'select', function (e) {
         var sel = pieChart.getSelection();
         var selectedValue = window.app.overviewPieData[surveyTemplateId].getValue(sel[0].row, 0);
         var url = "/Answer/GetCustomerWhichAnsweredXQuestions?surveyId=" + surveyTemplateId + "&nrOfAnsweredQuestions=" + selectedValue;
         var win = window.open(url, "_blank");
         win.focus();
      });
   }
   

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
         window.app.overviewPieData[surveyTemplateId] = piedata;
         loadingIndicator.hide();
         pieChart.draw(piedata, options);
        
         var tablechart = new google.visualization.Table(document.getElementById('tableOverviewChart_div' + surveyTemplateId));
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
      if ($(this).attr('qtype') === "Rating" || $(this).attr('qtype') === "YesNo" || $(this).attr('qtype') === "SelectOneFromMany"
         || $(this).attr('qtype') === "Numeric") {
         window.app.displayReportsForRatingQ($(this).attr('qid'), $(this).attr('qtype'));
      } else if ($(this).attr('qtype') === "FreeText") {
         window.app.displayReportsForFreeTextQ($(this).attr('qid'));
      } else if ($(this).attr('qtype') === "SelectManyFromMany") {
         window.app.displayReportForManyFromManyQ($(this).attr('qid'));
      }
   });
}
$(document).ready(function () {
   window.filterArea.initialize();
   $("#refreshReport").click(function () {
      window.app.runrunrun();
   });
   $("#exportToExcel").click(function () {
      window.app.exportRawDataToExcel($("#surveyId").text());
   });
 
   window.app.runrunrun();
});