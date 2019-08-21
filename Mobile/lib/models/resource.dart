import 'package:http/http.dart';

class Resource<T> {
  T Function(Response response) parse;

  Resource({this.parse});
}