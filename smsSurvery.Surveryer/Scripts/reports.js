window.app = window.app || {};
window.app.piedata = {};
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
            var selectedValue = window.app.piedata[questionId].getValue(sel[0].row,0);
            var url = "http://localhost:3288/Answer/GetRatingMessagesWithAnswer?questionId=" + questionId + "&answer=" + selectedValue;
            //window.location.href = url;
            window.open(url, "_blank");
            //$.ajax({
            //   data: {
            //      questionId: questionId,
            //      answer: selectedValue
            //   },
            //   url: "/Answer/GetRatingMessagesWithAnswer",
            //   dataType: 'html',
            //   async: false,
            //   cache: false,
            //   success: function (drilldowndata) {

            //   }
            //});
            //console.log(piechart.pd);
            //alert('selected: ' + sel[0].row);
         });
         var tablechart = new google.visualization.Table(document.getElementById('tableChart_div' + questionId));
         tabledata = new google.visualization.DataTable(jsonData.table);
         var tableOptions = {
            backgroundColor: '#F5F8FA',
            sliceVisibilityThreshold: 0,
            cssClassNames: { tableCell: "tCell" }
         };
         tablechart.draw(tabledata, tableOptions);
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