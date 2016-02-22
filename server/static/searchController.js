uncrm.controller('searchCtrl', function ($scope, $routeParams, $http, localStorageService, Auth) {
  $scope.loaded = false;
  $scope.isLoggedIn = Auth.isLoggedIn;

  console.log('why won\'t this section execute?!', arguments);

  // $http({
  //   url: ''
  // })
});
