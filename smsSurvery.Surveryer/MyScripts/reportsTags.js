window.app = window.app || {};
window.app.piedata = {};
window.app.overviewPieData = {};



window.app.displayComparisonReportsForQuestion = function (questionId) {
   var chartId = '#barChart_div' + questionId;
   var chartElem = $(chartId);
   var barchart = new google.visualization.ComboChart(chartElem[0]);
   //show the loading indicator in the space of the chart
   var loadingIndicatorId = "#loadingIndicator" + questionId;
   var loadingIndicator = $(loadingIndicatorId);
   var candidateHeight = chartElem.outerHeight();
   candidateHeight = candidateHeight != 0 ? candidateHeight +"px" : "200px";
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
      url: "/Reports/GetTagComparisonReportForFiniteAnswersQuestion",
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
            animation: {
               duration: 2000,
               easing: 'out'
            }
         };
         var bardata = new google.visualization.DataTable(jsonData);
         loadingIndicator.hide();
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
               window.app.displayComparisonReportsForQuestion($(this).attr('qid'));
         });         
         
         $("#noTags-error").fadeOut('fast');
      } else {
         $("#noTags-error").fadeIn('slow');
      }
   }
}

$(document).ready(function () {  
   window.filterArea.initialize();
   $("#refreshReport").click(function () {
      $(".graphsContainer label").each(function (index, item) { $(item).hide() });
      window.app.runrunrun(false);
   });
   window.app.runrunrun(true);
});