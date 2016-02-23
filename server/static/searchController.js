uncrm.controller('searchCtrl', function ($scope, $routeParams, $http, localStorageService, Auth) {
  $scope.loaded = false;
  $scope.isLoggedIn = Auth.isLoggedIn;

  console.log('this is not being executed');

  $http({
    url: '../incident/search',
    dataType: 'json',
    method: 'GET',
    headers: {
      'Content-Type': 'application/json; charset=utf-8'
    },
    data: {
      Query: 'test'
    }
  }).then(
    // success handler
    function (response) {
      console.log('success:', response);
      $scope.loaded = true;
      return;
    },
    //failure handler
    function (response) {
      console.log('failure:', response);
      $scope.loaded = true;
      return;
    }
  );

});
