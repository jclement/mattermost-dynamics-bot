var uncrm = angular.module('uncrm', ['ngRoute']);

uncrm.config(function($routeProvider) {

  $routeProvider.when('/', {
    templateUrl: 'templates/index.html',
    controller: 'mainCtrl'
  })

  $routeProvider.when('/incident/:num', {
    templateUrl: 'templates/incident.html',
    controller: 'incidentCtrl'
  })

});

uncrm.controller('mainCtrl', function($scope) {
  $scope.message = "Hello World";
});

uncrm.controller('incidentCtrl', function($scope, $routeParams, $http) {
  $http({
    url: "../incident/" + $routeParams.num,
    dataType: "json",
    method: "GET",
    headers: {
        "Content-Type": "application/json; charset=utf-8"
    }
  }).success(function(response){
      $scope.title = response.Title;
      $scope.description = response.Description;
      $scope.owner = response.Owner;
      $scope.company = response.Company;
      $scope.url = response.Url;
      $scope.loaded = true;
  }).error(function(error){
      $scope.error = error;
  });
  $scope.incidentNumber = $routeParams.num;
});