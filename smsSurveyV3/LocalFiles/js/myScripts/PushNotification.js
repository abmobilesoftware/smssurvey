var PushNotificationHandler = Backbone.Model.extend({
	initialize: function() {
		_.bindAll(this, 
				"pushNotificationListener",
				"pushNotificationRegisterSuccess",
				"pushNotificationRegisterError",
				"getGCMKey");
		this.storage = window.localStorage;
		this.pushNotificationManager = new PushNotificationManager();
		// GCM project id
		this.pushNotificationManager.accountID("92320531700");
		if (this.storage.getItem("GCMKey") == null) {
			this.pushNotificationManager.register(
					this.pushNotificationRegisterSuccess,
					this.pushNotificationRegisterError);
		} else {
			this.pushNotificationManager.listener(
					this.pushNotificationListener);
		}
	},
	pushNotificationListener: function(notification) {
	},
	pushNotificationRegisterSuccess: function(token) {
		alert(token);
		this.storage.setItem("GCMKey", token);
	},
	pushNotificationRegisterError: function() {
		alert("PushNotification register error");
	},
	getGCMKey: function() {
		return this.storage.getItem("GCMKey");
		//return "test pe chrome";
	}
});