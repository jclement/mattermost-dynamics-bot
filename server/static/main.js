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

    getToken: function() {
      return localStorageService.get('authenticationToken');
    },

    logout: function() {
      localStorageService.remove('authenticationToken');
    },

    login: function(username, password, callback) {
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
        console.log("Logging in");
        localStorageService.set('authenticationToken', response);
        if (callback) {
          callback();
        }
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

