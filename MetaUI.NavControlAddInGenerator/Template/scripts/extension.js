var repeaters = window.parent.document.getElementsByClassName('ms-nav-grid-container');
if (repeaters.length) {
	Array.prototype.forEach.call(repeaters, function(rp) {
		rp.style.display = 'none';
	});
}

function Initialize(
	settingsHostUrl,
	controlId,
	userName,
	accessKey,
	userSecurityId,
	companyId) {
	window.controlId = controlId;

	var body = document.getElementsByTagName('body')[0];
	body.innerHTML =
		'<meta-ui-grid id="' + controlId +
		'" cid="' + userSecurityId +
		'"></meta-ui-grid>';

	if (!accessKey) {
		document.body.innerHTML = '<div style="width:395px;position:absolute;left:50%;top:50%;transform: translate(-50%, -50%);" class="ui-card ui-widget ui-widget-content ui-corner-all"><div class="ui-card-body"><div class="ui-card-title ng-star-inserted">Web Service Access Key is required</div><div class="ui-card-content"><div style="line-height:1.5">You do not have web access key. Please ask your administrator to generate one for you.</div></div></div>'
		return;
	}

	companyId = companyId.substring(1);
	companyId = companyId.substring(0, companyId.length - 1);
	var metaUIUser = {
		companyId: companyId,
		userSecurityId: userSecurityId,
		userName: userName,
		accessKey: accessKey,
		userToken: btoa(userName + ':' + accessKey)
	};

	window.metaUIUser = metaUIUser;
	window.userToken = metaUIUser.userToken;

	var config = {
		settingsHostUrl: settingsHostUrl,
		controlId: controlId,
		userInfo: metaUIUser
	}

	window.initExtension(config);
}

function SetFilters(filtersString) {
	console.log(filtersString);
	window.metaUI.controls
		.get(window.controlId).dataSource.onSetFilters
		.next({ filterString: filtersString, forceReload: true });
}

function SetOrder(orderByString) {
	window.metaUI.controls.get(window.controlId).dataSource.onSetOrder.next({ orderByString: orderByString });
}


function CheckUserSecurityId() {
	// check if user is not the same and clearing local storage settings
	if (window.metaUIUser.userSecurityId && window.metaUIUser.userSecurityId !== window.localStorage.getItem('userSecurityId')) {
		window.localStorage.removeItem('userSecurityId');
		window.localStorage.removeItem('userSecurityId setitem');
		window.localStorage.setItem('userSecurityId', window.metaUIUser.userSecurityId);
	}
}

