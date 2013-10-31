/**
 * LogOnPage view is mapped on #logOnPage DOM element.
 * In case a user is logged in the system this page is skipped.
 * This behavior is implemented in checkLoggedOnStatus. 
 * TODO: Move checkLoggedOnStatus logic in LogOnPage model.
 */
var LogOnPage = Backbone.View.extend({
	initialize: function () {
		_.bindAll(this, "logOn", "loggedOnStatus",
				"hide", "clearFields", "close");
		var self = this;
		this.domElements = {
				$INPUT_USERNAME : $("#username", this.$el),
				$INPUT_PASSWORD : $("#password", this.$el),
				$INPUT_SUBMIT: $("#logOnBtn", this.$el),
				$FORM: $("#logOnForm", this.$el)
		};
		this.listenTo(this.model, this.model.events.LOGON_SUCCESS,
				this.clearFields);	
		// Check if the user is logged on
		this.model.isLoggedOn(this.loggedOnStatus)
		this.logOnButton = new google.ui.FastButton($("#logOnBtn", this.$el)[0], this.logOn);		
	},
	logOn: function(event) {
		event.preventDefault();
		this.model.logOn(this.domElements.$INPUT_USERNAME.val(),
				this.domElements.$INPUT_PASSWORD.val(),
				this.domElements.$FORM.prop("action"),
				this.domElements.$FORM.prop("method"));		
	},
	loggedOnStatus: function(data, a, b) {
		if (data == "success") {
			this.model.registerDevice();
		} else {
			this.show();
		}
	},
	hide: function() {
		this.$el.css("visibility", "hidden");
	},
	show: function() {
		this.$el.css("visibility", "visible");
	},
	clearFields: function() {
		this.domElements.$INPUT_USERNAME.val("");
		this.domElements.$INPUT_PASSWORD.val("");
	},
	close: function() {		
		this.logOnButton.reset();
		this.remove();
	}
});

/**
 * This class implements logic for:
 * 1. log on/log off
 * 2. check for logged in user
 * 3. get xmpp credentials for logged user
 * 4. register/unregister device GCM id in the system
 */
var LogOnModel = Backbone.Model.extend({
	initialize: function(attributes, options) {
		this.response = {
			LOGON_SUCCESS : "success",
			LOGOFF_SUCCESS : "success",
			REGISTER_SUCCESS : "addDevice success",
			REGISTER_FAIL : "addDevice fail",
			UNREGISTER_SUCCESS: "removeDevice success",
			UNREGISTER_FAIL: "removeDevice fail"
		}		
		this.events = {
				LOGON_SUCCESS : "logon_success_event",
				LOGOFF_SUCCESS : "logoff_success_event"
		};
		//var domain = "http://localhost:3288";
		var domain = "http://dev.txtfeedback.net";
		//this.xmppHandler = options.xmppHandler;
		this.pushNotificationHandler = options.pushNotificationHandler;
		this.url = {
			xmppCredentials: domain + "/Xmpp/GetConnectionDetailsForLoggedInUser",
			checkLoggedOnStatus : domain + "/SurveyTemplate/IsUserConnected",
			logOn: domain + "/Account/AjaxLogOn",
			logOff : domain + "/Account/AjaxLogOff",
			registerDevice : domain + "/Devices/AddDevice",
			unregisterDevice : domain + "/Devices/ReleaseDeviceFromCompany"
		}		
	},
	logOn: function(username, password, action, method) {
		/*$("#loading-modal").modal("show");
		$("#loading-modal").css("visibility", "visible");*/
		loader.showLoader();
		if (this.pushNotificationHandler.getGCMKey() != null && 
				this.pushNotificationHandler.getGCMKey() != "null") {
			var self = this;
			var credentials= {};
			credentials.UserName = username;
			credentials.Password = password;
			$.ajax({
				data: "{model:" + JSON.stringify(credentials) + "}",
				url: this.url.logOn,
				xhrFields: {
					withCredentials: true
				},
				crossDomain:true,
				type: "POST",
				contentType: "application/json; charset=utf-8",
				success: function(data, textStatus, jqXHR) {	
					if (data === self.response.LOGON_SUCCESS) {						
						self.registerDevice();						
					} else {
						/*$("#loading-modal").modal("hide");*/
						loader.hideLoader();
						alert("Invalid credentials");						
					}  				
				}, 
				error: function(jqXHR, textStatus, errorThrown) {
					/*$("#loading-modal").modal("hide");*/
					loader.hideLoader();
					alert("Network error. Please check your " +
							"internet connection and try again later.");
				}    		
			});
		} else {
			alert("Application busy. Try again in few moments.")
		}
	},
	logOff: function() {
		var self = this;
		/*this.unregisterDevice(
				this.pushNotificationHandler.getGCMKey());*/
		$.ajax({
			url: this.url.logOff,
			type: "POST",
			cache: false,
			xhrFields: {
				withCredentials: true
			},
			crossDomain: true,
			async: false,
			success: function(data) {
				if (data == self.response.LOGOFF_SUCCESS) {
					//self.trigger(self.eavents.LOGOFF_SUCCESS);
				}
			},
			error: function(jqXHR, textStatus, errorThrown) {
				alert("Network error. Please check your " +
						"internet connection and try again later.");
			}
		});
	},
	isLoggedOn: function(callback) {
		var loggedOn = false;		
		$.ajax({
			url: this.url.checkLoggedOnStatus,
			method: "GET",
			xhrFields: {
				withCredentials: true
			},
			crossDomain: true,
			success: callback			
		});
		
	},
	registerDevice: function() {
		var deviceId = this.pushNotificationHandler.getGCMKey()
		var self = this;
		$.ajax({
			url: this.url.registerDevice,
			type: "POST",
			data: {
				deviceId: deviceId
			},
			xhrFields: {
				withCredentials: true
			},
			/*async: false,*/
			crossDomain: true,
			/*contentType: "application/json; charset=utf-8",*/
			success: function(data) {
				if (data == self.response.REGISTER_SUCCESS) {
					self.trigger(self.events.LOGON_SUCCESS);
					return true;
				} else if (data == self.response.REGISTER_FAIL) {
					alert("register failed");
					return false;
				}
			},
			error: function(jqXHR, textStatus, errorThrown) {
				alert("Network error. Please check your " +
						"internet connection and try again later.");
				return false;
			}
		});
	},
	unregisterDevice: function(deviceId) {
		var self = this;
		$.ajax({
			url: this.url.unregisterDevice,
			type: "POST",
			data: {
				deviceId: deviceId
			},
			xhrFields: {
				withCredentials: true
			},
			async: false,
			crossDomain: true,
			success: function(data) {
				if (data == self.response.UNREGISTER_SUCCESS) {
					return true;
				} else if (data == self.response.UNREGISTER_FAIL) {
					return false;
				}
			}
		});
	}
});