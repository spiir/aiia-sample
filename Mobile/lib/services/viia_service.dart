import 'dart:async';
import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';
import 'package:http/http.dart';
import 'package:viia_sample/exceptions/viia_exception.dart';
import 'package:viia_sample/models/resource.dart';
import 'package:viia_sample/models/account.dart';
import 'package:viia_sample/models/transaction.dart';

class ViiaService {
  // Our local mobile api for calls that involve using client secret
  // which shouldn't be exposed to public
  static String _localServerBaseUrl = "http://localhost:5030/api";
  static String _viiaApiBaseUrl = "https://api.getviia.com";

  // Keys used for storing data in shared preferences
  static String _tokenKey = 'accessToken';
  static String _refreshTokenKey = 'refreshToken';

  ViiaService();


  Future<String> getPreferenceString(String key) async {
    var sharedPreferences = await SharedPreferences.getInstance();
    return sharedPreferences.getString(key);
  }

  setPreferenceString(String key, String value) async {
    var sharedPreferences = await SharedPreferences.getInstance();
    sharedPreferences.setString(key, value);
  }

  logout() async {
    await setPreferenceString(_tokenKey, null);
    await setPreferenceString(_refreshTokenKey, null);
  }


  String getConnectUrl() {
    return _localServerBaseUrl + '/connect';
  }

  Future<bool> isAuthenticated() async {
    var accessToken = await getPreferenceString(_tokenKey);
    return accessToken != null;
  }

  // Call our local api with code we received from Viia connect flow
  // local api will exchange it for access token and refresh token and return that to the app
  Future fetchAccessToken(String code) async {
    await post(_localServerBaseUrl + '/token',
        body: '{"code": "$code"}',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }).then((response) async {
      var jsonBody = json.decode(response.body);
      await setPreferenceString(_tokenKey, jsonBody['accessToken']);
      await setPreferenceString(_refreshTokenKey, jsonBody['refreshToken']);
    });
  }

  Future refreshAccessToken() async {
    var refreshToken = await getPreferenceString(_refreshTokenKey);
    await post(_localServerBaseUrl + '/token',
        body: '{"refreshToken":"$refreshToken"}',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }).then((response) async {
      var jsonBody = json.decode(response.body);
      await setPreferenceString(_tokenKey, jsonBody['accessToken']);
      await setPreferenceString(_refreshTokenKey, jsonBody['refreshToken']);
    });
  }

  Future<String> _getAuthorizationHeaderValue() async {
    var token = await getPreferenceString(_tokenKey);
    return 'Bearer $token';
  }

  Future<Map<String, String>> _getViiaHeader() async {
    return {
      "Accept": "application/json",
      "Authorization": await _getAuthorizationHeaderValue()
    };
  }

  // General method to fetch resources from Viia
  Future<T> _load<T>(Resource<T> resource, String path) async {
    var url = '$_viiaApiBaseUrl$path';
    final response = await get(url, headers: await _getViiaHeader());
    if (response.statusCode == 200) {
      return resource.parse(response);
    } else if (response.statusCode == 401) {
      await setPreferenceString(_tokenKey, null);
      throw ViiaException(
          ViiaErrorType.AuthenticationFailed, "Viia responded with 401.");
    } else {
      throw ViiaException(ViiaErrorType.BadRequest,
          "Viia responded with ${response.statusCode} code");
    }
  }

  Future<PaginatedTransactions> getTransactions(String accountId,
      {String pagingToken, int pageSize = 10, bool isRetry = false}) async {
    try {
      var url = '/v1/accounts/$accountId/transactions?pageSize=$pageSize';
      if(pagingToken != null) {
        url += '&pagingToken=$pagingToken';
      }
      return await _load(
          PaginatedTransactions.all, url);
    } on ViiaException catch (exception) {
      // Retry once on authorization error when calling Viia
      if (!isRetry &&
          exception.errorType == ViiaErrorType.AuthenticationFailed) {
        await refreshAccessToken();
        return getTransactions(accountId, isRetry: true);
      }

      // Otherwise rethrow
      throw exception;
    }
  }

  Future<List<Account>> getAccounts({bool isRetry = false}) async {
    try {
      return await _load(Account.all, '/v1/accounts');
    } on ViiaException catch (exception) {
      // Retry once on authorization error when calling Viia
      if (!isRetry &&
          exception.errorType == ViiaErrorType.AuthenticationFailed) {
        await refreshAccessToken();
        return getAccounts(isRetry: true);
      }

      // Otherwise rethrow
      throw exception;
    }
  }
}
