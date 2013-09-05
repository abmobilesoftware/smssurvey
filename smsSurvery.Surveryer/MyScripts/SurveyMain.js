$(document).ready(function () {
   var urlParts = document.URL.split("/");
   var urlLastPart = urlParts[urlParts.length - 1];
   var surveyId = "";
   var displayInfoTable = true;
   if (urlLastPart == "Create") {
      surveyId = SurveyUtilities.Utilities.CONSTANTS_MISC.NEW_SURVEY;
   } else {
      surveyId = urlLastPart;
      displayInfoTable = false;
   }
   var surveyModel = new SurveyBuilder.SurveyModel({
      Id: surveyId,
      Description: "",
      IntroMessage: "",
      ThankYouMessage: "",
      StartDate: "15/6/2013",
      EndDate: "17/7/2013",
      IsRunning: false,
      DisplayInfoTable: displayInfoTable,
      DefaultLanguage: "en-US"
   });
   var survey = new SurveyBuilder.SurveyView({
      el: $("#survey"),
      model: surveyModel
   });
});
