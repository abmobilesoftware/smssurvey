//#region Defines to stop jshint from complaining about "undefined objects"
/*global window */
/*global Strophe */
/*global document */
/*global console */
/*global $pres */
/*global $iq */
/*global $msg */
/*global Persist */
/*global DOMParser */
/*global ActiveXObject */
/*global Backbone */
/*global _ */
/*global Spinner */
//#endregion
var DateHelper = function () {
   // in format yyyy-mm-dd H:mm:ss   
   this.transformStartDate = function (date) {
      var curr_date = date.getDate();
      curr_date = (curr_date < 10) ? "0" + curr_date : curr_date;
      var curr_month = date.getMonth() + 1; //Months are zero based
      curr_month = (curr_month < 10) ? "0" + curr_month : curr_month;
      var curr_year = date.getFullYear();
      return curr_year + "-" + curr_month + "-" + curr_date + " 0:00:00";
   };
   this.transformEndDate = function (date) {
      var curr_date = date.getDate();
      curr_date = (curr_date < 10) ? "0" + curr_date : curr_date;
      var curr_month = date.getMonth() + 1; //Months are zero based
      curr_month = (curr_month < 10) ? "0" + curr_month : curr_month;
      var curr_year = date.getFullYear();
      return curr_year + "-" + curr_month + "-" + curr_date + " 23:59:59";
   };

   this.transformDateToLocal = function (date) {
      var displayPattern = 'DD, MM d, yy';
      if (window.app.calendarCulture === "ro") { displayPattern = 'DD, d MM, yy'; }
      var dateLocal = $.datepicker.formatDate(displayPattern, date,
                                  {
                                     dayNamesShort: $.datepicker.regional[window.app.calendarCulture].dayNamesShort, dayNames: $.datepicker.regional[window.app.calendarCulture].dayNames,
                                     monthNamesShort: $.datepicker.regional[window.app.calendarCulture].monthNamesShort, monthNames: $.datepicker.regional[window.app.calendarCulture].monthNames
                                  });
      return dateLocal;
   };
};