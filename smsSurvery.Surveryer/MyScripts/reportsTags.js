window.app = window.app || {};
window.app.piedata = {};
window.app.overviewPieData = {};



window.app.displayComparisonReportsForRatingQ = function (questionId) {
   $.ajax({
      data: {
         questionId: questionId,
         iIntervalStart: window.app.dateHelper.transformStartDate(window.app.startDate),
         iIntervalEnd: window.app.dateHelper.transformEndDate(window.app.endDate),
         tags: window.app.tags
      },
      traditional: true,
      url: "/Reports/GetTagComparisonReportForRatingQuestion",
      dataType: "json",
      async: true,
      success: function (jsonData) {
         var options = {
            backgroundColor: '#F5F8FA',
            'width': 'auto',
            vAxis: {
               title: "Percentage of answers",
               format: '##,##%',
               baseline: -1
            },
            hAxis: { title: "" },
            seriesType: "bars",
            animation: { duration: 2, easing: "out" }
         };

         var barchart = new google.visualization.ComboChart(document.getElementById('barChart_div' + questionId));
         var bardata = new google.visualization.DataTable(jsonData);
         barchart.draw(bardata, options);
      }
   });
};

window.app.runrunrun = function (firstTime) {
   if (firstTime) {
      $("#wrapperForTags").popover({
         placement: 'bottom',
         trigger: 'click'
      });
      $("#wrapperForTags").popover('show');
   } else {
      var canGenerateReport = window.app.tags.length >= 2;
      if (canGenerateReport) {
         $('div[qid]').each(function (index) {
            if ($(this).attr('qtype') === "Rating") {
               window.app.displayComparisonReportsForRatingQ($(this).attr('qid'));
            }
         });
         //var questionId = 1;
         
         $("#noTags-error").fadeOut('fast');
      } else {
         $("#noTags-error").fadeIn('slow');
      }
   }
}

$(document).ready(function () {  
   window.filterArea.initialize();
   $("#refreshReport").click(function () {
      window.app.runrunrun(false);
   });
   window.app.runrunrun(true);
});