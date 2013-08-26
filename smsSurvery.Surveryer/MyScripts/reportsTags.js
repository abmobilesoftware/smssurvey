window.app = window.app || {};
window.app.piedata = {};
window.app.overviewPieData = {};
window.app.tags = [];
window.app.displayComparisonReportsForRatingQ = function (questionId) {
   $.ajax({
      data: {
         questionId: questionId,
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
   $('.alert .close').on("click", function (e) {
      $(this).parent().hide();
   });

   $("#refreshReport").click(function () {
      window.app.runrunrun(false);
   });

   var tagsInput = $("#filterTag").tagsInput({
      'height': '22px',
      'width': 'auto',
      'autocomplete_url': "/Reports/FindMatchingLocationTags",
      'onAddTag': function (tagValue) {
         var delimiter = ',';
         window.app.tags = $("#filterTag").val().split(delimiter);

      },
      'onRemoveTag': function (tagValue) {
         var delimiter = ',';
         window.app.tags = $("#filterTag").val().split(delimiter);
         if ("" === window.app.tags[0]) {
            window.app.tags = [];
         }
      },
      'defaultText': 'add location tag here',
      'placeholder': 'add location tag here',
      'interactive': true,
      'placeholderColor': '#666666',
      'minChars': 3,
      'maxChars': 10,
      'autocomplete': {
         autoFocus: true,
         minLength: 1
      }
   });  


   window.app.runrunrun(true);
});