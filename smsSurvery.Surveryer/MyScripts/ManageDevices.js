(function () {
   /* Translation */
   var cAlertCommandFailed;
   var cAlertSetSurveySuccess;
   var cAlertRefreshSurveySuccess;
   $(document).ready(function () {
      cAlertCommandFailed = $("#alert-command-failed").val();
      cAlertSetSurveySuccess = $("#alert-set-survey-success").val();
      cAlertRefreshSurveySuccess = $("#alert-refresh-survey-success").val();
   });

   /* Alert blinking */
   var noOfBlinks = 3;
   var blinkTimer;
   var cBlinkInterval = 500;

   function startBlinking() {
      noOfBlinks = 3;
      blinkTimer = setInterval(blinking, cBlinkInterval);
   }
   function blinking() {
      if (noOfBlinks > 0) {
         if ($(".alert").hasClass("alert-visible")) {
            $(".alert").removeClass("alert-visible");
            $(".alert").addClass("alert-invisible");
         } else {
            $(".alert").addClass("alert-visible");
            $(".alert").removeClass("alert-invisible");
            noOfBlinks = noOfBlinks - 1;
         }
      } else {
         clearInterval(blinkTimer);
      }
   }

   $(".survey-link-select").on("change", function () {
      var newLink = $(this).val();
      var deviceId = $(this).attr("deviceId");
      if (newLink != "") {
         $.ajax({
            url: "/Devices/SendLinkToDevice",
            type: "POST",
            data: {
               deviceId: deviceId,
               link: newLink
            },
            success: function (response) {
               $(".alert").hide();
               if (response == "success") {
                  $(".alert-success").html(cAlertSetSurveySuccess);
                  $(".alert-success").show();
                  startBlinking();
               } else {
                  $(".alert-danger").html(cAlertCommandFailed);
                  $(".alert-danger").show();
                  startBlinking();
               }
            }
         });
      } else {
         alert("The new link cannot be empty... I think");
      }
   });

   $(".refresh-survey").click(function (event) {
      event.preventDefault();
      var deviceId = $(event.target).attr("deviceId");
      $.ajax({
         url: "/Devices/SendRefreshCommandToDevice",
         type: "POST",
         data: {
            deviceId: deviceId           
         },
         success: function (response) {
            var responseObject = JSON.parse(response);
            $(".alert").hide();
            if (responseObject.success == 1) {
               $(".alert-success").html(cAlertRefreshSurveySuccess);
               $(".alert-success").show();
               startBlinking();
            } else if (responseObject.failure == 1) {
               $(".alert-danger").html(cAlertCommandFailed);
               $(".alert-danger").show();
               startBlinking();
            }
         }
      });
   });

   $(".release-device").click(function (event) {
      event.preventDefault();
      var deviceId = $(event.target).attr("deviceId");
      $.ajax({
         url: "/Devices/ReleaseDevice",
         type: "POST",
         data: {
            deviceId: deviceId
         },
         success: function (response) {
            if (response == "success") {
               location.reload();
            } else {
               alert(response);
            }
         }
      });
   });
})();