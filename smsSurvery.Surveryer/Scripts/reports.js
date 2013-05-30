window.app = window.app || {};


window.app.displayReportsForRatingQ = function (questionId) {
   $.ajax({
      data: { questionId: questionId },
      url: "/Reports/GetSurveyQuestionResults",
      dataType: "json",
      async: false,
      success: function (jsonData) {
         var options = {
            backgroundColor: '#F5F8FA',
            sliceVisibilityThreshold: 0
         };
         var piechart = new google.visualization.PieChart(document.getElementById('pieChart_div' + questionId));
         data = new google.visualization.DataTable(jsonData.pie);
         piechart.draw(data, options);
         var tablechart = new google.visualization.Table(document.getElementById('tableChart_div' + questionId));
         data = new google.visualization.DataTable(jsonData.table);
         var tableOptions = {
            backgroundColor: '#F5F8FA',
            sliceVisibilityThreshold: 0,
            cssClassNames: { tableCell: "tCell" }
         };
         tablechart.draw(data, tableOptions);
         //var barchart = new google.visualization.ColumnChart(document.getElementById('barChart_div'));
         //barchart.draw(data, options);
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

$(document).ready(function () {
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