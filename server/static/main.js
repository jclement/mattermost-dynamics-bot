var uncrm = angular.module('uncrm', ['ngRoute', 'LocalStorageModule', 'ngFileUpload']);

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

uncrm.directive('textarea', function() {
    return {
        restrict: 'E',
        link: function( scope , element , attributes ) {
            var threshold    = 35,
                minHeight    = element[0].offsetHeight,
                paddingLeft  = element.css('paddingLeft'),
                paddingRight = element.css('paddingRight');

            var $shadow = angular.element('<div></div>').css({
                position:   'absolute',
                top:        -10000,
                left:       -10000,
                width:      element[0].offsetWidth - parseInt(paddingLeft || 0) - parseInt(paddingRight || 0),
                fontSize:   element.css('fontSize'),
                fontFamily: element.css('fontFamily'),
                lineHeight: element.css('lineHeight'),
                resize:     'none'
            });

            angular.element( document.body ).append( $shadow );

            var update = function() {
                var times = function(string, number) {
                    for (var i = 0, r = ''; i < number; i++) {
                        r += string;
                    }
                    return r;
                }

                var val = element.val().replace(/</g, '&lt;')
                    .replace(/>/g, '&gt;')
                    .replace(/&/g, '&amp;')
                    .replace(/\n$/, '<br/>&nbsp;')
                    .replace(/\n/g, '<br/>')
                    .replace(/\s{2,}/g, function( space ) {
                        return times('&nbsp;', space.length - 1) + ' ';
                    });

                $shadow.html( val );

                element.css( 'height' , Math.max( $shadow[0].offsetHeight + threshold , minHeight ) );
            }

            scope.$on('$destroy', function() {
                $shadow.remove();
            });

            element.bind( 'keyup keydown keypress change' , update );
            update();
        }
    }
});

uncrm.factory('Auth', function($http, $rootScope, localStorageService) {

  var authBusy = false;

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
      return authBusy;
    },

    login: function(username, password, success, failure) {
      authBusy = true;
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
        authBusy = false;
        console.log("Logged in");
        localStorageService.set('authenticationName', username);
        localStorageService.set('authenticationToken', response);
        if (success) {
          success();
        }
      }).error(function(response) {
        authBusy = false;
        noty({
          text: response.ResponseStatus.Message,
          timeout: 1000,
          type: "error"
        });
        if (failure) {
          failure();
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
    // TODO: An actual search by string implementation?
    if ($scope.incidentNumber) {
      $location.url("/incident/" + $scope.incidentNumber);
    }
  }
});

uncrm.controller('searchCtrl', function($scope, $location, Auth) {
  $scope.isLoggedIn = Auth.isLoggedIn;
  $scope.go = function() {
    // TODO: An actual search by string implementation?
    if ($scope.incidentNumber) {
      $location.url("/incident/" + $scope.incidentNumber);
      $scope.incidentNumber = '';
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
