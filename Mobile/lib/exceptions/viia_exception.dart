enum ViiaErrorType {
    AuthenticationFailed,
    BadRequest
}

class ViiaException implements Exception {
  ViiaErrorType errorType;
  String message;

  ViiaException(this.errorType, this.message);
}