window.app = window.app || {};
window.app.piedata = {};
window.app.yesnopiedata = {};
window.app.overviewPieData = {};
window.app.tags = [];
window.app.displayReportsForRatingQ = function (questionId) {
   var loadingIndicatorId = "#loadingIndicator" + questionId;
   var reportChartID = "#pieChart_div" + questionId;
   //var loadedReportChart = $(reportChartID)[0];
   var piechart = new google.visualization.PieChart($(reportChartID)[0]);
   google.visualization.events.addListener(piechart, 'select', function (e) {
      var sel = piechart.getSelection();
      var selectedValue = window.app.piedata[questionId].getValue(sel[0].row, 0);
      var url = "/Answer/GetMessagesWithOnePredefinedAnswer?questionId=" + questionId + "&answer=" + selectedValue;
      //window.location.href = url;
      var win = window.open(url, "_blank");
      win.focus();
   });
   //$(loadingIndicatorId).show();
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
         //$(loadingIndicatorId).hide();
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
         piechart.draw(piedata, options);
         
         var tablechart = new google.visualization.Table(document.getElementById('tableChart_div' + questionId));
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

window.app.displayReportsForFreeTextQ = function (questionId) {
   $.ajax({
      data: {
         questionId: questionId,
         iIntervalStart: window.app.dateHelper.transformStartDate(window.app.startDate),
         iIntervalEnd: window.app.dateHelper.transformEndDate(window.app.endDate),
         tags: window.app.tags
      },
      url: "/Reports/GetWordCloud",
      dataType: 'html',
      traditional: true,
      async: false,
      cache: false,
      success: function (wordCloudData) {
         $("#textCloudSection" + questionId).replaceWith(wordCloudData);
      }
   });
};

window.app.displayReportOverview = function (surveyPlanId) {
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
         var piechart = new google.visualization.PieChart(document.getElementById('pieOverviewChart_div' + surveyPlanId));
         var piedata = new google.visualization.DataTable(jsonData.pie);
         window.app.overviewPieData[surveyPlanId] = piedata;
         //window.app.piedata[questionId] = piedata;
         piechart.draw(piedata, options);
         google.visualization.events.addListener(piechart, 'select', function (e) {
            var sel = piechart.getSelection();
            var selectedValue = window.app.overviewPieData[surveyPlanId].getValue(sel[0].row, 0);
            var url = "/Answer/GetCustomerWhichAnsweredXQuestions?surveyId=" + surveyPlanId + "&nrOfAnsweredQuestions=" + selectedValue;            
            var win = window.open(url, "_blank");
            win.focus();
         });
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
         window.app.displayReportsForRatingQ($(this).attr('qid'));
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