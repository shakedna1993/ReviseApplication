var mainModel = angular.module("mainModel", ["ngRoute", "ngAnimate", "ngMaterial"]);

mainModel.config(function ($mdAriaProvider) {
    $mdAriaProvider.disableWarnings();
});

mainModel.animation('.slide', [function () {
    return {
        enter: function (element, doneFn) {
            jQuery(element).fadeIn(1000, doneFn);
        },

        move: function (element, doneFn) {
            jQuery(element).fadeIn(1000, doneFn);
        },

        leave: function (element, doneFn) {
            jQuery(element).fadeOut(1000, doneFn);
        }
    }
}]);

mainModel.controller("mainController", function ($scope, $rootScope, $http, $location, $window) {

    $scope.registered = false;
    $rootScope.fontAwasomeUserName = "fa-sign-in";
    $rootScope.UserConnectionUrl = "#!login";
    $rootScope.UserConnectionText = "Login";

    $rootScope.checkUserStatus = function () {
        $http({
            method: "GET",
            url: "/Authentication/GetUserInfo"
        }).then(function (response) {
            $rootScope.username = response.data.fullname;
            if (response.data.fullname) {
                $rootScope.fontAwasomeUserName = "fa-sign-out";
                $rootScope.UserConnectionUrl = "#!logout";
                $rootScope.UserConnectionText = "Logout";
            } else {
                $rootScope.fontAwasomeUserName = "fa-sign-in";
                $rootScope.UserConnectionUrl = "#!login";
                $rootScope.UserConnectionText = "Login";
                $location.path("/login");
            }
        }, function myError(response) {
            $window.alert("Error: " + response.StatusText);
        });
    };

    $scope.login = function () {
        $http({
            data: {
                username: this.login_username,
                password: this.login_password
            },
            method: "POST",
            url: "/Authentication/Login"
        }).then(function mySuccess(response) {
            if (response.data.success) {
                $rootScope.username = response.data.username;
                $location.path("/project/show");
            } else {
                $window.alert("Error: " + response.data.message);
            }
        }, function myError(response) {
            $window.alert("Error: " + response.StatusText);
        });
    }

    $scope.register = function () {
        $http({
            data: {
                firstname: this.register_firstname,
                lastname: this.register_lastname,
                username: this.register_username,
                email: this.register_email,
                password: this.register_password,
                birthday: this.register_birthday
            },
            method: "POST",
            url: "/Authentication/Registration"
        }).then(function mySuccess(response) {
            if (response.data.success) {
                $rootScope.username = response.data.username;
                $location.path("/project/show");
            } else {
                $window.alert("Error: " + response.data.message);
            }
        }, function myError(response) {
            $window.alert("Error: " + response.StatusText);
        });
    }

    $rootScope.logout = function () {
        $http({
            method: "GET",
            url: "/Authentication/Logout"
        }).then(function mySuccess(response) {
            if (response.data.success) {
                $rootScope.username = null;
                $rootScope.checkUserStatus();
            } else {
                $window.alert("Error: " + response.data.message);
            }
        }, function myError(response) {
            $window.alert("Error: " + response.StatusText);
        });
    }
});

mainModel.config(function ($routeProvider) {
    $routeProvider
        .when("/login", {
            templateUrl: "/Authentication/Login"
        })
        .when("/register", {
            templateUrl: "/Authentication/Register"
        })
        .when("/logout", {
            templateUrl: "/Authentication/Logout"
        })
});