var uncrm = angular.module('uncrm', ['ngRoute']);

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
    return $sce.trustAsHtml(marked(input));
  };
});

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
      var processNote = function(note) {
        note.Created = new Date(parseInt(note.Created.substr(6)));
        note.Modified = new Date(parseInt(note.Modified.substr(6)));
        return note;
      };
      var processAttachment = function(attachment) {
        attachment.Created = new Date(parseInt(attachment.Created.substr(6)));
        attachment.Modified = new Date(parseInt(attachment.Modified.substr(6)));
        return attachment;
      };
      console.log(response);
      $scope.id = response.Id;
      $scope.title = response.Title;
      $scope.networkAttachmentsFolder = response.NetworkAttachmentsFolder;
      $scope.description = response.Description;
      $scope.owner = response.Owner;
      $scope.company = response.Company;
      $scope.url = response.Url;
      $scope.notes = response.Notes;
      $scope.attachments = response.NetworkAttachments;
      _.each(response.Notes, processNote);
      _.each(response.NetworkAttachments, processAttachment);
      $scope.loaded = true;
      $scope.commentPosting = false;
      $scope.downloadNoteAttachment = function(attachmentId, filename) {
        document.getElementById('download_iframe').src = "../attachment/getfile/"+attachmentId+"/" + filename;
      };
      $scope.addComment = function() {
        if (!$scope.newCommentBody && !$scope.newCommentTitle)
          return;

        $scope.commentPosting = true;
        $http({
          url: "../incident/" + $routeParams.num + '/notes/add',
          dataType: "json",
          data: {
            'body': $scope.newCommentBody,
            'title': $scope.newCommentTitle
          },
          method: "POST",
          headers: {
              "Content-Type": "application/json; charset=utf-8"
          }
        }).success(function(response) {
          $scope.notes.push(processNote(response));
          $scope.newCommentBody = "";
          $scope.newCommentTitle = "";
          $scope.commentPosting = false;
        });
      }
  }).error(function(error){
      $scope.error = error;
  });
  $scope.incidentNumber = $routeParams.num;
});