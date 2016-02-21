uncrm.controller('incidentCtrl', function($scope, $routeParams, $http, localStorageService, Auth) {
  $scope.loaded = false;
  $scope.isLoggedIn = Auth.isLoggedIn;
  $http({
    url: "../incident/" + $routeParams.num,
    dataType: "json",
    method: "GET",
    headers: {
        "Content-Type": "application/json; charset=utf-8"
    }
  }).success(function(response){
      if (!response) {
        $scope.error = "Incident Not Found";
        $scope.loaded = true;
        return;
      }
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
      _.each(response.Notes, processNote);
      _.each(response.NetworkAttachments, processAttachment);
      $scope.incident = response;
      $scope.loaded = true;
      $scope.commentPosting = false;
      $scope.downloadNoteAttachment = function(attachmentId, filename) {
        document.getElementById('download_iframe').src = "../attachment/getfile/"+attachmentId+"/" + filename;
      };
      $scope.deleteNote = function(note) {
        note.doomed = true;
        $http({
          url: '../notes/' + note.Id + '/delete',
          dataType: "json",
          data: {
            'authenticationToken': Auth.getToken()
          },
          method: "POST",
          headers: {
              "Content-Type": "application/json; charset=utf-8"
          }
        }).success(function(response) {
          var i = $scope.incident.Notes.indexOf(note);
          if(i != -1) {
            $scope.incident.Notes.splice(i, 1);
          }
        }).error(function(response) {
          note.doomed = false;
          noty({
            text: response.ResponseStatus.Message,
            type: "error"
          });
        });
      };
      $scope.addNote = function() {
        if (!$scope.newCommentBody && !$scope.newCommentTitle)
          return;

        $scope.commentPosting = true;
        $http({
          url: "../incident/" + $routeParams.num + '/notes/add',
          dataType: "json",
          data: {
            'body': $scope.newCommentBody,
            'title': $scope.newCommentTitle,
            'authenticationToken': Auth.getToken()
          },
          method: "POST",
          headers: {
              "Content-Type": "application/json; charset=utf-8"
          }
        }).success(function(response) {
          $scope.incident.Notes.push(processNote(response));
          $scope.newCommentBody = "";
          $scope.newCommentTitle = "";
          $scope.commentPosting = false;
        }).error(function(response) {
          noty({
            text: response.ResponseStatus.Message,
            type: "error"
          });
        });
      };
  }).error(function(error){
      $scope.error = error;
  });
  $scope.incidentNumber = $routeParams.num;
});