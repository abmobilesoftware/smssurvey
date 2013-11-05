window.app = window.app || {};
window.filterArea = window.filterArea || {};

window.filterArea.initialize = function (hideTagFilterArea) {
   $('.alert .close').on("click", function (e) {
      $(this).parent().hide();
   });
   if (hideTagFilterArea) {
      $("#filterTag").hide();
   }
   else {
      window.filterArea.initializeLocationFilter();
   }
   window.filterArea.initializeDateFilter();
}
window.app.dateFormatForDatePicker = 'dd/mm/yy';
window.app.dateHelper = new DateHelper();
window.filterArea.initializeDateFilter = function () {
   var startDate = new Date();
   var endDate = new Date();
   startDate.setDate(endDate.getDate() - 31);
   window.app.startDate = startDate;
   window.app.endDate = endDate;
   var fromDatePicker = $("#from");
   // DA first set the default value using the same function as for DatePicker in In Store Customer Feedback
   var startDay = (window.app.startDate.getDate() < 10) ? "0" + window.app.startDate.getDate() : window.app.startDate.getDate();
   var startMonth = (window.app.startDate.getMonth() + 1 < 10) ? "0" + (window.app.startDate.getMonth() + 1) : window.app.startDate.getMonth() + 1;
   var startYear = window.app.startDate.getFullYear();
   var startDateString = startDay + "/" + startMonth + "/" + startYear;
   fromDatePicker.val(startDateString);

   var toDatePicker = $("#to");
   var endDay = (window.app.endDate.getDate() < 10) ? "0" + window.app.endDate.getDate() : window.app.endDate.getDate();
   var endMonth = (window.app.endDate.getMonth() + 1 < 10) ? "0" + (window.app.endDate.getMonth() + 1) : window.app.endDate.getMonth() + 1;
   var endYear = window.app.endDate.getFullYear();
   var endDateString = endDay + "/" + endMonth + "/" + endYear;
   toDatePicker.val(endDateString);

   // Setup the calendar
   fromDatePicker.datepicker({
      defaultDate: "+1w",
      changeMonth: true,
      numberOfMonths: 3,
      dateFormat: window.app.dateFormatForDatePicker,
      maxDate: window.app.endDate,
      onSelect: function (selectedDate) {
         window.app.newStartDate = fromDatePicker.datepicker("getDate");
         if (window.app.newStartDate !== window.app.startDate) {
            window.app.startDate = window.app.newStartDate;            
            var fromDateString = $.datepicker.formatDate(window.app.dateFormatForDatePicker, window.app.startDate);            
            toDatePicker.datepicker("option", "minDate", window.app.startDate);
         }
      }
   });

 
   toDatePicker.datepicker({
      defaultDate: "+1w",
      changeMonth: true,
      numberOfMonths: 3,
      dateFormat: window.app.dateFormatForDatePicker,
      minDate: window.app.startDate,
      onSelect: function (selectedDate) {
         window.app.newEndDate = toDatePicker.datepicker("getDate");
         if (window.app.newEndDate !== window.app.endDate) {            
            window.app.endDate = window.app.newEndDate;
            var endDateString = $.datepicker.formatDate(window.app.dateFormatForDatePicker, window.app.endDate);            
            fromDatePicker.datepicker("option", "maxDate", window.app.endDate);
         }
      }
   });
}
window.app.tags = [];
window.filterArea.initializeLocationFilter = function () {
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
}