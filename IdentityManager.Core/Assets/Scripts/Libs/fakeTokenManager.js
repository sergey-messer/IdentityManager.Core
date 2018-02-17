function FakeTokenManager(settings) {
    Object.defineProperty(this, "expired", {
        get: function () {
            return false;
        }
    });




}

FakeTokenManager.prototype.redirectForToken = function (cb) {

};

FakeTokenManager.prototype.addOnTokenRemoved = function (cb) {
};



FakeTokenManager.prototype.addOnTokenObtained = function (cb) {
};
FakeTokenManager.prototype.processTokenCallbackAsync = function (cb) {
};
FakeTokenManager.prototype.addOnTokenExpired = function (cb) {
};