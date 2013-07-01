window.app = window.app || {};
window.app.piedata = {};
window.app.overviewPieData = {};
window.app.displayReportsForRatingQ = function (questionId) {
   $.ajax({
      data: { questionId: questionId },
      url: "/Reports/GetSurveyQuestionResults",
      dataType: "json",
      async: false,
      success: function (jsonData) {
         var options = {
            backgroundColor: '#F5F8FA',
            sliceVisibilityThreshold: 0,
            'width': 'auto',
            'height': 350
         };
         var piechart = new google.visualization.PieChart(document.getElementById('pieChart_div' + questionId));
         var piedata = new google.visualization.DataTable(jsonData.pie);
         window.app.piedata[questionId] = piedata;
         piechart.draw(piedata, options);
         google.visualization.events.addListener(piechart, 'select', function (e) {
            var sel = piechart.getSelection();            
            var selectedValue = window.app.piedata[questionId].getValue(sel[0].row, 0);            
            var url = "/Answer/GetRatingMessagesWithAnswer?questionId=" + questionId + "&answer=" + selectedValue;
            //window.location.href = url;
            var win = window.open(url, "_blank");
            win.focus();
         });
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
      data: { questionId: questionId },
      url: "/Reports/GetWordCloud",
      dataType: 'html',
      async: true,
      cache: false,
      success: function (wordCloudData) {
         $("#textCloudSection" + questionId).replaceWith(wordCloudData);
      }
   });
};

window.app.displayReportOverview = function (surveyPlanId) {
   $.ajax({
      data: { surveyPlanId: surveyPlanId },
      url: "/Reports/GetSurveyOverview",
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

$(document).ready(function () {
   window.app.displayReportOverview($("#surveyId").text());
   //based on the type of the question get the report
   //for each question, show the outcome (if required)
   $('input[qid]').each(function (index) {
      if ($(this).attr('qtype') === "Rating") {
         window.app.displayReportsForRatingQ($(this).attr('qid'));
      } else if ($(this).attr('qtype') === "FreeText") {
         window.app.displayReportsForFreeTextQ($(this).attr('qid'));
      }
   });      
});