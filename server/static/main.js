var uncrm = angular.module('uncrm', ['ngRoute', 'LocalStorageModule']);

uncrm.config(function(localStorageServiceProvider) {
  localStorageServiceProvider
    .setPrefix("uncrm");
});

uncrm.config(function($routeProvider) {

  $routeProvider.when('/', {
    templateUrl: 'templates/index.html',
    controller: 'mainCtrl'
  });

  $routeProvider.when('/incident/:num', {
    templateUrl: 'templates/incident.html',
    controller: 'incidentCtrl'
  });

});

uncrm.factory('Auth', function($http, $rootScope, localStorageService) {

  return {
    isLoggedIn: function() {
      return !!localStorageService.get('authenticationToken');
    },

    getLoggedInName: function() {
      return localStorageService.get('authenticationName');
    },

    getToken: function() {
      return localStorageService.get('authenticationToken');
    },

    logout: function() {
      localStorageService.remove('authenticationToken');
      localStorageService.remove('authenticationName');
    },

    isBusy: function() {
      return !!this._busy;
    },

    login: function(username, password) {
      var that = this;
      that._busy = true;
      $http({
        url: "../login",
        dataType: "json",
        method: "POST",
        data: {
          Username: username,
          Password: password
        },
        headers: {
            "Content-Type": "application/json; charset=utf-8"
        }
      }).success(function(response){
        that._busy = false;
        console.log("Logged in");
        localStorageService.set('authenticationName', username);
        localStorageService.set('authenticationToken', response);
      }).error(function(response) {
        that._busy = false;
        noty({
          text: response.ResponseStatus.Message,
          timeout: 1000,
          type: "error"
        });
      });
    }

  };

});

marked.setOptions({
  renderer: new marked.Renderer(),
  gfm: true,
  tables: true,
  breaks: false,
  pedantic: false,
  sanitize: true,
  smartLists: true,
  smartypants: false
});

uncrm.filter('comment', function ($sce) {
  return function (input) {
    return $sce.trustAsHtml(_.escape(input).replace(/\n/g, "<br/>"));
  };
});

uncrm.filter('marked', function ($sce) {
  return function (input) {
    return $sce.trustAsHtml(marked(input || ''));
  };
});

uncrm.controller('mainCtrl', function($scope, $location) {
  $scope.go = function() {
    if ($scope.incidentNumber) {
      $location.url("/incident/" + $scope.incidentNumber);
    }
  }
});


uncrm.controller('authCtrl', function($scope, $location, Auth) {
  $scope.isLoggedIn = Auth.isLoggedIn;
  $scope.getLoggedInName = Auth.getLoggedInName;
  $scope.logout = Auth.logout;
  $scope.isBusy = Auth.isBusy;
  $scope.login = function() {
    Auth.login($scope.username, $scope.password, function() {
      $scope.username = '';
      $scope.password='';
    });
  }
});
